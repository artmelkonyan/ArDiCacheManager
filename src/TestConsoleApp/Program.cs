using ArDiCacheManager;
using ArDiCacheManager.MemoryCache;
using ArDiCacheManager.Redis;
using System;

namespace TestConsoleApp
{
    class Program
    {
        static void Main(string[] args)
        {
            string connection = "127.0.0.1:6379,ssl=False";
            IRedisConnectionWrapper redisConnectionWrapper = new RedisConnectionWrapper(connection);

            IArDiCacheManager arDiCacheManager = new RedisCacheManager(redisConnectionWrapper, false);

            var strKey = "mycacheitem";
            var key = new CacheKey(strKey);
            var result = arDiCacheManager.GetOrAdd(strKey, () =>
            {
                return "Hello from cacge";
            });

            Console.WriteLine(result);

            var result2 = arDiCacheManager.GetOrAdd(strKey, () =>
            {
                return "Hello from cacge";
            });

            Console.WriteLine(result2);

            Console.ReadLine();

        }

        //   var cache = new Microsoft.Extensions.Caching.Memory.MemoryCache(new Microsoft.Extensions.Caching.Memory.MemoryCacheOptions());



        //    IArDiCacheManager cacheManager = new ArDiMemoryCacheManager(cache);
        //    var strKey = "mycacheitem";
        //    var key = new CacheKey(strKey);
        //    var result = cacheManager.GetOrAdd(strKey, () =>
        //    {
        //        return "Hello from cacge";
        //    });

        //    var result2 = cacheManager.GetOrAdd(strKey, () =>
        //    {
        //        return "Hello from cacge";
        //    });
        //}
    }
}
