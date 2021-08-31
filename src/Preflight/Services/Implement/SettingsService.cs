using Preflight.Extensions;
using Preflight.Models;
using Preflight.Plugins;
using System;
using System.Collections.Generic;
using System.Linq;
using Preflight.Settings;
#if NETCOREAPP
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Services;
using Umbraco.Extensions;
using CharArrays = Umbraco.Cms.Core.Constants.CharArrays;
#else
using Preflight.Logging;
using Umbraco.Core;
using Umbraco.Core.Scoping;
using Umbraco.Core.Services;
using CharArrays = Umbraco.Core.Constants.CharArrays;
#endif

namespace Preflight.Services.Implement
{
    public class SettingsService : ISettingsService
    {
        private readonly ILogger<SettingsService> _logger;
        private readonly IUserService _userService;
        private readonly ICacheManager _cacheManager;
        private readonly ILocalizationService _localizationService;
        private readonly IScopeProvider _scopeProvider;
        private readonly PreflightPluginCollection _plugins;

        private const string _settingsCacheKey = "Preflight_Settings_";

        /// <summary>
        /// 
        /// </summary>
        /// <param name="logger"></param>
        public SettingsService(
            ILogger<SettingsService> logger,
            IUserService userService,
            PreflightPluginCollection plugins,
            ICacheManager cacheManager,
            ILocalizationService localizationService,
            IScopeProvider scopeProvider)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _userService = userService ?? throw new ArgumentNullException(nameof(userService));
            _plugins = plugins ?? throw new ArgumentNullException(nameof(plugins));
            _cacheManager = cacheManager ?? throw new ArgumentNullException(nameof(cacheManager));
            _localizationService = localizationService ?? throw new ArgumentNullException(nameof(localizationService));
            _scopeProvider = scopeProvider ?? throw new ArgumentNullException(nameof(scopeProvider));
        }

        /// <summary>
        /// Load the Preflight settings from the JSON file in app_plugins
        /// </summary>
        public PreflightSettings Get()
        {
            if (_cacheManager.TryGet(_settingsCacheKey, out PreflightSettings fromCache))
                return fromCache;

            PreflightSettings settings = GetSettings();

            if (settings != null)
            {
                _cacheManager.Set(_settingsCacheKey, settings);
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
                var fromCache = Get();

                _cacheManager.Set(_settingsCacheKey, settings);

                using (var scope = _scopeProvider.CreateScope())
                {
                    foreach (var setting in settings.Settings)
                    {
                        // only update modified properties
                        var settingFromCache = fromCache.Settings.First(x => x.Guid == setting.Guid);
                        if (setting.Equals(settingFromCache)) continue;

                        scope.Database.Update(new SettingDto(setting));
                    }
                    scope.Complete();
                }
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Could not save Preflight settings: {Message}", ex.Message);
                return false;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private PreflightSettings GetSettings()
        {
            var allLanguages = _localizationService.GetAllLanguages().Select(l => l.IsoCode);

            List<SettingDto> schema = new List<SettingDto>();
            using (var scope = _scopeProvider.CreateScope())
            {
                schema = scope.Database.Fetch<SettingDto>();
                scope.Complete();
            }

            List<SettingsModel> settings = CollateCoreAndPluginSettings();

            MergeSchemaValuesIntoSettings(allLanguages, schema, settings);

            EnsureGroupsAreStillValid(settings);

            var tabs = GenerateTabsFromSettings(settings);

            FinaliseSettings(tabs, settings);

            // tabs are sorted alpha, with general first
            return new PreflightSettings
            {
                Settings = settings
                    .DistinctBy(s => (s.Tab, s.Label))
                    .ToList(),
                Tabs = tabs
                    .GroupBy(x => x.Name)
                    .Select(y => y.First())
                    .OrderBy(i => i.Name != SettingsTabNames.General)
                    .ThenBy(i => i.Name).ToList()
            };
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private List<SettingsModel> CollateCoreAndPluginSettings()
        {
            var settings = new CoreSettings().Settings
                .Concat(new NaughtyNiceSettings().Settings).ToList();

            var pluginSettings = _plugins.Where(p => p.Settings.Any()).SelectMany(p => p.Settings);
            settings.AddRange(pluginSettings);

            return settings;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="tabs"></param>
        /// <param name="settings"></param>
        private static void FinaliseSettings(List<SettingsTab> tabs, List<SettingsModel> settings)
        {
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

                if (tabs.Any(x => x.Name == s.Tab)) continue;

                tabs.Add(new SettingsTab
                {
                    Name = s.Tab
                });
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="tabs"></param>
        /// <param name="settings"></param>
        private List<SettingsTab> GenerateTabsFromSettings(List<SettingsModel> settings)
        {
            List<SettingsTab> tabs = new List<SettingsTab>();

            foreach (IPreflightPlugin plugin in _plugins)
            {
                CheckPluginSettingExistsInSettingsCollection(settings, plugin);

                // generate a tab from the plugin if not added already
                // send back the summary and description for the plugin as part of the tab object for display in the settings view
                tabs.Add(new SettingsTab
                {
                    Name = plugin.Name,
                    Description = plugin.Description,
                    Summary = plugin.Summary
                });
            }

            return tabs;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="settings"></param>
        /// <param name="plugin"></param>
        private static void CheckPluginSettingExistsInSettingsCollection(List<SettingsModel> settings, IPreflightPlugin plugin)
        {
            foreach (SettingsModel setting in plugin.Settings)
            {
                if (settings.Any(x => x.Alias == setting.Alias)) continue;

                setting.Tab = plugin.Name;
                settings.Add(setting);
            }
        }

        /// <summary>
        /// populate prevalues for the groups setting
        /// intersect ensures any removed groups aren't kept as settings values
        /// since group name and alias can differ, needs to store both in the prevalue, and manage rebuilding this on the client side
        /// </summary>
        /// <param name="settings"></param>
        private void EnsureGroupsAreStillValid(List<SettingsModel> settings)
        {
            var allGroups = _userService.GetAllUserGroups();
            var settingGuid = Guid.Parse(KnownSettings.UserGroupOptIn);
            var groupSetting = settings.FirstOrDefault(x => x.Guid == settingGuid);

            if (groupSetting == null) return;

            var groupNames = allGroups.Select(x => new { value = x.Name, key = x.Alias });
            groupSetting.Prevalues = groupNames;

            if (groupSetting.Value == null) return;

            var newValue = new Dictionary<string, string>();
            foreach (var variantValue in groupSetting.Value.ToVariantDictionary())
            {
                var groupSettingValue = variantValue.Value.Split(CharArrays.Comma).Intersect(groupNames.Select(x => x.value));
                newValue.Add(variantValue.Key, string.Join(KnownStrings.Comma, groupSettingValue));
            }

            groupSetting.Value = newValue;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="allLanguages"></param>
        /// <param name="schema"></param>
        /// <param name="settings"></param>
        private static void MergeSchemaValuesIntoSettings(IEnumerable<string> allLanguages, List<SettingDto> schema, List<SettingsModel> settings)
        {
            foreach (SettingsModel setting in settings)
            {
                var schemaItem = schema.FirstOrDefault(s => s.Setting == setting.Guid);
                var value = schemaItem.Value;

                // if no value came from the db, assume it's a new plugin and use the default
                var valueDictionary = value.HasValue() ?
                    value.ToVariantDictionary() :
                    allLanguages.ToDictionary(k => k, v => setting.Value.ToString());

                // have new languages been added? better check...
                // if any are missing, add the default value
                AddMissingLanguagessToValueDictionary(allLanguages, setting, valueDictionary);

                setting.Value = valueDictionary;
                setting.Id = schemaItem.Id;
            }
        }

        /// <summary>
        /// Adds languages to the value dictionary to ensure all languages have an entry
        /// Value is set to the setting default value
        /// </summary>
        /// <param name="allLanguages"></param>
        /// <param name="setting"></param>
        /// <param name="valueDictionary"></param>
        private static void AddMissingLanguagessToValueDictionary(IEnumerable<string> allLanguages, SettingsModel setting, Dictionary<string, string> valueDictionary)
        {
            if (valueDictionary.Count == allLanguages.Count()) return;

            var missingLangs = allLanguages.Except(valueDictionary.Keys);
            foreach (var lang in missingLangs)
            {
                valueDictionary[lang] = setting.Value.ToString();
            }
        }
    }
}
