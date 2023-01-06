using Umbraco.Cms.Core.Models;

namespace Preflight.Extensions;

public static class ContentBaseExtensions
{
    public static IEnumerable<IProperty> GetPreflightProperties(this IContentBase content) => content.Properties
            .Where(p => p.PropertyType.PropertyEditorAlias == KnownPropertyAlias.Grid ||
                        p.PropertyType.PropertyEditorAlias == KnownPropertyAlias.BlockList ||
                        p.PropertyType.PropertyEditorAlias == KnownPropertyAlias.NestedContent ||
                        p.PropertyType.PropertyEditorAlias == KnownPropertyAlias.Textbox ||
                        p.PropertyType.PropertyEditorAlias == KnownPropertyAlias.Textarea ||
                        p.PropertyType.PropertyEditorAlias == KnownPropertyAlias.Rte);
}
