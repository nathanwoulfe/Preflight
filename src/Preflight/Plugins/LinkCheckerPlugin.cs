using Preflight.Constants;
using Preflight.Extensions;
using Preflight.Models;
using Preflight.Services;
using System.Collections.Generic;
using System.Linq;

namespace Preflight.Plugins
{
    public class LinkCheckerPlugin : IPreflightCorePlugin
    {
        private readonly ILinksService _linksService;
        private readonly ISafeBrowsingService _safeBrowsingService;

        public object Result { get; set; }
        public IEnumerable<SettingsModel> Settings { get; set; }

        public bool Failed { get; set; }
        public bool Core => true;

        public int SortOrder => -1;
        public int FailedCount { get; set; }

        public int TotalTests => 2;

        public string Name => "Link health";
        public string ViewPath => "/App_Plugins/Preflight/Backoffice/plugins/linkhealth/linkhealth.html";

        public string Summary => "Check links resolve correctly. Optionally check URLs against Google's SafeBrowsing API";
        public string Description { get; set; }

        public LinkCheckerPlugin(ILinksService linksService, ISafeBrowsingService safeBrowsingService)
        {
            _linksService = linksService;
            _safeBrowsingService = safeBrowsingService;

            Settings = PluginSettingsList.Populate(Name,
                false,
                true,
                settings: new SettingsModel[] {
                    new GenericSettingModel("Ensure safe links")
                    {
                        Description = "Set to true and Preflight will check links for potential malware and bad actors.",
                        View = SettingType.Boolean,
                        Value = "0",
                        Order = 1,
                        Core = true
                    },
                    new GenericSettingModel("Google SafeBrowsing API key")
                    {
                        Description = "If set, links will be scanned by the SafeBrowsing API to check for malware and unsafe sites.",
                        View = SettingType.String,
                        Value = "Get your key from the Google API Console",
                        Order = 2,
                        Core = true
                    }
                }
            );
        }

        public void Check(int id, string culture, string val, List<SettingsModel> settings)
        {
            var apiKey = settings.GetValue<string>(KnownSettings.GoogleApiKey, culture);
            var checkSafeBrowsing = settings.GetValue<bool>(KnownSettings.EnsureSafeLinks, culture);

            // check safebrowsing first to avoid double processing of links
            List<BrokenLinkModel> safeBrowsingResult = checkSafeBrowsing && apiKey.HasValue() ? _safeBrowsingService.Check(val, apiKey) : new List<BrokenLinkModel>();
            List<BrokenLinkModel> brokenLinksResult = _linksService.Check(val, safeBrowsingResult);

            // then set Failed
            Failed = brokenLinksResult.Any() || safeBrowsingResult.Any();
            // and set Result
            Result = new Dictionary<string, List<BrokenLinkModel>>
            {
                { "safeBrowsing", safeBrowsingResult},
                { "brokenLinks", brokenLinksResult }
            };

            // finally, tally failed tests
            FailedCount = safeBrowsingResult.Any() ? 1 : 0;
            FailedCount += brokenLinksResult.Any() ? 1 : 0;
        }
    }
}
