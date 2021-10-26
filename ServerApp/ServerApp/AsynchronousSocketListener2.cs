using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Core;

namespace ServerApp
{
    public class AsynchronousSocketListener2 : ClientBase
    {
        // Thread signal.  
        public  ManualResetEvent allDone = new ManualResetEvent(false);
        private  ManualResetEvent endReceived = new ManualResetEvent(false);
        private  ManualResetEvent sendDone = new ManualResetEvent(false);

        public  async Task StartListening()
        {
            // Establish the local endpoint for the socket.  
            // The DNS name of the computer  
            // running the listener is "host.contoso.com".  
            IPHostEntry ipHostInfo = Dns.GetHostEntry("localhost");
            IPAddress ipAddress = ipHostInfo.AddressList[0];
            IPEndPoint localEndPoint = new IPEndPoint(ipAddress, 11000);

            // Create a TCP/IP socket.  
            Socket listener = new Socket(ipAddress.AddressFamily,
                SocketType.Stream, ProtocolType.Tcp);

            // Bind the socket to the local endpoint and listen for incoming connections.  
            try
            {
                listener.Bind(localEndPoint);
                listener.Listen(100);

                while (true)
                {
                    // Set the event to nonsignaled state.  
                    allDone.Reset();

                    // Start an asynchronous socket to listen for connections.  
                    WriteLine("Waiting for a connection...");
                    listener.BeginAccept(
                        new AsyncCallback(AcceptCallback),
                        listener);

                    // Wait until a connection is made before continuing.  
                    allDone.WaitOne();
                }

            }
            catch (Exception e)
            {
                WriteLine(e.ToString());
            }

            WriteLine("\nPress ENTER to continue...");
            Console.Read();

        }

        public  void AcceptCallback(IAsyncResult ar)
        {
            // Signal the main thread to continue.  
            allDone.Set();

            // Get the socket that handles the client request.  
            Socket listener = (Socket)ar.AsyncState;
            Socket handler = listener.EndAccept(ar);

            Task.Run(async () =>
            {
                while (true)
                {
                    endReceived.Reset();

                    // Create the state object.  
                    StateObject state = new StateObject();
                    state.workSocket = handler;
                    handler.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
                        new AsyncCallback(ReadCallback), state);

                    endReceived.WaitOne();
                }
            });
        }

        public  async void ReadCallback(IAsyncResult ar)
        {
            // Retrieve the state object and the handler socket  
            // from the asynchronous state object.  
            StateObject state = (StateObject)ar.AsyncState;
            Socket handler = state.workSocket;

            // Read data from the client socket.
            int bytesRead = handler.EndReceive(ar);
            
            if (bytesRead > 0)
            {
                var res = Encoding.ASCII.GetString(
                    state.buffer, 0, bytesRead);

                if (true)//(res.IndexOf("@") > -1)
                {
                    WriteLine(res);
                    MessageHub.Queue.Enqueue(state.sb.ToString());

                    Console.WriteLine(res);

                    endReceived.Set();

                    sendDone.Reset();

                    // Send test data to the remote device.  
                    Send(handler, $"Received callback of '{res}' <EOF>");

                    sendDone.WaitOne();

                    state.sb = new StringBuilder();
                }
                else
                {
                    state.sb.Append(res);
                }

                handler.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
                    new AsyncCallback(ReadCallback), state);
            }
            else
            {
                handler.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
                    new AsyncCallback(ReadCallback), state);
            }
        }

        private  void Send(Socket handler, String data)
        {
            // Convert the string data to byte data using ASCII encoding.  
            byte[] byteData = Encoding.ASCII.GetBytes(data);

            // Begin sending the data to the remote device.  
            handler.BeginSend(byteData, 0, byteData.Length, 0,
                new AsyncCallback(SendCallback), handler);
        }

        private  async void SendCallback(IAsyncResult ar)
        {
            try
            {
                // Retrieve the socket from the state object.  
                Socket handler = (Socket)ar.AsyncState;

                // Complete sending the data to the remote device.  
                int bytesSent = handler.EndSend(ar);
            }
            catch (Exception e)
            {
                WriteLine(e.ToString());
            }
        }

        public override ConsoleColor TextColor => ConsoleColor.DarkRed;
    }
}