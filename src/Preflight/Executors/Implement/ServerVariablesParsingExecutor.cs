using Microsoft.AspNetCore.Routing;
using Preflight.Controllers;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Services;
using Umbraco.Extensions;

namespace Preflight.Executors.Implement;

internal sealed class ServerVariablesParsingExecutor : IServerVariablesParsingExecutor
{
    private readonly ILocalizationService _localizationService;
    private readonly IRuntimeState _runtimeState;
    private readonly LinkGenerator _linkGenerator;

    public ServerVariablesParsingExecutor(ILocalizationService localizationService, IRuntimeState runtimeState, LinkGenerator linkGenerator)
    {
        _localizationService = localizationService;
        _runtimeState = runtimeState;
        _linkGenerator = linkGenerator;
    }

    public void Generate(IDictionary<string, object> dictionary, IDictionary<string, object>? additionalData)
    {
        if (_runtimeState.Level != RuntimeLevel.Run)
        {
            return;
        }

        Dictionary<string, object> umbracoSettings = dictionary["umbracoSettings"] as Dictionary<string, object> ?? [];
        string pluginPath = $"{umbracoSettings["appPluginsPath"]}/Preflight/Backoffice";

        var preflightDictionary = new Dictionary<string, object>
        {
            { "ContentFailedChecks", KnownStrings.ContentFailedChecks },
            { "PluginPath", $"{umbracoSettings["appPluginsPath"]}/Preflight/Backoffice" },
            { "PropertyTypesToCheck", KnownPropertyAlias.All },
            { "ApiPath", _linkGenerator.GetUmbracoApiServiceBaseUrl<PreflightApiController>(controller => controller.GetSettings())! },
            { "DefaultCulture", _localizationService.GetDefaultLanguageIsoCode() },
            { "Platform", KnownStrings.CORE },
            {
                "SettingsGuid", new Dictionary<string, string>
                {
                    { "BindSaveHandler", KnownSettings.BindSaveHandler },
                    { "UserGroupOptIn", KnownSettings.UserGroupOptIn },
                }
            },
        };

        if (additionalData is not null)
        {
            preflightDictionary = preflightDictionary.Concat(additionalData).ToDictionary(k => k.Key, v => v.Value);
        }

        dictionary.Add(KnownStrings.Name, preflightDictionary);
    }
}
