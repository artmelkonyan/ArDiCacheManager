using ArDiCacheManager;
using ArDiCacheManager.MemoryCache;
using System;

namespace TestConsoleApp
{
    class Program
    {
        static void Main(string[] args)
        {
            var cache = new Microsoft.Extensions.Caching.Memory.MemoryCache(new Microsoft.Extensions.Caching.Memory.MemoryCacheOptions());

            IArDiCacheManager cacheManager = new ArDiMemoryCacheManager(cache);
            var key = new CacheKey("mycacheitem");
            var result = cacheManager.Get<string>(key, () =>
            {

                return "Hello from cacge";
            });

            var result2 = cacheManager.Get<string>(key, () =>
            {
                return "Hello from cacge";
            });
        }
    }
}
