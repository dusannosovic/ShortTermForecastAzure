using Common;
using DataPreparer;
using ExcelDataReader;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.ServiceFabric.Services.Client;
using Microsoft.ServiceFabric.Services.Communication.Client;
using Microsoft.ServiceFabric.Services.Communication.Wcf;
using Microsoft.ServiceFabric.Services.Communication.Wcf.Client;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Fabric;
using System.IO;
using System.Linq;
using System.ServiceModel;
using System.Threading.Tasks;
using System.Web;
using Web.Models;
using Common;

namespace Web.Controllers
{
    public class HomeController : Controller
    {
        public async Task<IActionResult> Index()
        {
            //List<WeatherEntity> weatherEntities = new List<WeatherEntity>();
            long numberOfRows = 0;
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
                numberOfRows = await servicePartitionClient1.InvokeWithRetryAsync(client => client.Channel.GetAllData());

                index1++;
            }
            return View(numberOfRows);
        }
        [HttpPost]
        [Route("/Home/UploadFile")]
        public async Task<IActionResult> UploadFile(IFormFile file, [FromServices] IHostingEnvironment hostingEnvironment)
        {
            long numberOfRows = 0;
            string fileName = $"{hostingEnvironment.WebRootPath}\\files\\{file.FileName}";
            //Stream fileStream = null;
            if (file.Length > 0 && file.FileName.EndsWith(".xlsx"))
            {
                    using (var fileStream = new MemoryStream()) {
                    file.CopyTo(fileStream);
                    var myBinding = new NetTcpBinding(SecurityMode.None);
                    var myEndpoint = new EndpointAddress("net.tcp://localhost:53850/WebCommunication");

                    using (var myChannelFactory = new ChannelFactory<IDataExtractorService>(myBinding, myEndpoint))
                    {
                        IDataExtractorService client = null;
                        ViewData["Title"] = null;
                        try
                        {
                            client = myChannelFactory.CreateChannel();

                            if (await client.SendDataToTraning(fileStream))
                            {
                                ViewData["Title"] = "Knjiga je uspesno kupljena";
                            }
                            else
                            {
                                ViewData["Title"] = "Polja nisu ispravno popunjena";
                            }
                            ((ICommunicationObject)client).Close();
                            myChannelFactory.Close();
                        }
                        catch
                        {
                            (client as ICommunicationObject)?.Abort();
                        }
                    }
                    numberOfRows = await GetNumberofRows();
                    return View("Index",numberOfRows);
                    
                }
            }
            numberOfRows = await GetNumberofRows();
            return View("Index", numberOfRows);
        }
        [HttpPost]
        [Route("/Home/Training")]
        public async Task<IActionResult> Training()
        {
            long numberOfRows = 0;
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
                bool a = await servicePartitionClient1.InvokeWithRetryAsync(client => client.Channel.TrainingData());

                index1++;
            }
            numberOfRows = await GetNumberofRows();
            return View("Index",numberOfRows);
        }
        [HttpPost]
        [Route("/Home/Prediction")]
        public async Task<IActionResult> Prediction(IFormFile file, [FromServices] IHostingEnvironment hostingEnvironment)
        {
            string fileName = $"{hostingEnvironment.WebRootPath}\\files\\{file.FileName}";
            //Stream fileStream = null;
            if (file.Length > 0)
            {
                using (var fileStream = new MemoryStream())
                {
                    file.CopyTo(fileStream);
                    DataSet dsexcelRecords = new DataSet();
                    IExcelDataReader reader = null;
                    if (file.FileName.EndsWith(".xlsx"))
                    {
                        reader = ExcelReaderFactory.CreateOpenXmlReader(fileStream);
                    }
                    else
                    {
                        ViewBag.Message = "Wrong format of file";
                        return View();
                    }
                    dsexcelRecords = reader.AsDataSet();
                    reader.Close();
                    if (dsexcelRecords != null && dsexcelRecords.Tables.Count > 0 && dsexcelRecords.Tables["load"] == null)
                    {
                        List<WeatherEntity> weatherEntities = FileParser.XlsxFileParser(dsexcelRecords, false);
                        //List<WeatherEntity> weatherEntities = new List<WeatherEntity>();

                        FabricClient fabricClient1 = new FabricClient();
                        int partitionsNumber1 = (await fabricClient1.QueryManager.GetPartitionListAsync(new Uri("fabric:/ShortTermForecastAzure/DataPreparer"))).Count;
                        var binding1 = WcfUtility.CreateTcpClientBinding();
                        int index1 = 0;
                        for (int i = 0; i < partitionsNumber1; i++)
                        {
                            ServicePartitionClient<WcfCommunicationClient<IDataPreparerService>> servicePartitionClient1 = new ServicePartitionClient<WcfCommunicationClient<IDataPreparerService>>(
                                new WcfCommunicationClientFactory<IDataPreparerService>(clientBinding: binding1),
                                new Uri("fabric:/ShortTermForecastAzure/DataPreparer"),
                                new ServicePartitionKey(index1 % partitionsNumber1));
                            bool a = await servicePartitionClient1.InvokeWithRetryAsync(client => client.Channel.Prediction(weatherEntities));

                            index1++;
                        }

                    }
                }
            }

            return View("Index");
        }
        public ActionResult About()
        {
            return View();
        }
        public ActionResult Contact()
        {
            return View();
        }
        public async Task<long> GetNumberofRows()
        {
            long numberOfRows = 0;
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
                numberOfRows = await servicePartitionClient1.InvokeWithRetryAsync(client => client.Channel.GetAllData());

                index1++;
            }
            return numberOfRows;
        }
        //try
        //{
        //    if (file.ContentLength > 0)
        //    {
        //        DataSet dsexcelRecords = new DataSet();
        //        Stream fileStream = null;

        //        IExcelDataReader reader = null;
        //        fileStream = file.InputStream;
        //        if(file != null && fileStream != null)
        //        {
        //            if (file.FileName.EndsWith(".xlsx"))
        //            {
        //                reader = ExcelReaderFactory.CreateOpenXmlReader(fileStream);
        //            }
        //            else
        //            {
        //                ViewBag.Message = "Wrong format of file";
        //                return View();
        //            }
        //            dsexcelRecords = reader.AsDataSet();
        //            reader.Close();
        //            if (dsexcelRecords != null && dsexcelRecords.Tables.Count > 0 && dsexcelRecords.Tables["load"] != null)
        //            {
        //                List<WeatherEntity> weatherEntities = FileParser.XlsxFileParser(dsexcelRecords, true);
        //            }
        //        }

        //    }
        //    ViewBag.Message = "File Uploaded Successfully!!";
        //    return View();
        //    }
        //    catch
        //    {
        //        ViewBag.Message = "File upload failed!!";
        //        return View();
        //    }
        //}

    }
}   
