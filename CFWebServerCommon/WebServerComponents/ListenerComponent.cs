using CFWebServer.Interfaces;
using CFWebServer.Models;
using System.Net;

namespace CFWebServer.WebServerComponents
{
    /// <summary>
    /// Listens for requests. Adds to queue
    /// </summary>
    internal class ListenerComponent : IWebServerComponent
    {
        private Thread? _thread;
        
        private HttpListener? _listener;
        private ISiteLogWriter _logWriter;

        private readonly ServerData _serverData;
        
        private CancellationToken _cancellationToken;

        public ListenerComponent(ISiteLogWriter logWriter,
                            ServerData serverData,                            
                            CancellationToken cancellationToken)
        {            
            _logWriter = logWriter;
            _serverData = serverData;            
            _cancellationToken = cancellationToken;
        }

        public void Start()
        {            
            _logWriter.Log($"Starting listening for requests at {_serverData.SiteConfig.Site}");

            if (!HttpListener.IsSupported)
            {
                throw new NotSupportedException("HttpListener is not supported on this platform");
            }

            _listener = new HttpListener();
            _listener.Prefixes.Add(_serverData.SiteConfig.Site);
            _listener.Start();

            _thread = new Thread(WorkerThread);
            _thread.Start();

            _logWriter.Log("Listening for requests");
        }

        public void Stop()
        {
            _logWriter.Log("Stopping listening");

            _listener.Stop();

            if (_thread != null)
            {
                _thread.Join();
                _thread = null;
            }

            _logWriter.Log("Stopping listening");
        }     

        public void WorkerThread()
        {
            while (!_cancellationToken.IsCancellationRequested)
            {
                try
                {
                    // Get listener context
                    HttpListenerContext listenerContext = _listener.GetContext();

                    // Add request to queue
                    RequestContext requestContext = new RequestContext(listenerContext.Request, listenerContext.Response);
                    _serverData.Mutex.WaitOne();
                    _serverData.RequestContextQueue.Enqueue(requestContext);
                    _serverData.Statistics.CountRequestsReceived++;
                    _serverData.Statistics.LastRequestReceivedTime = DateTimeOffset.UtcNow;
                    _serverData.Mutex.ReleaseMutex();
                }
                catch (HttpListenerException)
                {
                    if (!_cancellationToken.IsCancellationRequested) throw;                    
                }
            
                Thread.Yield();
            }
        }
    }
}
