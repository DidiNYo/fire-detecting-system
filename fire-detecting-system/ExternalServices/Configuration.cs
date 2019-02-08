using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExternalServices
{
    public class Configuration
    {
        public string Username { get; set; }

        public string Password { get; set; }

        public string GraphQLClient { get; set; }

        public string IdentityServer { get; set; }

        public int SecondsToRefresh { get; set; }
    }
}
