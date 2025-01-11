﻿using CFFileSystemConnection.Utilities;
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
    /// Handles request for site configs
    /// </summary>
    public class GetSiteConfigsWebRequestHandler : WebRequestHandlerBase, IWebRequestHandler
    {
        private readonly ISiteConfigService _siteConfigService;

        public GetSiteConfigsWebRequestHandler(IFileCacheService fileCacheService,
                                                ServerData serverData,
                                                ISiteConfigService siteConfigService) : base(fileCacheService, serverData)
        {
            _siteConfigService = siteConfigService;
        }

        public string Name => WebRequestHandlerNames.GetSiteConfigs;
        
        public async Task HandleAsync(RequestContext requestContext)
        {          
            var relativePath = requestContext.Request.Url.AbsolutePath;

            // Write            
            var request = requestContext.Request;
            var response = requestContext.Response;

            // Get all site configs
            var siteConfigs = _siteConfigService.GetAll();

            // Serialise
            var siteConfigsJson = JsonUtilities.SerializeToString(siteConfigs, JsonUtilities.DefaultJsonSerializerOptions);
            
            var content = Encoding.UTF8.GetBytes(siteConfigsJson);

            response.StatusCode = (int)HttpStatusCode.OK;
            response.ContentType = "application/json";                    
            response.ContentEncoding = Encoding.UTF8;
            response.ContentLength64 = content.LongLength;

            await response.OutputStream.WriteAsync(content, 0, content.Length);

            response.Close();
        }
    }
}
