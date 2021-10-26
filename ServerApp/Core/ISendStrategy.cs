using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Core
{
    public interface ISendStrategy
    {
        public void OnNewSocket(Socket socket);

        public void SendBroadcast(string msg);
    }

    public class CommonSendStrategy : ISendStrategy
    {
        private readonly List<Socket> _list = new List<Socket>();

        public void OnNewSocket(Socket socket)
        {
            _list.Add(socket);
        }

        public void SendBroadcast(string msg)
        {
            Parallel.For(0, _list.Count, async i =>
            {
                byte[] byteData = Encoding.ASCII.GetBytes(msg);

                _list[i].BeginSend(byteData, 0, byteData.Length, 0,
                    new AsyncCallback(SendCallback), _list[i]);
            });
        }

        private void SendCallback(IAsyncResult ar)
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
                throw;
            }
        }
    }
}