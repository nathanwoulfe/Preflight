using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Hosting;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Web.Common.Routing;
using Umbraco.Extensions;

namespace Preflight.Hubs;

internal sealed class PreflightHubRoutes : IAreaRoutes
{
    private readonly IRuntimeState _runtimeState;
    private readonly string _umbracoPathSegment;

    public PreflightHubRoutes(IOptions<GlobalSettings> globalSettings, IHostingEnvironment hostingEnvironment, IRuntimeState runtimeState)
    {
        _runtimeState = runtimeState;
        _umbracoPathSegment = globalSettings.Value.GetUmbracoMvcArea(hostingEnvironment);
    }

    public void CreateRoutes(IEndpointRouteBuilder endpoints)
    {
        if (_runtimeState.Level != RuntimeLevel.Run)
        {
            return;
        }

        _ = endpoints.MapHub<PreflightHub>(GetPreflightHubRoute());
    }

    public string GetPreflightHubRoute() => $"/{_umbracoPathSegment}/{nameof(PreflightHub)}";
}
