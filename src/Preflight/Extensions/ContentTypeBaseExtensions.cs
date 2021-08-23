using Preflight.Constants;
using System.Collections.Generic;
using System.Linq;
#if NET472
using Umbraco.Core.Models;
using IPropertyType = Umbraco.Core.Models.PropertyType;
#else
using Umbraco.Cms.Core.Models;
#endif

namespace Preflight.Extensions
{
    public static class ContentTypeBaseExtensions
    {
        public static IEnumerable<IPropertyType> GetPreflightProperties(this IContentTypeBase type)
        {
            return type.PropertyTypes
                .Where(p => p.PropertyEditorAlias == KnownPropertyAlias.Grid ||
                            p.PropertyEditorAlias == KnownPropertyAlias.NestedContent ||
                            p.PropertyEditorAlias == KnownPropertyAlias.Textbox ||
                            p.PropertyEditorAlias == KnownPropertyAlias.Textarea ||
                            p.PropertyEditorAlias == KnownPropertyAlias.Rte);
        }
    }
}
