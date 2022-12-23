using Preflight.Models;

namespace Preflight.Settings;

public class NaughtyNiceSettings
{
    public SettingsModel[] Settings => new SettingsModel[]
    {
        new GenericSettingModel("Nice words", new Guid(KnownSettings.NiceList))
        {
            Description = "These words will be excluded from the readability check",
            View = SettingType.MultipleTextbox,
            Tab = SettingsTabNames.NaughtyAndNice,
            Value = "Umbraco,preflight,hippopotamus",
            Order = 0,
            Core = true,
        },
        new GenericSettingModel("Naughty words", new Guid(KnownSettings.NaughtyList))
        {
            Description = "These words should never be used.",
            View = SettingType.MultipleTextbox,
            Tab = SettingsTabNames.NaughtyAndNice,
            Value = "bannedword,never_use_this",
            Order = 1,
            Core = true,
        },
    };
}
