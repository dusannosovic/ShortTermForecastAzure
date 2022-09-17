using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Common
{
    public class WeatherEntity
    {
        public DateTime LocalTime { get; set; }
    
        public Double Temperature { get; set; }
        
        public Double APressure { get; set; }
        
        public Double Pressure { get; set; }
        
        public Double PTencdency { get; set; }
        
        public Int32 Humidity { get; set; }
        
        public String WindDirection { get; set; }
        
        public Int32 WindSpeed { get; set; }
        
        public Double Clouds { get; set; }
        
        public Double HVisibility { get; set; }
        
        public Double DTemperature { get; set; }
        
        public Int32 Load { get; set; }
    }
}
