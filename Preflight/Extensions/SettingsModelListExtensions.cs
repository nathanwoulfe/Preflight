using System;
using System.Collections.Generic;
using System.Linq;
using Preflight.Models;

namespace Preflight.Extensions
{
    public static class SettingsModelListExtensions
    {
        public static bool HasValue(this IEnumerable<SettingsModel> settings, string name, object value)
        {
            return settings.Any(s => s.Alias == name.Camel() && ReferenceEquals(s.Value, value));
        }

        public static T GetValue<T>(this IEnumerable<SettingsModel> settings, string name) where T : IConvertible
        {
            object value = settings.First(s => s.Alias == name.Camel())?.Value;

            // why? because settings are stored as strings, and a string won't convert to bool
            if (typeof(T) == typeof(bool))
            {
                value = Convert.ToInt32(value);
            }

            return value == null ? default(T) : (T) Convert.ChangeType(value, typeof(T));
        }

        public static IEnumerable<TSource> DistinctBy<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector)
        {
            HashSet<TKey> known = new HashSet<TKey>();
            return source.Where(element => known.Add(keySelector(element)));
        }
    }
}
