using CFWebServer.Interfaces;
using CFWebServer.Models;

namespace CFWebServer.WebServerComponents
{
    /// <summary>
    /// Processes requests
    /// </summary>
    internal class RequestsComponent : IWebServerComponent
    {
        private Thread? _thread;

        private readonly ILogWriter _logWriter;
        private readonly ServerData _serverData;
        private readonly IWebRequestHandlerFactory _webRequestHandlerFactory;
        private CancellationToken _cancellationToken;


        public RequestsComponent(ILogWriter logWriter,
                            ServerData serverData,
                            IWebRequestHandlerFactory webRequestHandlerFactory,
                            CancellationToken cancellationToken)
        {
            _logWriter = logWriter;
            _serverData = serverData;
            _webRequestHandlerFactory = webRequestHandlerFactory;
            _cancellationToken = cancellationToken;
        }

        public void Start()
        {
            _thread = new Thread(WorkerThread);
            _thread.Start();
        }

        public void Stop()
        {
            if (_thread != null)
            {
                _thread.Join();
                _thread = null;
            }
        }

        public void WorkerThread()
        {
            while (!_cancellationToken.IsCancellationRequested)
            {
                if (_serverData.ActiveRequestContexts.Count < _serverData.MaxConcurrentRequests)
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

                Thread.Yield();
            }
        }

        private Task HandleAsync(RequestContext requestContext)
        {
            var task = Task.Factory.StartNew(() =>
            {
                _logWriter.LogRequest(requestContext);

                try
                {
                    var webRequestHandler = _webRequestHandlerFactory.Get(requestContext);

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
