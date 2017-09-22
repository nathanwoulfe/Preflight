using System.Web.Hosting;
using System.Xml;

namespace Preflight.Helpers
{
    public class Installer
    {
        public void AddSettingsSectionDashboard()
        {
            bool saveFile = false;
            var dashboardPath = "~/config/dashboard.config";

            //Path to the file resolved
            var dashboardFilePath = HostingEnvironment.MapPath(dashboardPath);

            //Load settings.config XML file
            XmlDocument dashboardXml = new XmlDocument();
            dashboardXml.Load(dashboardFilePath);

            XmlNode firstTab = dashboardXml.SelectSingleNode("//section [@alias='StartupSettingsDashboardSection']/areas");

            if (firstTab != null)
            {
                var xmlToAdd = "<tab caption='Preflight'>" +
                                    "<control addPanel='true' panelCaption=''>/app_plugins/preflight/views/settings.dashboard.html</control>" +
                                "</tab>";

                //Load in the XML string above
                XmlDocumentFragment frag = dashboardXml.CreateDocumentFragment();
                frag.InnerXml = xmlToAdd;

                //Append the xml above to the dashboard node
                dashboardXml.SelectSingleNode("//section [@alias='StartupSettingsDashboardSection']").InsertAfter(frag, firstTab);

                //Save the file flag to true
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