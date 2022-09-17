using Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PredictStatefulService
{
    class PredictService : IPredictService
    {
        public async Task<bool> Predict(List<WeatherEntity> weatherEntities)
        {

            return true;
        }
    }
}
