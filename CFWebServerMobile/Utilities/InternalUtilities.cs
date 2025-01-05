using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CFWebServerMobile.Utilities
{
    internal static class InternalUtilities
    {
        public static void Log(string message)
        {
            var file = Path.Combine(Android.OS.Environment.ExternalStorageDirectory.Path, "Music", "CFWebServerMobile.log");

            try
            {

                using (var streamWriter = new StreamWriter(file, true))
                {
                    streamWriter.WriteLine($"{DateTimeOffset.UtcNow.ToString()}\t{message}");
                    streamWriter.Flush();
                }
            }
            catch { };
        }
    }
}
