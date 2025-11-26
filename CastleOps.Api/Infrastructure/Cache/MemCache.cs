using Microsoft.Extensions.Caching.Memory;

namespace CastleOps.Api.Infrastructure.Cache
{
    public class MemCache<T> where T : class
    {
        private readonly IMemoryCache _memoryCache;
        private readonly IConfiguration _configuration;

        private int _absoluteExpirationMinutes { get; set; } = 60;
        private int _slidingExpirationMinutes { get; set; } = 10;

        public MemCache(IMemoryCache memoryCache, IConfiguration configuration)
        {
            _memoryCache = memoryCache;
            _configuration = configuration;

            _absoluteExpirationMinutes = _configuration.GetValue<int>("Cache:AbsoluteExpirationMinutes");
            _slidingExpirationMinutes = _configuration.GetValue<int>("Cache:SlidingExpirationMinutes");
        }

        public void SetCachedObject(string key, T obj)
        {
            if (obj is null)
            {
                throw new ArgumentNullException(nameof(obj), "Cannot cache a null object.");
            }

            _memoryCache.Set(key, obj, new MemoryCacheEntryOptions()
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(_absoluteExpirationMinutes),
                SlidingExpiration = TimeSpan.FromMinutes(_slidingExpirationMinutes)
            });
        }

        public bool IsCached(string key)
        {
            return _memoryCache.TryGetValue<T>(key, out var cachedObject) && cachedObject is not null;
        }

        public T? GetCachedObject(string key)
        {
            if (_memoryCache.TryGetValue<T>(key, out var cachedObject) && cachedObject is not null)
            {
                return cachedObject;
            }
            else
            {
                return null;
            }
        }

        public void RemoveCachedObject(string key)
        {
            _memoryCache.Remove(key);
        }
    }
}