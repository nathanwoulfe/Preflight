using Preflight.Extensions;

namespace Preflight.Models.Settings;

/// <summary>
/// Defines the Preflight setting for the disable/enable setting
/// </summary>
internal class DisabledSettingModel : SettingsModel
{
    public DisabledSettingModel(string tab, bool val, Guid guid)
    {
        Label = "Disabled";
        Alias = tab.DisabledAlias();
        DefaultValue = val ? KnownStrings.One : KnownStrings.Zero;
        Description = "Disable this plugin";
        View = SettingType.Boolean;
        Order = -10;
        Core = true;
        Tab = tab;
        Guid = guid;
    }
}
