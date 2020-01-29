using Preflight.Constants;
using System.Collections.Generic;
using System.Linq;
using Umbraco.Core.Models;

namespace Preflight.Extensions
{
    public static class ContentTypeBaseExtensions
    {
        public static IEnumerable<PropertyType> GetPreflightProperties(this IContentTypeBase type)
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
