using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace Common
{
    [ServiceContract]
    public interface IDataExtractorService
    {
        [OperationContract]
        Task<bool> SendDataToTraning(MemoryStream stream);
        [OperationContract]
        Task<bool> SendDataToPredict(MemoryStream stream);
    }
}
