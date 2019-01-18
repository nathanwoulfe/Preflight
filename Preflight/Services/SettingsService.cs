using System;
using Newtonsoft.Json;
using Preflight.Constants;
using Preflight.Models;
using Preflight.Plugins;
using Preflight.Services.Interfaces;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Caching;
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
            MemoryCache cache = MemoryCache.Default;
            object fromCache = cache.Get(KnownStrings.SettingsCacheKey);
            if (fromCache != null)
                return fromCache as PreflightSettings;

            // only get here when nothing is cached 
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
                if (!s.Alias.HasValue()) { 
                    s.Alias = s.Label.Camel();
                }

                tabs.Add(new SettingsTab(s.Tab));
            }

            // get any plugins and add their settings
            // once settings have been saved from the backoffice, need to check that plugins aren't added twice
            var pluginProvider = new PluginProvider();

            foreach (IPreflightPlugin plugin in pluginProvider.Get())
            {
                foreach (SettingsModel setting in plugin.Settings)
                {
                    setting.Tab = plugin.Name;
                    settings.Add(setting);
                }

                // generate a tab from the setting - this list is filtered later
                // send back the summary and description for the plugin as part of the tab object for display in the settings view
                var pluginTab = new SettingsTab(plugin.Name)
                {
                    Summary = plugin.Summary,
                    Description = plugin.Description
                };

                tabs.Add(pluginTab);
            }

            // tabs are sorted alpha, with general first
            var response = new PreflightSettings
            {
                Settings = settings.DistinctBy(s => new { s.Tab, s.Label }).ToList(),
                Tabs = tabs.GroupBy(x => x.Alias)
                    .Select(y => y.First())
                    .OrderBy(i => i.Name != SettingsTabNames.General)
                    .ThenBy(i => i.Name).ToList()
            };

            // if we are here, cache should be set
            cache.Set(KnownStrings.SettingsCacheKey, response, DateTimeOffset.UtcNow.AddMinutes(120));

            return response;
        }

        /// <summary>
        /// Save the Preflight settings to the JSON file in app_plugins and update cache
        /// </summary>
        public bool Save(PreflightSettings settings)
        {
            try
            {
                MemoryCache cache = MemoryCache.Default;
                cache.Set(KnownStrings.SettingsCacheKey, settings, DateTimeOffset.UtcNow.AddMinutes(120));

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
