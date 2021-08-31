using System.Collections.Generic;
using System.Linq;
#if NETCOREAPP
using Umbraco.Cms.Core.Models;
#else
using Umbraco.Core.Models;
using IProperty = Umbraco.Core.Models.Property;
#endif

namespace Preflight.Extensions
{
    public static class ContentBaseExtensions
    {
        public static IEnumerable<IProperty> GetPreflightProperties(this IContentBase content)
        {
            return content.Properties
                .Where(p => p.PropertyType.PropertyEditorAlias == KnownPropertyAlias.Grid ||
                            p.PropertyType.PropertyEditorAlias == KnownPropertyAlias.BlockList ||
                            p.PropertyType.PropertyEditorAlias == KnownPropertyAlias.NestedContent ||
                            p.PropertyType.PropertyEditorAlias == KnownPropertyAlias.Textbox ||
                            p.PropertyType.PropertyEditorAlias == KnownPropertyAlias.Textarea ||
                            p.PropertyType.PropertyEditorAlias == KnownPropertyAlias.Rte);
        }
    }
}
