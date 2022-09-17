using Microsoft.ML.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common
{
    public class WeatherMl
    {
        public float Year { get; set; }
        public float Month { get; set; }
        public float Day { get; set; }
        public float Hour { get; set; }
        public float Minute { get; set; }
        public float Temperature { get; set; }
        public float APressure { get; set; }
        public float Pressure { get; set; }
        public float PTendency { get; set; }
        public float Humidity { get; set; }
        public float WindSpeed { get; set; }
        public float HVisibility { get; set; }
        public float DTemperature { get; set; }
        public float Load { get; set; }

    }
    public class HouseData
    {
        public float Size { get; set; }
        public float Price { get; set; }
    }

    public class Prediction
    {
        [ColumnName("Score")]
        public float Price { get; set; }
    }
}
