using Preflight.Plugins;
using System.Linq;

namespace Preflight.Extensions
{
    public static class PreflightPluginExtensions
    {
        public static bool IsDisabled(this IPreflightPlugin plugin)
        {
            return plugin.Settings.Any() && 
                   plugin.Settings.Any(s => s.Alias == plugin.Name.DisabledAlias() && s.Value.ToString() == "1");
            
        }
    }
}
