using Newtonsoft.Json;

namespace Preflight.Extensions;

public static class ObjectExtensions
{
    public static Dictionary<string, string>? ToVariantDictionary(this object obj)
    {
        if (obj is IDictionary<string, string>)
        {
            return (Dictionary<string, string>)obj;
        }

        if (obj is null)
        {
            return new();
        }

        string? objectString = obj.ToString();
        if (string.IsNullOrEmpty(objectString))
        {
            return new();
        }

        return JsonConvert.DeserializeObject<Dictionary<string, string>>(objectString);
    }

    public static string? ForVariant(this object obj, string culture) =>
        obj.ToVariantDictionary()?[culture].ToString();
}
