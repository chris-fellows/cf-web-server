namespace CFWebServer.Interfaces
{
    /// <summary>
    /// Interface to web site
    /// </summary>
    public interface ISite
    {
        /// <summary>
        /// Starts site to handle requests
        /// </summary>
        void Start();

        /// <summary>
        /// Stops site
        /// </summary>
        void Stop();       

        /// <summary>
        /// Whether site has started
        /// </summary>
        bool IsStarted { get; }
    }
}
