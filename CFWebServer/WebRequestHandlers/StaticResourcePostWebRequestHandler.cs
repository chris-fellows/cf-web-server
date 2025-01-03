using CFWebServer.Interfaces;
using CFWebServer.Models;
using CFWebServer.Utilities;
using System.Net;

namespace CFWebServer.WebRequestHandlers
{
    /// <summary>
    /// Handles POST request for static resource
    /// </summary>
    internal class StaticResourcePostWebRequestHandler : IWebRequestHandler
    {
        private readonly ServerData _serverData;

        public StaticResourcePostWebRequestHandler(ServerData serverData)
        {
            _serverData = serverData;
        }

        public bool CanHandle(RequestContext requestContext)
        {
            return requestContext.Request.HttpMethod == "POST";
        }

        public async Task HandleAsync(RequestContext requestContext)
        {
            if (!CanHandle(requestContext))
            {
                throw new ArgumentException("Unable to handle request");
            }

            // Getlocal path
            var localResourcePath = HttpUtilities.GetResourceLocalPath(_serverData.RootFolder, requestContext.Request.Url.AbsolutePath);

            // Write            
            var request = requestContext.Request;
            var response = requestContext.Response;

            using (var fileStream = new FileStream(localResourcePath, FileMode.OpenOrCreate))
            {
                await request.InputStream.CopyToAsync(fileStream);
                await fileStream.FlushAsync();
            }

            response.StatusCode = (int)HttpStatusCode.OK;
            response.Close();
        }
    }
}
