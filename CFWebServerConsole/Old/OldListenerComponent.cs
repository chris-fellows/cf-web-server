//using CFWebServer.Models;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Net.Sockets;
//using System.Net;
//using System.Text;
//using System.Threading;
//using System.Threading.Tasks;
//using static CFWebServer.MyWebServer;
//using CFWebServer.Interfaces;

//namespace CFWebServer
//{
//    /// <summary>
//    /// Listens for client connections
//    /// </summary>
//    internal class ListenerComponent : IWebServerComponent
//    {
//        private CancellationToken _cancellationToken;

//        private readonly ServerData _serverData;
//        private int _listenPort;
//        private Thread? _thread;
//        private readonly ILogWriter _logWriter;

        
//        public ListenerComponent(int listenPort, ILogWriter logWriter, ServerData serverData, CancellationToken cancellationToken)
//        {
//            _listenPort = listenPort;
//            _logWriter = logWriter;
//            _serverData = serverData;

//            _cancellationToken = cancellationToken;
//        }

//        public void Start()
//        {
//            // Start listener thread
//            //_listenPort = _receivePort;
//            _thread = new Thread(Run);
//            _thread.Start();
//        }

//        public void Stop()
//        {
//            // Wait for listening thread to exit
//            if (_thread != null)
//            {
//                _thread.Join();
//                _thread = null;
//            }
//        }
        
//        /// <summary>
//        /// Worker thread that listens for connection requests
//        /// </summary>
//        public void Run()
//        {
//            // Start listening
//            var tcpLisener = new TcpListener(System.Net.IPAddress.Any, _listenPort);
//            tcpLisener.Start();

//            // Run until cancelled
//            while (!_cancellationToken.IsCancellationRequested)
//            {
//                try
//                {
//                    // Accept TCP client
//                    var tcpClient = AcceptTcpClientAsync(tcpLisener).Result;
//                    if (tcpClient != null)
//                    {
//                        var clientInfo = new ClientInfo()
//                        {
//                            TcpClient = tcpClient,
//                            Stream = tcpClient.GetStream(),
//                        };
//                        _serverData.ClientInfos.Add(clientInfo);

//                        IPEndPoint remoteEndpoint = clientInfo.TcpClient.Client.RemoteEndPoint as IPEndPoint;
//                        _logWriter.Log($"Client {remoteEndpoint.Address.ToString()}:{remoteEndpoint.Port} connected");

//                        //// Notify connected
//                        //if (OnClientConnected != null)
//                        //{
//                        //    OnClientConnected(new EndpointInfo() { Ip = remoteEndpoint.Address.ToString(), Port = remoteEndpoint.Port });
//                        //}
//                    }
//                }
//                catch (AggregateException aggregateException)
//                {
//                    if (aggregateException.InnerException != null &&
//                        aggregateException.InnerException is TaskCanceledException)
//                    {
//                        // No action
//                    }
//                    else
//                    {
//                        throw;
//                    }
//                }
//                catch (SocketException socketException)
//                {
//                    _logWriter.Log($"Error accepting TCP client: {socketException.Message}");
//                    Thread.Sleep(500);
//                }

//                System.Threading.Thread.Yield();
//            }

//            tcpLisener.Stop();
//        }

//        public bool IsListening => _thread != null;

//        private async Task<TcpClient?> AcceptTcpClientAsync(TcpListener tcpListener)
//        {
//            return await tcpListener.AcceptTcpClientAsync(_cancellationToken);
//        }

//    }
//}
