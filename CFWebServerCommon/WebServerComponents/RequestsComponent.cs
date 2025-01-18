using CFWebServer.Enums;
using CFWebServer.Interfaces;
using CFWebServer.Models;

namespace CFWebServer.WebServerComponents
{
    /// <summary>
    /// Processes requests and other periodic tasks (E.g. Expires cached files, logs statistics)
    /// </summary>
    /// <remarks>We could create a separate thread for periodic tasks but it is wasteful of resources.</remarks>
    internal class RequestsComponent : IWebServerComponent
    {
        private Thread? _thread;

        private readonly ICacheService _cacheService;
        private readonly IFileCacheService _fileCacheService;
        private readonly ISiteLogWriter _logWriter;
        private readonly ServerData _serverData;        
        private readonly IWebRequestHandlerFactory _webRequestHandlerFactory;
        private CancellationToken _cancellationToken;

        public RequestsComponent(ICacheService cacheService,
                            IFileCacheService fileCacheService,
                            ISiteLogWriter logWriter,
                            ServerData serverData,                            
                            IWebRequestHandlerFactory webRequestHandlerFactory,
                            CancellationToken cancellationToken)
        {
            _cacheService = cacheService;
            _fileCacheService = fileCacheService;
            _logWriter = logWriter;
            _serverData = serverData;            
            _webRequestHandlerFactory = webRequestHandlerFactory;
            _cancellationToken = cancellationToken;          
        }

        public void Start()
        {
            _logWriter.Log($"Starting waiting for requests to process at {_serverData.SiteConfig.Site}");

            _thread = new Thread(WorkerThread);
            _thread.Start();

            _logWriter.Log("Waiting for requests to process");
        }

        public void Stop()
        {
            _logWriter.Log("Stopping waiting for requests to process");

            if (_thread != null)
            {
                _thread.Join();
                _thread = null;
            }

            _logWriter.Log("Stopped waiting for requests to process");
        }

        public void WorkerThread()
        {
            Task? checkFileCacheTask = null;
            TimeSpan checkFileCacheInterval = TimeSpan.FromMinutes(5);
            DateTimeOffset lastCheckFileCacheTask = DateTimeOffset.MinValue;

            TimeSpan logStatisticsInterval = TimeSpan.FromMinutes(5);
            DateTimeOffset lastLogStatistics = DateTimeOffset.MinValue;          

            while (!_cancellationToken.IsCancellationRequested)
            {               
                // Process next queued request if any
                if (_serverData.ActiveRequestContexts.Count < _serverData.SiteConfig.MaxConcurrentRequests)
                {
                    if (_serverData.RequestContextQueue.Any())
                    {
                        var requestContext = _serverData.RequestContextQueue.Dequeue();
                        if (requestContext != null)
                        {
                            // Add to active requests
                            _serverData.Mutex.WaitOne();
                            _serverData.ActiveRequestContexts.Add(requestContext);
                            _serverData.Mutex.ReleaseMutex();

                            // Handle request
                            var task = HandleAsync(requestContext);
                        }
                    }
                }

                // Start task to removed expired cached files
                if (checkFileCacheInterval > TimeSpan.Zero &&
                    checkFileCacheTask == null &&
                    lastCheckFileCacheTask.Add(checkFileCacheInterval) <= DateTimeOffset.UtcNow)
                {
                    lastCheckFileCacheTask = DateTimeOffset.UtcNow;
                    checkFileCacheTask = CheckFileCacheTask();
                }
                else if (checkFileCacheTask != null &&
                    checkFileCacheTask.IsCompleted)
                {
                    checkFileCacheTask = null;
                }

                // Periodically log statistics
                if (logStatisticsInterval > TimeSpan.Zero &&
                    lastLogStatistics.Add(logStatisticsInterval) <= DateTimeOffset.UtcNow)
                {
                    lastLogStatistics = DateTimeOffset.UtcNow;
                    LogStatistics(_serverData.Statistics);
                }

                Thread.Yield();
            }            
        }
     
        /// <summary>
        /// Logs statistics
        /// </summary>
        /// <param name="serverStatistics"></param>
        private void LogStatistics(ServerStatistics serverStatistics)
        {
            _logWriter.Log($"Statistics: Server started={serverStatistics.StartedTime.ToString()}, " +
                    $"Requests processed={serverStatistics.CountRequestsReceived}; " +
                    $"Last request={(serverStatistics.LastRequestReceivedTime == null ? "none" : serverStatistics.LastRequestReceivedTime.ToString())}");
        }

        /// <summary>
        /// Expires cached files
        /// </summary>
        /// <returns></returns>
        private Task CheckFileCacheTask()
        {
            return Task.Factory.StartNew(() =>
            {
                _fileCacheService.RemoveExpired();
            });
        }

        /// <summary>
        /// Handles request
        /// </summary>
        /// <param name="requestContext"></param>
        /// <returns></returns>
        private Task HandleAsync(RequestContext requestContext)
        {
            var task = Task.Factory.StartNew(() =>
            {
                _logWriter.LogRequest(requestContext);

                try
                {
                    // Get request handler
                    var webRequestHandler = _webRequestHandlerFactory.Get(requestContext, _serverData);

                    // Handle request
                    if (webRequestHandler != null)
                    {
                        webRequestHandler.HandleAsync(requestContext).Wait();
                    }
                }
                finally
                {
                    // Remove request from active list
                    _serverData.Mutex.WaitOne();
                    if (_serverData.ActiveRequestContexts.Contains(requestContext))
                    {
                        _serverData.ActiveRequestContexts.Remove(requestContext);
                    }
                    _serverData.Mutex.ReleaseMutex();
                }
            });

            return task;
        }
    }
}
