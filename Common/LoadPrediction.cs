using Microsoft.ML.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common
{
    public class LoadPrediction
    {
        [ColumnName("Score")]
        public float Load { get; set; }
    }
}
