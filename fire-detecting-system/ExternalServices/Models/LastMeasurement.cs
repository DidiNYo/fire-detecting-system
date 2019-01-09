using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExternalServices.Models
{
    public class LastMeasurement
    {
        public string OrganizationItemName { get; set; }

        public int OrganizationItemID { get; set; }

        public string MeasurementType { get; set; }

        public List<List<TagItemValue>> Values { get; set; }

        public LastMeasurement()
        {
            Values = new List<List<TagItemValue>>();
        }
    }
}
