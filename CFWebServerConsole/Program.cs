using CFWebServer;
using CFWebServer.AuthorizationManagers;
using CFWebServer.Interfaces;
using CFWebServer.MimeTypes;
using CFWebServer.Models;
using CFWebServer.Services;
using CFWebServer.LogWriters;
using CFWebServer.Utilities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Net;

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
            var localIP = GetLocalIP(true);
            Console.WriteLine($"Local IP: {localIP}");
            
            // Create sites to start
            IWebServer webServer = new WebServer();

            var siteConfigService = serviceProvider.GetRequiredService<ISiteConfigService>();
            var siteFactory = serviceProvider.GetRequiredService<ISiteFactory>();

            // Create default site if no sites
            var siteConfigs = siteConfigService.GetAll();            
            if (!siteConfigs.Any())
            {
                var siteRootFolder = Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location), "Root", "Test1");
                //siteConfigs.Add(CreateDefaultSite(siteRootFolder, "Test 1", "http://0.0.0.0:10010/", siteConfigService, siteFactory));   // Errors on listen
                //siteConfigs.Add(CreateDefaultSite(siteRootFolder, "Test 1", "http://localhost:10010/", siteConfigService, siteFactory));
                siteConfigs.Add(CreateDefaultSite(siteRootFolder, "Test 1", $"http://{localIP}:10010/", siteConfigService, siteFactory));
            }

            // Create sites
            CreateSites(siteConfigService, siteFactory, siteConfigs).ForEach(site => webServer.Add(site));

            // Start sites
            webServer.Sites.ForEach(site => site.Start());

            /*
            // Wait for user to press escape
            do
            {
                Thread.Sleep(100);
                Thread.Yield();
            } while (true);
            */
            
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
        
        private static SiteConfig CreateDefaultSite(string folder,
                                                string name,
                                                string site,
                                               ISiteConfigService siteConfigService,
                                               ISiteFactory siteFactory)
        {
            Console.WriteLine($"Creating default site in {folder}");

            WebSiteUtilities.CreateSimpleWebsite(folder);

            var siteConfig = new SiteConfig()
            {
                Id = Guid.NewGuid().ToString(),
                Name = name,
                DefaultFile = "Index.html",
                MaxConcurrentRequests = 20,
                Enabled = true,
                RootFolder = folder,
                Site = site,
                CacheFileConfig = new FileCacheConfig()
                {
                    Compressed = true,
                    Expiry = TimeSpan.Zero,     // Cache disabled,
                    MaxFileSizeBytes = 1024 * 1000,
                    MaxTotalSizeBytes = 1024 * 10000
                },
                //FolderConfigs = new List<FolderConfig>()
                //        {
                //            new FolderConfig()
                //            {
                //                Id = Guid.NewGuid().ToString(),
                //                RelativePath = "/",     // Root
                //                Permissions = new List<FolderPermissions>()
                //                {
                //                    FolderPermissions.Read
                //                }
                //            }
                //        }
            };
            siteConfigService.Add(siteConfig);

            //var site = siteFactory.CreateSiteById(siteConfig.Id);
            //return site;

            Console.WriteLine("Creating default site");

            return siteConfig;
        }

        /// <summary>
        /// Gets sites to start
        /// </summary>
        /// <param name="siteConfigService"></param>
        /// <param name="siteFactory"></param>
        /// <returns></returns>
        private static List<ISite> CreateSites(ISiteConfigService siteConfigService,
                                               ISiteFactory siteFactory,
                                               List<SiteConfig> siteConfigs)
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
                foreach (var siteConfig in siteConfigs.Where(sc => sc.Enabled))
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

        private static string GetLocalIP(bool listAddresses)
        {
            string hostName = Dns.GetHostName(); // Retrive the Name of HOST
            
            var hostEntry = Dns.GetHostEntry(Dns.GetHostName());

            if (listAddresses)
            {
                foreach(var address in hostEntry.AddressList)
                {
                    Console.WriteLine($"IP: {address.ToString()}");
                }
            }            

            return hostEntry.AddressList[0].ToString();
        }
    }
}
