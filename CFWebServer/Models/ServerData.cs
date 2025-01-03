namespace CFWebServer.Models
{
    /// <summary>
    /// Web server data
    /// </summary>
    internal class ServerData
    {
        /// <summary>
        /// Port for receiving requests
        /// </summary>
        public int ReceivePort { get; internal set; }        

        /// <summary>
        /// Root folder
        /// </summary>
        public string RootFolder { get; internal set; } = String.Empty;

        /// <summary>
        /// Max concurrent requests
        /// </summary>
        public int MaxConcurrentRequests { get; internal set; }

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

        public ServerStatistics Statistics { get; internal set; }

        public ServerData(int listenPort, int maxConcurrentRequests, string rootFolder)
        {
            if (listenPort < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(listenPort));
            }
            if (maxConcurrentRequests < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(maxConcurrentRequests));
            }
            if (String.IsNullOrEmpty(rootFolder))
            {
                throw new ArgumentOutOfRangeException(nameof(rootFolder));
            }

            ReceivePort = listenPort;
            MaxConcurrentRequests = maxConcurrentRequests;
            RootFolder = rootFolder;
            Statistics = new ServerStatistics();
        }
    }
}
