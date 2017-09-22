namespace Preflight
{
    public class MagicStrings
    {
        public const string RteJsonPath = "$..controls[?(@.editor.alias == 'rte')]";
        public const string RteValueJsonPath = "$..value";
        public const string ArchetypeRteJsonPath = "$..[?(@.propertyEditorAlias == 'Umbraco.TinyMCEv3')]";

        public const string ClosingHtmlTags = @"(<\/li>|<\/h[1-6]{1}>)";
        public const string CharsToRemove = @"(<.*?>|\(|\)|\[|\]|,|\w'|'\w|\w""|""\w)";
        public const string DuplicateSpaces = @"\s+";
        public const string NewLine = "\n";

        public const string HrefXPath = "//a[@href]";
    }
}
