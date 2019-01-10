using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Configuration;
using System.Web.Http;
using System.Web.Routing;
using Preflight.Constants;
using Preflight.Extensions;
using Preflight.Models;
using Preflight.Services.Interfaces;
using Umbraco.Core.Components;
using Umbraco.Core.Events;
using Umbraco.Core.Models;
using Umbraco.Core.Services;
using Umbraco.Core.Services.Implement;
using Umbraco.Web;
using Umbraco.Web.UI.JavaScript;

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

            //Check to see if appSetting PreflightInstalled is true or even present
            string installAppSetting = WebConfigurationManager.AppSettings[KnownStrings.AppSettingKey];

            if (!installAppSetting.HasValue() || installAppSetting != true.ToString())
            {
                //Add Content dashboard XML
                Installer.AddSettingsSectionDashboard();

                //All done installing our custom stuff
                //As we only want this to run once - not every startup of Umbraco
                Configuration webConfig = WebConfigurationManager.OpenWebConfiguration("/");
                webConfig.AppSettings.Settings.Add(KnownStrings.AppSettingKey, true.ToString());
                webConfig.Save();
            }

            ServerVariablesParser.Parsing += ServerVariablesParser_Parsing;
            ContentService.Saving += ContentService_Saving;
        }

        public void Terminate()
        {

        }

        /// <summary>
        /// Add workflow-specific values to the servervariables dictionary
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="dictionary"></param>
        private static void ServerVariablesParser_Parsing(object sender, Dictionary<string, object> dictionary)
        {
            var urlHelper = new System.Web.Mvc.UrlHelper(new RequestContext(new HttpContextWrapper(HttpContext.Current), new RouteData()));

            dictionary.Add("preflight", new Dictionary<string, object>
            {
                { "contentFailedChecks", KnownStrings.ContentFailedChecks },
                { "apiPath", urlHelper.GetUmbracoApiServiceBaseUrl<Api.ApiController>(controller => controller.GetSettings()) }
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

            int onSave = Convert.ToInt32(settings.First(s => s.Alias == KnownSettings.BindSaveHandler.Camel()).Value);
            if (onSave == 0) return;

            IContent content = e.SavedEntities.First();

            // perform autoreplace before readability check
            // only do this in save handler as there's no point in updating if it's not being saved (potentially)
            if (settings.Any(s => s.Alias == KnownSettings.RunAutoreplace.Camel() && s.Value.ToString() == "1"))
            {
                content = _contentChecker.Autoreplace(content);
            }

            PreflightResponseModel result = _contentChecker.Check(content);

            // at least one property on the current document fails the preflight check
            if (result.Failed == false) return;

            // these values are retrieved in the notifications handler, and passed down to the client
            HttpContext.Current.Items["PreflightResponse"] = result;
            HttpContext.Current.Items["PreflightNodeId"] = content.Id;

            int cancelOnFail = Convert.ToInt32(settings.First(s => s.Alias == KnownSettings.CancelSaveOnFail.Camel()).Value);
            if (cancelOnFail == 1)
            {
                e.Cancel = true;
            }
        }
    }
}
