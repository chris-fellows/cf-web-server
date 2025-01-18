using CFWebServer.Interfaces;

namespace CFWebServer
{
    public class WebServer : IWebServer
    {
        private List<ISite> _sites = new List<ISite>();

        public void Add(ISite site)
        {
            _sites.Add(site);
        }

        public List<ISite> Sites => _sites;
    }
}
