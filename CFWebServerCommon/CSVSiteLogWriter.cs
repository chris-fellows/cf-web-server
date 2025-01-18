using CFWebServer.Interfaces;
using CFWebServer.Models;

namespace CFWebServer
{
    /// <summary>
    /// CSV site log writer
    /// </summary>
    public class CSVSiteLogWriter : ISiteLogWriter
    {
        private readonly string _folder;
        private const Char _delimiter = (Char)9;

        public CSVSiteLogWriter(string folder)
        {
            _folder = folder;
            Directory.CreateDirectory(_folder);
        }

        public void Log(string message)
        {
            var time = DateTimeOffset.UtcNow;

            var logFile = Path.Combine(_folder, $"Log-{time.ToString("yyyy-MM-dd")}.txt");

            var isWriteHeaders = !File.Exists(logFile);
            
            using (var stream = new StreamWriter(logFile))
            {
                if (isWriteHeaders)
                {
                    stream.WriteLine($"Time{_delimiter}Message");
                }

                stream.WriteLine($"{time.ToString()}{_delimiter}{message}");
                stream.Flush();
            }

            Console.WriteLine($"{time.ToString()} {message}");
        }

        public void LogRequest(RequestContext requestContext)
        {
            var time = DateTimeOffset.UtcNow;

            var logFile = Path.Combine(_folder, $"Log-Requests-{time.ToString("yyyy-MM-dd")}.txt");

            var isWriteHeaders = !File.Exists(logFile);

            var request = requestContext.Request;
            var response = requestContext.Response;
            
            using (var stream = new StreamWriter(logFile))
            {
                if (isWriteHeaders)
                {
                    stream.WriteLine($"Time{_delimiter}Method{_delimiter}URL{_delimiter}Status");
                }

                stream.WriteLine($"{time.ToString()}{_delimiter}{request.HttpMethod}{_delimiter}{request.Url.PathAndQuery}{_delimiter}{response.StatusCode}");
                stream.Flush();
            }

            Console.WriteLine($"{DateTimeOffset.UtcNow.ToString()} Request: Method={requestContext.Request.HttpMethod}; " +
                            $"URL={requestContext.Request.Url.ToString()}");
        }
    }
}
