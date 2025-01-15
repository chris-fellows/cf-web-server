using CFWebServer.Interfaces;
using CFWebServer.Models;

namespace CFWebServer.Services
{
    /// <summary>
    /// File cache service using local memory
    /// </summary>
    public class LocalMemoryFileCache : IFileCacheService
    {
        protected readonly ICacheService _cacheService;
        protected readonly FileCacheConfig _fileCacheConfig;

        public LocalMemoryFileCache(ICacheService cacheService,
                                    FileCacheConfig fileCacheConfig)
        {
            _cacheService = cacheService;
            _fileCacheConfig = fileCacheConfig;
        }

        public bool Enabled => _fileCacheConfig.Expiry == TimeSpan.Zero;     

        public int SizeBytes
        {
            get
            {
                int size = 0;
                var keys = _cacheService.GetKeys("File:").ToList();

                foreach(var key in keys)
                {
                    var cacheFile = _cacheService.GetItem<CacheFile>(key);
                    if (cacheFile != null)
                    {
                        size += cacheFile.ObjectSizeBytes;
                    }
                }

                return size;
            }
        }

        /// <summary>
        /// Whether file should be compressed in cache
        /// </summary>
        /// <param name="relativePath"></param>
        /// <returns></returns>
        private bool IsShouldCompress(string relativePath)
        {
            if (!Enabled) return false;

            // TODO: Move this
            var extensions = new[] { ".7z", ".gz", ".jpg", ".jpeg", ".lz", ".mp3", ".mp4", ".tar", ".zip" };

            foreach (var extension in extensions)
            {
                if (relativePath.ToLower().EndsWith(extension)) return false;
            }

            return true;            
        }

        public bool IsAllowedToCache(string relativePath, byte[] content)
        {
            return Enabled &&
                content.Length <= _fileCacheConfig.MaxTotalSizeBytes;
        }

        public CacheFile? Get(string relativePath)
        {
            if (!Enabled) return null;

            var cacheFileKey = $"File:{relativePath}";

            var cacheFile = _cacheService.GetItem<CacheFile>(cacheFileKey);
            return cacheFile;
        }

        public void Add(string relativePath, byte[] content, DateTimeOffset lastModified)
        {
            if (Enabled)
            {
                var cacheFileKey = $"File:{relativePath}";

                // Get whether to compress
                var compressed = content.Length > 0 && IsShouldCompress(relativePath);                

                var cacheFile = new CacheFile()
                {
                    RelativePath = relativePath,
                    LastModified = lastModified,
                    LastUsed = DateTimeOffset.UtcNow
                };
                cacheFile.SetContent(content, compressed);

                // Get total cache size
                var totalSizeBytes = SizeBytes; 
                
                // Add if sufficient size
                // TODO: Consider removing least used files to free cache space
                if (totalSizeBytes + cacheFile.ObjectSizeBytes < _fileCacheConfig.MaxTotalSizeBytes)
                {
                    _cacheService.AddItem(cacheFileKey, cacheFile, _fileCacheConfig.Expiry);
                }
            }
        }

        public void UpdateLastUsed(string relativePath, DateTimeOffset lastUsed)
        {
            if (Enabled)
            {
                var cacheFileKey = $"File:{relativePath}";

                var cacheFile = _cacheService.GetItem<CacheFile>(cacheFileKey);
                if (cacheFile != null)
                {
                    cacheFile.LastUsed = lastUsed;                    

                    // We pass the expiry parameter so that the cached file will be removed in N mins if it's 
                    // not requested again. If we pass null then it's removed N mins after it was added.
                    _cacheService.UpdateItem<CacheFile>(cacheFileKey, cacheFile, _fileCacheConfig.Expiry);
                }
            }
        }

        public void Remove(string relativePath)
        {
            if (Enabled)
            {
                var cacheFileKey = $"File:{relativePath}";

                var cacheFile = _cacheService.GetItem<CacheFile>(cacheFileKey);
                if (cacheFile != null)
                {
                    _cacheService.RemoveItem<CacheFile>(cacheFileKey);
                }
            }
        }

        public void RemoveExpired()
        {
            if (Enabled)
            {
                // Get all keys, forces expired items to be removed
                var keys = _cacheService.GetKeys("File:").ToList();
            }
        }
    }
}
