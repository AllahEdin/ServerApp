using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading;

namespace Core
{
    public class StartWithListenerOnManySocketsStrategy : IStartStrategy
    {
        private readonly ManualResetEvent connectDone =
            new ManualResetEvent(false);

        private Guid _id;
        private object _locker = new object();
        private ManualResetEvent _endReceived = new ManualResetEvent(false);
        private string _last;

        public event Action<ClientModel> OnNewSocketEvent;

        public void Start(Guid id)
        {
            _id = id;

            // Establish the local endpoint for the socket.  
            // The DNS name of the computer  
            // running the listener is "host.contoso.com".  
            IPHostEntry ipHostInfo = Dns.GetHostEntry("localhost");
            IPAddress ipAddress = ipHostInfo.AddressList[0];
            IPEndPoint localEndPoint = new IPEndPoint(ipAddress, 11000);

            // Create a TCP/IP socket.  
            var socket = new Socket(ipAddress.AddressFamily,
                SocketType.Stream, ProtocolType.Tcp);

            // Bind the socket to the local endpoint and listen for incoming connections.  
            try
            {
                socket.Bind(localEndPoint);
                socket.Listen(100);

                while (true)
                {
                    connectDone.Reset();

                    socket.BeginAccept(
                        new AsyncCallback(AcceptCallback),
                        socket);

                    connectDone.WaitOne();
                }
            }
            catch (Exception e)
            {
                throw;
            }
        }

        private void AcceptCallback(IAsyncResult ar)
        {
            // Get the socket that handles the client request.  
            var socket = (Socket)ar.AsyncState;
            var newSocket = socket.EndAccept(ar);

            StateObject state = new StateObject();
            state.workSocket = newSocket;
            newSocket.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
                new AsyncCallback(ReadCallback), state);

            _endReceived.WaitOne();

            connectDone.Set();
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

                    if (res.IndexOf(ConfigurationConstants.MessageSeparator) > -1)
                    {
                        var splited = res.Split(ConfigurationConstants.MessageSeparator);

                        if (splited.Length == 2)
                        {
                            if (splited[1] == "")
                            {
                                var init =
                                    JsonSerializer.Deserialize<InitialMessage>(splited[0]);

                                OnNewSocketEvent?.Invoke(new ClientModel()
                                {
                                    Id = init.Id,
                                    Socket = handler,
                                });
                            }
                        }
                    }

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