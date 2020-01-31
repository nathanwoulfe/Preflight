using System.Linq;

namespace Preflight.Constants
{
    public static class KnownPropertyAlias
    {
        public const string Grid = Umbraco.Core.Constants.PropertyEditors.Aliases.Grid;
        public const string Rte = Umbraco.Core.Constants.PropertyEditors.Aliases.TinyMce;
        public const string Textarea = Umbraco.Core.Constants.PropertyEditors.Aliases.TextArea;
        public const string Textbox = Umbraco.Core.Constants.PropertyEditors.Aliases.TextBox;
        public const string NestedContent = Umbraco.Core.Constants.PropertyEditors.Aliases.NestedContent;

        public static string All = string.Join(",", typeof(KnownPropertyAlias).GetFields()
                .Where(x => x.IsLiteral).Select(x => x.GetRawConstantValue().ToString()));
    }
}
