using Preflight.Models.Settings;

namespace Preflight.Extensions;

public static class SettingsModelListExtensions
{
    public static T? GetValue<T>(this IEnumerable<SettingsModel> settings, string guid, string culture)
        where T : IConvertible
    {
        if (Guid.TryParse(guid, out Guid guidGuid) == false)
        {
            return default;
        }

        SettingsModel? setting = settings.FirstOrDefault(x => x.Guid == guidGuid);

        if (setting?.Value is null)
        {
            return default;
        }

        if (setting.Value.TryGetValue(culture, out object? value))
        {
            return ConvertObject<T>(value?.ToString());
        }

        return default;
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
