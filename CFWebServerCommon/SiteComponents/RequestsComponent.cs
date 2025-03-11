using CFWebServer.Interfaces;
using CFWebServer.Models;

namespace CFWebServer.WebServerComponents
{
    /// <summary>
    /// Processes requests and other periodic tasks (E.g. Expires cached files, logs statistics)
    /// </summary>
    /// <remarks>We could create a separate thread for periodic tasks but it is wasteful of resources.</remarks>
    internal class RequestsComponent : ISiteComponent
    {
        private Thread? _thread;
        
        private readonly IFileCacheService _fileCacheService;
        private readonly ISiteLogWriter _logWriter;
        private readonly SiteData _siteData;        
        private readonly IWebRequestHandlerFactory _webRequestHandlerFactory;
        private CancellationToken _cancellationToken;

        public RequestsComponent(IFileCacheService fileCacheService,
                            ISiteLogWriter logWriter,
                            SiteData siteData,                            
                            IWebRequestHandlerFactory webRequestHandlerFactory,
                            CancellationToken cancellationToken)
        {
            _fileCacheService = fileCacheService;
            _logWriter = logWriter;
            _siteData = siteData;            
            _webRequestHandlerFactory = webRequestHandlerFactory;
            _cancellationToken = cancellationToken;          
        }

        public void Start()
        {
            _logWriter.Log($"Starting waiting for requests to process at {_siteData.SiteConfig.Site}");

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
                if (_siteData.ActiveRequestContexts.Count < _siteData.SiteConfig.MaxConcurrentRequests)
                {
                    if (_siteData.RequestContextQueue.Any() &&
                        _siteData.RequestContextQueue.TryDequeue(out var requestContext))
                    {                                                
                        var task = HandleRequestAsync(requestContext);                        
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
                    checkFileCacheTask.IsCompleted)     // Task completed, allow new instance to run
                {
                    checkFileCacheTask = null;
                }

                // Periodically log statistics
                if (logStatisticsInterval > TimeSpan.Zero &&
                    lastLogStatistics.Add(logStatisticsInterval) <= DateTimeOffset.UtcNow)
                {
                    lastLogStatistics = DateTimeOffset.UtcNow;
                    LogStatistics(_siteData.Statistics);
                }

                Thread.Sleep(1);
            }            
        }
     
        /// <summary>
        /// Logs statistics
        /// </summary>
        /// <param name="serverStatistics"></param>
        private void LogStatistics(SiteStatistics serverStatistics)
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
        private Task HandleRequestAsync(RequestContext requestContext)
        {
            // Add to active requests. Don't really need to do it in the task thread
            _siteData.Mutex.WaitOne();
            _siteData.ActiveRequestContexts.Add(requestContext);
            _siteData.Mutex.ReleaseMutex();

            var task = Task.Factory.StartNew(() =>
            {
                _logWriter.LogRequest(requestContext);

                try
                {                    
                    // Get request handler
                    var webRequestHandler = _webRequestHandlerFactory.Get(requestContext, _siteData);

                    // Handle request
                    if (webRequestHandler != null)
                    {
                        webRequestHandler.HandleAsync(requestContext).Wait();
                    }
                }
                finally
                {
                    // Remove request from active list
                    _siteData.Mutex.WaitOne();
                    if (_siteData.ActiveRequestContexts.Contains(requestContext))
                    {
                        _siteData.ActiveRequestContexts.Remove(requestContext);
                    }
                    _siteData.Mutex.ReleaseMutex();
                }
            });

            return task;
        }
    }
}
