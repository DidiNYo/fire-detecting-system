using System.Collections.Generic;

namespace ExternalServices.Models
{
    public class Organization
    {
        public string Name { get; set; }

        public List<OrganizationItem> Items { get; set; }
    }
}
