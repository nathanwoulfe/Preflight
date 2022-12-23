using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Text.RegularExpressions;

namespace Preflight.Extensions;

public static class StringExtensions
{
    private static readonly Regex _alphaNumericOnly = new("[^a-zA-Z0-9 ]");

    /// <summary>
    /// Is the string not null and not empty?
    /// </summary>
    /// <param name="str"></param>
    /// <returns></returns>
    public static bool HasValue([NotNullWhen(true)] this string? str) => !string.IsNullOrEmpty(str);

    /// <summary>
    /// camelCaseTheString
    /// </summary>
    /// <param name="str"></param>
    /// <returns></returns>
    public static string Camel(this string str)
    {
        str = _alphaNumericOnly.Replace(str, string.Empty);

        // to count spaces
        int cnt = 0;
        int n = str.Length;
        int res_ind = 0;
        char[] ch = str.ToCharArray();

        for (int i = 0; i < n; i++)
        {
            // check for spaces in the sentence
            if (ch[i] == ' ')
            {
                cnt++;

                // conversion into upper case
                ch[i + 1] = char.ToUpper(ch[i + 1]);
                continue;
            }

            // If not space, copy character
            else
            {
                ch[res_ind++] = ch[i];
            }
        }

        // new string will be resuced by the
        // size of spaces in the original string
        StringBuilder result = new();

        for (int i = 0; i < n - cnt; i++)
        {
            _ = result.Append(ch[i]);
        }

        return result.ToString().ToLowerCasedFirstLetter();
    }

    /// <summary>
    /// Appends 'on save only' and camelCases the given string
    /// </summary>
    /// <param name="str"></param>
    /// <returns></returns>
    public static string OnSaveOnlyAlias(this string str) => $"{str} on save only".Camel();

    /// <summary>
    /// Appends 'Disabled' and camelCases the given string
    /// </summary>
    /// <param name="str"></param>
    /// <returns></returns>
    public static string DisabledAlias(this string str) => $"{str} disabled".Camel();

    /// <summary>
    /// Appends 'properties to test' and camelCases the given string
    /// </summary>
    /// <param name="str"></param>
    /// <returns></returns>
    public static string PropertiesToTestAlias(this string str) => $"{str} properties to test".Camel();

    private static string ToLowerCasedFirstLetter(this string input) =>
        input.Length > 0
            ? $"{input[0].ToString().ToLower()}{input[1..]}"
            : input;
}
