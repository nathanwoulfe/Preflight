using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.IO;

namespace Preflight.Plugins
{
    internal class PluginProvider
    {
        [ImportMany]
        private IEnumerable<IPreflightPlugin> _preflightPlugins;

        internal IEnumerable<IPreflightPlugin> Get()
        {
            var catalog = new AggregateCatalog(
                new DirectoryCatalog(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "bin"), "*.dll"));
         
            var container = new CompositionContainer(catalog);

            try
            {
                container.ComposeParts(this);
            }
            catch (CompositionException e)
            {
                Console.WriteLine(e.ToString());
            }

            return _preflightPlugins;
        }
    }
}
