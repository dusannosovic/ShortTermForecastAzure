using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common
{
    public class DataTransform
    {
        public static WeatherMl[] TransformData(List<WeatherEntity> weatherEntities)
        {
            List<WeatherMl> weathers = new List<WeatherMl>();
            foreach(WeatherEntity weatherEntity in weatherEntities)
            {
                WeatherMl weather = new WeatherMl();
                weather.Year = weatherEntity.LocalTime.Year;
                weather.Month = weatherEntity.LocalTime.Month;
                weather.Day = (float)weatherEntity.LocalTime.DayOfWeek;
                weather.Hour = weatherEntity.LocalTime.Hour;
                weather.Minute = weatherEntity.LocalTime.Minute;
                weather.Temperature = (float)weatherEntity.Temperature;
                weather.APressure = (float)weatherEntity.APressure;
                weather.Pressure = (float)weatherEntity.Pressure;
                weather.PTendency = (float)weatherEntity.PTencdency;
                weather.Humidity = (float)weatherEntity.Humidity;
                weather.WindSpeed = (float)weatherEntity.WindSpeed;
                weather.HVisibility = (float)weatherEntity.HVisibility;
                weather.DTemperature = (float)weatherEntity.DTemperature;
                weather.Load = (float)weatherEntity.Load;
                weathers.Add(weather);
            }
            WeatherMl[] weatherMl = weathers.ToArray();
            return weatherMl;
        }
    }
}
