using CFWebServer;
using CFWebServer.Interfaces;
using CFWebServer.Models;
using CFWebServer.Services;

// NOTES:
// - Each WebServer instance serves one website. We should pass in separate dependencies for each WebServer
//   instance.
// - To support multiple websites then we'd need to add a config file with the list of settings per website.

// Create log writer
var logWriter = new DefaultLogWriter();
logWriter.Log("Starting CF Web Server");

// Process command line arguments. Use defaults if necessary
var defaultFile = "Index.html";
var fileCacheCompressed = true;   
TimeSpan fileCacheExpiry = TimeSpan.FromSeconds(900);       // If zero then cache is disabled
TimeSpan logStatisticsInterval = TimeSpan.FromSeconds(300);
int maxConcurrentRequests = 10;
int maxFileCacheBytes = 1024 * 10000;
int maxCachedFileSizeBytes = 1024 * 1000;
var root = Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location), "Root");
var site = "";    // "http://localhost:{_serverData.ReceivePort}/";

foreach (var arg in Environment.GetCommandLineArgs())
{
    if (arg.StartsWith("/default-file="))
    {
        defaultFile = arg.Split('=')[1].Trim();
    }
    else if (arg.StartsWith("/file-cache-compressed="))
    {
        fileCacheCompressed = Convert.ToBoolean(arg.Split('=')[1]);
    }
    else if (arg.StartsWith("/file-cache-expiry-secs="))   // 0=Cache disabled
    {
        fileCacheExpiry = TimeSpan.FromSeconds(Convert.ToInt32(arg.Split('=')[1]));
    }
    else if (arg.StartsWith("/file-cache-disabled"))
    {
        fileCacheExpiry = TimeSpan.Zero;
    }
    else if (arg.StartsWith("/log-statistics-secs="))
    {
        logStatisticsInterval = TimeSpan.FromSeconds(Convert.ToInt32(arg.Split('=')[1]));
    }
    else if (arg.StartsWith("/max-cached-file-size="))
    {
        maxCachedFileSizeBytes = Convert.ToInt32(arg.Split('=')[1]);
    }
    else if (arg.StartsWith("/max-concurrent-requests="))
    {
        maxConcurrentRequests = Convert.ToInt32(arg.Split('=')[1]);
    }
    else if (arg.StartsWith("/max-file-cache-size="))
    {
        maxFileCacheBytes = Convert.ToInt32(arg.Split('=')[1]);
    }    
    else if (arg.StartsWith("/root=")) 
    {        
        root = arg.Split('=')[1].Trim();
        root = root.Replace("{process-path}", Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location));        
    }
    else if (arg.StartsWith("/site"))
    {
        site = arg.Split('=')[1].Trim();
    }
}

var webServers = new List<WebServer>();

// Set server data
var serverData = new ServerData(defaultFile,
                            fileCacheCompressed,
                            fileCacheExpiry,                             
                            //port,
                            logStatisticsInterval,
                            maxCachedFileSizeBytes, 
                            maxConcurrentRequests, 
                            maxFileCacheBytes, 
                            root,
                            site);

// Set cache service.
var cacheService = new LocalMemoryCache();

// Set file cache service
var fileCacheService = new LocalMemoryFileCache(cacheService, serverData.CacheFileConfig);

// Set folder for config data
var configFolder = Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location), "Config");

// Set folder config service (Permissions)
var folderConfigService = new XmlFolderConfigService(Path.Combine(configFolder, "FolderConfig"));

// Set web request handler factory
IWebRequestHandlerFactory webRequestHandlerFactory = new WebRequestHandlerFactory(cacheService, fileCacheService, folderConfigService, serverData);

// Set cancellation token source
CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

// Initialise web server
var webServer = new WebServer(cacheService,
                                fileCacheService,
                                folderConfigService, 
                                logWriter, 
                                serverData, 
                                webRequestHandlerFactory, 
                                cancellationTokenSource.Token);

// Add to web servers
webServers.Add(webServer);

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


