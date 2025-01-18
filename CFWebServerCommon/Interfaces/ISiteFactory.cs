namespace CFWebServer.Interfaces
{
    /// <summary>
    /// Factory for ISite instances
    /// </summary>
    public interface ISiteFactory
    {
        /// <summary>
        /// Creates internal site for handling site management functions
        /// </summary>
        /// <returns></returns>
        ISite CreateInternalSite(string apiKey, string site);

        /// <summary>
        /// Create site by Site Config Id
        /// </summary>
        /// <param name="siteConfigId"></param>
        /// <returns></returns>
        ISite CreateSiteById(string siteConfigId);
    }
}
