using CFWebServer;
using CFWebServer.Interfaces;
using CFWebServer.Models;
using CFWebServer.Services;
using CFWebServerCommon.Utilities;
using CFWebServerMobile.Utilities;
using CFWebServerMobile.ViewModels;
using Microsoft.Extensions.Logging;

namespace CFWebServerMobile
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {            
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

            var configFolder = Path.Combine(FileSystem.AppDataDirectory, "Config");            

            //builder.Services.AddSingleton<IWebServer, IWebServer>();
            builder.Services.AddSingleton<IWebRequestHandlerFactory, WebRequestHandlerFactory>();            

            // Set config data services
            builder.Services.AddSingleton<IFolderConfigService>((scope) =>
            {
                return new XmlFolderConfigService(Path.Combine(configFolder, "FolderConfig"));
            });
            builder.Services.AddSingleton<ISiteConfigService>((scope) =>
            {
                var siteConfigService = new XmlSiteConfigService(Path.Combine(configFolder, "SiteConfig"));

                // Add default site config if necessary                
                var siteConfigs = siteConfigService.GetAll();                

                if (!siteConfigs.Any())
                {
                    // TODO: Move this folder
                    var sitesFolder = Path.Combine(Android.OS.Environment.ExternalStorageDirectory.Path, "Music", "Websites");
                    
                    var siteConfig = new SiteConfig()
                    {
                        Id = Guid.NewGuid().ToString(),
                        Name = "Test 1",
                        DefaultFile = "Index.html",
                        MaxConcurrentRequests = 20,
                        RootFolder = Path.Combine(sitesFolder, "Test1"),
                        Site = "http://localhost:10010/",
                        //Site = "http://192.168.1.48:10010/",
                        CacheFileConfig = new FileCacheConfig()
                        {
                            Compressed = true,
                            Expiry = TimeSpan.Zero,     // Cache disabled,
                            MaxFileSizeBytes = 1024 * 1000,
                            MaxTotalSizeBytes = 1024 * 10000
                        }
                    };
                    siteConfigService.Update(siteConfig);

                    // Create test website                    
                    try
                    {
                        WebSiteUtilities.CreateSimpleWebsite(siteConfig.RootFolder);
                    }
                    catch { };                 
                }

                return siteConfigService;
            });

            builder.Services.AddSingleton<ICacheService, LocalMemoryCache>();
            builder.Services.AddSingleton<IFileCacheService>((scope) =>
            {
                var cacheService = scope.GetRequiredService<ICacheService>();
                var siteConfigService = scope.GetRequiredService<ISiteConfigService>();

                var siteConfig = siteConfigService.GetAll().First();

                // TODO: Fix this to get SiteConfig from correct location
                return new LocalMemoryFileCache(cacheService, siteConfig.CacheFileConfig);
            });

            builder.Services.AddTransient<ILogWriter, DefaultLogWriter>();

            // Register pages & models
            builder.Services.AddSingleton<MainPageModel>();
            builder.Services.AddSingleton<MainPage>();
            builder.Services.AddSingleton<SiteConfigPageModel>();
            builder.Services.AddSingleton<SiteConfigPage>();

#if DEBUG
            builder.Logging.AddDebug();
#endif
            return builder.Build();
        }
    }
}
