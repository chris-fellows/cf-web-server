using CFWebServer.Constants;
using CFWebServer.Interfaces;
using CFWebServer.Models;
using System.Management.Automation;
using System.Net;
using System.Text;

namespace CFWebServer.WebRequestHandlers
{
    /// <summary>
    /// Handles PowerShell script web request
    ///   
    /// Script input: 
    ///         - Method (String)
    ///         - URL (String)
    ///         - Content-Base64=[Content Base 64]
    ///         - Form (Dictionary)
    ///         - Parameters (Dictionary)
    ///         - Headers (Dictionary)
    ///         - SiteParameters (Dictionary) E.g. Connection string
    ///         
    /// Script output:
    ///         - Content-Type=[Content Type]
    ///         - Content=[Content]
    ///         - Content-Base64=[Content Base 64]
    ///         - Status-Code=[Status Code]    
    /// </summary>
    public class PowerShellWebRequestHandler : WebRequestHandlerBase, IWebRequestHandler
    {
        public PowerShellWebRequestHandler(IFileCacheService fileCacheService,
                                                IMimeTypeDatabase mimeTypeDatabase,
                                                 SiteData siteData) : base(fileCacheService, mimeTypeDatabase, siteData)
        {

        }

        public string Name => WebRequestHandlerNames.PowerShell;

        public async Task HandleAsync(RequestContext requestContext)
        {
            var relativePath = requestContext.Request.Url.AbsolutePath;            

            // Getlocal path
            var localResourcePath = GetResourceLocalPath(relativePath);

            // Write            
            var request = requestContext.Request;
            var response = requestContext.Response;
            
            var messages = new List<string>();      // Debug messages

            int statusCode = (int)HttpStatusCode.OK;
            var contentType = "";
            var content = new byte[0];
            
            using (var powerShell = PowerShell.Create())
            {
                // Set script, ideally cached
                var cacheFile = _fileCacheService.Enabled ? _fileCacheService.Get(relativePath) : null;

                // Remove cached file if not latest
                if (cacheFile != null && IsCacheFileNotTheLatest(cacheFile))
                {
                    _fileCacheService.Remove(relativePath);
                    cacheFile = null;
                }
               
                if (cacheFile == null)   // Script not cached
                {
                    var scriptContent = File.ReadAllBytes(localResourcePath);
                    powerShell.AddScript(Encoding.UTF8.GetString(scriptContent));

                    // Cache file                    
                    if (_fileCacheService.Enabled)
                    {
                        _fileCacheService.Add(relativePath, scriptContent, new FileInfo(localResourcePath).LastWriteTimeUtc);
                    }
                }
                else     // Script cached
                {
                    powerShell.AddScript(Encoding.UTF8.GetString(cacheFile.GetContent()));
                }

                // Set parameters                
                powerShell.AddParameter("Method", requestContext.Request.HttpMethod.ToString());
                powerShell.AddParameter("URL", relativePath);                
                powerShell.AddParameter("Parameters", GetParameters(requestContext));
                powerShell.AddParameter("Headers", GetHeaders(requestContext));
                powerShell.AddParameter("SiteParameters", GetSiteParameters(requestContext, _siteData.SiteConfig));

                // Set content (Base 64) and form content
                using (var memoryStream = new MemoryStream())
                {
                    await request.InputStream.CopyToAsync(memoryStream);                    
                    var contentBytes = memoryStream.ToArray();

                    powerShell.AddParameter("Content-Base64", Convert.ToBase64String(contentBytes));

                    powerShell.AddParameter("Form", GetForm(requestContext, contentBytes));
                }

                var pipelineObjects = await powerShell.InvokeAsync().ConfigureAwait(false);
                messages.Add("HandleAsync: Executed script");

                var isHadErrors = powerShell.HadErrors;

                if (powerShell.Streams.Information != null && powerShell.Streams.Information.Any())
                {
                    messages.Add("HandleAsync: Script has information");
                    foreach (var info in powerShell.Streams.Information)
                    {
                        messages.Add($"HandleAsync: Info: {info.ToString()}");

                        if (info.ToString().StartsWith("Content-Type="))
                        {
                            contentType = info.ToString().Substring("Content-Type=".Length);
                        }
                        else if (info.ToString().StartsWith("Content="))
                        {
                            content = Encoding.UTF8.GetBytes(info.ToString().Substring("Content=".Length));
                        }
                        else if (info.ToString().StartsWith("Content-Base64="))
                        {
                            content = Convert.FromBase64String(info.ToString().Substring("Content-Base64=".Length));
                        }
                        else if (info.ToString().StartsWith("Status-Code="))
                        {
                            statusCode = Convert.ToInt32(info.ToString().Substring("Status-Code=".Length));
                        }
                    }
                }
                if (powerShell.Streams.Warning != null && powerShell.Streams.Warning.Any())
                {
                    messages.Add("HandleAsync: Script has warnings");
                }
                if (powerShell.Streams.Error != null && powerShell.Streams.Error.Any())
                {
                    messages.Add("HandleAsync: Script has errors");
                    foreach (var error in powerShell.Streams.Error)
                    {
                        if (error.Exception != null)
                        {
                            messages.Add($"HandleAsync: Error Exception: {error.Exception.Message}");
                        }
                        if (error.ErrorDetails != null)
                        {
                            messages.Add($"HandleAsync: Error ErrorDetails: {error.ErrorDetails.Message}");
                        }
                    }
                }
                if (powerShell.Streams.Debug != null && powerShell.Streams.Debug.Any())
                {
                    messages.Add("HandleAsync: Script has debug");
                }
                if (powerShell.Streams.Progress != null && powerShell.Streams.Progress.Any())
                {
                    messages.Add("HandleAsync: Script has progress");
                }
                if (powerShell.Streams.Verbose != null && powerShell.Streams.Verbose.Any())
                {
                    messages.Add("HandleAsync: Script has verbose");
                }

                // print the resulting pipeline objects to the console.
                foreach (var item in pipelineObjects)
                {
                    var result = item.ToString();
                    var type = item.GetType();

                    messages.Add("HandleAsync: Pipeline: " + item.BaseObject.ToString());
                }
            }

            response.StatusCode = statusCode;

            if (!String.IsNullOrEmpty(contentType))
            {
                response.ContentType = contentType;
                response.ContentEncoding = Encoding.UTF8;
                response.ContentLength64 = content.LongLength;
                await response.OutputStream.WriteAsync(content, 0, content.Length);
            }
            
            response.Close();

            //if (IsActionAllowedForFolderPermission(HttpUtilities.GetUrlWithoutLastElement(relativePath), FolderPermissions.Write))
            //{
            //    using (var fileStream = new FileStream(localResourcePath, FileMode.OpenOrCreate))
            //    {
            //        await request.InputStream.CopyToAsync(fileStream);
            //        await fileStream.FlushAsync();
            //    }

            //    response.StatusCode = (int)HttpStatusCode.OK;
            //    response.Close();

            //    // Update cache file if exists
            //    var cacheFile = _fileCacheService.Enabled ? _fileCacheService.Get(relativePath) : null;
            //    if (cacheFile != null)
            //    {
            //        _fileCacheService.Add(relativePath, new byte[0], new FileInfo(localResourcePath).LastWriteTimeUtc);
            //    }
            //}
            //else
            //{
            //    response.StatusCode = (int)HttpStatusCode.Unauthorized;
            //    response.Close();
            //}
        }

        /// <summary>
        /// Gets request parameters from querystring
        /// </summary>
        /// <param name="requestContext"></param>
        /// <returns></returns>
        private static Dictionary<string, string> GetParameters(RequestContext requestContext)
        {
            // Get 
            var parameters = new Dictionary<string, string>();

            if (requestContext.Request.Url != null &&
                requestContext.Request.Url.Query.Length > 0)
            {
                foreach (var param in requestContext.Request.Url.Query.Substring(1).Split('&'))
                {
                    var paramName = param.Split('=')[0];
                    var paramValue = param.Split('=')[1];
                    parameters.Add(paramName, paramValue);
                }
            }

            return parameters;
        }

        /// <summary>
        /// Gets request headers
        /// </summary>
        /// <param name="requestContext"></param>
        /// <returns></returns>
        private static Dictionary<string, string> GetHeaders(RequestContext requestContext)
        {
            var headers = new Dictionary<string, string>();
            foreach(var header in requestContext.Request.Headers.Keys)
            {
                headers.Add(header.ToString(), requestContext.Request.Headers[header.ToString()].ToString());
            }

            return headers;
        }

        /// <summary>
        /// Gets request form
        /// </summary>
        /// <param name="requestContext"></param>
        /// <param name="contentBytes"></param>
        /// <returns></returns>
        private static Dictionary<string, string> GetForm(RequestContext requestContext, byte[]? contentBytes)
        {
            var form = new Dictionary<string, string>();

            if (contentBytes != null && contentBytes.Length > 0)
            {
                var data = System.Text.Encoding.UTF8.GetString(contentBytes);

                foreach(var param in data.Split('&'))
                {
                    var paramName = param.Split('=')[0];
                    var paramValue = param.Split('=')[1];
                    form.Add(paramName, paramValue);
                }
            }

            return form;
        }

        /// <summary>
        /// Returns request site parameters. E.g. Connection string etc.
        /// </summary>
        /// <param name="requestContext"></param>
        /// <returns></returns>
        private static Dictionary<string, string> GetSiteParameters(RequestContext requestContext, SiteConfig siteConfig)
        {
            var siteParameters = new Dictionary<string, string>();

            foreach(var siteParameter in siteConfig.Parameters)
            {
                siteParameters.Add(siteParameter.Name, siteParameter.Value);
            }

            return siteParameters;
        }
    }
}
