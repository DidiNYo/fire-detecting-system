using System.Collections.Generic;

namespace ExternalServices.Models
{
    public class OrganizationItem
    {
        public int OrgItemId { get; set; }

        public string Name { get; set; }

        public int TypeId { get; set; }

        public List<Property> Properties { get; set; }

        public List<TagInfo> Tags { get; set; }
    }
}
