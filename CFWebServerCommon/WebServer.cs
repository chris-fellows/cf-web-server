using CFWebServer.Interfaces;
using CFWebServer.Models;
using CFWebServer.WebServerComponents;

namespace CFWebServer
{
    /// <summary>
    /// Web server. Serves one website.
    /// </summary>
    public class WebServer : IWebServer
    {
        private readonly ICacheService _cacheService;
        private readonly IFileCacheService _fileCacheService;
        private readonly IFolderConfigService _folderConfigService;
        private readonly ILogWriter _logWriter;
        private readonly ServerData _serverData;
        private readonly IWebRequestHandlerFactory _webRequestHandlerFactory;
        private readonly List<IWebServerComponent> _webServerComponents = new List<IWebServerComponent>();

        private CancellationToken _cancellationToken;

        public WebServer(ICacheService cacheService,
                         IFileCacheService fileCacheService,
                            IFolderConfigService folderConfigService,
                            ILogWriter logWriter, 
                            ServerData serverData,
                            IWebRequestHandlerFactory webRequestHandlerFactory,
                            CancellationToken cancellationToken)
        {                            
            if (String.IsNullOrEmpty(serverData.SiteConfig.Site))
            {
                throw new ArgumentException("Site must be set");
            }
            if (String.IsNullOrEmpty(serverData.SiteConfig.RootFolder))
            {
                throw new ArgumentException("Root Folder must be set");
            }

            _cacheService = cacheService;
            _fileCacheService = fileCacheService;
            _folderConfigService = folderConfigService;
            _logWriter = logWriter;
            _serverData = serverData;
            _webRequestHandlerFactory = webRequestHandlerFactory;
            _cancellationToken = cancellationToken;

            _logWriter.Log($"Site: {_serverData.SiteConfig.Site}");
            _logWriter.Log($"Root folder: {_serverData.SiteConfig.RootFolder}");
            _logWriter.Log($"Max concurrent requests: {_serverData.SiteConfig.MaxConcurrentRequests}");            
            _logWriter.Log($"File cache enabled: {(_serverData.SiteConfig.CacheFileConfig.Expiry > TimeSpan.Zero).ToString()}");
        }
        
        public void Start()
        {
            // Add listener component
            var listenerComponent = new ListenerComponent(_cacheService, _logWriter, _serverData, _webRequestHandlerFactory, _cancellationToken);
            _webServerComponents.Add(listenerComponent);

            // Add requests component
            var requestsComponent = new RequestsComponent(_cacheService, _fileCacheService, _logWriter, _serverData, _webRequestHandlerFactory, _cancellationToken);
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
