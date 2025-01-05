using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CFWebServerCommon.Utilities
{
    public static class WebSiteUtilities
    {
        public static void CreateSimpleWebsite(string folder)
        {
            Directory.CreateDirectory(folder);

            var fileListHtml = new StringBuilder("");

            // Add other pages
            for (int index = 0; index < 5; index++)
            {
                var file = Path.Combine(folder, $"File{index + 1}.html");

                File.WriteAllText(file,
                        "<html>" +
                    "<head>" +
                    "</head>" +
                    "<body>" +
                    $"This is test page {(index + 1)}<br/>" +
                    "<a href='Index.html'>Back</a>" +
                    "</body>" +
                    "</html>");

                // Add to file list
                fileListHtml.Append($"<a href='{Path.GetFileName(file)}'>File {index + 1}</a><br/>");
            }

            // Create Index.html
            File.WriteAllText(Path.Combine(folder, "Index.html"),
                    "<html>" +
                    "<head>" +
                    "</head>" +
                    "<body>" +
                    $"This is the Index page hosted on {Environment.MachineName}<br/>" +
                    "Files:<br/>" +
                    $"{fileListHtml.ToString()}" +                    
                    "</body>" +
                    "</html>");           

            // Create sub-folder
            string subFolder1 = Path.Combine(folder, "Folder1");
            Directory.CreateDirectory(subFolder1);

            for (int index = 0; index < 5; index++)
            {
                var file = Path.Combine(subFolder1, $"File{index + 1}.html");
                File.WriteAllText(file,
                        "<html>" +
                    "<head>" +
                    "</head>" +
                    "<body>" +
                    $"This is test page {(index + 1)} in Folder1" +
                    "</body>" +
                    "</html>");
            }
        }
    }
}
