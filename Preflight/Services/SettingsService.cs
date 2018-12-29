using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using Preflight.Constants;
using Preflight.Models;
using Preflight.Services.Interfaces;

namespace Preflight.Services
{
    public class SettingsService : ISettingsService
    {
        /// <summary>
        /// Load the Preflight settings from the JSON file in app_plugins
        /// </summary>
        public List<SettingsModel> Get()
        {
            List<SettingsModel> settings;

            using (var file = new StreamReader(KnownStrings.SettingsFilePath))
            {
                string json = file.ReadToEnd();
                settings = JsonConvert.DeserializeObject<List<SettingsModel>>(json);
            }

            return settings;
        }

        /// <summary>
        /// Save the Preflight settings to the JSON file in app_plugins
        /// </summary>
        public bool Save(List<SettingsModel> settings)
        {
            try
            {
                using (var file = new StreamWriter(KnownStrings.SettingsFilePath, false))
                {
                    var serializer = new JsonSerializer();
                    serializer.Serialize(file, settings);
                }
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
