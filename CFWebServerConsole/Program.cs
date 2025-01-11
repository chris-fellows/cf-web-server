using CFWebServer;
using CFWebServer.Constants;
using CFWebServer.Interfaces;
using CFWebServer.Models;
using CFWebServer.Services;
using CFWebServerCommon.Models;

// NOTES:
// - We start in one of the following modes:
//      a) All websites with a site config. Requires /all-sites command line param.
//      b) Website with a site config. Requires /site-config-id command line param.
//      c) Website without a site config. Requires other command line params.
// - Each WebServer instance serves one website. We pass in seperate dependencies for each because each site 
//   is independent.
// - We create an internal website which handles site config requests. E.g. Add site, update site permissions.

// Create log writer
var logWriter = new DefaultLogWriter();
logWriter.Log("Starting CF Web Server");

// Set default site config
var siteConfig = new SiteConfig()
{
    Id = "1",
    DefaultFile = "Index.html",
    MaxConcurrentRequests = 10,
    RootFolder = Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location), "Root"),
    Site = "",
    Name = "Default",    
    CacheFileConfig = new FileCacheConfig()
    {
        Compressed = true,
        Expiry = TimeSpan.FromSeconds(900),
        MaxFileSizeBytes = 1024 * 1000,
        MaxTotalSizeBytes = 1024 * 10000
    }
};


// Set folder for config data
var configFolder = Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location), "Config");

// Set services for config data
//var folderConfigService = new XmlFolderConfigService(Path.Combine(configFolder, "FolderConfig"));
var siteConfigService = new XmlSiteConfigService(Path.Combine(configFolder, "SiteConfig"));

// Process command line arguments. Use defaults if necessary
TimeSpan logStatisticsInterval = TimeSpan.FromSeconds(300);

var internalSite = "http://localhost:10011/";
var siteConfigsToStart = new List<SiteConfig>();
foreach (var arg in Environment.GetCommandLineArgs())
{
    if (arg.StartsWith("/all-sites"))
    {
        siteConfigsToStart.AddRange(siteConfigService.GetAll().Where(sc => sc.Enabled).ToList());
        if (!siteConfigsToStart.Any())
        {
            throw new ArgumentException("No websites to start");
        }
    }
    else if (arg.StartsWith("/default-file="))
    {
        siteConfig.DefaultFile = arg.Split('=')[1].Trim();
    }
    else if (arg.StartsWith("/file-cache-compressed="))
    {
        siteConfig.CacheFileConfig.Compressed = Convert.ToBoolean(arg.Split('=')[1]);
    }
    else if (arg.StartsWith("/file-cache-expiry-secs="))   // 0=Cache disabled
    {
        siteConfig.CacheFileConfig.Expiry = TimeSpan.FromSeconds(Convert.ToInt32(arg.Split('=')[1]));
    }
    else if (arg.StartsWith("/file-cache-disabled"))
    {
        siteConfig.CacheFileConfig.Expiry = TimeSpan.Zero;
    }
    else if (arg.StartsWith("/log-statistics-secs="))
    {
        logStatisticsInterval = TimeSpan.FromSeconds(Convert.ToInt32(arg.Split('=')[1]));
    }
    else if (arg.StartsWith("/max-cached-file-size="))
    {
        siteConfig.CacheFileConfig.MaxFileSizeBytes = Convert.ToInt32(arg.Split('=')[1]);
    }
    else if (arg.StartsWith("/max-concurrent-requests="))
    {
        siteConfig.MaxConcurrentRequests = Convert.ToInt32(arg.Split('=')[1]);
    }
    else if (arg.StartsWith("/max-file-cache-size="))
    {
        siteConfig.CacheFileConfig.MaxTotalSizeBytes = Convert.ToInt32(arg.Split('=')[1]);
    }    
    else if (arg.StartsWith("/root=")) 
    {        
        var root = arg.Split('=')[1].Trim();
        root = root.Replace("{process-path}", Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location));
        siteConfig.RootFolder = root;
    }
    else if (arg.StartsWith("/site"))
    {
        siteConfig.Site = arg.Split('=')[1].Trim();
    }
    else if (arg.StartsWith("/site-config-id"))     // Existing site config
    {
        siteConfig.Id= arg.Split('=')[1].Trim();

        var siteConfig2 = siteConfigService.GetById(siteConfig.Id);
        if (siteConfig2 == null)
        {
            throw new Exception($"Site config for Id {siteConfig.Id} does not exist");
        }
        siteConfigsToStart.Add(siteConfig2);
    }
    else if (arg.StartsWith("/internal-site="))
    {
        internalSite = arg.Split('=')[1].Trim();
    }
}

if (!siteConfigsToStart.Any())    // Use command line site settings
{
    siteConfigsToStart.Add(siteConfig);
}

// Set cancellation token source
CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

// Set server event queue. Shared between websites so that an update to internal website can be notified to 
// the other websites
var serverEventQueue = new ServerEventQueue();

var webServers = new List<IWebServer>();   // Web server, internal web server

// ------------------------------------------------------------------------------------------------------------------------
// Create internal web server

// Set site config. Only really use the Site property
var siteConfigInternal = new SiteConfig()
{    
    Name = "Internal",
    CacheFileConfig= new FileCacheConfig(),    
    FolderConfigs =new List<FolderConfig>(),
    Enabled = true,    
    MaxConcurrentRequests = 10,
    Site = internalSite
};

// Set cache service.
var cacheServiceInternal = new LocalMemoryCache();

// Set file cache service
var fileCacheServiceInternal = new LocalMemoryFileCache(cacheServiceInternal, siteConfigInternal.CacheFileConfig);

// Set server data
var serverDataInternal = new ServerData(TimeSpan.Zero, siteConfigInternal);

// Set route rules
var routeRulesInternal = new List<RouteRule>()
{
     // Site config handlers
    new RouteRule()
    {
        WebRequestHandlerName = WebRequestHandlerNames.UpdateSiteConfig,
        Methods = new List<string>() { "POST" },
        RelativePaths = new List<string>()
        {
            "/siteConfig"
        }
    },
    new RouteRule()
    {
        WebRequestHandlerName = WebRequestHandlerNames.GetSiteConfigs,
        Methods = new List<string>() { "GET" },
        RelativePaths = new List<string>()
        {
            "/siteConfig"
        }
    }
};

IWebRequestHandlerFactory webRequestHandlerFactoryInternal = new WebRequestHandlerFactory(fileCacheServiceInternal, routeRulesInternal, siteConfigService);

// Initialise web server for internal
var webServerInternal = new WebServer(cacheServiceInternal,
                                fileCacheServiceInternal,
                                logWriter,
                                serverDataInternal,
                                serverEventQueue,
                                siteConfigService,
                                webRequestHandlerFactoryInternal,
                                cancellationTokenSource.Token);

// Add to web servers
webServers.Add(webServerInternal);

// ------------------------------------------------------------------------------------------------------------------------
// Create each web site
foreach (var currentSiteConfig in siteConfigsToStart)
{
    // Set cache service.
    var cacheService = new LocalMemoryCache();

    // Set file cache service
    var fileCacheService = new LocalMemoryFileCache(cacheService, currentSiteConfig.CacheFileConfig);

    // Set server data
    var serverData = new ServerData(logStatisticsInterval, currentSiteConfig);

    // Set route rules
    var routeRules = new List<RouteRule>()
    {
        // Static content handlers
        new RouteRule()
        {
            WebRequestHandlerName = WebRequestHandlerNames.StaticResourceDelete,
            Methods = new List<string>() { "DELETE" }
        },
        new RouteRule()
        {
            WebRequestHandlerName = WebRequestHandlerNames.StaticResourceGet,
            Methods = new List<string>() { "GET" }
        },
        new RouteRule()
        {
            WebRequestHandlerName = WebRequestHandlerNames.StaticResourcePost,
            Methods = new List<string>() { "POST" }
        },
        new RouteRule()
        {
            WebRequestHandlerName = WebRequestHandlerNames.StaticResourcePut,
            Methods = new List<string>() { "PUT" }
        }
    };

    // Set web request handler factory
    IWebRequestHandlerFactory webRequestHandlerFactory = new WebRequestHandlerFactory(fileCacheService, routeRules, siteConfigService);

    // Initialise web server
    var webServer = new WebServer(cacheService,
                                    fileCacheService,
                                    logWriter,
                                    serverData,
                                    serverEventQueue,
                                    siteConfigService,
                                    webRequestHandlerFactory,
                                    cancellationTokenSource.Token);

    // Add to web servers
    webServers.Add(webServer);
}

// Start web servers servers
webServers.ForEach(ws => ws.Start());

// Wait for user to press escape
Console.WriteLine("Press ESCAPE to stop");
do
{
    while (!Console.KeyAvailable)
    {
        Thread.Sleep(100);
        Thread.Yield();
    }
} while (Console.ReadKey(true).Key != ConsoleKey.Escape);

logWriter.Log("Terminating CF Web Server");
