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
            var strKey = "mycacheitem sdsd";
            var key = new CacheKey(strKey);
            var result = cacheManager.Get(strKey, () =>
            {
                return "Hello from cacge";
            });

            var result2 = cacheManager.Get(strKey, () =>
            {
                return "Hello from cacge";
            });
        }
    }
}
