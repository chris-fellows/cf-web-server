using CFFileSystemConnection.Utilities;
using CFWebServer.Interfaces;
using CFWebServer.Models;
using System.Net;
using System.Text;

namespace CFWebServer.WebRequestHandlers
{
    /// <summary>
    /// Handles update of site config. E.g. Update folder permissions
    /// </summary>
    public class PostOrPutSiteConfigWebRequestHandler : WebRequestHandlerBase, IWebRequestHandler
    {
        private readonly string _name;
        private readonly ISiteConfigService _siteConfigService;

        public PostOrPutSiteConfigWebRequestHandler(IFileCacheService fileCacheService,                                                
                                                IMimeTypeDatabase mimeTypeDatabase,
                                                string name,
                                                ServerData serverData,
                                                ISiteConfigService siteConfigService) : base(fileCacheService, mimeTypeDatabase, serverData)
        {
            _name = name;
            _siteConfigService = siteConfigService;
        }

        public string Name => _name;    

        public async Task HandleAsync(RequestContext requestContext)
        {            
            var relativePath = requestContext.Request.Url.AbsolutePath;
            
            // Write            
            var request = requestContext.Request;
            var response = requestContext.Response;

            using (var memoryStream = new MemoryStream())
            {
                await request.InputStream.CopyToAsync(memoryStream);

                var siteConfigs = _siteConfigService.GetAll();

                // Get folder config DTOs from content
                var json = Encoding.UTF8.GetString(memoryStream.ToArray());
                var siteConfig = JsonUtilities.DeserializeFromString<SiteConfig>(json, JsonUtilities.DefaultJsonSerializerOptions);

                // Check if site config with same name exists
                var siteConfigWithSameName = siteConfigs.FirstOrDefault(sc => sc.Name == siteConfig.Name);                

                if (String.IsNullOrEmpty(siteConfig.Id))   // New site
                {
                    if (siteConfigWithSameName != null)     // Site exists
                    {                         
                        response.StatusCode = (int)HttpStatusCode.BadRequest;
                        response.StatusDescription = "Site name must be unique";                                                  
                    }
                
                    siteConfig.Id = Guid.NewGuid().ToString();
                    _siteConfigService.Add(siteConfig);
                }
                else    // Update site
                {
                    if (siteConfigWithSameName != null &&
                        siteConfigWithSameName.Id != siteConfig.Id)  // Updating site config with same name as other site
                    {
                        response.StatusCode = (int)HttpStatusCode.BadRequest;
                        response.StatusDescription = "Site name must be unique";
                    }                    

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
