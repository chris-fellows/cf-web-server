using System.Collections.Concurrent;

namespace CFWebServer.Models
{
    /// <summary>
    /// Web site data
    /// </summary>
    public class SiteData
    {
        /// <summary>
        /// Site config
        /// </summary>
        public SiteConfig SiteConfig { get; set; } = new();

        /// <summary>
        /// Interval to log statistics
        /// </summary>
        public TimeSpan LogStatisticsInterval { get; set; } = TimeSpan.FromSeconds(60);

        /// <summary>
        /// Mutex for shared resources
        /// </summary>
        public Mutex Mutex = new Mutex();

        /// <summary>
        /// New requests
        /// </summary>
        public ConcurrentQueue<RequestContext> RequestContextQueue = new();

        /// <summary>
        /// Active requests
        /// </summary>
        public List<RequestContext> ActiveRequestContexts = new();

        /// <summary>
        /// Site statistics
        /// </summary>
        public SiteStatistics Statistics = new();
    }
}
