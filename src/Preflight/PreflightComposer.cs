using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Preflight.Executors;
using Preflight.Executors.Implement;
using Preflight.Handlers;
using Preflight.Hubs;
using Preflight.Plugins;
using Preflight.Services;
using Preflight.Services.Implement;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Web.Common.ApplicationBuilder;

namespace Preflight;

public class PreflightComposer : IComposer
{
    public void Compose(IUmbracoBuilder builder)
    {
        _ = builder.ManifestFilters().Append<PreflightManifestFilter>();

        _ = builder.Services
            .AddSingleton<IContentChecker, ContentChecker>()
            .AddSingleton<ILinksService, LinksService>()
            .AddSingleton<IReadabilityService, ReadabilityService>()
            .AddSingleton<ISafeBrowsingService, SafeBrowsingService>()
            .AddSingleton<ISettingsService, SettingsService>()
            .AddSingleton<ICacheManager, CacheManager>()

            .AddSingleton<IPluginExecutor, PluginExecutor>()
            .AddSingleton<IServerVariablesParsingExecutor, ServerVariablesParsingExecutor>()
            .AddSingleton<IContentSavingExecutor, ContentSavingExecutor>()
            .AddSingleton<ISendingContentModelExecutor, SendingContentModelExecutor>()

            .AddSingleton<IMessenger, Messenger>()
            .AddSingleton<PreflightHubRoutes>()
            .AddSignalR();

        _ = builder.AddNotificationHandler<ServerVariablesParsingNotification, ServerVariablesParsingHandler>()
            .AddNotificationHandler<ContentSavingNotification, ContentSavingHandler>()
            .AddNotificationHandler<SendingContentNotification, SendingContentHandler>()
            .AddNotificationHandler<UmbracoApplicationStartingNotification, ApplicationStartedHandler>();

        _ = builder.WithCollectionBuilder<PreflightPluginCollectionBuilder>()
            .Add(() => builder.TypeLoader.GetTypes<IPreflightPlugin>());

        _ = builder.Services.Configure<UmbracoPipelineOptions>(options => options.AddFilter(new UmbracoPipelineFilter(
                "Preflight",
                applicationBuilder => { },
                applicationBuilder => { },
                applicationBuilder => _ = applicationBuilder.UseEndpoints(e =>
                    {
                        PreflightHubRoutes hubRoutes = applicationBuilder.ApplicationServices.GetRequiredService<PreflightHubRoutes>();
                        hubRoutes.CreateRoutes(e);
                    }))));
    }
}
