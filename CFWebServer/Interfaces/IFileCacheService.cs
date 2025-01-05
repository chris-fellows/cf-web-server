using CFWebServer.Models;

namespace CFWebServer.Interfaces
{
    /// <summary>
    /// File cache service
    /// </summary>
    internal interface IFileCacheService
    {
        /// <summary>
        /// Whether enabled
        /// </summary>
        bool Enabled { get; }

        /// <summary>
        /// Cache size (Bytes)
        /// </summary>
        int SizeBytes { get; }
        
        /// <summary>
        /// Whether file meets rules for caching. E.g. Not too big.
        /// </summary>
        /// <param name="relativePath"></param>
        /// <param name="content"></param>        
        /// <returns></returns>
        bool IsAllowedToCache(string relativePath, byte[] content);

        /// <summary>
        /// Gets cached file
        /// </summary>
        /// <param name="relativePath"></param>
        /// <returns></returns>
        CacheFile? Get(string relativePath);

        /// <summary>
        /// Adds cached file
        /// </summary>
        /// <param name="relativePath"></param>
        /// <param name="content"></param>
        /// <param name="compressed"></param>
        /// <param name="lastModified"></param>
        void Add(string relativePath, byte[] content, DateTimeOffset lastModified);

        /// <summary>
        /// Updates file last used
        /// </summary>
        /// <param name="relativePath"></param>
        /// <param name="lastUsed"></param>
        void UpdateLastUsed(string relativePath, DateTimeOffset lastUsed);

        /// <summary>
        /// Removes cached file
        /// </summary>
        /// <param name="relativePath"></param>
        void Remove(string relativePath);

        /// <summary>
        /// Removes expired cached files
        /// </summary>
        /// <remarks>This is a bit of a hack because if we use LocalMemoryCache (ICacheService) then it requires
        /// interaction with ICacheService for it to detect that items are expired.
        void RemoveExpired();
    }
}
