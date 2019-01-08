using Umbraco.Core;
using Umbraco.Core.Strings;

namespace Preflight.Extensions
{
    public static class StringExtensions
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static bool HasValue(this string str)
        {
            return !string.IsNullOrEmpty(str);
        }

        /// <summary>
        /// camelCaseTheString
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string Camel(this string str)
        {
            return str.ToCleanString(CleanStringType.CamelCase);
        }
    }
}
