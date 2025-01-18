using CFWebServer.Interfaces;
using CFWebServer.Models;
using System;
using System.Net;

namespace CFWebServer.WebRequestHandlers
{
    /// <summary>
    /// Web request handler that returns specific status code. E.g. Unauthorized, Not Found
    /// </summary>
    public class StatusCodeWebRequestHandler : WebRequestHandlerBase, IWebRequestHandler
    {
        private readonly string _name;
        private readonly HttpStatusCode _statusCode;

        public StatusCodeWebRequestHandler(IFileCacheService fileCacheService,
                                        string name,
                                        HttpStatusCode statusCode,
                                        IMimeTypeDatabase mimeTypeDatabase,                                        
                                        SiteData serverData) : base(fileCacheService, mimeTypeDatabase, serverData)
        {
            _name = name;
            _statusCode = statusCode;
        }

        public string Name => _name;    

        public async Task HandleAsync(RequestContext requestContext)
        {        
            var relativePath = requestContext.Request.Url.AbsolutePath;

            // Write            
            var request = requestContext.Request;
            var response = requestContext.Response;

            response.StatusCode = (int)_statusCode;

            response.Close();
        }
    }
}
