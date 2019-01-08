using System.Collections.Generic;
using System.Configuration;
using System.Web;
using System.Web.Configuration;
using System.Web.Routing;
using Preflight.Api;
using Preflight.Constants;
using Preflight.Plugins;
using Preflight.Services;
using Preflight.Services.Interfaces;
using Umbraco.Core.Components;
using Umbraco.Core.Composing;
using Umbraco.Web;
using Umbraco.Web.UI.JavaScript;

namespace Preflight.Startup
{
    public class UmbracoStartup : UmbracoComponentBase, IUmbracoUserComponent
    {
        public override void Compose(Composition composition)
        {
            composition.Container.RegisterSingleton<ILinksService, LinksService>();
            composition.Container.RegisterSingleton<IReadabilityService, ReadabilityService>();
            composition.Container.RegisterSingleton<ISafeBrowsingService, SafeBrowsingService>();
            composition.Container.RegisterSingleton<ISettingsService, SettingsService>();

            composition.Container.Register<IPreflightPlugin, PreflightPlugin>();
        }

        public void Initialize()
        {
            //Check to see if appSetting AnalyticsStartupInstalled is true or even present
            string installAppSetting = WebConfigurationManager.AppSettings[KnownStrings.AppSettingKey];

            if (string.IsNullOrEmpty(installAppSetting) || installAppSetting != true.ToString())
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
                { "apiPath", urlHelper.GetUmbracoApiServiceBaseUrl<ApiController>(controller => controller.GetSettings()) }
            });
        }
    }
}
