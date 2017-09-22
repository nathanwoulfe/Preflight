using Umbraco.Core;
using Umbraco.Web.Models.Trees;
using Umbraco.Web.Trees;

namespace Preflight.Startup
{
    class ContextMenuLoader : ApplicationEventHandler
    {
        protected override void ApplicationStarted(UmbracoApplicationBase umbracoApplication, ApplicationContext applicationContext)
        {
            TreeControllerBase.MenuRendering += TreeControllerBase_MenuRendering;
        }

        void TreeControllerBase_MenuRendering(TreeControllerBase sender, MenuRenderingEventArgs e)
        {
            if (e.Menu == null) return;
            
            if (sender.TreeAlias == "content")
            {
                var i = new MenuItem("preflightCheck", "Preflight check");
                i.LaunchDialogView("/App_Plugins/Preflight/Views/check.dialog.html", "Preflight check");
                i.Icon = "paper-plane";

                e.Menu.Items.Add(i);
            }            
        }
    }
}
