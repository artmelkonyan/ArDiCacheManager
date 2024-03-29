﻿using ArDiCacheManager;
using ArDiCacheManager.MemoryCache;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestConsoleApp.Framework
{
    class Program
    {
        static void Main(string[] args)
        {
            var cache = new Microsoft.Extensions.Caching.Memory.MemoryCache(new Microsoft.Extensions.Caching.Memory.MemoryCacheOptions());

            IArDiCacheManager cacheManager = new ArDiMemoryCacheManager(cache);
            var strKey = "mycacheitem";
            var key = new CacheKey(strKey);
            var result = cacheManager.GetOrAdd(strKey, () =>
            {
                return "Hello from cacge";
            });

            var result2 = cacheManager.GetOrAdd(strKey, () =>
            {
                return "Hello from cacge";
            });
        }
    }
}
