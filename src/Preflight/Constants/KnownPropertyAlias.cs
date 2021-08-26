using System.Collections.Generic;
using System.Linq;
#if NETCOREAPP 
using UmbConstants = Umbraco.Cms.Core.Constants;
#else 
using UmbConstants = Umbraco.Core.Constants;
#endif

namespace Preflight
{
    public static class KnownPropertyAlias
    {
        public const string Grid = UmbConstants.PropertyEditors.Aliases.Grid;
        public const string Rte = UmbConstants.PropertyEditors.Aliases.TinyMce;
        public const string Textarea = UmbConstants.PropertyEditors.Aliases.TextArea;
        public const string Textbox = UmbConstants.PropertyEditors.Aliases.TextBox;
        public const string NestedContent = UmbConstants.PropertyEditors.Aliases.NestedContent;
        public const string BlockList = UmbConstants.PropertyEditors.Aliases.BlockList;

        public static IEnumerable<string> All = typeof(KnownPropertyAlias).GetFields()
                .Where(x => x.IsLiteral).Select(x => x.GetRawConstantValue().ToString());
    }
}
