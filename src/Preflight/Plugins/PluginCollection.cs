using System.Collections.Generic;
#if NET472
using Umbraco.Core.Composing;
#else
using System;
using Umbraco.Cms.Core.Composing;
#endif

namespace Preflight.Plugins
{
    public class PreflightPluginCollectionBuilder : LazyCollectionBuilderBase<PreflightPluginCollectionBuilder, PreflightPluginCollection, IPreflightPlugin>
    {
        protected override PreflightPluginCollectionBuilder This => this;
    }

    public class PreflightPluginCollection : BuilderCollectionBase<IPreflightPlugin>
    {
#if NET472
        public PreflightPluginCollection(IEnumerable<IPreflightPlugin> plugins) : base(plugins) {}
#else
        public PreflightPluginCollection(Func<IEnumerable<IPreflightPlugin>> plugins) : base(plugins) { }
#endif
    }
}
