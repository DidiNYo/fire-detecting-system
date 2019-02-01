using Newtonsoft.Json;
using System.IO;

namespace ExternalServices
{
    public sealed class GetConfiguration
    {
        public Configuration ConfigurationData { get; } = JsonConvert.DeserializeObject<Configuration>(File.ReadAllText("Configuration.json"));

        static GetConfiguration()
        {

        }

        private GetConfiguration()
        {

        }

        public static GetConfiguration ConfigurationInstance { get; } = new GetConfiguration();
    }
}
