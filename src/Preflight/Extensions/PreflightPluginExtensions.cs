using Preflight.Models;
using Preflight.Plugins;
using System.Collections.Generic;
using System.Linq;

namespace Preflight.Extensions
{
    public static class PreflightPluginExtensions
    {
        public static bool IsDisabled(this IPreflightPlugin plugin, string culture) =>
            True(plugin.Settings, culture, plugin.Name.DisabledAlias());

        public static bool IsOnSaveOnly(this IPreflightPlugin plugin, string culture) => 
            True(plugin.Settings, culture, plugin.Name.OnSaveOnlyAlias());
        
        private static bool True(IEnumerable<SettingsModel> settings, string culture, string settingAlias) =>
            settings.Any(s => s.Alias == settingAlias && s.Value.ForVariant(culture) == KnownStrings.One);
    }
}
