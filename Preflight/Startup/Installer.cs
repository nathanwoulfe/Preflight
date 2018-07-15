using System.Web.Hosting;
using System.Xml;

namespace Preflight.Startup
{
    public static class Installer
    {
        public static void AddSettingsSectionDashboard()
        {
            const string dashboardPath = "~/config/dashboard.config";

            //Path to the file resolved
            string dashboardFilePath = HostingEnvironment.MapPath(dashboardPath);
            if (string.IsNullOrEmpty(dashboardFilePath)) return;

            //Load settings.config XML file
            var dashboardXml = new XmlDocument();
            dashboardXml.Load(dashboardFilePath);

            XmlNode findSection = dashboardXml.SelectSingleNode("//section [@alias='PreflightDashboardSection']");

            // exit if section exists
            if (findSection != null) return;

            //Let's add the xml
            const string xmlToAdd = "<section alias='PreflightDashboardSection'>" +
                                    "<areas>" +
                                    "<area>settings</area>" +
                                    "</areas>" +
                                    "<tab caption=\"Preflight\">" +
                                    "<control>../app_plugins/preflight/backoffice/views/settings.dashboard.html</control>" +
                                    "</tab>" +
                                    "</section>";

            //Get the main root <dashboard> node
            XmlNode dashboardNode = dashboardXml.SelectSingleNode("//dashBoard");

            if (dashboardNode != null)
            {
                //Load in the XML string above
                var xmlNodeToAdd = new XmlDocument();
                xmlNodeToAdd.LoadXml(xmlToAdd);

                XmlNode toAdd = xmlNodeToAdd.SelectSingleNode("*");

                //Append the xml above to the dashboard node
                if (toAdd != null && dashboardNode.OwnerDocument != null)
                {
                    dashboardNode.AppendChild(dashboardNode.OwnerDocument.ImportNode(toAdd, true));
                }

                dashboardXml.Save(dashboardFilePath);
            }
        }        
    }
}