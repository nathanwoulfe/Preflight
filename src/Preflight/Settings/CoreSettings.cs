using Preflight.Models;
using System;
using System.Linq;

namespace Preflight.Settings
{
    public class CoreSettings
    {
        public SettingsModel[] Settings
        {
            get
            {
                return new SettingsModel[] {
                    new GenericSettingModel("Run Preflight on save")
                    {
                        Description = "Set to true and Preflight will run on all saves, and alert users to any errors.",
                        View = SettingType.Boolean,
                        Tab = SettingsTabNames.General,
                        Value = 0,
                        Order = 0,
                        Core = true,
                        Guid = new Guid(KnownSettings.BindSaveHandler),
                    },
                    new GenericSettingModel("Cancel save when Preflight tests fail")
                    {
                        Description = "Set to true and Preflight will cancel the save event, if tests fail and Preflight is set to run on save.",
                        View = SettingType.Boolean,
                        Tab = SettingsTabNames.General,
                        Value = 0,
                        Order = 1,
                        Core = true,
                        Guid = new Guid(KnownSettings.CancelSaveOnFail),
                    },
                    new GenericSettingModel("Properties to test")
                    {
                        Description = "Restrict Preflight to a subset of testable properties.",
                        View = SettingType.CheckboxList,
                        Tab = SettingsTabNames.General,
                        Value = string.Join(KnownStrings.Comma, KnownPropertyAlias.All),
                        Prevalues = KnownPropertyAlias.All.Select(x => new { value = x, key = x }),
                        Order = 2,
                        Core = true,
                        Guid = new Guid(KnownSettings.PropertiesToTest),
                    },
                    new GenericSettingModel("User group opt in/out")
                    {
                        Description = "Select user groups to opt in to testing.",
                        View = SettingType.CheckboxList,
                        Tab = SettingsTabNames.General,
                        Value = "Administrators,Editors",                        
                        Order = 3,
                        Core = true,
                        Guid = new Guid(KnownSettings.UserGroupOptIn),
                    },
                };
            }
        }
    }
}
