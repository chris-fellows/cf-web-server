namespace CFWebServer.Models
{
    /// <summary>
    /// Site config
    /// </summary>
    public class SiteConfig : ICloneable
    {
        /// <summary>
        /// Unique Id
        /// </summary>
        public string Id { get; set; } = String.Empty;

        /// <summary>
        /// Site name
        /// </summary>
        public string Name { get; set; } = String.Empty;

        /// <summary>
        /// Site prefix
        /// </summary>
        public string Site { get; set; } = String.Empty;

        /// <summary>
        /// Root folder where context stored
        /// </summary>
        public string RootFolder { get; set; } = String.Empty;

        /// <summary>
        /// Default file
        /// </summary>
        public string DefaultFile { get; set; } = String.Empty;

        /// <summary>
        /// Max concurrent requests
        /// </summary>
        public int MaxConcurrentRequests { get; set; }

        /// <summary>
        /// Whether site is enabled
        /// </summary>
        public bool Enabled { get; set; } 

        /// <summary>
        /// File cache config
        /// </summary>
        public FileCacheConfig CacheFileConfig = new FileCacheConfig();

        /// <summary>
        /// Folder configs
        /// </summary>
        public List<FolderConfig> FolderConfigs = new List<FolderConfig>();

        public object Clone()
        {
            var siteConfig = new SiteConfig()
            {
                Id = Id,
                Name = Name,
                Site = Site,
                RootFolder = RootFolder,
                DefaultFile = DefaultFile,
                MaxConcurrentRequests = MaxConcurrentRequests,
                Enabled = Enabled,
                CacheFileConfig = (FileCacheConfig)CacheFileConfig.Clone(),
                FolderConfigs = FolderConfigs.Select(fc => (FolderConfig)fc.Clone()).ToList()
            };
            return siteConfig;
        }
    }
}
