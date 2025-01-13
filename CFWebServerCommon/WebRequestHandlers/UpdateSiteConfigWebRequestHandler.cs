using CFFileSystemConnection.Utilities;
using CFWebServer.Constants;
using CFWebServer.Interfaces;
using CFWebServer.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

namespace CFWebServer.WebRequestHandlers
{
    /// <summary>
    /// Handles update of site config. E.g. Update folder permissions
    /// </summary>
    public class UpdateSiteConfigWebRequestHandler : WebRequestHandlerBase, IWebRequestHandler
    {
        private readonly ISiteConfigService _siteConfigService;

        public UpdateSiteConfigWebRequestHandler(IFileCacheService fileCacheService,                                                
                                                IMimeTypeDatabase mimeTypeDatabase,
                                                ServerData serverData,
                                                ISiteConfigService siteConfigService) : base(fileCacheService, mimeTypeDatabase, serverData)
        {
            _siteConfigService = siteConfigService;
        }

        public string Name => WebRequestHandlerNames.UpdateSiteConfig;

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

            using (var memoryStream = new MemoryStream())
            {
                await request.InputStream.CopyToAsync(memoryStream);

                // Get folder config DTOs from content
                var json = Encoding.UTF8.GetString(memoryStream.ToArray());
                var siteConfig = JsonUtilities.DeserializeFromString<SiteConfig>(json, JsonUtilities.DefaultJsonSerializerOptions);

                if (String.IsNullOrEmpty(siteConfig.Id))   // New site
                {
                    siteConfig.Id = Guid.NewGuid().ToString();
                    _siteConfigService.Add(siteConfig);   // TODO: Need Add method
                }
                else    // Update site
                {
                    _siteConfigService.Update(siteConfig);
                }

                // Create root folder if not exists
                if (!Directory.Exists(siteConfig.RootFolder))
                {
                    Directory.CreateDirectory(siteConfig.RootFolder);
                }

                response.StatusCode = (int)HttpStatusCode.OK;
            }          
        }
    }
}
