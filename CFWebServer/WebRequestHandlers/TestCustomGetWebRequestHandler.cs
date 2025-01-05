﻿using CFWebServer.Interfaces;
using CFWebServer.Models;

namespace CFWebServer.WebRequestHandlers
{
    /// <summary>
    /// Custom web request handler for GET request
    /// 
    /// E.g. We can create a handler for our own custom resource by filtering on a specific resource file extension
    /// </summary>
    internal class TestCustomGetWebRequestHandler : WebRequestHandlerBase, IWebRequestHandler
    {
        public TestCustomGetWebRequestHandler(IFileCacheService fileCacheService,
                                            ServerData serverData) : base(fileCacheService, serverData)
        {

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
        }
    }
}
