//using CFWebServer.Models;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Net.Sockets;
//using System.Net;
//using System.Text;
//using System.Threading.Tasks;
//using CFWebServer.Interfaces;
//using CFWebServer.RequestHandlers;
//using System.Runtime.CompilerServices;

//namespace CFWebServer
//{
//    /// <summary>
//    /// Web server
//    /// </summary>
//    internal class MyWebServerV1
//    {        
//        private readonly ILogWriter _logWriter;

//        private ServerData _serverData = new ServerData();

//        private readonly int _receivePort;
        
//        private CancellationTokenSource? _cancellationTokenSource;
    
//        private List<IWebServerComponent> _webServerComponents = new List<IWebServerComponent>();
          
//        /// <summary>
//        /// Event for client connected
//        /// </summary>
//        /// <param name="endpointInfo"></param>
//        public delegate void ClientConnected(EndpointInfo endpointInfo);
//        public event ClientConnected? OnClientConnected;

//        /// <summary>
//        /// Event for client disconnected
//        /// </summary>
//        /// <param name="endpointInfo"></param>
//        public delegate void ClientDisconnected(EndpointInfo endpointInfo);
//        public event ClientDisconnected? OnClientDisconnected;

//        public MyWebServerV1(int receivePort, string rootFolder, ILogWriter logWriter)                   
//        {
//            _receivePort = receivePort;            
//            _logWriter = logWriter;

//            _serverData.RootFolder = rootFolder;
//        }

//        public void Dispose()
//        {
//            StopListening();

//            // Clean up clients            
//            while (_serverData.ClientInfos.Any())
//            {
//                _serverData.ClientInfos[0].TcpClient?.Close();
//                _serverData.ClientInfos[0].TcpClient?.Dispose();
//                _serverData.ClientInfos.RemoveAt(0);
//            }
//        }

//        /// <summary>
//        /// Endpoints for clients
//        /// </summary>
//        public List<EndpointInfo> ClientRemoteEndpoints
//        {
//            get
//            {
//                var endpoints = _serverData.ClientInfos.Select(clientInfo =>
//                {
//                    IPEndPoint remoteEndpoint = clientInfo.TcpClient.Client.RemoteEndPoint as IPEndPoint;
//                    return new EndpointInfo()
//                    {
//                        Ip = remoteEndpoint.Address.ToString(),
//                        Port = remoteEndpoint.Port
//                    };
//                }).ToList();

//                return endpoints;
//            }
//        }   

//        public void StartListening()
//        {
//            _logWriter.Log($"Starting listening on port {_receivePort}");

//            _cancellationTokenSource = new CancellationTokenSource();

//            // Add listener component
//            var listenerComponent = new ListenerComponent(_receivePort, _logWriter, _serverData,  _cancellationTokenSource.Token);
//            _webServerComponents.Add(listenerComponent);
            
//            // Add requests component
//            var requestsComponent = new RequestsComponent(_logWriter, _serverData, _cancellationTokenSource.Token);
//            _webServerComponents.Add(requestsComponent);

//            // Add responses component
//            var responsesComponent = new ResponsesComponent(_logWriter, _serverData, _cancellationTokenSource.Token);            
//            _webServerComponents.Add(responsesComponent);

//            // Start components
//            _webServerComponents.ForEach(component => component.Start());            

//            _logWriter.Log($"Listening");
//        }

//        public void StopListening()
//        {
//            _logWriter.Log("Stopping listening");

//            // Instruct components to cancel
//            if (_cancellationTokenSource != null)
//            {
//                _cancellationTokenSource.Cancel();
//            }

//            // Stop component (Just waits for thread to complete after cancel request)
//            while (_webServerComponents.Any())
//            {
//                _webServerComponents[0].Stop();
//                _webServerComponents.RemoveAt(0);
//            }

//            _logWriter.Log("Stopped listening");
//        }

//        //private async Task<TcpClient?> AcceptTcpClientAsync(TcpListener tcpListener)
//        //{
//        //    return await tcpListener.AcceptTcpClientAsync(_cancellationTokenSource.Token);
//        //}

//        ///// <summary>
//        ///// Checks if client is connection. TcpClient.Connected doesn't report the current state, only last state
//        ///// </summary>
//        ///// <param name="tcpClient"></param>
//        ///// <returns></returns>
//        //private bool IsClientConnected(TcpClient tcpClient)
//        //{
//        //    if (tcpClient.Connected)
//        //    {
//        //        var connected = !(tcpClient.Client.Poll(1, SelectMode.SelectRead) && tcpClient.Client.Available == 0);
//        //        return connected;
//        //    }

//        //    /*
//        //    if (tcpClient.Client.Poll(0, SelectMode.SelectRead))
//        //    {
//        //        byte[] data = new byte[1];
//        //        if (tcpClient.Client.Receive(data, SocketFlags.Peek) == 0)
//        //        {
//        //            return false;
//        //        }            
//        //        else
//        //        {
//        //            int xxx = 1000;
//        //        }
//        //    }
//        //    */

//        //    return false;
//        //}

//        ///// <summary>
//        ///// Checks for disconnected clients
//        ///// </summary>
//        //private void CheckClientsDisconnected()
//        //{
//        //    // Get disconnected clients
//        //    var clientInfos = _clientInfos.Where(ci => !IsClientConnected(ci.TcpClient)).ToList();

//        //    // Clean up disconnected clients
//        //    while (clientInfos.Any())
//        //    {
//        //        var clientInfo = clientInfos[0];
//        //        IPEndPoint remoteEndpoint = clientInfo.TcpClient.Client.RemoteEndPoint as IPEndPoint;

//        //        clientInfo.Stream.Close();
//        //        clientInfo.TcpClient.Close();

//        //        // Notify disconnected
//        //        if (OnClientDisconnected != null)
//        //        {
//        //            OnClientDisconnected(new EndpointInfo()
//        //            {
//        //                Ip = remoteEndpoint.Address.ToString(),
//        //                Port = remoteEndpoint.Port
//        //            });
//        //        }

//        //        clientInfos.Remove(clientInfo);
//        //        _clientInfos.Remove(clientInfo);
//        //    }
//        //}

//        ///// <summary>
//        ///// Worker thread that receives packets of data        
//        ///// </summary>
//        //public void ReceiveWorker()
//        //{
//        //    var receiveTasks = new List<Task>();

//        //    // Run until cancelled
//        //    var lastCheckClients = DateTimeOffset.UtcNow;
//        //    while (!_cancellationTokenSource.Token.IsCancellationRequested)
//        //    {
//        //        // Receive data from clients
//        //        var clientInfos = _clientInfos.Where(ci => ci.Stream.DataAvailable).ToList();
//        //        foreach (var clientInfo in clientInfos)
//        //        {
//        //            var receiveTask = ReceiveAsync(clientInfo);
//        //            receiveTasks.Add(receiveTask);
//        //        }
//        //        System.Threading.Thread.Sleep(5);

//        //        // Wait for receive complete
//        //        if (receiveTasks.Any())
//        //        {
//        //            Task.WaitAll(receiveTasks.ToArray());
//        //            receiveTasks.Clear();
//        //        }
//        //        System.Threading.Thread.Sleep(5);

//        //        // Process packets
//        //        if (_packets.Any())
//        //        {
//        //            ProcessPackets();
//        //        }

//        //        // Check clients
//        //        if (lastCheckClients.AddSeconds(30) <= DateTimeOffset.UtcNow)
//        //        {
//        //            lastCheckClients = DateTimeOffset.UtcNow;
//        //            CheckClientsDisconnected();
//        //        }

//        //        System.Threading.Thread.Sleep(5);
//        //    }
//        //}

//        ///// <summary>
//        ///// Worker thread that processes responses for server requests
//        ///// </summary>
//        //public void ResponseWorker()
//        //{
//        //    while (!_cancellationTokenSource.Token.IsCancellationRequested)
//        //    {
//        //        ProcessCompletedServerRequests();

//        //        System.Threading.Thread.Sleep(5);
//        //    }
//        //}

//        ///// <summary>
//        ///// Receives from client asynchronously
//        ///// </summary>
//        ///// <param name="clientInfo"></param>
//        ///// <returns></returns>
//        //private Task ReceiveAsync(ClientInfo clientInfo)
//        //{
//        //    return Task.Factory.StartNew(() =>
//        //    {
//        //        var data = new byte[1024 * 50];     // Use same array for all packets. No need to reset between packets
//        //        while (clientInfo.Stream.DataAvailable)
//        //        {
//        //            var byteCount = clientInfo.Stream.Read(data, 0, data.Length);
//        //            if (byteCount > 0)
//        //            {
//        //                // Add packet to queue
//        //                IPEndPoint remoteEndpoint = clientInfo.TcpClient.Client.RemoteEndPoint as IPEndPoint;
//        //                var packet = new Packet()
//        //                {
//        //                    Endpoint = new EndpointInfo()
//        //                    {
//        //                        Ip = remoteEndpoint.Address.ToString(),
//        //                        Port = remoteEndpoint.Port = remoteEndpoint.Port
//        //                    },
//        //                    Data = new byte[byteCount]
//        //                };
//        //                Buffer.BlockCopy(data, 0, packet.Data, 0, byteCount);

//        //                _mutex.WaitOne();
//        //                _packets.Add(packet);
//        //                _mutex.ReleaseMutex();

//        //                System.Diagnostics.Debug.WriteLine($"Packet received from {packet.Endpoint.Ip}:{packet.Endpoint.Port} ({packet.Data.Length} bytes)");
//        //            }
//        //            Thread.Sleep(5);
//        //        }
//        //    });
//        //}

//        ///// <summary>
//        ///// Sends message
//        ///// </summary>
//        ///// <param name="connectionMessage"></param>
//        ///// <param name="remoteEndpointInfo"></param>
//        //public void SendMessage(ConnectionMessage connectionMessage, EndpointInfo remoteEndpointInfo)
//        //{
//        //    // Serialize message
//        //    var data = InternalUtilities.Serialise(connectionMessage);

//        //    // Get ClientInfo for remote endpoint
//        //    var clientInfo = GetClientInfoByRemoteEndpoint(remoteEndpointInfo);

//        //    // If no connection then connect
//        //    if (clientInfo == null)
//        //    {
//        //        try
//        //        {
//        //            clientInfo = ConnectToClient(remoteEndpointInfo);
//        //        }
//        //        catch (Exception exception)
//        //        {
//        //            throw new ConnectionException($"Error connecting to {remoteEndpointInfo.Ip}:{remoteEndpointInfo.Port}", exception);
//        //        }
//        //    }

//        //    // Send data
//        //    clientInfo.TcpClient.Client.Send(data);
//        //}

//        ///// <summary>
//        ///// Connects to client
//        ///// </summary>
//        ///// <param name="remoteEndpointInfo"></param>
//        ///// <returns></returns>
//        //private ClientInfo ConnectToClient(EndpointInfo remoteEndpointInfo)
//        //{
//        //    System.Diagnostics.Debug.WriteLine($"Connecting to {remoteEndpointInfo.Ip}:{remoteEndpointInfo.Port}");
//        //    var tcpClient = new TcpClient();
//        //    tcpClient.Connect(IPAddress.Parse(remoteEndpointInfo.Ip), remoteEndpointInfo.Port);
//        //    System.Diagnostics.Debug.WriteLine($"Connected to {remoteEndpointInfo.Ip}:{remoteEndpointInfo.Port}");

//        //    var clientInfo = new ClientInfo()
//        //    {
//        //        TcpClient = tcpClient,
//        //        Stream = tcpClient.GetStream()
//        //    };
//        //    _clientInfos.Add(clientInfo);

//        //    // Notify client connected 
//        //    if (OnClientConnected != null)
//        //    {
//        //        OnClientConnected(new EndpointInfo()
//        //        {
//        //            Ip = remoteEndpointInfo.Ip,
//        //            Port = remoteEndpointInfo.Port
//        //        });
//        //    }

//        //    return clientInfo;
//        //}

//        /// <summary>
//        /// Gets ClientInfo by remote endpoint
//        /// </summary>
//        /// <param name="endpointInfo"></param>
//        /// <returns></returns>
//        private ClientInfo? GetClientInfoByRemoteEndpoint(EndpointInfo endpointInfo)
//        {
//            // Map to IPv6 address
//            var address = IPAddress.Parse($"{endpointInfo.Ip}").MapToIPv6();

//            // Check each client
//            foreach (var clientInfo in _serverData.ClientInfos)
//            {
//                IPEndPoint clientRemoteEndpoint = clientInfo.TcpClient.Client.RemoteEndPoint as IPEndPoint;
//                IPAddress clientAddress = clientRemoteEndpoint.Address.MapToIPv6();

//                if (clientAddress.ToString().Equals(address.ToString()) &&
//                    clientRemoteEndpoint.Port == endpointInfo.Port)
//                {
//                    return clientInfo;
//                }
//            }

//            return null;
//        }

//        //private void ProcessPackets()
//        //{            
//        //    while (_packets.Any())
//        //    {
//        //        var packet = _packets.First();
//        //        _packets.RemoveAt(0);

//        //        // Start request
//        //        var activeServerRequest = new ActiveServerRequest()
//        //        {
//        //            ServerRequest = GetServerRequest(new List<Packet>() { packet })
//        //        };
//        //        activeServerRequest.Task = ProcessRequestAsync(activeServerRequest.ServerRequest);
//        //        _activeServerRequests.Add(activeServerRequest);
//        //    }
//        //}       
//    }
//}
