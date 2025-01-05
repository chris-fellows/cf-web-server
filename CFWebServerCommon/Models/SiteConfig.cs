namespace CFWebServer.Models
{
    /// <summary>
    /// Site config
    /// </summary>
    public class SiteConfig
    {
        public string Id { get; set; } = String.Empty;

        public string Name { get; set; } = String.Empty;

        public string Site { get; set; } = String.Empty;

        public string RootFolder { get; set; } = String.Empty;

        public string DefaultFile { get; set; } = String.Empty;

        public int MaxConcurrentRequests { get; set; }

        public FileCacheConfig CacheFileConfig = new FileCacheConfig();
    }
}
