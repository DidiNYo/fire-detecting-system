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

        public List<KeyValuePair<string, List<TagItemValue>>> Values { get; set; }

        public LastMeasurement()
        {
            Values = new List<KeyValuePair<string, List<TagItemValue>>>();
        }

        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();
            int i = 0;
            builder.Append("\n");
            foreach (var list in Values)
            {
                foreach (var tagItemValue in list.Value)
                {
                    builder.Append(tagItemValue.Date);
                    builder.Append(" ");
                    builder.Append(list.Key);
                    builder.Append(": ");
                    builder.Append(tagItemValue.Value);
                    builder.Append("\n");
                }
            }

            return string.Format("Name: {0} (Id: {1})\n{2}", OrganizationItemName, OrganizationItemID, builder.ToString());
        }
    }
}
