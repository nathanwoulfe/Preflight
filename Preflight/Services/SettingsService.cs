using Newtonsoft.Json;
using Preflight.Constants;
using Preflight.Extensions;
using Preflight.Models;
using Preflight.Plugins;
using Preflight.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Umbraco.Core;
using Umbraco.Core.Cache;
using Umbraco.Web.Composing;

namespace Preflight.Services
{
    public class SettingsService : ISettingsService
    {

        /// <summary>
        /// Load the Preflight settings from the JSON file in app_plugins
        /// </summary>
        public PreflightSettings Get()
        {
            var fromCache = Current.AppCaches.RuntimeCache.GetCacheItem<PreflightSettings>(KnownStrings.SettingsCacheKey);
            if (fromCache != null)
            {
                var x = 1;
            }
            //return fromCache;

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

            // get any plugins and add their settings
            // once settings have been saved from the backoffice, need to check that plugins aren't added twice
            var pluginProvider = new PluginProvider();
            var plugins = pluginProvider.Get();

            foreach (IPreflightPlugin plugin in plugins)
            {
                foreach (SettingsModel setting in plugin.Settings)
                {
                    if (!settings.Any(x => x.Alias == setting.Alias))
                    {
                        setting.Tab = plugin.Name;
                        settings.Add(setting);
                    }
                }

                // generate a tab from the plugin if not added already
                // send back the summary and description for the plugin as part of the tab object for display in the settings view
                tabs.Add(new SettingsTab
                {
                    Name = plugin.Name,
                    Description = plugin.Description,
                    Summary = plugin.Summary
                });
            }

            foreach (SettingsModel s in settings)
            {
                if (!s.Alias.HasValue())
                {
                    s.Alias = s.Label.Camel();
                }

                if (!tabs.Any(x => x.Name == s.Tab))
                {
                    tabs.Add(new SettingsTab
                    {
                        Name = s.Tab
                    });
                }
            }

            // tabs are sorted alpha, with general first
            var response = new PreflightSettings
            {
                Settings = settings.DistinctBy(s => (s.Tab, s.Label)).ToList(),
                Tabs = tabs.GroupBy(x => x.Name)
                    .Select(y => y.First())
                    .OrderBy(i => i.Name != SettingsTabNames.General)
                    .ThenBy(i => i.Name).ToList()
            };

            // if we are here, cache should be set
            Current.AppCaches.RuntimeCache.InsertCacheItem(KnownStrings.SettingsCacheKey, () => response, new TimeSpan(24, 0, 0), false);

            return response;
        }

        /// <summary>
        /// Save the Preflight settings to the JSON file in app_plugins and update cache
        /// </summary>
        public bool Save(PreflightSettings settings)
        {
            try
            {
                Current.AppCaches.RuntimeCache.InsertCacheItem(KnownStrings.SettingsCacheKey, () => settings, new TimeSpan(24, 0, 0), false);

                // only persist the settings, tabs can be regenerated on startup
                using (var file = new StreamWriter(KnownStrings.SettingsFilePath, false))
                {
                    var serializer = new JsonSerializer();
                    serializer.Serialize(file, settings.Settings);
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
