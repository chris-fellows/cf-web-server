namespace CFWebServer.Interfaces
{
    /// <summary>
    /// Factory for IWebServer instances
    /// </summary>
    public interface IWebServerFactory
    {
        /// <summary>
        /// Creates internal web server for handling web server management functions
        /// </summary>
        /// <returns></returns>
        IWebServer CreateInternalWebServer(string apiKey, string site);

        /// <summary>
        /// Create web server
        /// </summary>
        /// <param name="siteConfigId"></param>
        /// <returns></returns>
        IWebServer CreateWebServerById(string siteConfigId);
    }
}
