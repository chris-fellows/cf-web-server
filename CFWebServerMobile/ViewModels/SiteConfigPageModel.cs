using CFWebServer.Interfaces;
using CFWebServer.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace CFWebServerMobile.ViewModels
{
    public class SiteConfigPageModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        //public LocalizationResources LocalizationResources => LocalizationResources.Instance;

        public void OnPropertyChanged([CallerMemberName] string name = "") =>
                     PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        private readonly ISiteConfigService _siteConfigService;

        public SiteConfigPageModel(ISiteConfigService siteConfigService)
        {
            _siteConfigService = siteConfigService;
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
    }
}
