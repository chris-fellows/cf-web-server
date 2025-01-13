//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace CFWebServerCommon
//{
//    public class MimeDatabaseCreator
//    {
//        public void Create(string sourceFile, string destinationFile)
//        {
//            using (var outputStream = new StreamWriter(destinationFile, true))
//            {
//                outputStream.WriteLine("var mimeTypes = new List<MimeTypeInfo>();");
//                outputStream.WriteLine("");

//                using (var inputReader = new StreamReader(sourceFile))
//                {
//                    Char delimiter = (Char)9;

//                    StringBuilder code = new StringBuilder("");

//                    int lineCount = 0;
//                    while (!inputReader.EndOfStream)
//                    {
//                        var line = inputReader.ReadLine();
//                        lineCount++;

//                        if (lineCount > 1)
//                        {
//                            var elements = line.Split(delimiter);

//                            // Get file extensions
//                            var extensions = elements[1].Split(',');
//                            for(int index = 0; index < extensions.Length; index++)
//                            {
//                                extensions[index] = extensions[index].Trim().Replace("\"", "");
//                            }

//                            // Create line
//                            var outputLine = new StringBuilder($"mimeTypes.Add(new MimeTypeInfo(" + "\"" + elements[0] + "\"" + ", new string[] { ");
//                            foreach(var extension in extensions)
//                            {
//                                if (extension != extensions.First())
//                                {
//                                    outputLine.Append(", ");
//                                }
//                                outputLine.Append("\"" + extension + "\"");
//                            }
//                            outputLine.Append(" }));");

//                            outputStream.WriteLine(outputLine.ToString());
//                        }
//                    }                    
//                }

//                outputStream.Flush();
//            }            
//        }
//    }
//}
