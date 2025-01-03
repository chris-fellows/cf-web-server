using CFWebServer.Interfaces;
using CFWebServer.Models;
using CFWebServer.Utilities;
using System.Net;

namespace CFWebServer.WebRequestHandlers
{
    /// <summary>
    /// Handles DELETE request for static resource
    /// </summary>
    internal class StaticResourceDeleteWebRequestHandler : IWebRequestHandler
    {
        private readonly ServerData _serverData;

        public StaticResourceDeleteWebRequestHandler(ServerData serverData)
        {
            _serverData = serverData;
        }

        public bool CanHandle(RequestContext requestContext)
        {
            return requestContext.Request.HttpMethod == "DELETE";
        }

        public async Task HandleAsync(RequestContext requestContext)
        {
            if (!CanHandle(requestContext))
            {
                throw new ArgumentException("Unable to handle request");
            }

            // Get local path
            var localResourcePath = HttpUtilities.GetResourceLocalPath(_serverData.RootFolder, requestContext.Request.Url.AbsolutePath);
            
            var response = requestContext.Response;
            if (File.Exists(localResourcePath))
            {
                File.Delete(localResourcePath);
            }
            else
            {
                response.StatusCode = (int)HttpStatusCode.NotFound;
            }
            response.Close();
        }
    }
}
