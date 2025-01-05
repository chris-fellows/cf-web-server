namespace CFWebServerMobile
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();

            // CMF Added
            Routing.RegisterRoute(nameof(MainPage), typeof(MainPage));
            Routing.RegisterRoute(nameof(SiteConfigPage), typeof(SiteConfigPage));
        }
    }
}
