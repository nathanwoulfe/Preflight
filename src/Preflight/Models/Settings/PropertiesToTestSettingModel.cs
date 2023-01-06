using Preflight.Extensions;

namespace Preflight.Models.Settings;

/// <summary>
/// 
/// </summary>
internal class PropertiesToTestSettingModel : SettingsModel
{
    public PropertiesToTestSettingModel(string tab, string propsToTest, Guid guid)
    {
        Label = "Properties to test";
        Alias = tab.PropertiesToTestAlias();
        DefaultValue = propsToTest;
        Description = "Restrict this plugin to run against a subset of testable properties";
        View = SettingType.CheckboxList;
        Prevalues = KnownPropertyAlias.All.Select(x => new { value = x, key = x });
        Order = -15;
        Core = true;
        Tab = tab;
        Guid = guid;
    }
}
