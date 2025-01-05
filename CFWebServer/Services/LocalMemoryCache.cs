using CFWebServer.Interfaces;

namespace CFWebServer.Services
{
    /// <summary>
    /// Local memory cache. Thread safe.
    /// </summary>
    internal class LocalMemoryCache : ICacheService, IDisposable
    {
        private readonly Dictionary<string, CacheItem> _items = new Dictionary<string, CacheItem>();
        private readonly Mutex _mutex = new Mutex();

        private class CacheItem
        {            
            public object? Item { get; set; }            

            public DateTimeOffset ExpiryTime { get; set; } = DateTimeOffset.UtcNow;
        }

        public void Dispose()
        {
            _items.Clear();     
        }

        private void RemoveExpired()
        {
            _mutex.WaitOne();

            var expiredKeys = _items.Where(i => i.Value.ExpiryTime < DateTimeOffset.UtcNow)
                                .Select(i => i.Key).ToList();

            foreach(var key in expiredKeys)
            {
                _items.Remove(key);
            }

            _mutex.ReleaseMutex();
        }

        public T? GetItem<T>(string key)
        {
            RemoveExpired();

            _mutex.WaitOne();
            var item = _items.ContainsKey(key) ? _items[key] : null;
            _mutex.ReleaseMutex();
            if (item != null && item.ExpiryTime < DateTimeOffset.UtcNow)
            {
                return (T?)item.Item;
            }            

            return default(T?);
        }

        public List<string> GetKeys(string? startsWith)
        {
            RemoveExpired();

            _mutex.WaitOne();
            var keys = _items.Where(i => i.Value.ExpiryTime < DateTimeOffset.UtcNow &&
                                    (String.IsNullOrEmpty(startsWith) || i.Key.StartsWith(startsWith)))
                            .Select(i => i.Key).ToList();
            _mutex.ReleaseMutex();
            return keys;            
        }

        public void AddItem<T>(string key, T item, TimeSpan expiry)
        {
            RemoveExpired();

            // Remove existing
            RemoveItem<T>(key);

            _mutex.WaitOne();
            _items.Add(key, new CacheItem()
            {
                Item = item,
                ExpiryTime = expiry == TimeSpan.Zero ? DateTimeOffset.MaxValue : DateTimeOffset.UtcNow.Add(expiry)
            });
            _mutex.ReleaseMutex();
        }

        public void RemoveItem<T>(string key)
        {
            RemoveExpired();

            _mutex.WaitOne();
            if (_items.ContainsKey(key))
            {
                _items.Remove(key);
            }
            _mutex.ReleaseMutex();
        }

        public void UpdateItem<T>(string key, T item, TimeSpan? expiry)
        {
            RemoveExpired();

            _mutex.WaitOne();
            var currentItem = _items.ContainsKey(key) ? _items[key] : null;
            if (currentItem == null)
            {                
                _items.Add(key, new CacheItem()
                {
                    Item = item,
                    ExpiryTime = expiry == null || expiry == TimeSpan.Zero  ? DateTimeOffset.MaxValue : DateTimeOffset.UtcNow.Add(expiry.Value)
                });
            }
            else
            {
                currentItem.Item = item;
                if (expiry != null)
                {
                    currentItem.ExpiryTime = expiry == TimeSpan.Zero ? DateTimeOffset.MaxValue : DateTimeOffset.UtcNow.Add(expiry.Value);
                }
            }
            _mutex.ReleaseMutex();
        }        
    }
}
