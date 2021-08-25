using Newtonsoft.Json;
using System.Collections.Generic;

namespace Preflight.Extensions
{
    public static class ObjectExtensions
    {
        public static Dictionary<string, string> ToVariantDictionary(this object obj)
        {
            if (obj is IDictionary<string, string>)
                return (Dictionary<string, string>)obj;

            if (obj == null)
                return new Dictionary<string, string>();

            return JsonConvert.DeserializeObject<Dictionary<string, string>>(obj.ToString());
        }

        public static string ForVariant(this object obj, string culture)
        {
            return obj.ToVariantDictionary()[culture].ToString();
        }
    }
}
