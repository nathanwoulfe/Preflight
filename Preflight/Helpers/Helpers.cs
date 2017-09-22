using Newtonsoft.Json;
using Preflight.Models;
using System.Collections.Generic;
using System.IO;
using System.Web;

namespace Preflight.Helpers
{
    public class SettingsHelper
    {
        private static readonly string SettingsFilePath = "~/App_Plugins/Preflight/settings.json";

        /// <summary>
        /// Load the Preflight settings from the JSON file in app_plugins
        /// </summary>
        public static List<SettingsModel> GetSettings()
        {
            var settings = new List<SettingsModel>();

            using (StreamReader file = new StreamReader(HttpContext.Current.Server.MapPath(SettingsFilePath)))
            {
                string json = file.ReadToEnd();
                settings = JsonConvert.DeserializeObject<List<SettingsModel>>(json);
            }

            return settings;
        }

        /// <summary>
        /// Save the Preflight settings to the JSON file in app_plugins
        /// </summary>
        public static bool SaveSettings(List<SettingsModel> settings)
        {
            try
            {
                using (StreamWriter file = new StreamWriter(HttpContext.Current.Server.MapPath(SettingsFilePath), false))
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
