using Preflight.Extensions;

namespace Preflight.Models.Settings;

/// <summary>
/// Defines the Preflight setting for the on-save-only setting
/// </summary>
internal class OnSaveOnlySettingModel : SettingsModel
{
    public OnSaveOnlySettingModel(string tab, bool val, Guid guid)
    {
        Label = "Run on save only";
        Alias = tab.OnSaveOnlyAlias();
        DefaultValue = val ? KnownStrings.One : KnownStrings.Zero;
        Description = "Restrict this plugin to run only in a save event";
        View = SettingType.Boolean;
        Order = -5;
        Core = true;
        Tab = tab;
        Guid = guid;
    }
}
