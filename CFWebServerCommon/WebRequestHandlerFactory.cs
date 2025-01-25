using CFWebServer.Constants;
using CFWebServer.Interfaces;
using CFWebServer.Models;
using CFWebServer.Utilities;
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
        private readonly IServerNotifications _serverNotifications;
        private readonly ISiteConfigService _siteConfigService;

        public WebRequestHandlerFactory(IEnumerable<IAuthorizationManager> authorizationManagers,
                                        IFileCacheService fileCacheService,                                        
                                        IMimeTypeDatabase mimeTypeDatabase,                                        
                                        IServerNotifications serverNotifications,
                                        ISiteConfigService siteConfigService)
        {
            _authorizationManagers = authorizationManagers.ToList();
            _fileCacheService = fileCacheService;
            _mimeTypeDatabase = mimeTypeDatabase;
            _serverNotifications = serverNotifications;
            _siteConfigService = siteConfigService;
        }     

        private List<IWebRequestHandler> GetAll(SiteData siteData)
        {
            // TODO: Use DI or reflection            
            var list = new List<IWebRequestHandler>()
            {
                // Static resources
                new StaticResourceDeleteWebRequestHandler(_fileCacheService, _mimeTypeDatabase, siteData),
                new StaticResourceGetWebRequestHandler(_fileCacheService, _mimeTypeDatabase, siteData),
                new StaticResourcePostWebRequestHandler(_fileCacheService,_mimeTypeDatabase, siteData),
                new StaticResourcePutWebRequestHandler(_fileCacheService, _mimeTypeDatabase, siteData),

                // Site config
                new GetSiteConfigWebRequestHandler(_fileCacheService, _mimeTypeDatabase, siteData, _siteConfigService),
                new GetSiteConfigsWebRequestHandler(_fileCacheService, _mimeTypeDatabase, siteData, _siteConfigService),
                new PostOrPutSiteConfigWebRequestHandler(_fileCacheService, _mimeTypeDatabase, WebRequestHandlerNames.PostSiteConfig, 
                                    _serverNotifications, siteData, _siteConfigService),
                new PostOrPutSiteConfigWebRequestHandler(_fileCacheService, _mimeTypeDatabase, WebRequestHandlerNames.PutSiteConfig, 
                                    _serverNotifications, siteData, _siteConfigService),

                // PowerShell resources (.ps1)
                new PowerShellWebRequestHandler(_fileCacheService, _mimeTypeDatabase, siteData),

                // Specific status code
                new StatusCodeWebRequestHandler(_fileCacheService, WebRequestHandlerNames.StatusCodeNotFound, HttpStatusCode.NotFound, 
                                    _mimeTypeDatabase,  siteData),
                new StatusCodeWebRequestHandler(_fileCacheService, WebRequestHandlerNames.StatusCodeUnauthorized, HttpStatusCode.Unauthorized, 
                                    _mimeTypeDatabase,  siteData)
            };            

            return list;
        }

        /// <summary>
        /// Whether request is authorized. E.g. Valid API key
        /// </summary>
        /// <param name="requestContext"></param>
        /// <param name="routeRule"></param>
        /// <param name="allAuthorizationRules"></param>
        /// <returns></returns>
        private bool IsAuthorized(RequestContext requestContext, RouteRule routeRule, List<AuthorizationRule> siteConfigAuthorizationRules)
        {
            if (routeRule.AuthorizationRuleIds == null || !routeRule.AuthorizationRuleIds.Any())
            {
                return true;
            }

            foreach(var authorizationRuleId in routeRule.AuthorizationRuleIds)
            {
                // Get authorization rule
                var authorizationRule = siteConfigAuthorizationRules.First(ar => ar.Id == authorizationRuleId);

                // Check authorization rule
                var authorizationManager = _authorizationManagers.First(am => am.Scheme == authorizationRule.Scheme);
                if (authorizationManager.IsAuthorized(requestContext, authorizationRule))
                {
                    return true;
                }
            }

            return false;            
        }

        /// <summary>
        /// Gets the route rule for the request. If more than two final routes then return the highest priority. Static web 
        /// request handler typically has lowest priority.
        /// </summary>
        /// <param name="requestContext"></param>
        /// <param name="siteConfig"></param>
        /// <returns></returns>
        private RouteRule? GetRouteRule(RequestContext requestContext, SiteConfig siteConfig)
        {
            var relativePath = requestContext.Request.Url.AbsolutePath;

            var urlFileExtension = HttpUtilities.GetResourceExtension(relativePath);

            var request = requestContext.Request;

            // Filter route rules by method
            var routeRules = siteConfig.RouteRules.Where(rr => rr.Methods == null ||
                                                !rr.Methods.Any() ||
                                                (rr.Methods.Any() && rr.Methods.Contains(request.HttpMethod))).ToList();            

            // Filter route rules by relative path:
            // - No specific path rule.
            // - Specific file extension for resource.
            // - URL pattern.
            routeRules = routeRules.Where(rr => rr.RelativePathPatterns == null ||
                                                !rr.RelativePathPatterns.Any() ||
                                                (rr.RelativePathPatterns.Any(rr => rr.Equals($"#extension#:{urlFileExtension}"))) ||
                                                (rr.RelativePathPatterns.Any() && rr.RelativePathPatterns.Any(rp => HttpUtilities.IsRelativeUrlMatchesPattern(relativePath, rp, '*')))).ToList();

            // Return highest priority rule
            return routeRules.OrderBy(rr => rr.Priority).LastOrDefault();
        }
       
        public IWebRequestHandler? Get(RequestContext requestContext, SiteData siteData)
        {
            var relativePath = requestContext.Request.Url.AbsolutePath;

            var request = requestContext.Request;

            // Get route rule
            var routeRule = GetRouteRule(requestContext, siteData.SiteConfig);

            // Get all web request handlers
            var allWebRequestHandlers = GetAll(siteData);

            // Get web request handler for route rule
            IWebRequestHandler? webRequestHandler = null;           
            if (routeRule != null)
            {
                // Check authorization valid (E.g. API key set)
                if (!IsAuthorized(requestContext, routeRule, siteData.SiteConfig.AuthorizationRules))
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
