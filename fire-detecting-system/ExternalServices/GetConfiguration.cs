using Newtonsoft.Json;
using System;
using System.IO;

namespace ExternalServices
{
    public sealed class GetConfiguration
    {
        private static Configuration configurationData;

        public Configuration ConfigurationData { get => configurationData; }
        
        static GetConfiguration()
        {
            try
            {
                string fileResult = File.ReadAllText("Configuration.json");
                try
                {
                    configurationData = JsonConvert.DeserializeObject<Configuration>(fileResult);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    throw;
                }
            }
            catch (FileNotFoundException e)
            {
                Console.WriteLine(e.Message);
                throw;
            }
        }

        private GetConfiguration()
        {

        }

        public static GetConfiguration ConfigurationInstance { get; } = new GetConfiguration();
    }
}
