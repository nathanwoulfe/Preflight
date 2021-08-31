using System.Collections.Generic;
using Preflight.Controllers;
using System.Linq;
#if NETCOREAPP
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core;
#else
using Umbraco.Core;
using Umbraco.Core.Services;
using Umbraco.Web;
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

#if NETCOREAPP
            var platform = KnownStrings.CORE;
#else
            var platform = KnownStrings.FRAMEWORK;
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
                { "Platform", platform },
                { "SettingsGuid", new Dictionary<string, string>
                    {
                        { "BindSaveHandler", KnownSettings.BindSaveHandler },
                        { "UserGroupOptIn", KnownSettings.UserGroupOptIn },
                    }
                }
            };

            if (additionalData != null)
            {
                preflightDictionary = preflightDictionary.Concat(additionalData).ToDictionary(k => k.Key, v => v.Value);
            }

            dictionary.Add(KnownStrings.Name, preflightDictionary);
        }
    }
}