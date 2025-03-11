using CFWebServer.Enums;
using CFWebServer.Interfaces;
using CFWebServer.Models;
using CFWebServer.WebServerComponents;

namespace CFWebServer
{
    /// <summary>
    /// Web site
    /// </summary>
    public class Site : ISite, IDisposable
    {
        private readonly ICacheService _cacheService;
        private CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
        private readonly IFileCacheService _fileCacheService;        
        private readonly ISiteLogWriter _logWriter;       
        private readonly SiteData _siteData;
        private readonly IServerNotifications _serverNotifications;        
        private readonly ISiteConfigService _siteConfigService;
        private readonly IWebRequestHandlerFactory _webRequestHandlerFactory;
        private readonly List<ISiteComponent> _siteComponents = new List<ISiteComponent>();       

        private string _siteConfigUpdatedSubscribeId = String.Empty;                

        public Site(ICacheService cacheService,
                            IFileCacheService fileCacheService,                            
                            ISiteLogWriter logWriter,                                                         
                            IServerNotifications serverNotifications,                            
                            ISiteConfigService siteConfigService,
                            SiteData siteData,
                            IWebRequestHandlerFactory webRequestHandlerFactory)                            
        {                                     
            _cacheService = cacheService;
            _fileCacheService = fileCacheService;            
            _logWriter = logWriter;                        
            _serverNotifications = serverNotifications;            
            _siteConfigService = siteConfigService;
            _siteData = siteData;
            _webRequestHandlerFactory = webRequestHandlerFactory;                

            SubscribeToServerEvents();
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
                   _siteData.SiteConfig != null)
                {
                    var siteConfigId = (string)serverEvent.Parameters["SiteConfigId"];
                    if (siteConfigId == _siteData.SiteConfig.Id)   // This site
                    {
                        _logWriter.Log($"Refreshing site config for {_siteData.SiteConfig.Name}");
                        _siteData.SiteConfig = _siteConfigService.GetById(_siteData.SiteConfig.Id);
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
            _fileCacheService.SetConfig(_siteData.SiteConfig.CacheFileConfig);

            _cancellationTokenSource = new CancellationTokenSource();

            _siteComponents.Clear();       // Sanity check

            // Add listener component
            var listenerComponent = new ListenerComponent(_logWriter, _siteData, 
                                                    _cancellationTokenSource.Token);
            _siteComponents.Add(listenerComponent);

            // Add requests component
            var requestsComponent = new RequestsComponent(_fileCacheService, _logWriter, 
                                                    _siteData, _webRequestHandlerFactory, _cancellationTokenSource.Token);
            _siteComponents.Add(requestsComponent);

            // Start components
            _siteComponents.ForEach(component => component.Start());
        }

        public void Stop()
        {
            // Instruct components to cancel processing
            _cancellationTokenSource.Cancel();

            // Stop components            
            while (_siteComponents.Any())
            {
                _siteComponents[0].Stop();
                _siteComponents.RemoveAt(0);
            }
        }

        public bool IsStarted => _siteComponents.Any();
    }
}
