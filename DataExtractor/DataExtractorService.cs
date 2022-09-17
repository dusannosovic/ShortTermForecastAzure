using Common;
using ExcelDataReader;
using Microsoft.ServiceFabric.Services.Client;
using Microsoft.ServiceFabric.Services.Communication.Client;
using Microsoft.ServiceFabric.Services.Communication.Wcf;
using Microsoft.ServiceFabric.Services.Communication.Wcf.Client;
using System;
using System.Collections.Generic;
using System.Data;
using System.Fabric;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataExtractor
{
    public class DataExtractorService : IDataExtractorService
    {
        public async Task<bool> SendDataToPredict(MemoryStream stream)
        {
            DataSet dsexcelRecords = new DataSet();
            IExcelDataReader reader = null;
            reader = ExcelReaderFactory.CreateOpenXmlReader(stream);
            dsexcelRecords = reader.AsDataSet();
            reader.Close();
            if (dsexcelRecords != null && dsexcelRecords.Tables.Count > 0 && dsexcelRecords.Tables["load"] != null)
            {
                List<WeatherEntity> weatherEntities = FileParser.XlsxFileParser(dsexcelRecords, false);
                //List<WeatherEntity> weatherEntities = new List<WeatherEntity>();

                FabricClient fabricClient1 = new FabricClient();
                int partitionsNumber1 = (await fabricClient1.QueryManager.GetPartitionListAsync(new Uri("fabric:/ShortTermForecastAzure/PredictStatefulService"))).Count;
                var binding1 = WcfUtility.CreateTcpClientBinding();
                int index1 = 0;
                for (int i = 0; i < partitionsNumber1; i++)
                {
                    ServicePartitionClient<WcfCommunicationClient<IPredictService>> servicePartitionClient1 = new ServicePartitionClient<WcfCommunicationClient<IPredictService>>(
                        new WcfCommunicationClientFactory<IPredictService>(clientBinding: binding1),
                        new Uri("fabric:/ShortTermForecastAzure/TrainingStatefulService"),
                        new ServicePartitionKey(index1 % partitionsNumber1));
                    bool a = await servicePartitionClient1.InvokeWithRetryAsync(client => client.Channel.Predict(weatherEntities));

                    index1++;
                }
                return true;
            }

            return false;
        }

        public async Task<bool> SendDataToTraning(MemoryStream stream)
        {
            DataSet dsexcelRecords = new DataSet();
            IExcelDataReader reader = null;
            reader = ExcelReaderFactory.CreateOpenXmlReader(stream);
            dsexcelRecords = reader.AsDataSet();
            reader.Close();
            if (dsexcelRecords != null && dsexcelRecords.Tables.Count > 0 && dsexcelRecords.Tables["load"] != null)
            {
                List<WeatherEntity> weatherEntities = FileParser.XlsxFileParser(dsexcelRecords, true);
                //List<WeatherEntity> weatherEntities = new List<WeatherEntity>();

                FabricClient fabricClient1 = new FabricClient();
                int partitionsNumber1 = (await fabricClient1.QueryManager.GetPartitionListAsync(new Uri("fabric:/ShortTermForecastAzure/TrainingStatefulService"))).Count;
                var binding1 = WcfUtility.CreateTcpClientBinding();
                int index1 = 0;
                for (int i = 0; i < partitionsNumber1; i++)
                {
                    ServicePartitionClient<WcfCommunicationClient<ITrainingService>> servicePartitionClient1 = new ServicePartitionClient<WcfCommunicationClient<ITrainingService>>(
                        new WcfCommunicationClientFactory<ITrainingService>(clientBinding: binding1),
                        new Uri("fabric:/ShortTermForecastAzure/TrainingStatefulService"),
                        new ServicePartitionKey(index1 % partitionsNumber1));
                    bool a = await servicePartitionClient1.InvokeWithRetryAsync(client => client.Channel.ReceiveData(weatherEntities));

                    index1++;
                }
                return true;
            }

            return false;
        }
    }
}
