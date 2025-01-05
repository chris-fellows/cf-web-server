//using CFWebServer.Interfaces;
//using CFWebServer.Models;
//using CFWebServer.RequestHandlers;
//using CFWebServer.Utilities;
//using System;
//using System.Collections.Generic;
//using System.ComponentModel;
//using System.Linq;
//using System.Net;
//using System.Net.Sockets;
//using System.Text;
//using System.Threading;
//using System.Threading.Tasks;
//using static CFWebServer.MyWebServer;

//namespace CFWebServer
//{
//    /// <summary>
//    /// Handles server requests
//    /// </summary>
//    internal class RequestsComponent : IWebServerComponent
//    {
//        private CancellationToken _cancellationToken;

//        private readonly ServerData _serverData;
//        private readonly ILogWriter _logWriter;
//        private Thread? _thread;

//        private List<Packet> _packets = new List<Packet>();

//        public RequestsComponent(ILogWriter logWriter, ServerData serverData,  CancellationToken cancellationToken)
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
//        /// Worker thread that receives packets of data        
//        /// </summary>
//        public void Run()
//        {
//            var receiveTasks = new List<Task>();

//            // Run until cancelled
//            var lastCheckClients = DateTimeOffset.UtcNow;
//            while (!_cancellationToken.IsCancellationRequested)
//            {
//                // Receive data from clients
//                var clientInfos = _serverData.ClientInfos.Where(ci => ci.Stream.DataAvailable).ToList();
//                foreach (var clientInfo in clientInfos)
//                {
//                    var receiveTask = ReceiveAsync(clientInfo);
//                    receiveTasks.Add(receiveTask);
//                }
//                System.Threading.Thread.Sleep(5);

//                // Wait for receive complete
//                if (receiveTasks.Any())
//                {
//                    Task.WaitAll(receiveTasks.ToArray());
//                    receiveTasks.Clear();
//                }
//                System.Threading.Thread.Sleep(5);

//                // Process packets
//                if (_packets.Any())
//                {
//                    ProcessPackets();
//                }

//                // Check clients
//                if (lastCheckClients.AddSeconds(30) <= DateTimeOffset.UtcNow)
//                {
//                    lastCheckClients = DateTimeOffset.UtcNow;
//                    CheckClientsDisconnected();
//                }

//                System.Threading.Thread.Sleep(5);
//            }
//        }

//        /// <summary>
//        /// Receives from client asynchronously
//        /// </summary>
//        /// <param name="clientInfo"></param>
//        /// <returns></returns>
//        private Task ReceiveAsync(ClientInfo clientInfo)
//        {
//            return Task.Factory.StartNew(() =>
//            {
//                var data = new byte[1024 * 50];     // Use same array for all packets. No need to reset between packets
//                while (clientInfo.Stream.DataAvailable)
//                {
//                    var byteCount = clientInfo.Stream.Read(data, 0, data.Length);
                    
//                    if (byteCount > 0)
//                    {                       
//                        // Add packet to queue
//                        IPEndPoint remoteEndpoint = clientInfo.TcpClient.Client.RemoteEndPoint as IPEndPoint;
//                        var packet = new Packet()
//                        {
//                            Endpoint = new EndpointInfo()
//                            {
//                                Ip = remoteEndpoint.Address.ToString(),
//                                Port = remoteEndpoint.Port = remoteEndpoint.Port
//                            },
//                            Data = new byte[byteCount]
//                        };
//                        Buffer.BlockCopy(data, 0, packet.Data, 0, byteCount);

//                        _serverData.Mutex.WaitOne();
//                        _packets.Add(packet);
//                        _serverData.Mutex.ReleaseMutex();

//                        System.Diagnostics.Debug.WriteLine($"Packet received from {packet.Endpoint.Ip}:{packet.Endpoint.Port} ({packet.Data.Length} bytes)");
//                    }
//                    Thread.Sleep(5);
//                }
//            });
//        }

//        private void ProcessPackets()
//        {
//            while (_packets.Any())
//            {
//                var packet = _packets.First();
//                _packets.RemoveAt(0);

//                // Start request
//                var activeServerRequest = new ActiveServerRequest()
//                {
//                    ServerRequest = GetServerRequest(new List<Packet>() { packet })
//                };
//                activeServerRequest.Task = ProcessRequestAsync(activeServerRequest.ServerRequest);
//                _serverData.ActiveServerRequests.Add(activeServerRequest);
//            }
//        }

//        /// <summary>
//        /// Checks for disconnected clients
//        /// </summary>
//        private void CheckClientsDisconnected()
//        {
//            // Get disconnected clients
//            var clientInfos = _serverData.ClientInfos.Where(ci => !IsClientConnected(ci.TcpClient)).ToList();

//            // Clean up disconnected clients
//            while (clientInfos.Any())
//            {
//                var clientInfo = clientInfos[0];
//                IPEndPoint remoteEndpoint = clientInfo.TcpClient.Client.RemoteEndPoint as IPEndPoint;

//                clientInfo.Stream.Close();
//                clientInfo.TcpClient.Close();

//                //// Notify disconnected
//                //if (OnClientDisconnected != null)
//                //{
//                //    OnClientDisconnected(new EndpointInfo()
//                //    {
//                //        Ip = remoteEndpoint.Address.ToString(),
//                //        Port = remoteEndpoint.Port
//                //    });
//                //}

//                clientInfos.Remove(clientInfo);
//                _serverData.ClientInfos.Remove(clientInfo);
//            }
//        }

//        /// <summary>
//        /// Checks if client is connection. TcpClient.Connected doesn't report the current state, only last state
//        /// </summary>
//        /// <param name="tcpClient"></param>
//        /// <returns></returns>
//        private bool IsClientConnected(TcpClient tcpClient)
//        {
//            if (tcpClient.Connected)
//            {
//                var connected = !(tcpClient.Client.Poll(1, SelectMode.SelectRead) && tcpClient.Client.Available == 0);
//                return connected;
//            }

//            /*
//            if (tcpClient.Client.Poll(0, SelectMode.SelectRead))
//            {
//                byte[] data = new byte[1];
//                if (tcpClient.Client.Receive(data, SocketFlags.Peek) == 0)
//                {
//                    return false;
//                }            
//                else
//                {
//                    int xxx = 1000;
//                }
//            }
//            */

//            return false;
//        }

//        private ServerRequest GetServerRequest(List<Packet> packets)
//        {
//            var serverRequest = new ServerRequest()
//            {
//                Endpoint = packets[0].Endpoint
//            };

//            //var reversed = packets[0].Data.Reverse().ToArray();

//            var request = Encoding.UTF8.GetString(packets[0].Data);

//            var headers = HttpUtilities.GetHeadersAsDictionary(request);

//            return serverRequest;
//        }

//        private Task<ServerResponse> ProcessRequestAsync(ServerRequest serverRequest)
//        {
//            var task = Task.Factory.StartNew(() =>
//            {
//                ServerResponse serverResponse = new ServerResponse();

//                var requestHandler = GetRequestHandler(serverRequest);

//                if (requestHandler == null)   // No request handler
//                {

//                }
//                else
//                {
//                    serverResponse = requestHandler.Handle(serverRequest).Result;
//                }

//                return serverResponse;
//            });

//            return task;
//        }

//        private IRequestHandler GetRequestHandler(ServerRequest serverRequest)
//        {
//            return new StaticResourceRequestHandler();
//        }
//    }
//}
