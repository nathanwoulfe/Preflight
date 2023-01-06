using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Preflight.Extensions;
using Preflight.Models;
using Preflight.Models.Dtos;
using Preflight.Models.Settings;
using Preflight.Plugins;
using Preflight.Settings;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.Scoping;
using Umbraco.Extensions;
using CharArrays = Umbraco.Cms.Core.Constants.CharArrays;

namespace Preflight.Services.Implement;

public class SettingsService : ISettingsService
{
    private readonly ILogger<SettingsService> _logger;
    private readonly IUserService _userService;
    private readonly ICacheManager _cacheManager;
    private readonly ILocalizationService _localizationService;
    private readonly ILocalizedTextService _localizedTextService;
    private readonly IScopeProvider _scopeProvider;
    private readonly PreflightPluginCollection _plugins;

    private const string SettingsCacheKey = "Preflight_Settings_";

    /// <summary>
    /// 
    /// </summary>
    /// <param name="logger"></param>
    /// <param name="userService"></param>
    /// <param name="plugins"></param>
    /// <param name="cacheManager"></param>
    /// <param name="localizationService"></param>
    /// <param name="scopeProvider"></param>
    /// <param name="localizedTextService"></param>
    public SettingsService(
        ILogger<SettingsService> logger,
        IUserService userService,
        PreflightPluginCollection plugins,
        ICacheManager cacheManager,
        ILocalizationService localizationService,
        IScopeProvider scopeProvider,
        ILocalizedTextService localizedTextService)
    {
        _logger = logger;
        _userService = userService;
        _plugins = plugins;
        _cacheManager = cacheManager;
        _localizationService = localizationService;
        _scopeProvider = scopeProvider;
        _localizedTextService = localizedTextService;
    }

    /// <summary>
    /// </summary>
    public PreflightSettingsModel Get()
    {
        PreflightSettingsModel settings = /*_cacheManager.TryGet(SettingsCacheKey, out PreflightSettingsModel fromCache) ? fromCache :*/ GetSettings();

        if (settings is null)
        {
            return new();
        }

        _cacheManager.Set(SettingsCacheKey, settings);

        // can't cache this, since it would be cached when the user lang changes
        // which admittedly isn't likely, but is possible
        LocalizeSettings(settings);

        return settings;
    }

    /// <summary>
    /// Save the Preflight settings to the JSON file in app_plugins and update cache
    /// </summary>
    public bool Save(PreflightSettingsModel settings)
    {
        try
        {
            using IScope scope = _scopeProvider.CreateScope();
            foreach (SettingsModel setting in settings.Settings)
            {
                scope.Database.Save(new SettingDto(setting));
            }

            _ = scope.Complete();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Could not save Preflight settings: {Message}", ex.Message);
            return false;
        }

        _cacheManager.Set(SettingsCacheKey, settings);
        return true;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    private PreflightSettingsModel GetSettings()
    {
        List<SettingDto> schema = new();
        using (IScope scope = _scopeProvider.CreateScope())
        {
            schema = scope.Database.Fetch<SettingDto>();
            _ = scope.Complete();
        }

        List<SettingsModel> settings = CollateCoreAndPluginSettings();

        MergeSchemaValuesIntoSettings(schema, settings);

        EnsureGroupsAreStillValid(settings);

        List<SettingsTabModel> tabs = GenerateTabsFromSettings(settings);

        FinaliseSettings(tabs, settings);

        // tabs are sorted alpha, with general first
        return new PreflightSettingsModel
        {
            Settings = settings
                .DistinctBy(s => (s.Tab, s.Alias))
                .ToList(),
            Tabs = tabs
                .GroupBy(x => x.Name)
                .Select(y => y.First())
                .OrderBy(i => i.Name != SettingsTabNames.General)
                .ThenBy(i => i.Name).ToList(),
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

        IEnumerable<SettingsModel> pluginSettings = _plugins.Where(p => p.Settings.Any()).SelectMany(p => p.Settings);
        settings.AddRange(pluginSettings);

        return settings;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="tabs"></param>
    /// <param name="settings"></param>
    private static void FinaliseSettings(List<SettingsTabModel> tabs, List<SettingsModel> settings)
    {
        foreach (SettingsModel s in settings)
        {
            if (!s.View.Contains(".html"))
            {
                s.View = "views/propertyeditors/" + s.View + "/" + s.View + ".html";
            }

            if (tabs.Any(x => x.Name == s.Tab))
            {
                continue;
            }

            tabs.Add(new SettingsTabModel
            {
                Name = s.Tab,
                Alias = s.Tab.Camel(),
            });
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="settings"></param>
    private List<SettingsTabModel> GenerateTabsFromSettings(List<SettingsModel> settings)
    {
        List<SettingsTabModel> tabs = new();

        foreach (IPreflightPlugin plugin in _plugins)
        {
            CheckPluginSettingExistsInSettingsCollection(settings, plugin);

            // generate a tab from the plugin if not added already
            // send back the summary and description for the plugin as part of the tab object for display in the settings view
            tabs.Add(new SettingsTabModel
            {
                Name = plugin.Name,
                Alias = plugin.Name.Camel(),
                Description = plugin.Description,
                Summary = plugin.Summary,
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
            if (settings.Any(x => x.Alias == setting.Alias))
            {
                continue;
            }

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
        IEnumerable<IUserGroup> allGroups = _userService.GetAllUserGroups();
        var settingGuid = Guid.Parse(KnownSettings.UserGroupOptIn);
        SettingsModel? groupSetting = settings.FirstOrDefault(x => x.Guid == settingGuid);

        if (groupSetting is null)
        {
            return;
        }

        var groupNames = allGroups.Select(x => new { value = x.Name, key = x.Alias });
        groupSetting.Prevalues = groupNames;

        if (groupSetting.Value is null)
        {
            return;
        }

        CaseInsensitiveValueDictionary newValue = new();

        if (groupSetting.Value is null)
        {
            return;
        }

        foreach (KeyValuePair<string, object?> variantValue in groupSetting.Value)
        {
            string valueString = variantValue.Value?.ToString() ?? string.Empty;
            IEnumerable<string?> groupSettingValue = valueString.Split(CharArrays.Comma).Intersect(groupNames.Select(x => x.value));
            newValue.Add(variantValue.Key, string.Join(KnownStrings.Comma, groupSettingValue));
        }

        groupSetting.Value = newValue;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="schema"></param>
    /// <param name="settings"></param>
    private void MergeSchemaValuesIntoSettings(List<SettingDto> schema, List<SettingsModel> settings)
    {
        IEnumerable<string> allLanguages = _localizationService.GetAllLanguages().Select(l => l.IsoCode);

        foreach (SettingsModel setting in settings)
        {
            SettingDto? schemaItem = schema.FirstOrDefault(s => s.Setting == setting.Guid);

            if (schemaItem is null)
            {
                continue;
            }

            // if no value came from the db, assume it's a new plugin and use the default
            // dict is case-insensitive as culture casing seems to vary...
            var valueDictionary = (CaseInsensitiveValueDictionary?)(schemaItem.Value.HasValue() ?
                JsonConvert.DeserializeObject<CaseInsensitiveValueDictionary>(schemaItem.Value) :
                allLanguages.ToDictionary(k => k, v => setting.Value?[v]));

            // have new languages been added? better check...
            // if any are missing, add the default value
            AddMissingLanguagesToValueDictionary(allLanguages, setting, valueDictionary);

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
    private static void AddMissingLanguagesToValueDictionary(IEnumerable<string> allLanguages, SettingsModel setting, CaseInsensitiveValueDictionary? valueDictionary)
    {
        if (valueDictionary is null || valueDictionary.Count == allLanguages.Count())
        {
            return;
        }

        IEnumerable<string> missingLangs = allLanguages.Except(valueDictionary.Keys);
        foreach (string? lang in missingLangs)
        {
            valueDictionary[lang] = setting.Value?[lang];
        }
    }

    /// <summary>
    /// Updates settings name, summary, tab etc where a value exists
    /// </summary>
    /// <param name="settings"></param>
    private void LocalizeSettings(PreflightSettingsModel settings)
    {
        const string prefix = "preflight-";

        foreach (SettingsTabModel tab in settings.Tabs)
        {
            string area = prefix + tab.Alias;
            tab.Name = _localizedTextService.Localize(area, "tabName");
            tab.Summary = _localizedTextService.Localize(area, "summary");
        }

        foreach (SettingsModel setting in settings.Settings)
        {
            string area = prefix + setting.Tab.Camel();
            string alias = setting.Alias;

            // this is gross, would rather use a switch, but that won't work here
            // code is using the generic values for translating the default, preflight-added properties
            if (setting.Alias == setting.Tab.DisabledAlias())
            {
                area = KnownStrings.Alias;
                alias = "disabled";
            }
            else if (setting.Alias == setting.Tab.OnSaveOnlyAlias())
            {
                area = KnownStrings.Alias;
                alias = "onSaveOnly";
            }
            else if (setting.Alias == setting.Tab.PropertiesToTestAlias())
            {
                area = KnownStrings.Alias;
                alias = "propertiesToTest";
            }

            setting.Label = _localizedTextService.Localize(area, alias);
            setting.Description = _localizedTextService.Localize(area, alias + "Description");
        }
    }
}
