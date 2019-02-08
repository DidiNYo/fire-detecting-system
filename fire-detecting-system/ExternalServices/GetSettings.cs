using Newtonsoft.Json;
using System;
using System.IO;

namespace ExternalServices
{
    public sealed class GetSettings
    {
        private static Settings settingsData;

        public Settings SettingsData { get => settingsData; }

        static GetSettings()
        {
            try
            {
                string fileResult = File.ReadAllText("Settings.json");
                try
                {
                    settingsData = JsonConvert.DeserializeObject<Settings>(fileResult);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    throw;
                }
            }
            catch (FileNotFoundException e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        private GetSettings()
        {

        }

        public static GetSettings GetSettingsInstance { get; } = new GetSettings();
    }
}
