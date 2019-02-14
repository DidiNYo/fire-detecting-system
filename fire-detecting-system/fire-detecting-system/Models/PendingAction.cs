using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace fire_detecting_system.Models
{

    public class PendingAction
    {
        public string SensorName { get; set; }

        public string Measurement { get; set; }

        public Predicate<double> predicate;
    }
}
