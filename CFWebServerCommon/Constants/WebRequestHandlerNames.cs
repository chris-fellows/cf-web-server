using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CFWebServer.Constants
{
    public static class WebRequestHandlerNames
    {
        // Static resources
        public const string StaticResourceDelete = "STATIC_RESOURCE_DELETE";
        public const string StaticResourceGet = "STATIC_RESOURCE_GET";
        public const string StaticResourcePost = "STATIC_RESOURCE_POST";
        public const string StaticResourcePut = "STATIC_RESOURCE_PUT";

        public const string GetSiteConfigs = "GET_SITE_CONFIGS";
        public const string UpdateSiteConfig = "UPDATE_SITE_CONFIG";
        public const string TestCustomGet = "TEST_CUSTOM_GET";

        public const string NotFound = "NOT_FOUND";
    }
}
