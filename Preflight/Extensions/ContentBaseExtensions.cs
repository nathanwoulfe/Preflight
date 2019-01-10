using System.Collections.Generic;
using System.Linq;
using Preflight.Constants;
using Umbraco.Core.Models;

namespace Preflight.Extensions
{
    public static class ContentBaseExtensions
    {
        public static IEnumerable<Property> GetPreflightProperties(this IContentBase content)
        {
            return content.Properties
                .Where(p => p.PropertyType.PropertyEditorAlias == KnownPropertyAlias.Grid ||
                            p.PropertyType.PropertyEditorAlias == KnownPropertyAlias.Archetype ||
                            p.PropertyType.PropertyEditorAlias == KnownPropertyAlias.Rte);
        }
    }
}
