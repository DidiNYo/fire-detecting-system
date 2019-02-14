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

        public ActiveAlarm()
        {

        }

        public ActiveAlarm(string name, string measurement, double value)
        {
            Name = name;
            Measurement = measurement;
            Value = value;
        }

        public override bool Equals(object obj)
        {
            ActiveAlarm alarm = obj as ActiveAlarm;

            return alarm != null && alarm.Name == this.Name && alarm.Measurement == this.Measurement && alarm.Value == this.Value;
        }

        public override int GetHashCode()
        {
            return Name.GetHashCode() ^ Measurement.GetHashCode() ^ Value.GetHashCode();
        }
    }
}
