using Umbraco.Cms.Core.Models;

namespace Preflight.Extensions;

public static class ContentBaseExtensions
{
    public static IEnumerable<IProperty> GetPreflightProperties(this IContentBase content)
    {
        IEnumerable<string?> propertyAliases = KnownPropertyAlias.All;

        return content.Properties
            .Where(p => propertyAliases.Contains(p.PropertyType.PropertyEditorAlias));
    }
}
