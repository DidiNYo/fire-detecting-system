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

        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();

            foreach (var list in Values)
            {
                foreach (var tagItemValue in list)
                {
                    builder.Append("Date: ");
                    builder.Append(tagItemValue.Date);
                    builder.Append(" Value: ");
                    builder.Append(tagItemValue.Value);
                    builder.Append("\n");
                }
            }
            

            return string.Format("Name: {0}\nId: {1}\nType of measurement: {2}\n{3}", OrganizationItemName, OrganizationItemID, MeasurementType, builder.ToString());
        }
    }
}
