using CFWebServer.Interfaces;
using CFWebServer.Models;

namespace CFWebServer
{
    /// <summary>
    /// Web server
    /// </summary>
    internal class WebServer
    {
        private readonly IFolderConfigService _folderConfigService;
        private readonly ILogWriter _logWriter;
        private readonly ServerData _serverData;
        private readonly IWebRequestHandlerFactory _webRequestHandlerFactory;
        private readonly List<IWebServerComponent> _webServerComponents = new List<IWebServerComponent>();

        private CancellationToken _cancellationToken;

        public WebServer(IFolderConfigService folderConfigService,
                            ILogWriter logWriter, 
                            ServerData serverData,
                            IWebRequestHandlerFactory webRequestHandlerFactory,
                            CancellationToken cancellationToken)
        {
            if (serverData.ReceivePort < 1)
            {
                throw new ArgumentException("Receive Port must be set");
            }
            if (String.IsNullOrEmpty(serverData.RootFolder))
            {
                throw new ArgumentException("Root Folder must be set");
            }

            _folderConfigService = folderConfigService;
            _logWriter = logWriter;
            _serverData = serverData;
            _webRequestHandlerFactory = webRequestHandlerFactory;
            _cancellationToken = cancellationToken;

            _logWriter.Log($"Listening port: {_serverData.ReceivePort}");
            _logWriter.Log($"Max concurrent requests: {_serverData.MaxConcurrentRequests}");
            _logWriter.Log($"Root folder: {_serverData.RootFolder}");
        }
        
        public void Start()
        {
            // Add listener component
            var listenerComponent = new ListenerComponent(_logWriter, _serverData, _webRequestHandlerFactory, _cancellationToken);
            _webServerComponents.Add(listenerComponent);

            // Add requests component
            var requestsComponent = new RequestsComponent(_logWriter, _serverData, _webRequestHandlerFactory, _cancellationToken);
            _webServerComponents.Add(requestsComponent);

            // Start components
            _webServerComponents.ForEach(component => component.Start());
        }

        public void Stop()
        {
            while(_webServerComponents.Any())
            {
                _webServerComponents[0].Stop();
                _webServerComponents.RemoveAt(0);
            }
        }
    }
}
