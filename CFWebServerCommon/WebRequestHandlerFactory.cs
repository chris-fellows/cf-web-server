using CFWebServer.Constants;
using CFWebServer.Interfaces;
using CFWebServer.Models;
using CFWebServer.WebRequestHandlers;
using CFWebServerCommon.Models;
using CFWebServerCommon.WebRequestHandlers;

namespace CFWebServer
{
    /// <summary>
    /// Factory for IWebRequestHandler instances
    /// </summary>
    public class WebRequestHandlerFactory : IWebRequestHandlerFactory
    {        
        private readonly IFileCacheService _fileCacheService;        
        private readonly List<RouteRule> _routeRules;
        private readonly ISiteConfigService _siteConfigService;

        public WebRequestHandlerFactory(IFileCacheService fileCacheService,                                        
                                        List<RouteRule> routeRules,
                                        ISiteConfigService siteConfigService)
        {            
            _fileCacheService = fileCacheService;            
            _routeRules = routeRules;
            _siteConfigService = siteConfigService;
        }     

        private List<IWebRequestHandler> GetAll(ServerData serverData)
        {
            // TODO: Use DI or reflection            
            var list = new List<IWebRequestHandler>()
            {
                // Static resources
                new StaticResourceDeleteWebRequestHandler(_fileCacheService, serverData),
                new StaticResourceGetWebRequestHandler(_fileCacheService, serverData),
                new StaticResourcePostWebRequestHandler(_fileCacheService, serverData),
                new StaticResourcePutWebRequestHandler(_fileCacheService, serverData),

                // Site config
                new GetSiteConfigsWebRequestHandler(_fileCacheService, serverData, _siteConfigService),
                new UpdateSiteConfigWebRequestHandler(_fileCacheService, serverData, _siteConfigService),

                // Other
                new NotFoundWebRequestHandler(_fileCacheService, serverData)
            };

            return list;
        }

        public IWebRequestHandler? Get(RequestContext requestContext, ServerData serverData)
        {
            var relativePath = requestContext.Request.Url.AbsolutePath;

            // Filter route rules by method
            var routeRules = _routeRules.Where(rr => rr.Methods == null ||
                                                !rr.Methods.Any() ||
                                                (rr.Methods.Any() && rr.Methods.Contains(requestContext.Request.HttpMethod))).ToList();

            // Filter route rules by relative path
            routeRules = routeRules.Where(rr => rr.RelativePaths == null ||
                                                !rr.RelativePaths.Any() ||
                                                (rr.RelativePaths.Any() && rr.RelativePaths.Contains(relativePath))).ToList();

            // Get all web request handlers
            var allWebRequestHandlers = GetAll(serverData);

            // Get web request handlers than can handle this request
            var webRequestHandlers = allWebRequestHandlers.Where(h => h.Name != WebRequestHandlerNames.NotFound &&
                                        routeRules.Select(rr => rr.WebRequestHandlerName).Contains(h.Name)).ToList();
            
            // Return first
            // TODO: Do we need additional logic if more than one are valid?
            var webRequestHandler = webRequestHandlers.FirstOrDefault();
            if (webRequestHandler == null)
            {
                webRequestHandler = allWebRequestHandlers.FirstOrDefault(h => h.Name == WebRequestHandlerNames.NotFound);
            }
            return webRequestHandler;
        }
    }
}
