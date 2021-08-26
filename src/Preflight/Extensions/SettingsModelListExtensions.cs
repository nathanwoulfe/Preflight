using Preflight.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Preflight.Extensions
{
    public static class SettingsModelListExtensions
    {
        public static T GetValue<T>(this IEnumerable<SettingsModel> settings, string guid, string culture) where T: IConvertible
        {
            var guidGuid = new Guid(guid);
            var stringValue = settings.FirstOrDefault(x => x.Guid == guidGuid).Value.ForVariant(culture);
            return ConvertObject<T>(stringValue);            
        }

        private static T ConvertObject<T>(object obj) where T : IConvertible
        {
            if (typeof(T) == typeof(bool))
            {
                obj = Convert.ToInt32(obj);
            }

            return obj == null ? default : (T)Convert.ChangeType(obj, typeof(T));
        }
    }
}
