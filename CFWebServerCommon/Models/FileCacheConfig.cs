namespace CFWebServer.Models
{
    /// <summary>
    /// File cache config
    /// </summary>
    public class FileCacheConfig
    {   
        /// <summary>
        /// Whether to compress files by default. Files that are already compressed (E.g. .zip) will
        /// not be compressed again.
        /// </summary>
        public bool Compressed { get; set; }

        /// <summary>
        /// Max total cache size
        /// </summary>
        public int MaxTotalSizeBytes { get; set; }

        /// <summary>
        /// Max size of single file to cache
        /// </summary>
        public int MaxFileSizeBytes { get; set; }

        /// <summary>
        /// Cache expiry (0=Cache disabled)
        /// </summary>
        public TimeSpan Expiry { get; set; }
    }
}
