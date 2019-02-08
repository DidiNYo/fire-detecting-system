using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace fire_detecting_system.Models
{
    public class AlarmRule
    {
        public string SensorName { get; set; }

        public string MeasurementType { get; set; }

        public string Sign { get; set; }

        public double Value { get; set; }

        public AlarmRule()
        {

        }

        public AlarmRule(string name, string measurementType, string sign, double value)
        {
            SensorName = name;
            MeasurementType = measurementType;
            Sign = sign;
            Value = value;
        }
    }
}
