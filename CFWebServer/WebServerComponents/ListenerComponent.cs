using CFWebServer.Interfaces;
using CFWebServer.Models;
using System.Net;

namespace CFWebServer.WebServerComponents
{
    /// <summary>
    /// Listens for requests
    /// </summary>
    internal class ListenerComponent : IWebServerComponent
    {
        private Thread? _thread;

        private HttpListener? _listener;
        private ILogWriter _logWriter;

        private readonly ServerData _serverData;

        private readonly IWebRequestHandlerFactory _webRequestHandlerFactory;

        private CancellationToken _cancellationToken;

        public ListenerComponent(ILogWriter logWriter,
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
            _logWriter.Log("Starting listening for requests");

            _listener = new HttpListener();
            _listener.Prefixes.Add($"http://localhost:{_serverData.ReceivePort}/");
            _listener.Start();

            _thread = new Thread(WorkerThread);
            _thread.Start();

            _logWriter.Log("Listening for requests");
        }

        public void Stop()
        {
            _logWriter.Log("Stopping listening");

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
                // Get listener context
                HttpListenerContext listenerContext = _listener.GetContext();

                // Add request to queue
                RequestContext requestContext = new RequestContext(listenerContext.Request, listenerContext.Response);
                _serverData.Mutex.WaitOne();
                _serverData.RequestContextQueue.Enqueue(requestContext);
                _serverData.Statistics.CountRequestsReceived++;
                _serverData.Statistics.LastRequestReceivedTime = DateTimeOffset.UtcNow;
                _serverData.Mutex.ReleaseMutex();

                /*
                // Peel out the requests and response objects
                HttpListenerRequest request = listenerContext.Request;
                HttpListenerResponse response = listenerContext.Response;

                // Print out some info about the request                
                Console.WriteLine(request.Url.ToString());
                Console.WriteLine(request.HttpMethod);
                Console.WriteLine(request.UserHostName);
                Console.WriteLine(request.UserAgent);
                Console.WriteLine();               
                */

                Thread.Yield();
            }
        }
    }
}
