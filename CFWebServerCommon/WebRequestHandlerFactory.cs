using CFWebServer.Constants;
using CFWebServer.Interfaces;
using CFWebServer.Models;
using CFWebServer.WebRequestHandlers;
using System.Net;

namespace CFWebServer
{
    /// <summary>
    /// Factory for IWebRequestHandler instances
    /// </summary>
    public class WebRequestHandlerFactory : IWebRequestHandlerFactory
    {
        private readonly List<IAuthorizationManager> _authorizationManagers;
        private readonly IFileCacheService _fileCacheService;
        private readonly IMimeTypeDatabase _mimeTypeDatabase;
        private readonly List<RouteRule> _routeRules;
        private readonly ISiteConfigService _siteConfigService;

        public WebRequestHandlerFactory(IEnumerable<IAuthorizationManager> authorizationManagers,
                                        IFileCacheService fileCacheService,                                        
                                        IMimeTypeDatabase mimeTypeDatabase,
                                        List<RouteRule> routeRules,
                                        ISiteConfigService siteConfigService)
        {
            _authorizationManagers = authorizationManagers.ToList();
            _fileCacheService = fileCacheService;
            _mimeTypeDatabase = mimeTypeDatabase;
            _routeRules = routeRules;
            _siteConfigService = siteConfigService;
        }     

        private List<IWebRequestHandler> GetAll(ServerData serverData)
        {
            // TODO: Use DI or reflection            
            var list = new List<IWebRequestHandler>()
            {
                // Static resources
                new StaticResourceDeleteWebRequestHandler(_fileCacheService, _mimeTypeDatabase, serverData),
                new StaticResourceGetWebRequestHandler(_fileCacheService, _mimeTypeDatabase, serverData),
                new StaticResourcePostWebRequestHandler(_fileCacheService,_mimeTypeDatabase, serverData),
                new StaticResourcePutWebRequestHandler(_fileCacheService, _mimeTypeDatabase, serverData),

                // Site config
                new GetSiteConfigsWebRequestHandler(_fileCacheService, _mimeTypeDatabase, serverData, _siteConfigService),
                new UpdateSiteConfigWebRequestHandler(_fileCacheService, _mimeTypeDatabase, serverData, _siteConfigService),

                // Specific status code
                new StatusCodeWebRequestHandler(_fileCacheService, WebRequestHandlerNames.StatusCodeNotFound, HttpStatusCode.NotFound, _mimeTypeDatabase,  serverData),
                new StatusCodeWebRequestHandler(_fileCacheService, WebRequestHandlerNames.StatusCodeUnauthorized, HttpStatusCode.Unauthorized, _mimeTypeDatabase,  serverData)
            };            

            return list;
        }

        /// <summary>
        /// Whether request is authorized. E.g. Valid API key
        /// </summary>
        /// <param name="requestContext"></param>
        /// <param name="routeRule"></param>
        /// <returns></returns>
        private bool IsAuthorized(RequestContext requestContext, RouteRule routeRule)
        {
            if (routeRule.AuthorizationRules == null || !routeRule.AuthorizationRules.Any())
            {
                return true;
            }

            foreach(var authorizationRule in routeRule.AuthorizationRules)
            {
                var authorizationManager = _authorizationManagers.First(am => am.Scheme == authorizationRule.Scheme);
                if (authorizationManager.IsAuthorized(requestContext, authorizationRule))
                {
                    return true;
                }
            }

            return false;            
        }
       
        public IWebRequestHandler? Get(RequestContext requestContext, ServerData serverData)
        {
            var relativePath = requestContext.Request.Url.AbsolutePath;

            var request = requestContext.Request;

            // Filter route rules by method
            var routeRules = _routeRules.Where(rr => rr.Methods == null ||
                                                !rr.Methods.Any() ||
                                                (rr.Methods.Any() && rr.Methods.Contains(request.HttpMethod))).ToList();

            // Filter route rules by relative path
            routeRules = routeRules.Where(rr => rr.RelativePaths == null ||
                                                !rr.RelativePaths.Any() ||
                                                (rr.RelativePaths.Any() && rr.RelativePaths.Contains(relativePath))).ToList();

            // Select first rule            
            var routeRule = routeRules.FirstOrDefault();            

            // Get all web request handlers
            var allWebRequestHandlers = GetAll(serverData);

            // Get web request handler for route rule
            IWebRequestHandler? webRequestHandler = null;           
            if (routeRule != null)
            {
                // Check authorization valid (E.g. API key set)
                if (!IsAuthorized(requestContext, routeRule))
                {
                    return allWebRequestHandlers.FirstOrDefault(h => h.Name == WebRequestHandlerNames.StatusCodeUnauthorized);                    
                }
               
                var webRequestHandlers = allWebRequestHandlers.Where(h => h.Name != WebRequestHandlerNames.StatusCodeNotFound &&
                                                 h.Name == routeRule.WebRequestHandlerName).ToList();

                webRequestHandler = webRequestHandlers.FirstOrDefault();
            }
           
            // If no web request handler then return Not Found
            if (webRequestHandler == null)
            {
                webRequestHandler = allWebRequestHandlers.FirstOrDefault(h => h.Name == WebRequestHandlerNames.StatusCodeNotFound);
            }           
            
            return webRequestHandler;
        }
    }
}
