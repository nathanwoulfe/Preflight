using Microsoft.Extensions.DependencyInjection;
using Preflight.Extensions;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.DependencyInjection;

namespace Preflight;

public class PreflightComposer : IComposer
{
    public void Compose(IUmbracoBuilder builder)
    {
        _ = builder
            .AddServices()
            .AddExecutors()
            .AddNotificationHandlers()
            .AddCorePlugins()
            .AddSignalR();

        _ = builder.ManifestFilters().Append<PreflightManifestFilter>();
    }
}
