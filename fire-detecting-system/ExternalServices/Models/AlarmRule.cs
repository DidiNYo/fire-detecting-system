using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExternalServices.Models
{
    public class AlarmRule
    {
        private string sensorName;

        private string measurementType;

        private char sign;

        private double value;

        public string SensorName
        {
            get
            {
                return sensorName;
            }
            set
            {
                sensorName = value != null ? value : null;
            }
        }

        public string MeasurementType
        {
            get
            {
                return measurementType;
            }
            set
            {
                measurementType = value != null ? value : null;
            }
        }

        public char Sign
        {
            get
            {
                return sign;
            }
            set
            {
                sign = value != '\0' ? value : '\0';
            }
        }

        public double Value
        {
            get
            {
                return value;
            }
            set
            {
                value = value != 0 ? value : 0;
            }
        }
    }
}
