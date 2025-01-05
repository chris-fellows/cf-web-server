using CFWebServer.Models;

namespace CFWebServer.Interfaces
{
    /// <summary>
    /// Factory for IWebRequestHandler instances
    /// </summary>
    public interface IWebRequestHandlerFactory
    {   
        /// <summary>
        /// Gets web request handler for request
        /// </summary>
        /// <param name="requestContext"></param>
        /// <returns></returns>
        IWebRequestHandler? Get(RequestContext requestContext, ServerData serverData);
    }
}
