using CFWebServer.Enums;
using CFWebServer.Interfaces;
using CFWebServer.Models;
using CFWebServer.WebServerComponents;

namespace CFWebServer
{
    /// <summary>
    /// Web server. Serves one website.
    /// </summary>
    public class WebServer : IWebServer, IDisposable
    {
        private readonly ICacheService _cacheService;
        private readonly IFileCacheService _fileCacheService;        
        private readonly ILogWriter _logWriter;
        private readonly ServerData _serverData;
        private readonly IServerEventQueue _serverEventQueue;
        private readonly ISiteConfigService _siteConfigService;
        private readonly IWebRequestHandlerFactory _webRequestHandlerFactory;
        private readonly List<IWebServerComponent> _webServerComponents = new List<IWebServerComponent>();

        private string _siteConfigUpdatedSubscribeId = String.Empty;
        private CancellationToken _cancellationToken;

        public WebServer(ICacheService cacheService,
                         IFileCacheService fileCacheService,                            
                            ILogWriter logWriter, 
                            ServerData serverData,
                            IServerEventQueue serverEventQueue,
                            ISiteConfigService siteConfigService,
                            IWebRequestHandlerFactory webRequestHandlerFactory,
                            CancellationToken cancellationToken)
        {                            
            if (String.IsNullOrEmpty(serverData.SiteConfig.Site))
            {
                throw new ArgumentException("Site must be set");
            }
            if (String.IsNullOrEmpty(serverData.SiteConfig.RootFolder) &&
                !String.IsNullOrEmpty(serverData.SiteConfig.Id))        // Root folder isn't set for internal 
            {
                throw new ArgumentException("Root Folder must be set");
            }

            _cacheService = cacheService;
            _fileCacheService = fileCacheService;            
            _logWriter = logWriter;
            _serverData = serverData;
            _serverEventQueue = serverEventQueue;
            _siteConfigService = siteConfigService;
            _webRequestHandlerFactory = webRequestHandlerFactory;
            _cancellationToken = cancellationToken;

            SubscribeToServerEvents();

            _logWriter.Log($"Site: {_serverData.SiteConfig.Site}");
            _logWriter.Log($"Root folder: {_serverData.SiteConfig.RootFolder}");
            _logWriter.Log($"Max concurrent requests: {_serverData.SiteConfig.MaxConcurrentRequests}");            
            _logWriter.Log($"File cache enabled: {(_serverData.SiteConfig.CacheFileConfig.Expiry > TimeSpan.Zero).ToString()}");
        }

        /// <summary>
        /// Subscribes to server events
        /// </summary>
        private void SubscribeToServerEvents()
        {
            // Subscribe to site config updated event. E.g. Permissions updated
            _siteConfigUpdatedSubscribeId = _serverEventQueue.Subscribe(ServerEventTypes.SiteConfigUpdated, (serverEvent) =>
            {
                if (serverEvent.EventType == ServerEventTypes.SiteConfigUpdated)
                {
                    var siteConfigId = (string)serverEvent.Parameters["SiteConfigId"];
                    if (siteConfigId == _serverData.SiteConfig.Id)   // This site
                    {
                        _logWriter.Log($"Refreshing site config for {_serverData.SiteConfig.Name}");
                        _serverData.SiteConfig = _siteConfigService.GetById(_serverData.SiteConfig.Id);
                        _logWriter.Log("Refreshed site config");
                    }
                }
            });
        }

        public void Dispose()
        {
            if (!String.IsNullOrEmpty(_siteConfigUpdatedSubscribeId))
            {
                _serverEventQueue.Unsubscribe(_siteConfigUpdatedSubscribeId);
            }
        }
        
        public void Start()
        {
            // Add listener component
            var listenerComponent = new ListenerComponent(_cacheService, _logWriter, _serverData, 
                                                    _webRequestHandlerFactory, _cancellationToken);
            _webServerComponents.Add(listenerComponent);

            // Add requests component
            var requestsComponent = new RequestsComponent(_cacheService, _fileCacheService, _logWriter, 
                                                    _serverData, _webRequestHandlerFactory, _cancellationToken);
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
