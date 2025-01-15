using CFWebServer.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace CFWebServer.Utilities
{
    public class HttpUtilities
    {
        //public static Dictionary<string, string> GetHeadersAsDictionary(string headerString)
        //{
        //    var headerLines = headerString.Split('\r', '\n');
        //    string firstLine = headerLines[0];
        //    var headerValues = new Dictionary<string, string>();
        //    foreach (var headerLine in headerLines)
        //    {
        //        var headerDetail = headerLine.Trim();
        //        var delimiterIndex = headerLine.IndexOf(':');
        //        if (delimiterIndex >= 0)
        //        {
        //            var headerName = headerLine.Substring(0, delimiterIndex).Trim();
        //            var headerValue = headerLine.Substring(delimiterIndex + 1).Trim();
        //            headerValues.Add(headerName, headerValue);
        //        }
        //    }

        //    return headerValues;
        //}

        public static string GetResourceLocalPath(string rootFolder, string url)
        {
            // Set local path          
            var elements = url.Split('/');

            var localPath = rootFolder;
            for (int index = 0; index < elements.Length; index++)
            {
                localPath = Path.Combine(localPath, elements[index]);
            }

            return localPath;
        }

        public static string GetUrlWithoutLastElement(string url)
        {
            var elements = GetRelativeUrlElements(url);

            var newUrl = new StringBuilder("");
            for (int index = 0; index < elements.Length - 1; index++)
            {
                newUrl.Append(elements[index]);
            }

            return newUrl.ToString();           
        }

        public static string GetUrlFileExtension(string url)
        {
            var elements = url.Split('/');
            return elements.Last().Split('.').Last();
        }

        /// <summary>
        /// Whether relative URL matches pattern where pattern can contain wildcards.
        /// 
        /// Limits:
        ///  - Pattern must contain same number of / characters as URL.
        ///  - Pattern element (The bit between a pair of /) does not support wildcard and other characters, only
        ///    a wildcard. "/F1/*/F3" works but "/F1/F*/F3" does not work.
        /// </summary>
        /// <param name="url"></param>
        /// <param name="pattern"></param>
        /// <param name="wildcard"></param>
        /// <returns></returns>
        public static bool IsRelativeUrlMatchesPattern(string url, string pattern, Char wildcard)
        {
            // E.g.
            // url="/siteConfig/1234"
            // pattern="/siteConfig/*"
            if (pattern.Contains(wildcard))
            {
                var urlElements = GetRelativeUrlElements(url);
                var patternElements = GetRelativeUrlElements(pattern);

                if (urlElements.Length == patternElements.Length)
                {
                    for (int index = 0; index < urlElements.Length; index++)
                    {
                        if (urlElements[index] == "/")
                        {
                            if (patternElements[index] != "/") return false;
                        }
                        else
                        {                                                        
                            if (patternElements[index] == wildcard.ToString())
                            {
                                // Wildcard matches
                            }
                            else if (urlElements[index] != patternElements[index])
                            {
                                return false;
                            }
                        }
                    }

                    return true;
                }
                else    // Different number of elements
                {
                    return false;
                }              
            }
            else
            {
                return url == pattern;
            }            
        }

        /// <summary>
        /// Gets relative URL elements. E..g. "/siteConfig/Test/123" = [ "/", "siteConfig", "/", "Test", "1234" ]
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public static string[] GetRelativeUrlElements(string url)
        {
            var elements = new List<string>();

            var currentElement = new StringBuilder("");
            for (int index = 0; index < url.Length; index++)
            {
                if (url[index] == '/')   
                {
                    // If current element then add it and start a new element
                    if (currentElement.Length > 0)
                    {
                        elements.Add(currentElement.ToString());
                        currentElement.Length = 0;
                    }

                    elements.Add("/");
                }
                else   // Append character to current element
                {
                    currentElement.Append(url[index]);
                }
            }

            // Add final element
            if (currentElement.Length > 0)
            {
                elements.Add(currentElement.ToString());
            }

            return elements.ToArray();
        }
    }
}
