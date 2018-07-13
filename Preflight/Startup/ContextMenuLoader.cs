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

        private static void TreeControllerBase_MenuRendering(TreeControllerBase sender, MenuRenderingEventArgs e)
        {
            if (e.Menu == null || sender.TreeAlias != Constants.Trees.Content) return;

            var i = new MenuItem("preflightCheck", "Preflight check");
            i.LaunchDialogView("/App_Plugins/Preflight/backoffice/Views/check.dialog.html", "Preflight check");
            i.Icon = "paper-plane";

            e.Menu.Items.Add(i);
        }
    }
}
