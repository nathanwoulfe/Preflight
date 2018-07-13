using System.Web.Hosting;
using System.Xml;

namespace Preflight.Startup
{
    public class Uninstaller
    {
        /// <summary>
        /// Removes the XML for the Section Dashboard from the XML file
        /// </summary>
        public static void RemoveSettingsSectionDashboard()
        {
            var saveFile = false;

            //Open up language file
            //umbraco/config/lang/en.xml
            const string dashboardPath = "~/config/dashboard.config";

            //Path to the file resolved
            string dashboardFilePath = HostingEnvironment.MapPath(dashboardPath);

            //Load settings.config XML file
            var dashboardXml = new XmlDocument();
            dashboardXml.Load(dashboardFilePath);

            // Dashboard Root Node
            // <dashboard>
            XmlNode dashboardNode = dashboardXml.SelectSingleNode("//dashBoard");          

            XmlNode contentTab = dashboardNode.SelectSingleNode("//tab[@caption='Preflight']");

            if (contentTab != null)
            {
                contentTab.ParentNode.RemoveChild(contentTab);
                saveFile = true;
            }

            //If saveFile flag is true then save the file
            if (saveFile)
            {
                //Save the XML file
                dashboardXml.Save(dashboardFilePath);
            }
        }
    }
}