using System.Web.Hosting;
using System.Xml;

namespace Preflight.Startup
{
    public static class Uninstaller
    {
        /// <summary>
        /// Removes the XML for the Section Dashboard from the XML file
        /// </summary>
        public static void RemoveSettingsSectionDashboard()
        {
            const string dashboardPath = "~/config/dashboard.config";

            //Path to the file resolved
            string dashboardFilePath = HostingEnvironment.MapPath(dashboardPath);
            if (dashboardFilePath == null) return;

            //Load settings.config XML file
            var dashboardXml = new XmlDocument();
            dashboardXml.Load(dashboardFilePath);

            // Dashboard Root Node
            // <dashboard>
            XmlNode dashboardNode = dashboardXml.SelectSingleNode("//dashBoard");          

            XmlNode contentTab = dashboardNode?.SelectSingleNode("//tab[@caption='Preflight']");

            if (contentTab == null) return;

            contentTab.ParentNode?.RemoveChild(contentTab);
            //Save the XML file
            dashboardXml.Save(dashboardFilePath);
        }
    }
}