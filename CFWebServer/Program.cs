using CFWebServer;
using CFWebServer.Interfaces;
using CFWebServer.Models;
using CFWebServer.Services;

// Create log writer
var logWriter = new DefaultLogWriter();
logWriter.Log("Starting CF Web Server");

// Set server data
var serverData = new ServerData(10010, 10, "D:\\Test\\CFWebServer\\Root");

// Set folder for config data
var configFolder = Path.Combine(System.Reflection.Assembly.GetExecutingAssembly().Location, "Config");

// Set folder config service (Permissions)
var folderConfigService = new XmlFolderConfigService(Path.Combine(configFolder, "FolderConfig"));

IWebRequestHandlerFactory webRequestHandlerFactory = new WebRequestHandlerFactory(folderConfigService, serverData);

// Set cancellation token source
CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

// Initialise web server
var webServer = new WebServer(folderConfigService, 
                                logWriter, 
                                serverData, 
                                webRequestHandlerFactory, 
                                cancellationTokenSource.Token);

// Run server
webServer.Start();

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


