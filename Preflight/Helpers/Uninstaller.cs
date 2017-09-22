using System.Web.Hosting;
using System.Xml;

namespace Preflight.Helpers
{
    public class Uninstaller
    {
        
        /// <summary>
        /// Removes the XML for the Section Dashboard from the XML file
        /// </summary>
        public void RemoveSettingsSectionDashboard()
        {
            bool saveFile = false;

            //Open up language file
            //umbraco/config/lang/en.xml
            var dashboardPath = "~/config/dashboard.config";

            //Path to the file resolved
            var dashboardFilePath = HostingEnvironment.MapPath(dashboardPath);

            //Load settings.config XML file
            XmlDocument dashboardXml = new XmlDocument();
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
                saveFile = false;
            }
        }
    }
}