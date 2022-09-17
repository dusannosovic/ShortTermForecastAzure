using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace Common
{
    [ServiceContract]
    public interface ITrainingService
    {
        [OperationContract]
        Task<bool> ReceiveData(List<WeatherEntity> weatherEntities);
        [OperationContract]
        Task<bool> TrainingData();
        [OperationContract]
        Task<long> GetAllData();
    }
}
