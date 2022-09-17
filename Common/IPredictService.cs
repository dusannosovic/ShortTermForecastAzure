using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace Common
{
    [ServiceContract]
    public interface IPredictService
    {
        [OperationContract]
        Task<bool> Predict(List<WeatherEntity> weatherEntities);
       
    }
}
