using Preflight.Models.Settings;

namespace Preflight.Extensions;

public static class SettingsModelListExtensions
{
    public static T? GetValue<T>(this IEnumerable<SettingsModel> settings, string guid, string culture)
        where T : IConvertible
    {
        var guidGuid = new Guid(guid);
        string? stringValue = settings.FirstOrDefault(x => x.Guid == guidGuid)?.Value?[culture]?.ToString();

        return ConvertObject<T>(stringValue);
    }

    private static T? ConvertObject<T>(object? obj)
        where T : IConvertible
    {
        if (typeof(T) == typeof(bool))
        {
            obj = Convert.ToInt32(obj);
        }

        return obj is null ? default : (T)Convert.ChangeType(obj, typeof(T));
    }
}
