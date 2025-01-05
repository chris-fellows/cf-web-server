namespace CFWebServer.Models
{
    /// <summary>
    /// Web server data
    /// </summary>
    internal class ServerData
    {
        /// <summary>
        /// Site. E.g. http://localhost:10010
        /// </summary>
        public string Site { get; internal set; }

        /// <summary>
        /// Root folder
        /// </summary>
        public string RootFolder { get; internal set; } = String.Empty;

        /// <summary>
        /// Default file for / request
        /// </summary>
        public string DefaultFile { get; internal set; } = String.Empty;

        /// <summary>
        /// Max concurrent requests
        /// </summary>
        public int MaxConcurrentRequests { get; internal set; }

        public TimeSpan LogStatisticsInterval { get; internal set; }

        /// <summary>
        /// Mutex for shared resources
        /// </summary>
        public Mutex Mutex = new Mutex();

        /// <summary>
        /// New requests
        /// </summary>
        public Queue<RequestContext> RequestContextQueue = new Queue<RequestContext>();

        /// <summary>
        /// Active requests
        /// </summary>
        public List<RequestContext> ActiveRequestContexts = new List<RequestContext>();
        
        /// <summary>
        /// Server statistics
        /// </summary>
        public ServerStatistics Statistics { get; internal set; }

        /// <summary>
        /// File cache config
        /// </summary>
        public FileCacheConfig CacheFileConfig { get; internal set; }        

        public ServerData(string defaultFile,
                            bool fileCacheCompressed,
                            TimeSpan fileCacheExpiry,                             
                            TimeSpan logStatisticsInterval,
                            int maxCachedFileBytes,
                            int maxConcurrentRequests, 
                            int maxFileCacheBytes,
                            string rootFolder,
                            string site)
        {          
            if (String.IsNullOrEmpty(rootFolder))
            {
                throw new ArgumentOutOfRangeException(nameof(rootFolder));
            }
            if (String.IsNullOrEmpty(site))
            {
                throw new ArgumentOutOfRangeException(nameof(site));
            }

            CacheFileConfig = new FileCacheConfig()
            {
                Compressed = fileCacheCompressed,
                Expiry = fileCacheExpiry,
                MaxFileSizeBytes = maxCachedFileBytes,
                MaxTotalSizeBytes = maxFileCacheBytes
            };

            DefaultFile = defaultFile;
            LogStatisticsInterval = logStatisticsInterval;            
            MaxConcurrentRequests = maxConcurrentRequests;
            RootFolder = rootFolder;
            Site = site;
            Statistics = new ServerStatistics();
        }
    }
}
