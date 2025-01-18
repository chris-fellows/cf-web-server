using CFWebServer;
using CFWebServer.AuthorizationManagers;
using CFWebServer.Interfaces;
using CFWebServer.Models;
using CFWebServer.Services;
using CFWebServerCommon;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace CFWebServerConsole
{
    /// <summary>
    /// NOTES:
    /// - We start in one of the following modes:
    ///      a) All enabled websites with a config. If /sit-config-id command line param not set.
    ///      b) Website with a site config. Requires /site-config-id command line param.    
    /// - Each WebServer instance serves one website. We pass in seperate dependencies for each because each site 
    ///   is independent.
    /// - We create an internal website which handles site config requests. E.g. Add site, update site permissions.
    /// - Logs are separate per website.
    /// </summary>
    internal class Program
    {
        private static void Main(string[] args)
        {
            var serviceProvider = CreateServiceProvider();
                        
            Console.WriteLine("Starting CF Web Server");

            // Create web servers to start
            var webServers = CreateWebServers(serviceProvider.GetRequiredService<ISiteConfigService>(),
                                            serviceProvider.GetRequiredService<IWebServerFactory>());

            // Start web servers
            webServers.ForEach(webServer => webServer.Start());

            // Wait for user to press escape
            do
            {
                Console.WriteLine("Press ESCAPE to stop");  // Also displayed if user presses other key
                while (!Console.KeyAvailable)
                {
                    Thread.Sleep(100);
                    Thread.Yield();
                }
            } while (Console.ReadKey(true).Key != ConsoleKey.Escape);

            Console.WriteLine("Terminating CF Web Server");

            // Stop web servers
            webServers.ForEach(webServer => webServer.Stop());

            Console.WriteLine("Terminated CF Web Server");
        }

        /// <summary>
        /// Gets web servers to start
        /// </summary>
        /// <param name="siteConfigService"></param>
        /// <param name="webServerFactory"></param>
        /// <returns></returns>
        private static List<IWebServer> CreateWebServers(ISiteConfigService siteConfigService,
                                                                IWebServerFactory webServerFactory)
        {
            // Process command line args
            var internalSite = "";
            var siteCongfigId = "";
            foreach (var arg in Environment.GetCommandLineArgs())
            {
                if (arg.StartsWith("/site-config-id="))     // Existing site config by Id
                {
                    siteCongfigId = arg.Split('=')[1].Trim();
                }
                else if (arg.StartsWith("/site-config-name="))   // Existing site config by Name
                {
                    siteCongfigId = arg.Split('=')[1].Trim();
                    siteCongfigId = siteConfigService.GetAll().First(sc => sc.Name.Equals(siteCongfigId, StringComparison.InvariantCultureIgnoreCase)).Id;
                }
                else if (arg.StartsWith("/site-internal-site="))   // Internal site
                {
                    internalSite = arg.Split('=')[1].Trim();
                }
            }

            var webServers = new List<IWebServer>();
            
            // Create internal web server (Website management)
            if (!String.IsNullOrEmpty(internalSite))
            {
                webServers.Add(webServerFactory.CreateInternalWebServer("111111", internalSite));
            }

            // Create web server(s), either specific site or all enabled sites
            if (!String.IsNullOrEmpty(siteCongfigId))     // Specific site
            {
                webServers.Add(webServerFactory.CreateWebServerById(siteCongfigId));
            }
            else    // All enabled sites
            {
                var siteConfigs = siteConfigService.GetAll().Where(sc => sc.Enabled);

                foreach (var siteConfig in siteConfigs)
                {
                    webServers.Add(webServerFactory.CreateWebServerById(siteConfig.Id));
                }
            }

            return webServers;
        }

        private static IServiceProvider CreateServiceProvider()
        {
            var configFolder = Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location), "Config");
            var logFolder = Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location), "Log");

            var configuration = new ConfigurationBuilder()                
                .Build();

            var serviceProvider = new ServiceCollection()                
                .AddSingleton<IMimeTypeDatabase, StaticMimeTypeInfoDatabase>()
                .AddSingleton<IServerNotifications, ServerNotifications>()
                .AddSingleton<IWebServerFactory, WebServerFactory>()

                // Add site context so that we can create site scopes
                .AddScoped<ISiteContext, SiteContext>()

                // Set ILogWriter, site specific
                .AddScoped<ISiteLogWriter, CSVSiteLogWriter>((scope) =>
                {
                    ISiteContext siteContext = scope.GetRequiredService<ISiteContext>();
                    return new CSVSiteLogWriter(Path.Combine(logFolder, siteContext.SiteConfigId));
                })                

                .AddScoped<IAuthorizationManager, ApiKeyAuthorizationManager>()
                .AddScoped<IAuthorizationManager, BearerAuthorizationManager>()
                
                .AddScoped<ICacheService, LocalMemoryCache>()
                .AddScoped<IFileCacheService, LocalMemoryFileCache>()

                .AddScoped<IWebRequestHandlerFactory, WebRequestHandlerFactory>()                
                .AddScoped<IWebServer, WebServer>()

                .AddScoped<ISiteConfigService>((scope) =>
                {
                    return new XmlSiteConfigService(Path.Combine(configFolder, "SiteConfig"));
                })

                .BuildServiceProvider();

            return serviceProvider;
        }

        ///// <summary>
        ///// Registers all types implementing interface
        ///// </summary>
        ///// <typeparam name="T"></typeparam>
        ///// <param name="services"></param>
        ///// <param name="assemblies"></param>
        ///// <param name="lifetime"></param>
        //private static void RegisterAllTypes<T>(this IServiceCollection services, IEnumerable<Assembly> assemblies, ServiceLifetime lifetime = ServiceLifetime.Transient)
        //{
        //    var typesFromAssemblies = assemblies.SelectMany(a => a.DefinedTypes.Where(x => x.GetInterfaces().Contains(typeof(T))));
        //    foreach (var type in typesFromAssemblies)
        //    {
        //        services.Add(new ServiceDescriptor(typeof(T), type, lifetime));
        //    }
        //}
    }
}
