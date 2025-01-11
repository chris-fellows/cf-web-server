using CFFileSystemConnection.Utilities;
using CFWebServer.Constants;
using CFWebServer.Interfaces;
using CFWebServer.Models;
using CFWebServer.WebRequestHandlers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace CFWebServerCommon.WebRequestHandlers
{
    /// <summary>
    /// Handles not found.
    /// </summary>
    public class NotFoundWebRequestHandler : WebRequestHandlerBase, IWebRequestHandler
    {
        private readonly ISiteConfigService _siteConfigService;

        public NotFoundWebRequestHandler(IFileCacheService fileCacheService,
                                                ServerData serverData) : base(fileCacheService, serverData)
        {

        }

        public string Name => WebRequestHandlerNames.NotFound;

        //public bool CanHandle(RequestContext requestContext)
        //{
        //    return requestContext.Request.HttpMethod == "POST";
        //}

        public async Task HandleAsync(RequestContext requestContext)
        {
            //if (!CanHandle(requestContext))
            //{
            //    throw new ArgumentException("Unable to handle request");
            //}

            var relativePath = requestContext.Request.Url.AbsolutePath;

            // Write            
            var request = requestContext.Request;
            var response = requestContext.Response;

            response.StatusCode = (int)HttpStatusCode.NotFound;

            response.Close();
        }
    }
}
