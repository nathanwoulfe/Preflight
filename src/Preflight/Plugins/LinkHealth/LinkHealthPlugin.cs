using Preflight.Extensions;
using Preflight.Models.Settings;
using Preflight.Plugins.LinkHealth.Models;
using Preflight.Plugins.LinkHealth.Services;

namespace Preflight.Plugins.LinkHealth;

public class LinkHealthPlugin : IPreflightCorePlugin
{
    private readonly ILinksService _linksService;
    private readonly ISafeBrowsingService _safeBrowsingService;

    public string DisabledSettingIdentifier => "3f7e0bbc-7f76-4471-b700-ac1f20aaa015";

    public string OnSaveOnlySettingIdentifier => "cec5f2d4-d8de-4271-97fe-e9e38cad5259";

    public string PropertiesToTestSettingIdentifier => "a69f2558-2c79-48c5-9ee2-aede89515f74";

    public object? Result { get; set; }

    public IEnumerable<SettingsModel> Settings { get; set; } = Enumerable.Empty<SettingsModel>();

    public bool Failed { get; set; }

    public bool Core => true;

    public int SortOrder => -1;

    public int FailedCount { get; set; }

    public int TotalTests => 2;

    public string Name => "Link health";

    public string ViewPath => "/App_Plugins/Preflight/Backoffice/plugins/linkhealth/linkhealth.html";

    public string Summary => string.Empty;

    public string Description { get; set; } = string.Empty;

    public LinkHealthPlugin(ILinksService linksService, ISafeBrowsingService safeBrowsingService)
    {
        _linksService = linksService;
        _safeBrowsingService = safeBrowsingService;

        this.GenerateDefaultSettings(
            false,
            true,
            settings: new SettingsModel[]
            {
                new GenericSettingModel("ensureSafeLinks", new Guid(KnownSettings.EnsureSafeLinks))
                {
                    View = SettingType.Boolean,
                    DefaultValue = KnownStrings.Zero,
                    Order = 1,
                    Core = true,
                },
                new GenericSettingModel("googleSafeBrowsingAPIKey", new Guid(KnownSettings.GoogleApiKey))
                {
                    View = SettingType.String,
                    DefaultValue = "Get your key from the Google API Console",
                    Order = 2,
                    Core = true,
                },
            });
    }

    public void Check(int id, string culture, string val, List<SettingsModel> settings)
    {
        string? apiKey = settings.GetValue<string>(KnownSettings.GoogleApiKey, culture);
        bool checkSafeBrowsing = settings.GetValue<bool>(KnownSettings.EnsureSafeLinks, culture);

        // check safebrowsing first to avoid double processing of links
        List<BrokenLinkModel> safeBrowsingResult = checkSafeBrowsing && apiKey.HasValue() ? _safeBrowsingService.Check(val, apiKey) : [];
        List<BrokenLinkModel> brokenLinksResult = _linksService.Check(val, safeBrowsingResult);

        // then set Failed
        Failed = brokenLinksResult.Any() || safeBrowsingResult.Any();

        // and set Result
        Result = new Dictionary<string, List<BrokenLinkModel>>
        {
            { "safeBrowsing", safeBrowsingResult},
            { "brokenLinks", brokenLinksResult },
        };

        // finally, tally failed tests
        FailedCount = safeBrowsingResult.Any() ? 1 : 0;
        FailedCount += brokenLinksResult.Any() ? 1 : 0;
    }
}
