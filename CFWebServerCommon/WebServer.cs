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
        private CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
        private readonly IFileCacheService _fileCacheService;        
        private readonly ISiteLogWriter _logWriter;       
        private readonly ServerData _serverData;
        private readonly IServerNotifications _serverNotifications;        
        private readonly ISiteConfigService _siteConfigService;
        private readonly IWebRequestHandlerFactory _webRequestHandlerFactory;
        private readonly List<IWebServerComponent> _webServerComponents = new List<IWebServerComponent>();       

        private string _siteConfigUpdatedSubscribeId = String.Empty;                

        public WebServer(ICacheService cacheService,
                            IFileCacheService fileCacheService,                            
                            ISiteLogWriter logWriter,                             
                            ServerData serverData,
                            IServerNotifications serverNotifications,                            
                            ISiteConfigService siteConfigService,
                            IWebRequestHandlerFactory webRequestHandlerFactory)                            
        {                                     
            _cacheService = cacheService;
            _fileCacheService = fileCacheService;            
            _logWriter = logWriter;            
            _serverData = serverData;
            _serverNotifications = serverNotifications;            
            _siteConfigService = siteConfigService;
            _webRequestHandlerFactory = webRequestHandlerFactory;                

            SubscribeToServerEvents();

            //_logWriter.Log($"Site: {_serverData.SiteConfig.Site}");
            //_logWriter.Log($"Root folder: {_serverData.SiteConfig.RootFolder}");
            //_logWriter.Log($"Max concurrent requests: {_serverData.SiteConfig.MaxConcurrentRequests}");            
            //_logWriter.Log($"File cache enabled: {(_serverData.SiteConfig.CacheFileConfig.Expiry > TimeSpan.Zero).ToString()}");
        }

        /// <summary>
        /// Subscribes to server events
        /// </summary>
        private void SubscribeToServerEvents()
        {
            // Subscribe to site config updated event. E.g. Permissions updated
            _siteConfigUpdatedSubscribeId = _serverNotifications.Subscribe(ServerEventTypes.SiteConfigUpdated, (serverEvent) =>
            {
                if (serverEvent.EventType == ServerEventTypes.SiteConfigUpdated &&
                   _serverData.SiteConfig != null)
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
                _serverNotifications.Unsubscribe(_siteConfigUpdatedSubscribeId);
            }
        }
        
        public void Start()
        {
            if (IsStarted)
            {
                throw new ApplicationException("Web server has already started");
            }

            // Set file cache config
            _fileCacheService.SetConfig(_serverData.SiteConfig.CacheFileConfig);

            _cancellationTokenSource = new CancellationTokenSource();

            _webServerComponents.Clear();       // Sanity check

            // Add listener component
            var listenerComponent = new ListenerComponent(_cacheService, _logWriter, _serverData, 
                                                    _cancellationTokenSource.Token);
            _webServerComponents.Add(listenerComponent);

            // Add requests component
            var requestsComponent = new RequestsComponent(_cacheService, _fileCacheService, _logWriter, 
                                                    _serverData, _webRequestHandlerFactory, _cancellationTokenSource.Token);
            _webServerComponents.Add(requestsComponent);

            // Start components
            _webServerComponents.ForEach(component => component.Start());
        }

        public void Stop()
        {
            // Instruct components to cancel processing
            _cancellationTokenSource.Cancel();

            // Stop components            
            while (_webServerComponents.Any())
            {
                _webServerComponents[0].Stop();
                _webServerComponents.RemoveAt(0);
            }
        }

        public bool IsStarted => _webServerComponents.Any();
    }
}
