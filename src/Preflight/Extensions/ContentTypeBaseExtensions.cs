using Umbraco.Cms.Core.Models;

namespace Preflight.Extensions;

public static class ContentTypeBaseExtensions
{
    public static IEnumerable<IPropertyType> GetPreflightProperties(this IContentTypeBase type) => type.PropertyTypes
            .Where(p => p.PropertyEditorAlias == KnownPropertyAlias.Grid ||
                        p.PropertyEditorAlias == KnownPropertyAlias.NestedContent ||
                        p.PropertyEditorAlias == KnownPropertyAlias.Textbox ||
                        p.PropertyEditorAlias == KnownPropertyAlias.Textarea ||
                        p.PropertyEditorAlias == KnownPropertyAlias.BlockList ||
                        p.PropertyEditorAlias == KnownPropertyAlias.Rte);
}
