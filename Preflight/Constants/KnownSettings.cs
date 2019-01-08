namespace Preflight.Constants
{
    public static class KnownSettings
    {
        public const string BindSaveHandler = "Run Preflight on save";
        public const string CheckLinks = "Check broken links";
        public const string CheckReadability = "Check readability";
        public const string EnsureSafeLinks = "Ensure safe links";
        public const string RunAutoreplace = "Run autoreplace";
        public const string CancelSaveOnFail = "Cancel save when Preflight tests fail";
        public const string ReadabilityMin = "Readability target - minimum";
        public const string ReadabilityMax = "Readability target - maximum";
        public const string LongWordSyllables = "Long word syllable count";
        public const string Whitelist = "Whitelisted words";
        public const string Blacklist = "Blacklisted words";
        public const string AutoreplaceTerms = "Auto-replace terms";
        public const string GoogleApiKey = "Google SafeBrowsing API key";
        public const string HideDisabled = "Hide disabled tests";
    }
}
