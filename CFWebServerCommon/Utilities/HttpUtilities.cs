﻿using CFWebServer.Models;
using System;
using System.Collections.Generic;
using System.Linq;
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
            // TODO: FIx this
            return url;
        }

        public static string GetUrlFileExtension(string url)
        {
            var elements = url.Split('/');
            return elements.Last().Split('.').Last();
        }
    }
}
