using System.Collections.Generic;
#if NETCOREAPP
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.Models.Membership;
#else
using Umbraco.Core.Composing;
using Umbraco.Core.Models;
using Umbraco.Core.Models.ContentEditing;
using Umbraco.Core.Models.Membership;
using Umbraco.Web;
#endif

namespace Preflight
{
#if NETCOREAPP
    public class PreflightContentAppComposer : IComposer
    {
        public void Compose(IUmbracoBuilder builder)
        {
            builder.ContentApps().Append<PreflightContentApp>();
        }
    }
#else
    public class PreflightContentAppComposer : IUserComposer
    {
        public void Compose(Composition composition)
        {
            composition.ContentApps().Append<PreflightContentApp>();
        }
    }
#endif

    public class PreflightContentApp : IContentAppFactory
    {
        public ContentApp GetContentAppFor(object source, IEnumerable<IReadOnlyUserGroup> userGroups)
        {
            if (source is IContent content && !content.ContentType.IsElement)
            {
                var app = new ContentApp
                {
                    Alias = KnownStrings.Alias,
                    Name = KnownStrings.Name,
                    Icon = KnownStrings.Icon,
                    View = "/App_Plugins/Preflight/Backoffice/views/app.html",
                    Weight = 0
                };
                return app;
            }
            return null;
        }
    }
}
