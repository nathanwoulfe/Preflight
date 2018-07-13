namespace Preflight.Helpers
{
    public class Constants
    {
        public const string Name = "Preflight";
        public const string AppSettingKey = "PreflightInstalled";

        public const string ContentFailedChecks = "Content failed Preflight checks";

        public const string RteJsonPath = "$..controls[?(@.editor.alias == 'rte')]";
        public const string RteValueJsonPath = "$..value";
        public const string ArchetypeRteJsonPath = "$..[?(@.propertyEditorAlias == 'Umbraco.TinyMCEv3')]";

        public const string ClosingHtmlTags = @"(<\/li>|<\/h[1-6]{1}>)";
        public const string CharsToRemove = @"(<.*?>|\(|\)|\[|\]|,|\w'|'\w|\w""|""\w)";
        public const string DuplicateSpaces = @"\s+";
        public const string NewLine = "\n";

        public const string HrefXPath = "//a[@href]";

        public const string CacheKey = "Preflight_SafeBrowsing_";

        public readonly char[] Vowels = { 'a', 'e', 'i', 'o', 'u', 'y' };
        public readonly char[] WordDelimiters = { '.', '!', '?', ':', ';' };
        public readonly string[] Endings = { "es", "ed" };

        public const string ApiPath = "/preflight/api/";
        public const string SettingsFilePath = "~/App_Plugins/Preflight/backoffice/settings.json";

    }
}
