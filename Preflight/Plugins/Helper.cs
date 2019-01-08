using System;
using System.Collections.Generic;
using System.Linq;

namespace Preflight.Plugins
{
    public static class PluginsHelper
    {
        public static IEnumerable<PreflightPlugin> GetPlugins()
        {
            List<Type> types = AppDomain.CurrentDomain.GetAssemblies().SelectMany(x => x.GetTypes())
                .Where(x => typeof(PreflightPlugin).IsAssignableFrom(x) && !x.IsInterface && !x.IsAbstract).ToList();

            List<PreflightPlugin> plugins = new List<PreflightPlugin>();

            foreach (Type t in types)
            {
                plugins.Add((PreflightPlugin)Activator.CreateInstance(t));
            }

            return plugins;
        }
    }
}
