using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.Models.Membership;

namespace Preflight;

internal sealed class PreflightContentAppComposer : IComposer
{
    public void Compose(IUmbracoBuilder builder) => _ = builder.ContentApps().Append<PreflightContentApp>();
}

internal sealed class PreflightContentApp : IContentAppFactory
{
    public ContentApp? GetContentAppFor(object source, IEnumerable<IReadOnlyUserGroup> userGroups)
    {
        if (source is IContent content && !content.ContentType.IsElement)
        {
            return new ContentApp
            {
                Alias = KnownStrings.Alias,
                Name = KnownStrings.Name,
                Icon = KnownStrings.Icon,
                View = "/App_Plugins/Preflight/Backoffice/views/app.html",
                Weight = 0,
            };
        }

        return null;
    }
}
