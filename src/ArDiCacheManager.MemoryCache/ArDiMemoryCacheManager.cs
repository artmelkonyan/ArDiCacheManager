﻿using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Primitives;

namespace ArDiCacheManager.MemoryCache
{
    public class ArDiMemoryCacheManager: IArDiCacheManager
    {
        #region Fields

        // Flag: Has Dispose already been called?
        private bool _disposed;

        private readonly IMemoryCache _memoryCache;

        private static readonly ConcurrentDictionary<string, CancellationTokenSource> _prefixes = new ConcurrentDictionary<string, CancellationTokenSource>();
        private static CancellationTokenSource _clearToken = new CancellationTokenSource();

        #endregion

        #region Ctor

        public ArDiMemoryCacheManager(IMemoryCache memoryCache)
        {
            _memoryCache = memoryCache;
        }

        #endregion

        #region Utilities

        /// <summary>
        /// Prepare cache entry options for the passed key
        /// </summary>
        /// <param name="key">Cache key</param>
        /// <returns>Cache entry options</returns>
        private MemoryCacheEntryOptions PrepareEntryOptions(CacheKey key)
        {
            //set expiration time for the passed cache key
            var options = new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(key.CacheTime)
            };

            //add tokens to clear cache entries
            options.AddExpirationToken(new CancellationChangeToken(_clearToken.Token));
            foreach (var keyPrefix in key.Prefixes.ToList())
            {
                var tokenSource = _prefixes.GetOrAdd(keyPrefix, new CancellationTokenSource());
                options.AddExpirationToken(new CancellationChangeToken(tokenSource.Token));
            }

            return options;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Get a cached item. If it's not in the cache yet, then load and cache it
        /// </summary>
        /// <typeparam name="T">Type of cached item</typeparam>
        /// <param name="key">Cache key</param>
        /// <param name="acquire">Function to load item if it's not in the cache yet</param>
        /// <returns>The cached value associated with the specified key</returns>
        [Obsolete("Use GetOrAdd<T> instead of  Get<T>")]
        public T Get<T>(CacheKey key, Func<T> acquire)
        {
            if (key.CacheTime <= 0)
                return acquire();

            var result = _memoryCache.GetOrCreate(key.Key, entry =>
            {
                entry.SetOptions(PrepareEntryOptions(key));

                return acquire();
            });

            //do not cache null value
            //if (result == null)
            //    Remove(key);

            return result;
        }

        /// <summary>
        /// Get a cached item. If it's not in the cache yet, then load and cache it
        /// </summary>
        /// <typeparam name="T">Type of cached item</typeparam>
        /// <param name="key">Cache key</param>
        /// <param name="acquire">Function to load item if it's not in the cache yet</param>
        /// <returns>The cached value associated with the specified key</returns>
        public T GetOrAdd<T>(CacheKey key, Func<T> acquire)
        {
            if (key.CacheTime <= 0)
                return acquire();

            var result = _memoryCache.GetOrCreate(key.Key, entry =>
            {
                entry.SetOptions(PrepareEntryOptions(key));

                return acquire();
            });

            //do not cache null value
            //if (result == null)
            //    Remove(key);

            return result;
        }


        /// <summary>
        /// Removes the value with the specified key from the cache
        /// </summary>
        /// <param name="key">Key of cached item</param>
        public void Remove(CacheKey key)
        {
            _memoryCache.Remove(key.Key);
        }

        /// <summary>
        /// Get a cached item. If it's not in the cache yet, then load and cache it
        /// </summary>
        /// <typeparam name="T">Type of cached item</typeparam>
        /// <param name="key">Cache key</param>
        /// <param name="acquire">Function to load item if it's not in the cache yet</param>
        /// <returns>The cached value associated with the specified key</returns>
        [Obsolete("Use GetOrAddAsync<T> instead of  GetAsync<T>")]
        public async Task<T> GetAsync<T>(CacheKey key, Func<Task<T>> acquire)
        {
            if (key.CacheTime <= 0)
                return await acquire();

            var result = await _memoryCache.GetOrCreateAsync(key.Key, async entry =>
            {
                entry.SetOptions(PrepareEntryOptions(key));

                return await acquire();
            });

            //do not cache null value
            if (result == null)
                Remove(key);

            return result;
        }


        /// <summary>
        /// Get a cached item. If it's not in the cache yet, then load and cache it
        /// </summary>
        /// <typeparam name="T">Type of cached item</typeparam>
        /// <param name="key">Cache key</param>
        /// <param name="acquire">Function to load item if it's not in the cache yet</param>
        /// <returns>The cached value associated with the specified key</returns>
        public async Task<T> GetOrAddAsync<T>(CacheKey key, Func<Task<T>> acquire)
        {
            if (key.CacheTime <= 0)
                return await acquire();

            var result = await _memoryCache.GetOrCreateAsync(key.Key, async entry =>
            {
                entry.SetOptions(PrepareEntryOptions(key));

                return await acquire();
            });

            //do not cache null value
            if (result == null)
                Remove(key);

            return result;
        }

        /// <summary>
        /// Adds the specified key and object to the cache
        /// </summary>
        /// <param name="key">Key of cached item</param>
        /// <param name="data">Value for caching</param>
        public void Set(CacheKey key, object data)
        {
            if (key.CacheTime <= 0 || data == null)
                return;

            _memoryCache.Set(key.Key, data, PrepareEntryOptions(key));
        }

        /// <summary>
        /// Gets a value indicating whether the value associated with the specified key is cached
        /// </summary>
        /// <param name="key">Key of cached item</param>
        /// <returns>True if item already is in cache; otherwise false</returns>
        public bool IsSet(CacheKey key)
        {
            return _memoryCache.TryGetValue(key.Key, out _);
        }



        /// <summary>
        /// Removes items by key prefix
        /// </summary>
        /// <param name="prefix">String key prefix</param>
        public void RemoveByPrefix(string prefix)
        {
            _prefixes.TryRemove(prefix, out var tokenSource);
            tokenSource?.Cancel();
            tokenSource?.Dispose();
        }

        /// <summary>
        /// Removes the value with the specified key from the cache
        /// </summary>
        /// <param name="key">Key of cached item</param>
        public void Remove(string key)
        {
            _memoryCache.Remove(key);
        }

        /// <summary>
        /// Get a cached item. If it's not in the cache yet, then load and cache it
        /// </summary>
        /// <typeparam name="T">Type of cached item</typeparam>
        /// <param name="key">Cache key</param>
        /// <param name="acquire">Function to load item if it's not in the cache yet</param>
        /// <returns>The cached value associated with the specified key</returns>        
        [Obsolete("Use GetOrAdd<T> instead of  Get<T>")]
        public T Get<T>(string key, Func<T> acquire)
        {
            return Get(new CacheKey(key), acquire);
        }

        /// <summary>
        /// Get a cached item. If it's not in the cache yet, then load and cache it
        /// </summary>
        /// <typeparam name="T">Type of cached item</typeparam>
        /// <param name="key">Cache key</param>
        /// <param name="acquire">Function to load item if it's not in the cache yet</param>
        /// <returns>The cached value associated with the specified key</returns>                
        public T GetOrAdd<T>(string key, Func<T> acquire)
        {
            return GetOrAdd(new CacheKey(key), acquire);
        }

        /// <summary>
        /// Get a cached item. If it's not in the cache yet, then load and cache it
        /// </summary>
        /// <typeparam name="T">Type of cached item</typeparam>
        /// <param name="key">Cache key</param>
        /// <param name="acquire">Function to load item if it's not in the cache yet</param>
        /// <returns>The cached value associated with the specified key</returns>
        [Obsolete("Use GetOrAddAsync<T> instead of  GetAsync<T>")]
        public Task<T> GetAsync<T>(string key, Func<Task<T>> acquire)
        {
            return GetAsync(new CacheKey(key), acquire);
        }

        /// <summary>
        /// Get a cached item. If it's not in the cache yet, then load and cache it
        /// </summary>
        /// <typeparam name="T">Type of cached item</typeparam>
        /// <param name="key">Cache key</param>
        /// <param name="acquire">Function to load item if it's not in the cache yet</param>
        /// <returns>The cached value associated with the specified key</returns>        
        public Task<T> GetOrAddAsync<T>(string key, Func<Task<T>> acquire)
        {
            return GetOrAddAsync(new CacheKey(key), acquire);
        }

        /// <summary>
        /// Adds the specified key and object to the cache
        /// </summary>
        /// <param name="key">Key of cached item</param>
        /// <param name="data">Value for caching</param>
        public void Set(string key, object data)
        {
            Set(new CacheKey(key), data);
        }

        /// <summary>
        /// Gets a value indicating whether the value associated with the specified key is cached
        /// </summary>
        /// <param name="key">Key of cached item</param>
        /// <returns>True if item already is in cache; otherwise false</returns>
        public bool IsSet(string key)
        {
            return IsSet(new CacheKey(key));
        }

        /// <summary>
        /// Clear all cache data
        /// </summary>
        public void Clear()
        {
            _clearToken.Cancel();
            _clearToken.Dispose();

            _clearToken = new CancellationTokenSource();

            foreach (var prefix in _prefixes.Keys.ToList())
            {
                _prefixes.TryRemove(prefix, out var tokenSource);
                tokenSource?.Dispose();
            }
        }

        /// <summary>
        /// Dispose cache manager
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        // Protected implementation of Dispose pattern.
        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            if (disposing)
            {
                _memoryCache.Dispose();
            }

            _disposed = true;
        }      

        #endregion
    }
}
