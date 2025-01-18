
namespace CFWebServer.Interfaces
{
    /// <summary>
    /// Interface to web server
    /// </summary>
    public interface IWebServer
    {
        /// <summary>
        /// Adds site
        /// </summary>
        /// <param name="site"></param>
        void Add(ISite site);

        /// <summary>
        /// All sites
        /// </summary>
        List<ISite> Sites { get; }
    }
}
