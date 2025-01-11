namespace CFWebServer.Models
{
    /// <summary>
    /// Web server data
    /// </summary>
    public class ServerData
    {
        ///// <summary>
        ///// Site. E.g. http://localhost:10010
        ///// </summary>
        //public string Site { get; internal set; }
       
        /// <summary>
        /// Site config
        /// </summary>
        public SiteConfig SiteConfig { get; set; }

        /// <summary>
        /// Interval to log statistics
        /// </summary>
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

        public ServerData(TimeSpan logStatisticsInterval,
                        SiteConfig siteConfig)
        {                 
            LogStatisticsInterval = logStatisticsInterval;            
            SiteConfig = siteConfig;
            Statistics = new ServerStatistics();
        }
    }
}
