using Preflight.Migrations.Schema;
using System.IO;
using Preflight.Models;
using Newtonsoft.Json;
using System.Collections.Generic;
using Preflight.IO;
using Preflight.Settings;
using Preflight.Plugins;
using System.Linq;
using Preflight.Extensions;
#if NETCOREAPP
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.Migrations;
#else
using Preflight.Logging;
using Umbraco.Core.Migrations;
using Umbraco.Core.Scoping;
using Umbraco.Core.Services;
#endif

namespace Preflight.Migrations
{
    public class Preflight_TwoZeroZero : MigrationBase
    {
        private const string _settingsFilePath = "~/App_Plugins/Preflight/Backoffice/settings.json";

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

#if NETCOREAPP
        protected override void Migrate()
#else
        public override void Migrate()
#endif
        {
            _logger.LogDebug("Creating Preflight settings table");

            if (!TableExists(KnownStrings.SettingsTable))
            {
                Create.Table<SettingsSchema>().Do();
            }

            _logger.LogDebug("Populate from existing settings.json");

            // get all settings, update the value to a variant dictionary,
            // then save the value and guid for each
            var settings = new List<SettingsModel>();

            if (File.Exists(_ioHelper.MapPath(_settingsFilePath)))
            {
                using (var file = new StreamReader(_ioHelper.MapPath(_settingsFilePath)))
                {
                    string json = file.ReadToEnd();
                    settings = JsonConvert.DeserializeObject<List<SettingsModel>>(json);
                }
            }

            // update settings values to variant dictionaries
            var allLanguages = _localizationService.GetAllLanguages().Select(l => l.IsoCode);
            foreach (var setting in settings)
            {
                // if not a dictionary, make it one                
                try
                {
                    var _ = setting.Value.ToVariantDictionary();
                } catch { 
                    setting.Value = allLanguages.ToDictionary(key => key, value => setting.Value);
                }
            }

            // mash all known settings into a common object
            // these provide the guid value
            var allSettings = new CoreSettings().Settings
                .Concat(new NaughtyNiceSettings().Settings).ToList();

            var pluginSettings = _plugins.Where(p => p.Settings.Any()).SelectMany(p => p.Settings);
            allSettings.AddRange(pluginSettings);

            // generate the list of saveable settings
            var settingsToSave = new List<SettingsSchema>();

            // new installs will have no settings, but need the defaults stored
            if (settings.Any())
            {
                foreach (var setting in settings)
                {
                    var schema = new SettingsSchema
                    {
                        Value = setting.Value.ToString(),
                        Setting = allSettings.FirstOrDefault(x => x.Alias == setting.Alias).Guid,
                    };

                    settingsToSave.Add(schema);
                }
            } else
            {
                foreach (var setting in allSettings)
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
            using (var scope = _scopeProvider.CreateScope())
            {
                foreach (var setting in settingsToSave)
                {
                    scope.Database.Insert(setting);
                }
                scope.Complete();
            }
        }
    }
}