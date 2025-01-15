using CFFileSystemConnection.Utilities;
using CFWebServer.Constants;
using CFWebServer.Interfaces;
using CFWebServer.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace CFWebServer.WebRequestHandlers
{
    /// <summary>
    /// Handles request for site config
    /// </summary>
    public class GetSiteConfigWebRequestHandler : WebRequestHandlerBase, IWebRequestHandler
    {
        private readonly ISiteConfigService _siteConfigService;

        public GetSiteConfigWebRequestHandler(IFileCacheService fileCacheService,
                                                IMimeTypeDatabase mimeTypeDatabase,
                                                ServerData serverData,
                                                ISiteConfigService siteConfigService) : base(fileCacheService, mimeTypeDatabase, serverData)
        {
            _siteConfigService = siteConfigService;
        }

        public string Name => WebRequestHandlerNames.GetSiteConfig;
        
        public async Task HandleAsync(RequestContext requestContext)
        {          
            var relativePath = requestContext.Request.Url.AbsolutePath;      

            // Write            
            var request = requestContext.Request;
            var response = requestContext.Response;

            // Get site config
            var siteConfigId = relativePath.Split('/').Last();
            var siteConfig = _siteConfigService.GetById(siteConfigId);

            if (siteConfig == null) // Site config doesn't exist
            {
                response.StatusCode = (int)HttpStatusCode.NotFound;
            }
            else   // Site config exists
            {
                // Serialise
                var siteConfigJson = JsonUtilities.SerializeToString(siteConfig, JsonUtilities.DefaultJsonSerializerOptions);

                var content = Encoding.UTF8.GetBytes(siteConfigJson);

                response.StatusCode = (int)HttpStatusCode.OK;
                response.ContentType = "application/json";
                response.ContentEncoding = Encoding.UTF8;
                response.ContentLength64 = content.LongLength;

                await response.OutputStream.WriteAsync(content, 0, content.Length);
            }

            response.Close();
        }
    }
}
