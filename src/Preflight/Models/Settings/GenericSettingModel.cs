namespace Preflight.Models.Settings;

/// <summary>
/// Defines a generic Preflight setting
/// </summary>
public class GenericSettingModel : SettingsModel
{
    public GenericSettingModel(string alias, Guid guid)
    {
        Alias = alias;
        Guid = guid;
    }
}
