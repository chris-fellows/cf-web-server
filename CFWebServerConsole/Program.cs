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
    /// - Each Site instance serves one website. We pass in seperate dependencies for each because each site 
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
            
            // Create sites to start
            IWebServer webServer = new WebServer();
            CreateSites(serviceProvider.GetRequiredService<ISiteConfigService>(),
                                            serviceProvider.GetRequiredService<ISiteFactory>())
                .ForEach(site => webServer.Add(site));

            // Start sites
            webServer.Sites.ForEach(site => site.Start());

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

            // Stop sites servers
            webServer.Sites.ForEach(site => site.Stop());

            Console.WriteLine("Terminated CF Web Server");
        }

        /// <summary>
        /// Gets sites to start
        /// </summary>
        /// <param name="siteConfigService"></param>
        /// <param name="siteFactory"></param>
        /// <returns></returns>
        private static List<ISite> CreateSites(ISiteConfigService siteConfigService,
                                               ISiteFactory siteFactory)
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

            var sites = new List<ISite>();
            
            // Create internal web server (Website management)
            if (!String.IsNullOrEmpty(internalSite))
            {
                sites.Add(siteFactory.CreateInternalSite("111111", internalSite));
            }

            // Create web server(s), either specific site or all enabled sites
            if (!String.IsNullOrEmpty(siteCongfigId))     // Specific site
            {
                sites.Add(siteFactory.CreateSiteById(siteCongfigId));
            }
            else    // All enabled sites
            {
                var siteConfigs = siteConfigService.GetAll().Where(sc => sc.Enabled);

                foreach (var siteConfig in siteConfigs)
                {
                    sites.Add(siteFactory.CreateSiteById(siteConfig.Id));
                }
            }

            return sites;
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
                .AddSingleton<ISiteFactory, SiteFactory>()

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
                .AddScoped<ISite, Site>()

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
