using Preflight.Models.Settings;

namespace Preflight.Settings;

public class NaughtyNiceSettings
{
    public SettingsModel[] Settings => new SettingsModel[]
    {
        new GenericSettingModel(KnownSettingsAlias.NiceWords, new Guid(KnownSettings.NiceList))
        {
            View = SettingType.MultipleTextbox,
            Tab = SettingsTabNames.NaughtyAndNice,
            DefaultValue = "Umbraco,preflight,hippopotamus",
            Order = 0,
            Core = true,
        },
        new GenericSettingModel(KnownSettingsAlias.NaughtyWords, new Guid(KnownSettings.NaughtyList))
        {
            View = SettingType.MultipleTextbox,
            Tab = SettingsTabNames.NaughtyAndNice,
            DefaultValue = "bannedword,never_use_this",
            Order = 1,
            Core = true,
        },
    };
}
