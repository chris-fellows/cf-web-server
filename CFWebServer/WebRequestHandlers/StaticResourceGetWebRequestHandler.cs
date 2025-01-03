using CFWebServer.Interfaces;
using CFWebServer.Models;
using CFWebServer.Utilities;
using System.Net;
using System.Text;

namespace CFWebServer.WebRequestHandlers
{
    /// <summary>
    /// Handles GET request for static resource
    /// </summary>
    internal class StaticResourceGetWebRequestHandler : IWebRequestHandler
    {
        private readonly ServerData _serverData;

        public StaticResourceGetWebRequestHandler(ServerData serverData)
        {
            _serverData = serverData;
        }

        public bool CanHandle(RequestContext requestContext)
        {
            return requestContext.Request.HttpMethod == "GET";
        }            

        public async Task HandleAsync(RequestContext requestContext)
        {
            if (!CanHandle(requestContext))
            {
                throw new ArgumentException("Unable to handle request");
            }

            // Getlocal path
            var localResourcePath = HttpUtilities.GetResourceLocalPath(_serverData.RootFolder, requestContext.Request.Url.AbsolutePath);

            //response.ContentType = "text/html";
            //        response.ContentEncoding = Encoding.UTF8;
            //        response.ContentLength64 = data.LongLength;

            //        // Write out to the response stream (asynchronously), then close it
            //        await response.OutputStream.WriteAsync(data, 0, data.Length);
            //        response.Close();

            var response = requestContext.Response;
            if (File.Exists(localResourcePath))
            {
                var content = File.ReadAllBytes(localResourcePath);

                response.StatusCode = (int)HttpStatusCode.OK;
                //response.ContentType = "";                    
                response.ContentEncoding = Encoding.UTF8;
                response.ContentLength64 = content.LongLength;

                await response.OutputStream.WriteAsync(content, 0, content.Length);
            }
            else
            {
                response.StatusCode = (int)HttpStatusCode.NotFound;
            }
            response.Close();
        }        
    }
}
