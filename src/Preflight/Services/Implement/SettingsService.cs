using Newtonsoft.Json;
using Preflight.Constants;
using Preflight.Extensions;
using Preflight.Models;
using Preflight.Plugins;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Preflight.IO;
#if NET472
using Preflight.Logging;
using Umbraco.Core;
using Umbraco.Core.Services;
#else
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.Services;
using Umbraco.Extensions;
#endif

namespace Preflight.Services.Implement
{
    public class SettingsService : ISettingsService
    {
        private readonly ILogger<SettingsService> _logger;
        private readonly IUserService _userService;
        private readonly IIOHelper _ioHelper;
        private readonly ICacheManager _cacheManager;
        private readonly ILocalizationService _localizationService;
        private readonly PreflightPluginCollection _plugins;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="logger"></param>
        public SettingsService(
            ILogger<SettingsService> logger, 
            IUserService userService, 
            PreflightPluginCollection plugins, 
            IIOHelper ioHelper, 
            ICacheManager cacheManager, 
            ILocalizationService localizationService)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _userService = userService ?? throw new ArgumentNullException(nameof(userService));
            _plugins = plugins ?? throw new ArgumentNullException(nameof(plugins));
            _ioHelper = ioHelper ?? throw new ArgumentNullException(nameof(ioHelper));
            _cacheManager = cacheManager ?? throw new ArgumentNullException(nameof(cacheManager));
            _localizationService = localizationService ?? throw new ArgumentNullException(nameof(localizationService));
        }

        /// <summary>
        /// Load the Preflight settings from the JSON file in app_plugins
        /// </summary>
        public PreflightSettings Get()
        {
            //if (_cacheManager.TryGet(KnownStrings.SettingsCacheKey, out PreflightSettings fromCache))
            //    return fromCache;

            PreflightSettings settings = GetSettings();

            if (settings != null)
            {
                _cacheManager.Set(KnownStrings.SettingsCacheKey, settings);
                return settings;
            }

            var ex = new NullReferenceException("Could not get Preflight settings");
            _logger.LogError(ex, ex.Message);

            return null;
        }

        /// <summary>
        /// Save the Preflight settings to the JSON file in app_plugins and update cache
        /// </summary>
        public bool Save(PreflightSettings settings)
        {
            try
            {                
                _cacheManager.Set(KnownStrings.SettingsCacheKey, settings);

                // only persist the settings, tabs can be regenerated on startup
                using (var file = new StreamWriter(_ioHelper.MapPath(KnownStrings.SettingsFilePath), false))
                {
                    var serializer = new JsonSerializer();
                    serializer.Serialize(file, settings.Settings);
                }
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Could not save Preflight settings: {Message}", ex.Message);
                return false;
            }
        }

        private PreflightSettings GetSettings()
        {
            // not all settings are variant, so use the default culture
            var defaultCulture = _localizationService.GetDefaultLanguageIsoCode();
            var allLanguages = _localizationService.GetAllLanguages().Select(l => l.IsoCode);

            // only get here when nothing is cached 
            List<SettingsModel> settings = new List<SettingsModel>();
            List<SettingsTab> tabs = new List<SettingsTab>();

            // json initially stores the core checks only
            // once it has been saved in the backoffice, settings store all current plugins, with alias
            using (var file = new StreamReader(_ioHelper.MapPath(KnownStrings.SettingsFilePath)))
            {
                string json = file.ReadToEnd();
                settings = JsonConvert.DeserializeObject<List<SettingsModel>>(json);
            }


            // populate prevalues for the groups setting
            // intersect ensures any removed groups aren't kept as settings values
            // since group name and alias can differ, needs to store both in the prevalue, and manage rebuilding this on the client side
            var allGroups = _userService.GetAllUserGroups();
            var groupSetting = settings.FirstOrDefault(x => string.Equals(x.Label, KnownSettings.UserGroupOptIn, StringComparison.InvariantCultureIgnoreCase));

            if (groupSetting != null)
            {
                var groupNames = allGroups.Select(x => new { value = x.Name, key = x.Alias });
                groupSetting.Prevalues = groupNames;

                // here the value is a string and needs to be manually updated to a dictionary

                var value = allLanguages.ToDictionary(k => k, v => groupSetting.Value?.ToString());
                if (value.Any())
                {
                    var newValue = new Dictionary<string, string>();
                    foreach (var variantValue in value)
                    {
                        var groupSettingValue = variantValue.Value.Split(',').Intersect(groupNames.Select(x => x.value));
                        newValue.Add(variantValue.Key, string.Join(",", groupSettingValue));
                    }

                    groupSetting.Value = newValue;
                }
            }

            // populate prevalues for subsetting testable properties, default value to all if nothing exists
            var testablePropertiesProp = settings.FirstOrDefault(x => string.Equals(x.Label, KnownSettings.PropertiesToTest, StringComparison.InvariantCultureIgnoreCase));

            if (testablePropertiesProp != null)
            {
                testablePropertiesProp.Prevalues = KnownPropertyAlias.All.Select(x => new { value = x, key = x });

                var value = allLanguages.ToDictionary(k => k, v => testablePropertiesProp.Value?.ToString());
                if (!value.Any())
                {
                    var newValue = new Dictionary<string, string>();
                    foreach (var variantValue in value)
                    {
                        newValue.Add(variantValue.Key, string.Join(",", KnownPropertyAlias.All));
                    }

                    testablePropertiesProp.Value = newValue;
                }
            }

            // adds all discovered plugins
            foreach (IPreflightPlugin plugin in _plugins)
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

                if (!s.View.Contains(".html"))
                {
                    s.View = "views/propertyeditors/" + s.View + "/" + s.View + ".html";
                }

                if (!tabs.Any(x => x.Name == s.Tab))
                {
                    tabs.Add(new SettingsTab
                    {
                        Name = s.Tab
                    });
                }

                // if the values are already variant, do nothing
                // otherwise create a dictionary for all languages, with the default value
                try
                {
                    var _ = s.Value.ToVariantDictionary();
                }
                catch
                {
                    s.Value = allLanguages.ToDictionary(key => key, value => s.Value?.ToString() ?? "");
                }
            }

            // tabs are sorted alpha, with general first
            return new PreflightSettings
            {
                Settings = settings.DistinctBy(s => (s.Tab, s.Label)).ToList(),
                Tabs = tabs.GroupBy(x => x.Name)
                    .Select(y => y.First())
                    .OrderBy(i => i.Name != SettingsTabNames.General)
                    .ThenBy(i => i.Name).ToList()
            };
        }      
    }
}
