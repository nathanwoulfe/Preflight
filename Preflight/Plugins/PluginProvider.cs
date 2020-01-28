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
            AggregateCatalog catalog = new AggregateCatalog(
                new DirectoryCatalog(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "bin"), "*.dll"));

            CompositionContainer container = new CompositionContainer(catalog);

            try
            {
                container.ComposeParts(this);
            }
            catch (CompositionException e)
            {
                Console.WriteLine(e.ToString());
            }
            finally
            {
                container.Dispose();
            }

            return _preflightPlugins;
        }
    }
}
