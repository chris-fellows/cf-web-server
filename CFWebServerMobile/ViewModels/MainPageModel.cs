using CFWebServer;
using CFWebServer.Interfaces;
using CFWebServer.Models;
using CFWebServerMobile.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace CFWebServerMobile.ViewModels
{
    public class MainPageModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        //public LocalizationResources LocalizationResources => LocalizationResources.Instance;

        public void OnPropertyChanged([CallerMemberName] string name = "") =>
                     PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        private readonly ICacheService _cacheService;
        private readonly IFileCacheService _fileCacheService;        
        private readonly ISiteLogWriter _logWriter;
        private readonly IServerNotifications _serverEventQueue;
        private readonly ISiteConfigService _siteConfigService;
        private readonly IWebRequestHandlerFactory _webRequestHandlerFactory;

        private CancellationTokenSource? _cancellationTokenSource;

        private IWebServer? _webServer;

        public ICommand StartSiteCommand { get; set; }

        public ICommand StopSiteCommand { get; set; }

        private string _errorMessage = "None";

        public MainPageModel(ICacheService cacheService,
                                IFileCacheService fileCacheService,                                
                                ISiteLogWriter logWriter,
                                IServerNotifications serverEventQueue,
                                ISiteConfigService siteConfigService,
                                IWebRequestHandlerFactory webRequestHandlerFactory)
        {            
                InternalUtilities.Log("MainPageModel.Constructor : Entered");

                _cacheService = cacheService;
                _fileCacheService = fileCacheService;                
                _logWriter = logWriter;
                _serverEventQueue = serverEventQueue;
                _siteConfigService = siteConfigService;
                _webRequestHandlerFactory = webRequestHandlerFactory;

                StartSiteCommand = new Command(DoStartSite);
                StopSiteCommand = new Command(DoStopSite);

                // Load site configs
                _siteConfigs = _siteConfigService.GetAll();
                _selectedSiteConfig = _siteConfigs.FirstOrDefault();

                InternalUtilities.Log("MainPageModel.Constructor : Leaving");     
        }

        private List<SiteConfig> _siteConfigs = new List<SiteConfig>();
        public List<SiteConfig> SiteConfigs
        {
            get { return _siteConfigs; }
            set
            {
                _siteConfigs = value;

                OnPropertyChanged(nameof(SiteConfigs));
            }
        }

        private SiteConfig? _selectedSiteConfig;
        public SiteConfig? SelectedSiteConfig
        {
            get { return _selectedSiteConfig; }
            set
            {
                _selectedSiteConfig = value;

                OnPropertyChanged(nameof(SelectedSiteConfig));
            }
        }

        public bool IsStartSiteEnabled => _selectedSiteConfig != null && _webServer == null;

        public bool IsStopSiteEnabled => _selectedSiteConfig != null && _webServer != null;

        private void DoStartSite(object parameter)
        {
            try                       
            {
                ErrorMessage = "None";

                var serverData = new ServerData(TimeSpan.FromSeconds(300), _selectedSiteConfig);

                _cancellationTokenSource = new CancellationTokenSource();

                _webServer = new WebServer(_cacheService,
                        _fileCacheService,                        
                        _logWriter,
                        serverData,
                        _serverEventQueue, 
                        _siteConfigService,
                        _webRequestHandlerFactory,
                        _cancellationTokenSource.Token);

                _webServer.Start();                
            }
            catch(Exception exception)
            {
                _webServer = null;
                ErrorMessage = $"Error starting site: {exception.Message}";
            }
            
            OnPropertyChanged(nameof(IsStartSiteEnabled));
            OnPropertyChanged(nameof(IsStopSiteEnabled));
        }

        private void DoStopSite(object parameter)
        {
            // Notify cancel
            _cancellationTokenSource.Cancel();

            _webServer.Stop();
            _webServer = null;

            OnPropertyChanged(nameof(IsStartSiteEnabled));
            OnPropertyChanged(nameof(IsStopSiteEnabled));
        }

        public string ErrorMessage
        {
            get { return _errorMessage; }
            set
            {
                _errorMessage = value;

                OnPropertyChanged(nameof(ErrorMessage));
            }
        }        
    }
}
