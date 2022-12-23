using Umbraco.Cms.Core.Composing;

namespace Preflight.Plugins;

public class PreflightPluginCollectionBuilder : LazyCollectionBuilderBase<PreflightPluginCollectionBuilder, PreflightPluginCollection, IPreflightPlugin>
{
    protected override PreflightPluginCollectionBuilder This => this;
}

public class PreflightPluginCollection : BuilderCollectionBase<IPreflightPlugin>
{
    public PreflightPluginCollection(Func<IEnumerable<IPreflightPlugin>> plugins)
        : base(plugins)
    {
    }
}
