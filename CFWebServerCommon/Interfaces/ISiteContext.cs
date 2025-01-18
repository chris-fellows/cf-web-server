namespace CFWebServer.Interfaces
{
    /// <summary>
    /// Site context for dependency injection
    /// </summary>
    public interface ISiteContext
    {
        string SiteConfigId { get; set; }
    }
}
