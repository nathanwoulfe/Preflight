using Preflight.Models.Settings;

namespace Preflight.Settings;

public class CoreSettings
{
    public SettingsModel[] Settings => new SettingsModel[]
    {
        new GenericSettingModel(KnownSettingsAlias.RunOnSave, new Guid(KnownSettings.BindSaveHandler))
        {
            View = SettingType.Boolean,
            Tab = SettingsTabNames.General,
            DefaultValue = "0",
            Order = 0,
            Core = true,
        },
        new GenericSettingModel(KnownSettingsAlias.CancelSaveOnFail, new Guid(KnownSettings.CancelSaveOnFail))
        {
            View = SettingType.Boolean,
            Tab = SettingsTabNames.General,
            DefaultValue = "0",
            Order = 1,
            Core = true,
        },
        new GenericSettingModel(KnownSettingsAlias.PropertiesToTest, new Guid(KnownSettings.PropertiesToTest))
        {
            View = SettingType.CheckboxList,
            Tab = SettingsTabNames.General,
            DefaultValue = string.Join(KnownStrings.Comma, KnownPropertyAlias.All),
            Prevalues = KnownPropertyAlias.All.Select(x => new { value = x, key = x }),
            Order = 2,
            Core = true,
        },
        new GenericSettingModel(KnownSettingsAlias.UserGroupOptIn, new Guid(KnownSettings.UserGroupOptIn))
        {
            View = SettingType.CheckboxList,
            Tab = SettingsTabNames.General,
            DefaultValue = "Administrators,Editors",
            Order = 3,
            Core = true,
        },
    };
}
