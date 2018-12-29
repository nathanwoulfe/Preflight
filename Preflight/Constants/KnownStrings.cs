using System.Web;

namespace Preflight.Constants
{
    public static class KnownStrings
    {
        public const string Name = "Preflight";
        public const string AppSettingKey = "PreflightInstalled";

        public const string ContentFailedChecks = "Content failed Preflight checks";

        public const string CheckLinksAlias = "checkLinks";
        public const string CheckReadabilityAlias = "checkReadability";
        public const string CheckSafeBrowsingAlias = "checkSafeBrowsing";
        public const string DoAutoreplace = "doAutoreplace";

        public const string RteJsonPath = "$..controls[?(@.editor.alias == 'rte')]";
        public const string RteValueJsonPath = "$..value";
        public const string ArchetypeRteJsonPath = "$..[?(@.propertyEditorAlias == 'Umbraco.TinyMCEv3')]";

        public const string ClosingHtmlTags = @"(<\/li>|<\/h[1-6]{1}>)";
        public const string CharsToRemove = @"(<.*?>|\(|\)|\[|\]|,|\w'|'\w|\w""|""\w)";
        public const string DuplicateSpaces = @"\s+";
        public const string NewLine = "\n";

        public const string HrefXPath = "//a[@href]";

        public const string CacheKey = "Preflight_SafeBrowsing_";

        public const string SafeBrowsingUrl = "https://safebrowsing.googleapis.com/v4/threatMatches:find?key=";

        public static readonly char[] Vowels = { 'a', 'e', 'i', 'o', 'u', 'y' };
        public static readonly char[] WordDelimiters = { '.', '!', '?', ':', ';' };
        public static readonly string[] Endings = { "es", "ed" };

        public static readonly string SettingsFilePath = HttpContext.Current.Server.MapPath("~/App_Plugins/Preflight/backoffice/settings.json");
    }
}
