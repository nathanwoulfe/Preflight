using Preflight.Models;
using System;

namespace Preflight.Settings
{
    public class NaughtyNiceSettings
    {
        public SettingsModel[] Settings
        {
            get
            {
                return new SettingsModel[] {
                    new GenericSettingModel("Nice words")
                    {
                        Description = "These words will be excluded from the readability check.",
                        View = SettingType.MultipleTextbox,
                        Tab = SettingsTabNames.NaughtyAndNice,
                        Value = "Umbraco,preflight,hippopotamus",
                        Order = 0,
                        Core = true,
                        Guid = new Guid(KnownSettings.NiceList),
                    },
                    new GenericSettingModel("Naughty words")
                    {
                        Description = "These words should never be used.",
                        View = SettingType.MultipleTextbox,
                        Tab = SettingsTabNames.NaughtyAndNice,
                        Value = "bannedword,never_use_this",
                        Order = 1,
                        Core = true,
                        Guid = new Guid(KnownSettings.NaughtyList),
                    },
                };
            }
        }
    }
}
