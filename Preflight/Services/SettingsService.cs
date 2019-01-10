using Newtonsoft.Json;
using Preflight.Constants;
using Preflight.Models;
using Preflight.Plugins;
using Preflight.Services.Interfaces;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Preflight.Extensions;

namespace Preflight.Services
{
    public class SettingsService : ISettingsService
    {

        /// <summary>
        /// Load the Preflight settings from the JSON file in app_plugins
        /// </summary>
        public PreflightSettings Get()
        {
            List<SettingsModel> settings;

            // json initially stores the core checks only
            // once it has been saved in the backoffice, settings store all current plugins, with alias
            using (var file = new StreamReader(KnownStrings.SettingsFilePath))
            {
                string json = file.ReadToEnd();
                settings = JsonConvert.DeserializeObject<List<SettingsModel>>(json);
            }

            // add tabs for core items
            List<SettingsTab> tabs = new List<SettingsTab>();
            foreach (SettingsModel s in settings)
            {
                if (s.Core || !s.Alias.HasValue())
                {
                    s.Alias = s.Label.Camel();
                }

                tabs.Add(new SettingsTab(s.Tab));
            }

            // get any plugins and add their settings
            // once settings have been saved from the backoffice, need to check that plugins aren't added twice
            foreach (PreflightPlugin plugin in PluginsHelper.GetPlugins())
            {
                foreach (SettingsModel setting in plugin.Settings)
                {
                    setting.Alias = setting.Label.Camel();

                    if (settings.Any(s => s.Alias == setting.Alias)) continue;

                    setting.Tab = plugin.Name;
                    settings.Add(setting);
                }

                string disabledAlias = plugin.Name.DisabledAlias();
                if (settings.All(s => s.Alias != disabledAlias))
                {
                    settings.Add(new SettingsModel
                    {
                        Label = "Disabled",
                        Alias = disabledAlias,
                        View = KnownPropertyEditors.Boolean,
                        Value = 0,
                        Order = 0,
                        Tab = plugin.Name
                    });
                }

                // generate a tab from the setting - this list is filtered later
                tabs.Add(new SettingsTab(plugin.Name));
            }

            // tabs are sorted alpha, with general first
            var response = new PreflightSettings
            {
                Settings = settings,
                Tabs = tabs.GroupBy(x => x.Alias)
                    .Select(y => y.First())
                    .OrderBy(i => i.Name != SettingsTabNames.General)
                    .ThenBy(i => i.Name).ToList()
            };

            return response;
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
