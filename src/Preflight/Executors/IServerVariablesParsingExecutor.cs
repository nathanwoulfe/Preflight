using System.Collections.Generic;
using Preflight.Constants;
using Preflight.Controllers;
using System.Linq;
#if NET472
using Umbraco.Core;
using Umbraco.Core.Services;
using Umbraco.Web;
#else
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core;
#endif

namespace Preflight.Executors
{
    public interface IServerVariablesParsingExecutor
    {
        void Generate(IDictionary<string, object> dictionary, IDictionary<string, object> additionalData = null);
    }

    public class ServerVariablesParsingExecutor : IServerVariablesParsingExecutor
    {
        private readonly ILocalizationService _localizationService;
        private readonly IRuntimeState _runtimeState;
        private readonly ILinkGenerator _linkGenerator;
        public ServerVariablesParsingExecutor(ILocalizationService localizationService, IRuntimeState runtimeState, ILinkGenerator linkGenerator)
        {
            _localizationService = localizationService;
            _runtimeState = runtimeState;
            _linkGenerator = linkGenerator;
        }

        public void Generate(IDictionary<string, object> dictionary, IDictionary<string, object> additionalData = null)
        {
            if (_runtimeState.Level != RuntimeLevel.Run)
                return;

#if NET472
            var platform = KnownStrings.FRAMEWORK;
#else
            var platform = KnownStrings.CORE;
#endif

            Dictionary<string, object> umbracoSettings = dictionary["umbracoSettings"] as Dictionary<string, object> ?? new Dictionary<string, object>();
            string pluginPath = $"{umbracoSettings["appPluginsPath"]}/Preflight/Backoffice";

            var preflightDictionary = new Dictionary<string, object>
            {
                { "ContentFailedChecks", KnownStrings.ContentFailedChecks },
                { "PluginPath", $"{umbracoSettings["appPluginsPath"]}/Preflight/Backoffice" },
                { "PropertyTypesToCheck", KnownPropertyAlias.All },
                { "ApiPath", _linkGenerator.GetUmbracoApiServiceBaseUrl<ApiController>(controller => controller.GetSettings()) },
                { "DefaultCulture", _localizationService.GetDefaultLanguageIsoCode() },
                { "Platform", platform }
            };

            if (additionalData != null)
            {
                preflightDictionary = preflightDictionary.Concat(additionalData).ToDictionary(k => k.Key, v => v.Value);
            }

            dictionary.Add(KnownStrings.Name, preflightDictionary);
        }
    }
}