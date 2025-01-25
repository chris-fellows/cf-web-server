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
        /// Site parameters. E.g. Connection string
        /// </summary>
        public List<SiteParameter> Parameters = new List<SiteParameter>();

        /// <summary>
        /// File cache config
        /// </summary>
        public FileCacheConfig CacheFileConfig = new FileCacheConfig();

        /// <summary>
        /// Folder configs
        /// </summary>
        public List<FolderConfig> FolderConfigs = new List<FolderConfig>();

        /// <summary>
        /// Authorization rules
        /// </summary>
        public List<AuthorizationRule> AuthorizationRules = new List<AuthorizationRule>();

        /// <summary>
        /// Route rules
        /// </summary>
        public List<RouteRule> RouteRules = new List<RouteRule>();

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
                Parameters = (Parameters.Select(p => (SiteParameter)p.Clone())).ToList(),
                CacheFileConfig = (FileCacheConfig)CacheFileConfig.Clone(),
                FolderConfigs = FolderConfigs.Select(fc => (FolderConfig)fc.Clone()).ToList(),
                AuthorizationRules = AuthorizationRules.Select(fc => (AuthorizationRule)fc.Clone()).ToList(),
                RouteRules = RouteRules.Select(fc => (RouteRule)fc.Clone()).ToList()
            };
            return siteConfig;
        }
    }
}
