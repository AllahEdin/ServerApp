using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Core
{
    public class ClientModel
    {
        public Guid Id { get; set; }

        public Socket Socket { get; set; }
    }

    public class InitialMessage
    {
        public Guid Id { get; set; }

        public string Name { get; set; }
    }

    public class StartWithConnectStrategy : IStartStrategy
    {
        // ManualResetEvent instances signal completion.  
        private readonly ManualResetEvent connectDone =
            new ManualResetEvent(false);

        private readonly int _port = 11000;
        private readonly object _locker = new object();
        private Guid _id;
        private string _last;
        private ManualResetEvent _endReceived = new ManualResetEvent(false);
        private ManualResetEvent _sendDone = new ManualResetEvent(false);

        public event Action<ClientModel> OnNewSocketEvent;

        public void Start(Guid id)
        {
            _id = id;

            // Establish the remote endpoint for the socket.  
            // The name of the
            // remote device is "host.contoso.com".  
            IPHostEntry ipHostInfo = Dns.GetHostEntry("localhost");
            IPAddress ipAddress = ipHostInfo.AddressList[0];
            IPEndPoint remoteEP = new IPEndPoint(ipAddress, _port);

            // Create a TCP/IP socket.  
            var socket = new Socket(ipAddress.AddressFamily,
                SocketType.Stream, ProtocolType.Tcp);

            // Connect to the remote endpoint.  
            socket.BeginConnect(remoteEP,
                new AsyncCallback(ConnectCallback), socket);
            connectDone.WaitOne();
        }

        private void ConnectCallback(IAsyncResult ar)
        {
            try
            {
                // Retrieve the socket from the state object.  
                Socket socket = (Socket)ar.AsyncState;

                // Complete the connection.  
                socket.EndConnect(ar);

                _sendDone.Reset();

                var json =
                    JsonSerializer.Serialize(new InitialMessage()
                    {
                        Id = _id,
                        Name = "Андрей",
                    }) + ConfigurationConstants.MessageSeparator;

                var byteData = Encoding.ASCII.GetBytes(json);

                socket.BeginSend(byteData, 0, byteData.Length, 0,
                    new AsyncCallback(SendCallback), socket);

                _sendDone.WaitOne();

                StateObject state = new StateObject();
                state.workSocket = socket;
                socket.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
                    new AsyncCallback(ReadCallback), state);

                _endReceived.WaitOne();

                OnNewSocketEvent?.Invoke(new ClientModel()
                {
                    Socket = socket,
                    Id = _id
                });

                // Signal that the connection has been made.  
                connectDone.Set();
            }
            catch (Exception e)
            {
                throw;
            }
        }

        private void SendCallback(IAsyncResult ar)
        {
            try
            {
                // Retrieve the socket from the state object.  
                var socket = (Socket)ar.AsyncState;

                // Complete sending the data to the remote device.  
                int bytesSent = socket.EndSend(ar);

                _sendDone.Set();
            }
            catch (Exception e)
            {
                throw;
            }
        }

        public void ReadCallback(IAsyncResult ar)
        {
            lock (_locker)
            {
                // Retrieve the state object and the handler socket  
                // from the asynchronous state object.  
                StateObject state = (StateObject)ar.AsyncState;
                Socket handler = state.workSocket;

                // Read data from the client socket.
                int bytesRead = handler.EndReceive(ar);

                if (bytesRead > 0)
                {
                    var res =
                        _last + Encoding.ASCII.GetString(
                            state.buffer, 0, bytesRead);

                    state.sb = new StringBuilder();
                    _endReceived.Set();
                }
                else
                {
                    handler.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
                        new AsyncCallback(ReadCallback), state);
                }
            }
        }
    }
}