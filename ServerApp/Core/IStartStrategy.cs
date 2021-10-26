using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace Core
{
    public interface IStartStrategy
    {
        public event Action<Socket> OnNewSocketEvent;

        public void Start();
    }

    public class StartWithConnectStrategy : IStartStrategy
    {
        // ManualResetEvent instances signal completion.  
        private readonly ManualResetEvent connectDone =
            new ManualResetEvent(false);

        private readonly int _port = 11000;

        public event Action<Socket> OnNewSocketEvent;

        public void Start()
        {
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

                OnNewSocketEvent?.Invoke(socket);

                // Signal that the connection has been made.  
                connectDone.Set();
            }
            catch (Exception e)
            {
                throw;
            }
        }
    }

    public class StartWithListenerOnManySocketsStrategy : IStartStrategy
    {
        private readonly ManualResetEvent connectDone =
            new ManualResetEvent(false);

        public event Action<Socket> OnNewSocketEvent;

        public void Start()
        {
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

            OnNewSocketEvent?.Invoke(newSocket);

            connectDone.Set();
        }
    }
}