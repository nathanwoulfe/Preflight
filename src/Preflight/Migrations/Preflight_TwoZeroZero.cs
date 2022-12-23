using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Preflight.Extensions;
using Preflight.Migrations.Schema;
using Preflight.Models;
using Preflight.Plugins;
using Preflight.Settings;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.Migrations;
using Umbraco.Cms.Infrastructure.Scoping;

namespace Preflight.Migrations;

public class Preflight_TwoZeroZero : MigrationBase
{
    private const string SettingsFilePath = "~/App_Plugins/Preflight/Backoffice/settings.json";

    private readonly ILogger<Preflight_TwoZeroZero> _logger;
    private readonly ILocalizationService _localizationService;
    private readonly IIOHelper _ioHelper;
    private readonly IScopeProvider _scopeProvider;
    private readonly PreflightPluginCollection _plugins;

    public Preflight_TwoZeroZero(
        IMigrationContext context,
        ILogger<Preflight_TwoZeroZero> logger,
        ILocalizationService localizationService,
        IIOHelper ioHelper,
        PreflightPluginCollection plugins,
        IScopeProvider scopeProvider)
        : base(context)
    {
        _logger = logger;
        _localizationService = localizationService;
        _ioHelper = ioHelper;
        _plugins = plugins;
        _scopeProvider = scopeProvider;
    }

    protected override void Migrate()
    {
        _logger.LogDebug("Creating Preflight settings table");

        if (!TableExists(KnownStrings.SettingsTable))
        {
            Create.Table<SettingsSchema>().Do();
        }

        _logger.LogDebug("Populate from existing settings.json");

        // get all settings, update the value to a variant dictionary,
        // then save the value and guid for each
        List<SettingsModel> settings = new();

        if (File.Exists(_ioHelper.MapPath(SettingsFilePath)))
        {
            using var file = new StreamReader(_ioHelper.MapPath(SettingsFilePath));
            string json = file.ReadToEnd();
            settings = JsonConvert.DeserializeObject<List<SettingsModel>>(json) ?? new();
        }

        // update settings values to variant dictionaries
        IEnumerable<string> allLanguages = _localizationService.GetAllLanguages().Select(l => l.IsoCode);
        foreach (SettingsModel setting in settings)
        {
            // if not a dictionary, make it one
            try
            {
                _ = setting.Value?.ToVariantDictionary();
            }
            catch
            {
                setting.Value = allLanguages.ToDictionary(key => key, value => setting.Value);
            }
        }

        // mash all known settings into a common object
        // these provide the guid value
        var allSettings = new CoreSettings().Settings
            .Concat(new NaughtyNiceSettings().Settings).ToList();

        IEnumerable<SettingsModel> pluginSettings = _plugins.Where(p => p.Settings.Any()).SelectMany(p => p.Settings);
        allSettings.AddRange(pluginSettings);

        // generate the list of saveable settings
        var settingsToSave = new List<SettingsSchema>();

        // new installs will have no settings, but need the defaults stored
        if (settings.Any())
        {
            foreach (SettingsModel setting in settings)
            {
                SettingsModel? settingObject = allSettings.FirstOrDefault(x => x.Alias == setting.Alias);

                if (settingObject is null)
                {
                    continue;
                }

                var schema = new SettingsSchema
                {
                    Value = setting.Value?.ToString() ?? string.Empty,
                    Setting = settingObject.Guid,
                };

                settingsToSave.Add(schema);
            }
        }
        else
        {
            foreach (SettingsModel? setting in allSettings)
            {
                var schema = new SettingsSchema
                {
                    Value = JsonConvert.SerializeObject(allLanguages.ToDictionary(key => key, value => setting.Value)),
                    Setting = setting.Guid,
                };

                settingsToSave.Add(schema);
            }
        }

        // finally, persist just the guid and value for each setting
        using IScope scope = _scopeProvider.CreateScope();
        foreach (SettingsSchema setting in settingsToSave)
        {
            _ = scope.Database.Insert(setting);
        }

        _ = scope.Complete();
    }
}
