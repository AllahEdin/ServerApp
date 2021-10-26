using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Assets.Scripts.Net;
using Core;

public class AsynchronousClient2 : ClientBase
{
    // The port number for the remote device.  
    private const int port = 11000;

    // ManualResetEvent instances signal completion.  
    private static ManualResetEvent connectDone =
        new ManualResetEvent(false);
    private static ManualResetEvent sendDone =
        new ManualResetEvent(false);
    private static ManualResetEvent receiveDone =
        new ManualResetEvent(false);
    private static ManualResetEvent recievedCallback =
        new ManualResetEvent(false);


    public async Task StartClient()
    {
        // Connect to a remote device.  
        try
        {
            // Establish the remote endpoint for the socket.  
            // The name of the
            // remote device is "host.contoso.com".  
            IPHostEntry ipHostInfo = Dns.GetHostEntry("localhost");
            IPAddress ipAddress = ipHostInfo.AddressList[0];
            IPEndPoint remoteEP = new IPEndPoint(ipAddress, port);

            // Create a TCP/IP socket.  
            Socket socket = new Socket(ipAddress.AddressFamily,
                SocketType.Stream, ProtocolType.Tcp);

            // Connect to the remote endpoint.  
            socket.BeginConnect(remoteEP,
                new AsyncCallback(ConnectCallback), socket);
            connectDone.WaitOne();

            Task.Run(async () =>
            {
                int count = 0;
                while (count < 10000)
                {
                    recievedCallback.Reset();
                    sendDone.Reset();
                    count++;

                    // Send test data to the remote device.  
                    Send(socket, $"This is a test {count} @");
                    sendDone.WaitOne();

                    recievedCallback.WaitOne();
                }
            });

            Task.Run(async () =>
            {
                try
                {
                    while (true)
                    {
                        receiveDone.Reset();

                        // Create the state object.  
                        StateObject state = new StateObject();
                        state.workSocket = socket;
                        socket.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
                            new AsyncCallback(ReadCallback), state);

                        receiveDone.WaitOne();
                    }
                }
                catch (Exception e)
                {
                   WriteLine(e.ToString());
                }
            });


        }
        catch (Exception e)
        {
           WriteLine(e.ToString());
        }
    }

    private void ConnectCallback(IAsyncResult ar)
    {
        try
        {
            // Retrieve the socket from the state object.  
            Socket client = (Socket)ar.AsyncState;

            // Complete the connection.  
            client.EndConnect(ar);

            // Signal that the connection has been made.  
            connectDone.Set();
        }
        catch (Exception e)
        {
           WriteLine(e.ToString());
        }
    }

    public void ReadCallback(IAsyncResult ar)
    {
        String content = String.Empty;

        // Retrieve the state object and the handler socket  
        // from the asynchronous state object.  
        StateObject state = (StateObject)ar.AsyncState;
        Socket handler = state.workSocket;

        // Read data from the client socket.
        int bytesRead = handler.EndReceive(ar);

        if (bytesRead > 0)
        {
            // There  might be more data, so store the data received so far.  
            state.sb.Append(Encoding.ASCII.GetString(
                state.buffer, 0, bytesRead));

            // Check for end-of-file tag. If it is not there, read
            // more data.  
            content = state.sb.ToString();

            if (true)//(content.IndexOf("<EOF>") > -1)
            {
                // All the data has been read from the
                // client. Display it on the console.  
               WriteLine($"Read {bytesRead} bytes from socket. \n Data : {content}");

                receiveDone.Set();
                recievedCallback.Set();
            }
            else
            {
                handler.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
                    new AsyncCallback(ReadCallback), state);
            }
        }
    }

    private void Send(Socket handler, String data)
    {
        // Convert the string data to byte data using ASCII encoding.  
        byte[] byteData = Encoding.ASCII.GetBytes(data);

        // Begin sending the data to the remote device.  
        handler.BeginSend(byteData, 0, byteData.Length, 0,
            new AsyncCallback(SendCallback), handler);
    }

    private void SendCallback(IAsyncResult ar)
    {
        try
        {
            // Retrieve the socket from the state object.  
            Socket handler = (Socket)ar.AsyncState;

            // Complete sending the data to the remote device.  
            int bytesSent = handler.EndSend(ar);

            sendDone.Set();

        }
        catch (Exception e)
        {
           WriteLine(e.ToString());
        }
    }

    public override ConsoleColor TextColor => ConsoleColor.Cyan;
}