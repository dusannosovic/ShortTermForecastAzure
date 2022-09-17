using Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace DataPreparer
{
    [ServiceContract]
    
    public interface IDataPreparerService
    {
        [OperationContract]
        Task<bool> PublishEntites(List<WeatherEntity> weatherEntities);
        [OperationContract]
        Task<bool> Prediction(List<WeatherEntity> weatherEntities);
    }
}
