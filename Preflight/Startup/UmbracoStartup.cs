using System.Collections.Generic;
using System.Configuration;
using System.Web;
using System.Web.Configuration;
using System.Web.Mvc;
using System.Web.Routing;
using Preflight.Api;
using umbraco.cms.businesslogic.packager;
using Umbraco.Core;
using Umbraco.Web;
using Umbraco.Web.UI.JavaScript;

using Constants = Preflight.Helpers.Constants;

namespace Preflight.Startup
{
    public class UmbracoStartup : ApplicationEventHandler
    {
        protected override void ApplicationStarted(UmbracoApplicationBase umbracoApplication, ApplicationContext context)
        {
            //Check to see if appSetting AnalyticsStartupInstalled is true or even present
            string installAppSetting = WebConfigurationManager.AppSettings[Constants.AppSettingKey];

            if (string.IsNullOrEmpty(installAppSetting) || installAppSetting != true.ToString())
            {
                //Add Content dashboard XML
                Installer.AddSettingsSectionDashboard();

                //All done installing our custom stuff
                //As we only want this to run once - not every startup of Umbraco
                Configuration webConfig = WebConfigurationManager.OpenWebConfiguration("/");
                webConfig.AppSettings.Settings.Add(Constants.AppSettingKey, true.ToString());
                webConfig.Save();
            }

            //Add OLD Style Package Event
            InstalledPackage.BeforeDelete += InstalledPackage_BeforeDelete;

            ServerVariablesParser.Parsing += ServerVariablesParser_Parsing;
        }

        /// <summary>
        /// Add workflow-specific values to the servervariables dictionary
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="dictionary"></param>
        private void ServerVariablesParser_Parsing(object sender, Dictionary<string, object> dictionary)
        {
            var urlHelper = new UrlHelper(new RequestContext(new HttpContextWrapper(HttpContext.Current), new RouteData()));

            dictionary.Add("preflight", new Dictionary<string, object>
            {
                { "contentFailedChecks", Constants.ContentFailedChecks },
                { "baseApiPath", urlHelper.GetUmbracoApiServiceBaseUrl<ApiController>(controller => controller.GetSettings()) }
            });
        }

        /// <summary>
        /// Uninstall Package - Before Delete (Old style events, no V6/V7 equivelant)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void InstalledPackage_BeforeDelete(InstalledPackage sender, System.EventArgs e)
        {
            //Check which package is being uninstalled
            if (sender.Data.Name != Constants.Name) return;

            //Start Uninstall - clean up process...
            Uninstaller.RemoveSettingsSectionDashboard();

            //Remove AppSetting key when all done
            Configuration webConfig = WebConfigurationManager.OpenWebConfiguration("/");
            webConfig.AppSettings.Settings.Remove(Constants.AppSettingKey);
            webConfig.Save();
        }
    }
}
