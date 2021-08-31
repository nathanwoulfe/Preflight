#if NETCOREAPP
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Web.BackOffice.Trees;
using Umbraco.Cms.Web.Common.Attributes;
using Umbraco.Cms.Web.Common.ModelBinders;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Trees;
using UmbConstants = Umbraco.Cms.Core.Constants;
#else
using System.Net.Http.Formatting;
using System.Web.Http.ModelBinding;
using Umbraco.Web.Models.Trees;
using Umbraco.Web.Mvc;
using Umbraco.Web.Trees;
using Umbraco.Web.WebApi.Filters;
using UmbConstants = Umbraco.Core.Constants;
#endif

namespace Preflight.Controllers
{
    [Tree(UmbConstants.Applications.Settings, 
        treeAlias: KnownStrings.Alias, 
        TreeTitle = KnownStrings.Name, 
        TreeGroup = UmbConstants.Trees.Groups.ThirdParty)]
    [PluginController(KnownStrings.Name)]
    public class PreflightTreeController : TreeController
    {
        private void SetRootNode(TreeNode root)
        {
            root.RoutePath = string.Format("{0}/{1}/overview", UmbConstants.Applications.Settings, KnownStrings.Alias);
            root.Icon = KnownStrings.Icon;
            root.HasChildren = false;
            root.MenuUrl = null;
        }
#if NETCOREAPP
        public PreflightTreeController(ILocalizedTextService textService, UmbracoApiControllerTypeCollection umbracoApiControllerTypeCollection,
            IEventAggregator eventAggregator) : base(textService, umbracoApiControllerTypeCollection, eventAggregator)
        {
        }

        protected override ActionResult<TreeNode> CreateRootNode(FormCollection queryStrings)
        {
            var root = base.CreateRootNode(queryStrings);
            SetRootNode(root.Value);

            return root;
        }

        protected override ActionResult<MenuItemCollection> GetMenuForNode(string id, [ModelBinder(typeof(HttpQueryStringModelBinder))] FormCollection queryStrings) => null;

        protected override ActionResult<TreeNodeCollection> GetTreeNodes(string id, [ModelBinder(typeof(HttpQueryStringModelBinder))] FormCollection queryStrings) => 
            new TreeNodeCollection();
#else
        protected override TreeNode CreateRootNode(FormDataCollection queryStrings)
        {
            TreeNode root = base.CreateRootNode(queryStrings);
            SetRootNode(root);            

            return root;
        }

        protected override MenuItemCollection GetMenuForNode(string id, [ModelBinder(typeof(HttpQueryStringModelBinder))] FormDataCollection queryStrings) => null;

        protected override TreeNodeCollection GetTreeNodes(string id, [ModelBinder(typeof(HttpQueryStringModelBinder))] FormDataCollection queryStrings) =>
            new TreeNodeCollection();

#endif
    }
}
