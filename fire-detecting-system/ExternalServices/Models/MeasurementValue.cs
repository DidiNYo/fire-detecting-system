using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExternalServices.Models
{
    public class MeasurementValue
    {
        public string Name { get; set; }

        public List<TagItemValue> TagItemValues { get; set; }

        public MeasurementValue(string name, List<TagItemValue> tagItemValues)
        {
            Name = name;
            TagItemValues = tagItemValues;
        }
    }
}
