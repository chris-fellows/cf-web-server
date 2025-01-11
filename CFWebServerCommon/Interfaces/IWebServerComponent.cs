namespace CFWebServer.Interfaces
{
    /// <summary>
    /// Web server component
    /// </summary>
    public interface IWebServerComponent
    {
        /// <summary>
        /// Start component
        /// </summary>
        void Start();

        /// <summary>
        /// Stop component
        /// </summary>
        void Stop();       
    }
}
