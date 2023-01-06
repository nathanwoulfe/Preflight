using Preflight.Extensions;
using Preflight.Models.Settings;

namespace Preflight.Plugins.Readability;

public class ReadabilityPlugin : IPreflightCorePlugin
{
    private readonly IReadabilityService _readabilityService;

    public string DisabledSettingIdentifier => "dfeab01a-e5c0-49ed-a018-9f1c546dfe10";

    public string OnSaveOnlySettingIdentifier => "1485f35b-4c2e-4a53-a0b2-fefa02bcf810";

    public string PropertiesToTestSettingIdentifier => "3984cf76-b598-4bb8-b073-698f39111f7c";

    public object? Result { get; set; }

    public IEnumerable<SettingsModel> Settings { get; set; } = Enumerable.Empty<SettingsModel>();

    public string Description { get; set; } = string.Empty;

    public bool Failed { get; set; }

    public bool Core => true;

    public int SortOrder => -2;

    public int FailedCount { get; set; }

    public int TotalTests => 3;

    public string Name => "Readability";

    public string Summary => string.Empty;

    public string ViewPath => "/App_Plugins/Preflight/Backoffice/plugins/readability/readability.html";

    /// <summary>
    /// 
    /// </summary>
    /// <param name="readabilityService"></param>
    public ReadabilityPlugin(IReadabilityService readabilityService)
    {
        _readabilityService = readabilityService;

        this.GenerateDefaultSettings(
            false,
            false,
            settings: new SettingsModel[]
            {
                new GenericSettingModel("readabilityTargetMinimum", new Guid(KnownSettings.ReadabilityMin))
                {
                    DefaultValue = 60,
                    View = SettingType.Slider,
                    Order = 1,
                    Core = true,
                },
                new GenericSettingModel("readabilityTargetMaximum", new Guid(KnownSettings.ReadabilityMax))
                {
                    DefaultValue = 100,
                    View = SettingType.Slider,
                    Order = 2,
                    Core = true,
                },
                new GenericSettingModel("longWordSyllableCount", new Guid(KnownSettings.LongWordSyllables))
                {
                    DefaultValue = 5,
                    View = SettingType.Slider,
                    Order = 3,
                    Core = true,
                },
            });
    }

    public void Check(int id, string culture, string val, List<SettingsModel> settings)
    {
        // must get a result of any type
        ReadabilityResponseModel result = _readabilityService.Check(val, culture, settings);

        // then set Failed
        Failed = result.Failed;

        // and set Result
        Result = result;

        // finally, tally failed tests
        FailedCount = result.FailedReadability ? 1 : 0;
        FailedCount += result.Blacklist.Any() ? 1 : 0;
        FailedCount += result.LongWords.Any() ? 1 : 0;
    }
}
