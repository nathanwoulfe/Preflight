using System.Text.RegularExpressions;
using Preflight.Extensions;
using Preflight.Models.Settings;
using CharArrays = Umbraco.Cms.Core.Constants.CharArrays;

namespace Preflight.Plugins.AutoCorrect;

public class AutocorrectPlugin : IPreflightCorePlugin
{
    public string DisabledSettingIdentifier => "7d34c7dd-0167-42c9-a1a4-f7245d5e555a";

    public string OnSaveOnlySettingIdentifier => "9665c018-80be-402f-890a-4bb7f56deaac";

    public string PropertiesToTestSettingIdentifier => "8a567a58-5417-4562-a1b9-12f1e80c8dbb";

    public object? Result { get; set; }

    public IEnumerable<SettingsModel> Settings { get; set; } = Enumerable.Empty<SettingsModel>();

    public bool Failed { get; set; }

    public bool Core => true;

    public int SortOrder => -1;

    public int FailedCount { get; set; }

    public string Name => "Autocorrect";

    public string ViewPath => string.Empty;

    public string Summary => string.Empty;

    public string Description { get; set; } = string.Empty;

    public int TotalTests => 1;

    /// <summary>
    ///
    /// </summary>
    public AutocorrectPlugin() => this.GenerateDefaultSettings(
        false,
        true,
        settings: new SettingsModel[]
        {
            new GenericSettingModel("autocorrectTerms", new Guid(KnownSettings.AutocorrectTerms))
            {
                View = SettingType.MultipleTextbox,
                DefaultValue = "replacethis|new term",
                Order = 1,
                Core = true,
            },
        });

    /// <summary>
    /// perform autocorrect before readability check
    /// only do this in save handler as there's no point in updating if it's not being saved (potentially)
    /// </summary>
    /// <param name="id"></param>
    /// <param name="culture"></param>
    /// <param name="val"></param>
    /// <param name="settings"></param>
    public void Check(int id, string culture, string val, List<SettingsModel> settings)
    {
        var autocorrect = settings.GetValue<string>(KnownSettings.AutocorrectTerms, culture)?.Split(CharArrays.Comma)
            .ToDictionary(
                s => s.Split(CharArrays.VerticalTab)[0],
                s => s.Split(CharArrays.VerticalTab)[1]);

        if (autocorrect is null || !autocorrect.Any())
        {
            return;
        }

        foreach (KeyValuePair<string, string> term in autocorrect)
        {
            string pattern = $@"\b{term.Key}\b";
            _ = Regex.Replace(val, pattern, term.Value, RegexOptions.IgnoreCase);
        }
    }
}
