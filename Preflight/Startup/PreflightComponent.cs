using ClientDependency.Core;
using Preflight.Constants;
using Preflight.Extensions;
using Preflight.Models;
using Preflight.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Http;
using System.Web.Routing;
using Umbraco.Core.Composing;
using Umbraco.Core.Events;
using Umbraco.Core.Models;
using Umbraco.Core.Services;
using Umbraco.Core.Services.Implement;
using Umbraco.Web;
using Umbraco.Web.JavaScript;

namespace Preflight.Startup
{
    public class PreflightComponent : IComponent
    {
        private readonly ISettingsService _settingsService;
        private readonly IContentChecker _contentChecker;

        public PreflightComponent(ISettingsService settingsService, IContentChecker contentChecker)
        {
            _settingsService = settingsService;
            _contentChecker = contentChecker;
        }

        public void Initialize()
        {
            GlobalConfiguration.Configuration.MessageHandlers.Add(new NotificationsHandler());

            ServerVariablesParser.Parsing += ServerVariablesParser_Parsing;
            ContentService.Saving += ContentService_Saving;
        }

        public void Terminate()
        {

        }

        /// <summary>
        /// Add preflight-specific values to the servervariables dictionary
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="dictionary"></param>
        private static void ServerVariablesParser_Parsing(object sender, Dictionary<string, object> dictionary)
        {
            var urlHelper = new System.Web.Mvc.UrlHelper(new RequestContext(new HttpContextWrapper(HttpContext.Current), new RouteData()));
            IDictionary<string, object> settings = dictionary["umbracoSettings"].ToDictionary();

            List<string> typesToCheck = new List<string>();
            foreach (FieldInfo field in typeof(KnownPropertyAlias).GetFields())
            {
                typesToCheck.Add(field.GetValue(null).ToString());
            }

            dictionary.Add("Preflight", new Dictionary<string, object>
            {
                { "ContentFailedChecks", KnownStrings.ContentFailedChecks },
                { "PluginPath", $"{settings["appPluginsPath"]}/preflight/backoffice" },
                { "PropertyTypesToCheck", typesToCheck },
                { "ApiPath", urlHelper.GetUmbracoApiServiceBaseUrl<Api.ApiController>(controller => controller.GetSettings()) }
            });
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ContentService_Saving(IContentService sender, SaveEventArgs<IContent> e)
        {
            List<SettingsModel> settings = _settingsService.Get().Settings;

            // only check if current user group is opted in to testing on save
            var groupSetting = settings.FirstOrDefault(x => string.Equals(x.Label, KnownSettings.UserGroupOptIn, StringComparison.InvariantCultureIgnoreCase));
            if (groupSetting != null && groupSetting.Value.HasValue())
            {
                var currentUserGroups = Umbraco.Web.Composing.Current.UmbracoContext.Security.CurrentUser?.Groups?.Select(x => x.Name) ?? default;
                if (currentUserGroups.Any())
                {
                    bool testOnSave = groupSetting.Value.Split(',').Intersect(currentUserGroups).Any();

                    if (!testOnSave)
                        return;
                }
            }

            if (!settings.GetValue<bool>(KnownSettings.BindSaveHandler)) return;

            var cancelSaveOnFail = settings.GetValue<bool>(KnownSettings.CancelSaveOnFail);

            IContent content = e.SavedEntities.First();

            bool failed = _contentChecker.CheckContent(content, true);

            // at least one property on the current document fails the preflight check
            if (!failed) return;

            // these values are retrieved in the notifications handler, and passed down to the client
            HttpContext.Current.Items["PreflightFailed"] = true;
            HttpContext.Current.Items["PreflightCancelSaveOnFail"] = cancelSaveOnFail;
            HttpContext.Current.Items["PreflightNodeId"] = content.Id;

            if (e.CanCancel && cancelSaveOnFail)
            {
                e.CancelOperation(new EventMessage("PreflightFailed", content.Id.ToString()));
            }
        }
    }
}
