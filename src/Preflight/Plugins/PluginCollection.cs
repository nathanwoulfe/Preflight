using System.Collections.Generic;
#if NETCOREAPP
using System;
using Umbraco.Cms.Core.Composing;
#else
using Umbraco.Core.Composing;
#endif

namespace Preflight.Plugins
{
    public class PreflightPluginCollectionBuilder : LazyCollectionBuilderBase<PreflightPluginCollectionBuilder, PreflightPluginCollection, IPreflightPlugin>
    {
        protected override PreflightPluginCollectionBuilder This => this;
    }

    public class PreflightPluginCollection : BuilderCollectionBase<IPreflightPlugin>
    {
#if NETCOREAPP
        public PreflightPluginCollection(Func<IEnumerable<IPreflightPlugin>> plugins) : base(plugins) { }
#else
        public PreflightPluginCollection(IEnumerable<IPreflightPlugin> plugins) : base(plugins) {}
#endif
    }
}
