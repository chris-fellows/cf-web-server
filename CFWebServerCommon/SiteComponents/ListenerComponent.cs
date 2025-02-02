using CFWebServer.Interfaces;
using CFWebServer.Models;
using System.Net;

namespace CFWebServer.WebServerComponents
{
    /// <summary>
    /// Listens for requests. Adds to queue
    /// </summary>
    internal class ListenerComponent : ISiteComponent
    {
        private Thread? _thread;
        
        private HttpListener? _listener;
        private ISiteLogWriter _logWriter;

        private readonly SiteData _siteData;
        
        private CancellationToken _cancellationToken;

        public ListenerComponent(ISiteLogWriter logWriter,
                            SiteData siteData,                            
                            CancellationToken cancellationToken)
        {            
            _logWriter = logWriter;
            _siteData = siteData;            
            _cancellationToken = cancellationToken;
        }

        public void Start()
        {            
            _logWriter.Log($"Starting listening for requests at {_siteData.SiteConfig.Site}");

            if (!HttpListener.IsSupported)
            {
                throw new NotSupportedException("HttpListener is not supported on this platform");
            }

            _listener = new HttpListener();
            _listener.Prefixes.Add(_siteData.SiteConfig.Site);
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

                    Console.WriteLine("Received request");

                    // Add request to queue
                    RequestContext requestContext = new RequestContext(listenerContext.Request, listenerContext.Response);
                    _siteData.Mutex.WaitOne();
                    _siteData.RequestContextQueue.Enqueue(requestContext);                    
                    _siteData.Statistics.CountRequestsReceived = (_siteData.Statistics.CountRequestsReceived % int.MaxValue) + 1;
                    _siteData.Statistics.LastRequestReceivedTime = DateTimeOffset.UtcNow;
                    _siteData.Mutex.ReleaseMutex();
                }
                catch (HttpListenerException)
                {
                    if (!_cancellationToken.IsCancellationRequested) throw;                    
                }

                Thread.Sleep(1);
            }
        }
    }
}
