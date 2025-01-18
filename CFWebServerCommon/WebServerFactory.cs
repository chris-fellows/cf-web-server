using CFWebServer.Constants;
using CFWebServer.Extensions;
using CFWebServer;
using CFWebServer.Interfaces;
using CFWebServer.Models;
using Microsoft.Extensions.DependencyInjection;

namespace CFWebServerCommon
{
    public class WebServerFactory : IWebServerFactory
    {        
        private readonly IMimeTypeDatabase _mimeTypeDatabase;
        private readonly IServerNotifications _serverNotifications;
        private readonly IServiceProvider _serviceProvider;
        private readonly ISiteConfigService _siteConfigService;

        public WebServerFactory(IMimeTypeDatabase mimeTypeDatabase,
                                IServerNotifications serverNotifications,
                                IServiceProvider serviceProvider,
                                ISiteConfigService siteConfigService)
        {                 
            _mimeTypeDatabase = mimeTypeDatabase;
            _serverNotifications = serverNotifications;
            _serviceProvider = serviceProvider;
            _siteConfigService = siteConfigService;
        }

        public IWebServer CreateInternalWebServer(string apiKey, string site)
        {
            using (var scope = _serviceProvider.CreateSiteConfigScope("Internal"))
            {              
                // Set site config. Only really use the Site property
                var siteConfig = new SiteConfig()
                {
                    Name = "Internal",
                    CacheFileConfig = new FileCacheConfig(),
                    FolderConfigs = new List<FolderConfig>(),
                    Enabled = true,
                    MaxConcurrentRequests = 10,
                    Site = site,
                    AuthorizationRules = new List<AuthorizationRule>()
                    {
                           new AuthorizationRule()
                            {
                                Id = "API_KEY_RULE",
                                HeaderName = "Authorization",
                                Scheme = "Apikey",       // Apikey [key], Bearer [JWT]
                                APIKey = apiKey
                            }
                    },
                    RouteRules = new List<RouteRule>()
                    {
                        // Site config handlers
                        new RouteRule()
                        {
                            WebRequestHandlerName = WebRequestHandlerNames.PostSiteConfig,
                            Methods = new List<string>() { "POST" },
                            RelativePathPatterns = new List<string>()
                            {
                                "/siteConfig"
                            },
                            AuthorizationRuleIds = new List<string>()
                            {
                                "API_KEY_RULE"
                            }
                        },
                        new RouteRule()
                        {
                            WebRequestHandlerName = WebRequestHandlerNames.PutSiteConfig,
                            Methods = new List<string>() { "PUT" },
                            RelativePathPatterns = new List<string>()
                            {
                                "/siteConfig"
                            },
                            AuthorizationRuleIds = new List<string>()
                            {
                                "API_KEY_RULE"
                            }
                        },
                        new RouteRule()
                        {
                            WebRequestHandlerName = WebRequestHandlerNames.GetSiteConfigs,
                            Methods = new List<string>() { "GET" },
                            RelativePathPatterns = new List<string>()
                            {
                                "/siteConfig"
                            },
                            AuthorizationRuleIds = new List<string>()
                            {
                                "API_KEY_RULE"
                            }
                        },
                        new RouteRule()
                        {
                            WebRequestHandlerName = WebRequestHandlerNames.GetSiteConfig,
                            Methods = new List<string>() { "GET" },
                            RelativePathPatterns = new List<string>()
                            {
                                "/siteConfig/*"
                            },
                            AuthorizationRuleIds = new List<string>()
                            {
                                "API_KEY_RULE"
                            }
                        }
                    }
                };

                var logWriter = scope.ServiceProvider.GetRequiredService<ISiteLogWriter>();

                var cacheService = scope.ServiceProvider.GetRequiredService<ICacheService>();

                var fileCacheService = scope.ServiceProvider.GetRequiredService<IFileCacheService>();

                // Set server data
                var serverData = new ServerData() { SiteConfig = siteConfig };

                var authorizationManagers = scope.ServiceProvider.GetServices<IAuthorizationManager>();

                IWebRequestHandlerFactory webRequestHandlerFactoryInternal = new WebRequestHandlerFactory(authorizationManagers,
                                                                            fileCacheService,
                                                                            _mimeTypeDatabase, _siteConfigService);

                // Initialise web server for internal
                var webServerInternal = new WebServer(cacheService,
                                                fileCacheService,
                                                logWriter,
                                                serverData,
                                                _serverNotifications,
                                                _siteConfigService,
                                                webRequestHandlerFactoryInternal);

                return webServerInternal;
            }
        }

        public IWebServer CreateWebServerById(string siteConfigId)
        {
            using (var scope = _serviceProvider.CreateSiteConfigScope(siteConfigId))
            {
                // Get site config
                var siteConfig = _siteConfigService.GetById(siteConfigId);
                if (siteConfig == null)
                {
                    throw new ArgumentException($"Site config for Id {siteConfigId} does not exist");
                }

                // Set default route rules if not set
                if (!siteConfig.RouteRules.Any())
                {
                    siteConfig.RouteRules = new List<RouteRule>()
                    {
                        // Static content handlers
                        new RouteRule()
                        {
                            Priority = 1,
                            WebRequestHandlerName = WebRequestHandlerNames.StaticResourceDelete,
                            Methods = new List<string>() { "DELETE" }
                        },
                        new RouteRule()
                        {
                            Priority = 1,
                            WebRequestHandlerName = WebRequestHandlerNames.StaticResourceGet,
                            Methods = new List<string>() { "GET" }
                        },
                        new RouteRule()
                        {
                            Priority = 1,
                            WebRequestHandlerName = WebRequestHandlerNames.StaticResourcePost,
                            Methods = new List<string>() { "POST" }
                        },
                        new RouteRule()
                        {
                            Priority = 1,
                            WebRequestHandlerName = WebRequestHandlerNames.StaticResourcePut,
                            Methods = new List<string>() { "PUT" }
                        },

                        // PowerShell script handler
                        new RouteRule()
                        {
                            Priority = 2,   // Picked about GET for static content
                            WebRequestHandlerName = WebRequestHandlerNames.PowerShell,
                            Methods = new List<string>(),
                            RelativePathPatterns = new List<string>()
                            {
                                "#extension#:.ps1"
                            }
                        }
                    };
                }

                var logWriter = scope.ServiceProvider.GetRequiredService<ISiteLogWriter>();

                var cacheService = scope.ServiceProvider.GetRequiredService<ICacheService>();

                var fileCacheService = scope.ServiceProvider.GetRequiredService<IFileCacheService>();

                // Set server data
                var serverData = new ServerData() { SiteConfig = siteConfig };

                var authorizationManagers = scope.ServiceProvider.GetServices<IAuthorizationManager>();

                // Set web request handler factory
                IWebRequestHandlerFactory webRequestHandlerFactory = new WebRequestHandlerFactory(authorizationManagers, fileCacheService,
                                                                                _mimeTypeDatabase, _siteConfigService);

                // Initialise web server
                var webServer = new WebServer(cacheService,
                                                fileCacheService,
                                                logWriter,
                                                serverData,
                                                _serverNotifications,                                                
                                                _siteConfigService,                                                
                                                webRequestHandlerFactory);

                return webServer;
            }
        }
    }
}
