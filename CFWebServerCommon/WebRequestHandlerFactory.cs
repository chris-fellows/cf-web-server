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
        private readonly ISiteConfigService _siteConfigService;

        public WebRequestHandlerFactory(IEnumerable<IAuthorizationManager> authorizationManagers,
                                        IFileCacheService fileCacheService,                                        
                                        IMimeTypeDatabase mimeTypeDatabase,                                        
                                        ISiteConfigService siteConfigService)
        {
            _authorizationManagers = authorizationManagers.ToList();
            _fileCacheService = fileCacheService;
            _mimeTypeDatabase = mimeTypeDatabase;            
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
                new GetSiteConfigWebRequestHandler(_fileCacheService, _mimeTypeDatabase, serverData, _siteConfigService),
                new GetSiteConfigsWebRequestHandler(_fileCacheService, _mimeTypeDatabase, serverData, _siteConfigService),
                new PostOrPutSiteConfigWebRequestHandler(_fileCacheService, _mimeTypeDatabase, WebRequestHandlerNames.PostSiteConfig, serverData, _siteConfigService),
                new PostOrPutSiteConfigWebRequestHandler(_fileCacheService, _mimeTypeDatabase, WebRequestHandlerNames.PutSiteConfig, serverData, _siteConfigService),

                // PowerShell resources (.ps1)
                new PowerShellWebRequestHandler(_fileCacheService, _mimeTypeDatabase, serverData),

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
        /// Gets the route rule for the request
        /// </summary>
        /// <param name="requestContext"></param>
        /// <param name="serverData"></param>
        /// <returns></returns>
        private RouteRule? GetRouteRule(RequestContext requestContext, ServerData serverData)
        {
            var relativePath = requestContext.Request.Url.AbsolutePath;

            var urlFileExtension = HttpUtilities.GetResourceExtension(relativePath);

            var request = requestContext.Request;

            // Filter route rules by method
            var routeRules = serverData.SiteConfig.RouteRules.Where(rr => rr.Methods == null ||
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

            // Return first rule                        
            return routeRules.LastOrDefault();
        }
       
        public IWebRequestHandler? Get(RequestContext requestContext, ServerData serverData)
        {
            var relativePath = requestContext.Request.Url.AbsolutePath;

            var request = requestContext.Request;

            // Get route rule
            var routeRule = GetRouteRule(requestContext, serverData);

            // Get all web request handlers
            var allWebRequestHandlers = GetAll(serverData);

            // Get web request handler for route rule
            IWebRequestHandler? webRequestHandler = null;           
            if (routeRule != null)
            {
                // Check authorization valid (E.g. API key set)
                if (!IsAuthorized(requestContext, routeRule, serverData.SiteConfig.AuthorizationRules))
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
