namespace Preflight.Plugins.Readability;

public static class Constants
{
    public static readonly char[] Vowels = { 'a', 'e', 'i', 'o', 'u', 'y' };
    public static readonly char[] WordDelimiters = { '.', '!', '?', ':', ';' };
    public static readonly string[] Endings = { "es", "ed" };

    public const string ClosingHtmlTags = @"(<\/li>|<\/h[1-6]{1}>)";
    public const string CharsToRemove = @"(<.*?>|\(|\)|\[|\]|,|\w'|'\w|\w""|""\w)";
    public const string DuplicateSpaces = @"\s+";
    public const string Ied = "ied";
    public const string Space = " ";
    public const string Period = ".";
    public const string Ampersand = "&";
    public const string AmpersandEntity = "&amp;";

    public const char l = 'l';
    public const char e = 'e';
}
