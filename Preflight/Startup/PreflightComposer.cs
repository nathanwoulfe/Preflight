using Preflight.Plugins;
using Preflight.Services;
using Preflight.Services.Interfaces;
using Umbraco.Core.Components;
using Umbraco.Core.Composing;

namespace Preflight.Startup
{
    public class PreflightComposer : IUserComposer
    {
        public void Compose(Composition composition)
        {
            composition.Register<IContentChecker, ContentChecker>();
            composition.Register<ILinksService, LinksService>();
            composition.Register<IReadabilityService, ReadabilityService>();
            composition.Register<ISafeBrowsingService, SafeBrowsingService>();
            composition.Register<ISettingsService, SettingsService>();

            //composition.Register<IPreflightPlugin, ReadabilityPlugin>();

            composition.Components().Append<PreflightComponent>();
        }
    }
}
