using Microsoft.Extensions.DependencyInjection;
using Preflight.Hubs;
using Preflight.Plugins.LinkHealth.Services;
using Preflight.Plugins.Readability;
using Preflight.Services.Implement;
using Preflight.Services;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Web.Common.ApplicationBuilder;
using Microsoft.AspNetCore.Builder;
using Preflight.Plugins;
using Preflight.Executors.Implement;
using Preflight.Executors;
using Preflight.Handlers;
using Umbraco.Cms.Core.Notifications;

namespace Preflight.Extensions;

internal static class UmbracoBuilderExtensions
{
    public static IUmbracoBuilder AddServices(this IUmbracoBuilder builder)
    {
        _ = builder.Services
            .AddSingleton<IContentChecker, ContentChecker>()
            .AddSingleton<ISettingsService, SettingsService>()
            .AddSingleton<ICacheManager, CacheManager>();

        return builder;
    }

    public static IUmbracoBuilder AddExecutors(this IUmbracoBuilder builder)
    {
        _ = builder.Services
            .AddSingleton<IPluginExecutor, PluginExecutor>()
            .AddSingleton<IServerVariablesParsingExecutor, ServerVariablesParsingExecutor>()
            .AddSingleton<IContentSavingExecutor, ContentSavingExecutor>()
            .AddSingleton<ISendingContentModelExecutor, SendingContentModelExecutor>();

        return builder;
    }

    public static IUmbracoBuilder AddNotificationHandlers(this IUmbracoBuilder builder)
    {
        _ = builder
            .AddNotificationHandler<ServerVariablesParsingNotification, ServerVariablesParsingHandler>()
            .AddNotificationHandler<ContentSavingNotification, ContentSavingHandler>()
            .AddNotificationHandler<SendingContentNotification, SendingContentHandler>()
            .AddNotificationHandler<UmbracoApplicationStartingNotification, ApplicationStartedHandler>();

        return builder;
    }

    public static IUmbracoBuilder AddCorePlugins(this IUmbracoBuilder builder)
    {
        _ = builder.Services
             .AddSingleton<ILinksService, LinksService>()
             .AddSingleton<IReadabilityService, ReadabilityService>()
             .AddSingleton<ISafeBrowsingService, SafeBrowsingService>();

        _ = builder.WithCollectionBuilder<PreflightPluginCollectionBuilder>()
            .Add(() => builder.TypeLoader.GetTypes<IPreflightPlugin>());

        return builder;
    }

    public static IUmbracoBuilder AddSignalR(this IUmbracoBuilder builder)
    {
        _ = builder.Services
            .AddSingleton<IMessenger, Messenger>()
            .AddSingleton<PreflightHubRoutes>()
            .AddSignalR();

        _ = builder.Services.Configure<UmbracoPipelineOptions>(options => options.AddFilter(new UmbracoPipelineFilter(
               "Preflight",
               applicationBuilder => { },
               applicationBuilder => { },
               applicationBuilder => _ = applicationBuilder.UseEndpoints(e =>
               {
                   PreflightHubRoutes hubRoutes = applicationBuilder.ApplicationServices.GetRequiredService<PreflightHubRoutes>();
                   hubRoutes.CreateRoutes(e);
               }))));

        return builder;
    }
}
