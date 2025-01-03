//using CFWebServer.Interfaces;
//using CFWebServer.Models;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading;
//using System.Threading.Tasks;

//namespace CFWebServer
//{
//    /// <summary>
//    /// Handles server responses
//    /// </summary>
//    internal class ResponsesComponent : IWebServerComponent
//    {
//        private CancellationToken _cancellationToken;

//        private readonly ServerData _serverData;
//        private readonly ILogWriter _logWriter;
//        private Thread? _thread;        

//        public ResponsesComponent(ILogWriter logWriter, ServerData serverData, CancellationToken cancellationToken)
//        {
//            _logWriter = logWriter;
//            _serverData = serverData;

//            _cancellationToken = cancellationToken;
//        }

//        public void Start()
//        {
//            // Start receive thread
//            _thread = new Thread(Run);
//            _thread.Start();
//        }

//        public void Stop()
//        {
//            // Wait for receive thread to exit
//            if (_thread != null)
//            {
//                _thread.Join();
//                _thread = null;
//            }
//        }

//        /// <summary>
//        /// Worker thread that processes responses for server requests
//        /// </summary>
//        public void Run()
//        {
//            while (!_cancellationToken.IsCancellationRequested)
//            {
//                ProcessCompletedServerRequests();

//                System.Threading.Thread.Sleep(5);
//            }
//        }

//        private void ProcessCompletedServerRequests()
//        {
//            var completedRequests = _serverData.ActiveServerRequests.Where(asr => asr.Task.IsCompleted).ToList();

//            while (completedRequests.Any())
//            {
//                var completedRequest = completedRequests.First();
//                completedRequests.RemoveAt(0);

//                var task = ProcessCompletedRequestAsync(completedRequest);
//            }

//        }

//        private Task ProcessCompletedRequestAsync(ActiveServerRequest activeServerRequest)
//        {
//            var task = Task.Factory.StartNew(() =>
//            {

//            });
//            return task;
//        }
//    }
//}
