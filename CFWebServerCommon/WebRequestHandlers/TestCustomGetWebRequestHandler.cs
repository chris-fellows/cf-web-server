using CFWebServer.Constants;
using CFWebServer.Interfaces;
using CFWebServer.Models;

namespace CFWebServer.WebRequestHandlers
{
    /// <summary>
    /// Custom web request handler for GET request
    /// 
    /// E.g. We can create a handler for our own custom resource by filtering on a specific resource file extension
    /// </summary>
    public class TestCustomGetWebRequestHandler : WebRequestHandlerBase, IWebRequestHandler
    {
        public TestCustomGetWebRequestHandler(IFileCacheService fileCacheService,
                                            IMimeTypeDatabase mimeTypeDatabase,
                                            ServerData serverData) : base(fileCacheService, mimeTypeDatabase, serverData)
        {

        }

        public string Name => WebRequestHandlerNames.TestCustomGet;

        //public bool CanHandle(RequestContext requestContext)
        //{
        //    return requestContext.Request.HttpMethod == "GET";
        //}

        public async Task HandleAsync(RequestContext requestContext)
        {
            //if (!CanHandle(requestContext))
            //{
            //    throw new ArgumentException("Unable to handle request");
            //}         
        }
    }
}
