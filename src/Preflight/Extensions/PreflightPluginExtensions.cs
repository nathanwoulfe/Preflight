using Preflight.Models;
using Preflight.Plugins;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Preflight.Extensions
{
    public static class PreflightPluginExtensions
    {
        /// <summary>
        /// Is the given plugin disabled?
        /// </summary>
        /// <param name="plugin"></param>
        /// <param name="culture"></param>
        /// <returns></returns>
        public static bool IsDisabled(this IPreflightPlugin plugin, string culture) =>
            True(plugin.Settings, culture, plugin.Name.DisabledAlias());

        /// <summary>
        /// Is the given plugin set to run on save only?
        /// </summary>
        /// <param name="plugin"></param>
        /// <param name="culture"></param>
        /// <returns></returns>
        public static bool IsOnSaveOnly(this IPreflightPlugin plugin, string culture) => 
            True(plugin.Settings, culture, plugin.Name.OnSaveOnlyAlias());
        
        /// <summary>
        /// Get the setting represented by the settingAlias param, as a boolean
        /// Will be true if the value is "1", false in all other cases
        /// </summary>
        /// <param name="settings"></param>
        /// <param name="culture"></param>
        /// <param name="settingAlias"></param>
        /// <returns></returns>
        private static bool True(IEnumerable<SettingsModel> settings, string culture, string settingAlias) =>
            settings.Any(s => s.Alias == settingAlias && s.Value.ForVariant(culture) == KnownStrings.One);

        /// <summary>
        /// Generates the default settings (disabled, onSaveOnly, propertiesToTest) for the given plugin,
        /// provided the plugin has valid guid values for each property
        /// </summary>
        /// <param name="plugin"></param>
        /// <param name="disabled"></param>
        /// <param name="runOnSaveOnly"></param>
        /// <param name="propsToTest"></param>
        /// <param name="settings"></param>
        public static void GenerateDefaultSettings(this IPreflightPlugin plugin, 
            bool disabled,
            bool runOnSaveOnly,
            string propsToTest = "",
            params SettingsModel[] settings)
        {
            if (!settings.Any())
            {
                plugin.Settings = new List<SettingsModel>();
                return;
            }

            if (!propsToTest.HasValue())
            {
                propsToTest = string.Join(KnownStrings.Comma, KnownPropertyAlias.All);
            }

            List<SettingsModel> response = new List<SettingsModel>();

            if (Guid.TryParse(plugin.DisabledSettingIdentifier, out Guid disabledGuid))
            {
                response.Add(new DisabledSettingModel(plugin.Name, disabled, disabledGuid));
            }

            if (Guid.TryParse(plugin.OnSaveOnlySettingIdentifier, out Guid onSaveOnlyGuid))
            {
                response.Add(new OnSaveOnlySettingModel(plugin.Name, runOnSaveOnly, onSaveOnlyGuid));
            }

            if (Guid.TryParse(plugin.PropertiesToTestSettingIdentifier, out Guid propsToTestGuid))
            {
                response.Add(new PropertiesToTestSettingModel(plugin.Name, propsToTest, propsToTestGuid));
            }

            foreach (SettingsModel s in settings)
            {
                s.Tab = plugin.Name;
                response.Add(s);
            }

            plugin.Settings = response;
        }
    }
}
