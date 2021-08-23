using System;
using System.Runtime.Caching;

namespace Preflight.Services.Implement
{
    public class CacheManager : ICacheManager
    {
        /// <summary>
        /// Convenience method for getting from memory cache
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool TryGet<T>(string key, out T value)
        {
            value = default;

            MemoryCache cache = MemoryCache.Default;
            if (cache[key] != null)
            {
                value = (T)cache.Get(key);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Convenience method for pushing to mem cache
        /// </summary>
        /// <param name="key"></param>
        /// <param name="thing"></param>
        public void Set(string key, object thing) =>
            MemoryCache.Default.Set(key, thing,
                new CacheItemPolicy { AbsoluteExpiration = DateTime.Now.AddHours(24) });
    }
}
