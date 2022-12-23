using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Trees;
using Umbraco.Cms.Web.BackOffice.Trees;
using Umbraco.Cms.Web.Common.Attributes;
using Umbraco.Cms.Web.Common.ModelBinders;
using UmbConstants = Umbraco.Cms.Core.Constants;

namespace Preflight.Controllers;

[Tree(UmbConstants.Applications.Settings, KnownStrings.Alias, SortOrder = 20, TreeGroup = UmbConstants.Trees.Groups.ThirdParty)]
[PluginController(KnownStrings.Name)]
public class PreflightTreeController : TreeController
{
    public PreflightTreeController(
        ILocalizedTextService textService,
        UmbracoApiControllerTypeCollection umbracoApiControllerTypeCollection,
        IEventAggregator eventAggregator)
        : base(textService, umbracoApiControllerTypeCollection, eventAggregator)
    {
    }

    /// <inheritdoc/>
    protected override ActionResult<TreeNode?> CreateRootNode(FormCollection queryStrings)
    {
        ActionResult<TreeNode?> rootResult = base.CreateRootNode(queryStrings);

        if (rootResult.Result is not null)
        {
            return rootResult;
        }

        TreeNode? root = rootResult.Value;

        if (root is null)
        {
            return root;
        }

        root.RoutePath = $"{UmbConstants.Applications.Settings}/{KnownStrings.Alias}/overview";
        root.Icon = KnownStrings.Icon;
        root.HasChildren = false;
        root.MenuUrl = null;

        return root;
    }

    /// <inheritdoc/>
    protected override ActionResult<MenuItemCollection> GetMenuForNode(string id, [ModelBinder(typeof(HttpQueryStringModelBinder))] FormCollection queryStrings) =>
        throw new NotImplementedException();

    /// <inheritdoc/>
    protected override ActionResult<TreeNodeCollection> GetTreeNodes(string id, [ModelBinder(typeof(HttpQueryStringModelBinder))] FormCollection queryStrings) =>
        throw new NotImplementedException();
}
