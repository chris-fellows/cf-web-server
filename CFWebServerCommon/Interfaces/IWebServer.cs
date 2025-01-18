using CFWebServer.Models;

namespace CFWebServer.Interfaces
{
    /// <summary>
    /// Interface to web server
    /// </summary>
    public interface IWebServer
    {
        /// <summary>
        /// Starts web server to handle requests
        /// </summary>
        void Start();

        /// <summary>
        /// Stops web server
        /// </summary>
        void Stop();       

        /// <summary>
        /// Whether web server has started
        /// </summary>
        bool IsStarted { get; }
    }
}
