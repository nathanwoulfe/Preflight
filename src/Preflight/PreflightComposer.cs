using Preflight.Services;
using Preflight.Executors;
using Preflight.Plugins;
using Preflight.IO;
using Preflight.Services.Implement;
#if NETCOREAPP
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Preflight.Handlers;
using Preflight.Hubs;
using Umbraco.Cms.Web.Common.ApplicationBuilder;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Notifications;
#else
using Preflight.Security;
using Preflight.Logging;
using System.Web;
using Umbraco.Core;
using Umbraco.Core.Composing;
#endif

namespace Preflight
{
#if NETCOREAPP
    public class PreflightComposer : IComposer
    {
        public void Compose(IUmbracoBuilder builder)
        {
            builder.Services
                .AddSingleton<IContentChecker, ContentChecker>()
                .AddSingleton<ILinksService, LinksService>()
                .AddSingleton<IReadabilityService, ReadabilityService>()
                .AddSingleton<ISafeBrowsingService, SafeBrowsingService>()
                .AddSingleton<ISettingsService, SettingsService>()
                .AddSingleton<ICacheManager, CacheManager>()

                .AddSingleton<IPluginExecutor, PluginExecutor>()
                .AddSingleton<ILinkGenerator, LinkGenerator>()
                .AddSingleton<IServerVariablesParsingExecutor, ServerVariablesParsingExecutor>()
                .AddSingleton<IContentSavingExecutor, ContentSavingExecutor>()
                .AddSingleton<ISendingContentModelExecutor, SendingContentModelExecutor>()

                .AddSingleton<IMessenger, Messenger>()
                .AddSingleton<PreflightHubRoutes>()
                .AddSingleton<IIOHelper, PreflightIoHelper>()
                .AddSignalR();

            builder.AddNotificationHandler<ServerVariablesParsingNotification, ServerVariablesParsingHandler>()
                .AddNotificationHandler<ContentSavingNotification, ContentSavingHandler>()
                .AddNotificationHandler<SendingContentNotification, SendingContentHandler>()
                .AddNotificationHandler<UmbracoApplicationStartingNotification, AppStartingHandler>();

            builder.WithCollectionBuilder<PreflightPluginCollectionBuilder>()
                .Add(() => builder.TypeLoader.GetTypes<IPreflightPlugin>());

            builder.Services.Configure<UmbracoPipelineOptions>(options =>
            {
                options.AddFilter(new UmbracoPipelineFilter(
                    "Preflight",
                    applicationBuilder => { },
                    applicationBuilder => { },
                    applicationBuilder =>
                    {
                        applicationBuilder.UseEndpoints(e =>
                        {
                            var hubRoutes = applicationBuilder.ApplicationServices.GetRequiredService<PreflightHubRoutes>();
                            hubRoutes.CreateRoutes(e);
                        });
                    }
                ));
            });
        }
    }
#else
    [RuntimeLevel(MinLevel = RuntimeLevel.Run)]
    public class PreflightComposer : IUserComposer
    {
        public void Compose(Composition composition)
        {
            composition.Register<IPluginExecutor, PluginExecutor>();
            composition.Register<ILinkGenerator, LinkGenerator>();
            composition.Register<IServerVariablesParsingExecutor, ServerVariablesParsingExecutor>();
            composition.Register<IContentSavingExecutor, ContentSavingExecutor>();
            composition.Register<ISendingContentModelExecutor, SendingContentModelExecutor>();

            composition.Register<IContentChecker, ContentChecker>();
            composition.Register<ILinksService, LinksService>();
            composition.Register<IReadabilityService, ReadabilityService>();
            composition.Register<ISafeBrowsingService, SafeBrowsingService>();
            composition.Register<ISettingsService, SettingsService>();
            composition.Register<ICacheManager, CacheManager>();

            composition.Register<IMessenger, Messenger>();
            composition.Register<IBackOfficeSecurityAccessor, BackOfficeSecurityAccessor>();
            composition.Register(typeof(ILogger<>), typeof(Logger<>));
            composition.Register<IIOHelper, PreflightIoHelper>();

            composition.WithCollectionBuilder<PreflightPluginCollectionBuilder>()
                .Add(() => composition.TypeLoader.GetTypes<IPreflightPlugin>());

            composition.Components().Append<PreflightComponent>();

            PreflightContext.Set(new HttpContextWrapper(HttpContext.Current));
        }
    }
#endif
}
