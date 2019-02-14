using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace fire_detecting_system.Models
{
    public class ActiveAlarm
    {
        public string Name { get; set; }

        public string Measurement { get; set; }

        public double Value { get; set; }

        public DateTime TimeOfActivation { get; set; }

        public ActiveAlarm()
        {

        }

        public ActiveAlarm(string name, string measurement, double value, DateTime timeOfActivation)
        {
            Name = name;
            Measurement = measurement;
            Value = value;
            TimeOfActivation = timeOfActivation;
        }

        public override bool Equals(object obj)
        {
            ActiveAlarm alarm = obj as ActiveAlarm;

            return alarm != null && alarm.Name == Name && alarm.Measurement == Measurement && alarm.Value == Value;
        }

        public override int GetHashCode()
        {
            return (Name == null ? 0 : Name.GetHashCode()) ^ (Measurement == null ? 0 : Measurement.GetHashCode()) ^ Value.GetHashCode();
        }
    }
}
