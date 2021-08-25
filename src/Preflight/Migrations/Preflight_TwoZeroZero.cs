using Preflight.Constants;
using Preflight.Migrations.Schema;
using Preflight.Services;
#if NET472
using Preflight.Logging;
using Umbraco.Core.Migrations;
using Umbraco.Core.Services;
#else
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.Migrations;
#endif

namespace Preflight.Migrations
{
    public class Preflight_TwoZeroZero : MigrationBase
    {
        private readonly ILogger<Preflight_TwoZeroZero> _logger;
        private readonly ISettingsService _settingsService;
        private readonly ILocalizationService _localizationService;

        public Preflight_TwoZeroZero(IMigrationContext context, ILogger<Preflight_TwoZeroZero> logger, ISettingsService settingsService, ILocalizationService localizationService)
            : base(context)
        {
            _logger = logger;
            _settingsService = settingsService;
            _localizationService = localizationService;
        }

#if NET472
        public override void Migrate()
#else
        protected override void Migrate()
#endif
        {
            _logger.LogDebug("Creating Preflight settings table");

            if (!TableExists(KnownStrings.SettingsTable))
            {
                Create.Table<SettingsSchema>().Do();
            }

            _logger.LogDebug("Populate from existing settings.json");

            var settings = _settingsService.Get();
            var defaultCulture = _localizationService.GetDefaultLanguageIsoCode();

            //foreach (var setting in settings.Settings)
            //{
            //    setting.Culture = defaultCulture;
            //}

            Update.Table(KnownStrings.SettingsTable).Set(settings.Settings);            
        }
    }
}