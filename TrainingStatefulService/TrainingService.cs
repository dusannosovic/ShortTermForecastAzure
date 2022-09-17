using Azure.Storage.Blobs;
using Common;
using Microsoft.ML;
using Microsoft.ServiceFabric.Data;
using Microsoft.ServiceFabric.Data.Collections;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrainingStatefulService
{
    class TrainingService : ITrainingService
    {
        IReliableDictionary<int, WeatherEntity> WeatherEntities;
        IReliableStateManager StateManager;
        public TrainingService()
        {

        }
        public TrainingService(IReliableStateManager stateManager)
        {
            StateManager = stateManager;
        }

        public async Task<long> GetAllData()
        {
            //List<WeatherEntity> weatherEntities = new List<WeatherEntity>();
            long number = 0;
            WeatherEntities = await this.StateManager.GetOrAddAsync<IReliableDictionary<int, WeatherEntity>>("WeatherEntities");
            using(var tx = this.StateManager.CreateTransaction())
            {
                //var enumerator = (await WeatherEntities.CreateEnumerableAsync(tx)).GetAsyncEnumerator();
                //while(await enumerator.MoveNextAsync(new System.Threading.CancellationToken()))
                //{
                //    weatherEntities.Add(enumerator.Current.Value);
                //}
                number = await WeatherEntities.GetCountAsync(tx);
            }

            return number ;
        }

        public async Task<bool> ReceiveData(List<WeatherEntity> weatherEntities)
        {
            WeatherEntities = await this.StateManager.GetOrAddAsync<IReliableDictionary<int, WeatherEntity>>("WeatherEntities");
            using (var tx = this.StateManager.CreateTransaction())
            {
                var enumerator = (await WeatherEntities.CreateEnumerableAsync(tx)).GetAsyncEnumerator();
                while(await enumerator.MoveNextAsync(new System.Threading.CancellationToken()))
                {
                    await WeatherEntities.TryRemoveAsync(tx, enumerator.Current.Key);

                }
                await tx.CommitAsync();
            }
            using (var tx = this.StateManager.CreateTransaction())
            {
                for(int i = 0; i < weatherEntities.Count; i++)
                {
                    await WeatherEntities.TryAddAsync(tx, i, weatherEntities[i]);
                }
                await tx.CommitAsync();
            }
            return true;
        }

        public async Task<bool> TrainingData()
        {

            List<WeatherEntity> weatherEntities = new List<WeatherEntity>();
            WeatherEntities = await this.StateManager.GetOrAddAsync<IReliableDictionary<int, WeatherEntity>>("WeatherEntities");
            long numberofRows = 0;
            using (var tx = this.StateManager.CreateTransaction())
            {
                numberofRows = await WeatherEntities.GetCountAsync(tx);
                var enumerator = (await WeatherEntities.CreateEnumerableAsync(tx)).GetAsyncEnumerator();
                while (await enumerator.MoveNextAsync(new System.Threading.CancellationToken()))
                {
                    weatherEntities.Add(enumerator.Current.Value);
                }
            }
            if(numberofRows == 0)
            {
                return false;
            }

            MLContext mlContext = new MLContext();
            List<WeatherEntity> smallerListWeather = weatherEntities.GetRange(0, 29000);
            //List<WeatherEntity> smallerListWeather = weatherEntities;
            IDataView trainingData = mlContext.Data.LoadFromEnumerable(DataTransform.TransformData(smallerListWeather));
            List<WeatherEntity> smallertestListWeather = weatherEntities.GetRange(29000, 279);
            //IDataView testData = mlContext.Data.LoadFromEnumerable(smallertestListWeather);
            ////long a = (long)trainingData.GetRowCount();
            //var settings = new RegressionExperimentSettings
            //{
            //    MaxExperimentTimeInSeconds = 10
            //};
            //var pipeline = mlContext.Transforms.Concatenate("Features", new[] { "Year","Month","Day","Hour","Minute","Temperature","APressure","Pressure","PTendency","Humidity","WindSpeed","HVisibility","DTemperature" });
            //var trainer = mlContext.Regression.Trainers.Sdca(labelColumnName: "Load", featureColumnName: "Features");
            //var trainingPipeline = pipeline.Append(trainer);
            //ITransformer model = trainingPipeline.Fit(trainingData);


            //var load = mlContext.Model.CreatePredictionEngine<WeatherMl, LoadPrediction>(model);
            //var pResult = load.Predict(test);
            //IEstimator<ITransformer> forecastEstimator = mlContext.Forecasting.ForecastBySsa(

            //)

            //var trainer = mlContext.Regression.Trainers.Sdca(labelColumnName: "Load");
            //MLContext mlContext = new MLContext();

            // 1. Import or create training data
            //IDataView trainingData = mlContext.Data.LoadFromEnumerable(houseData);

            // 2. Specify data preparation and model training pipeline
            var pipeline = mlContext.Transforms.Concatenate("Features", new[] { "Year", "Month", "Day", "Hour", "Minute", "Temperature", "APressure", "Pressure", "PTendency", "Humidity", "WindSpeed", "HVisibility", "DTemperature" })
                .Append(mlContext.Regression.Trainers.Sdca(labelColumnName: "Load", maximumNumberOfIterations: 500));

            // 3. Train model
            var model = pipeline.Fit(trainingData);



            // 4. Make a prediction
            //var size = new HouseData() { Size = 2.5F };
            float avg = 0;
            foreach (WeatherEntity wEntity in smallertestListWeather)
            {
                var test = new WeatherMl() { Year = wEntity.LocalTime.Year, Month = wEntity.LocalTime.Month, Day = (float)wEntity.LocalTime.DayOfWeek, Hour = wEntity.LocalTime.Hour, Minute = wEntity.LocalTime.Minute, Temperature = (float)wEntity.Temperature, APressure = (float)wEntity.APressure, Pressure = (float)wEntity.Pressure, PTendency = (float)wEntity.PTencdency, Humidity = (float)wEntity.Humidity, WindSpeed = wEntity.WindSpeed, HVisibility = (float)wEntity.HVisibility, DTemperature = (float)wEntity.DTemperature, Load = (float)wEntity.Load };

                var price = mlContext.Model.CreatePredictionEngine<WeatherMl, LoadPrediction>(model).Predict(test);
                avg += (Math.Abs(price.Load - test.Load) / test.Load) * 100;
            }
            float prosek = avg / smallertestListWeather.Count;

            //var metrics = mlContext.Regression.Evaluate(testData, "Label", "Score");

            //Console.WriteLine($"Predicted price for size: {size.Size * 1000} sq ft= {price.Price * 100:C}k");

            var blobServiceClient = new BlobServiceClient("UseDevelopmentStorage=true");
            string containerName = "modelsaver";
            blobServiceClient.DeleteBlobContainer(containerName);
            BlobContainerClient containerClient = await blobServiceClient.CreateBlobContainerAsync(containerName);
            BlobClient blobClient = containerClient.GetBlobClient(containerName);
            var memoryStream = new MemoryStream();
            mlContext.Model.Save(model, trainingData.Schema, memoryStream);
            BinaryData binary = new BinaryData(memoryStream.ToArray());
            await blobClient.UploadAsync(binary, true);

            return true;
        }
        
    }
}
