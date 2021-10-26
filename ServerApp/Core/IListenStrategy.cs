using System;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Core
{
    public interface IListenStrategy
    {
        public void StartListen(Socket socket);
    }

    public class CommonListenStrategy : IListenStrategy
    {
        public void StartListen(Socket socket)
        {
            // Incoming data from the client.    
            string data = null;
            byte[] bytes = null;

            Task.Run(() =>
            {
                while (true)
                {
                    bytes = new byte[1024];
                    int bytesRec = socket.Receive(bytes);
                    data += Encoding.ASCII.GetString(bytes, 0, bytesRec);
                    if (data.IndexOf("<EOF>", StringComparison.Ordinal) > -1)
                    {
                        Console.WriteLine(data);
                        data = String.Empty;
                    }
                }
            });
        }
    }
}