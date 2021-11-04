using System.IO;
using System.Net.Sockets;

namespace Core
{
    public abstract class ListenStrategyBase
    {
        protected Socket Socket { get; }

        protected Stream InStream { get; }

        protected string FileName;

        public ListenStrategyBase(Socket socket)
        {
            Socket = socket;
            FileName = Path.GetTempFileName();
            InStream = new MemoryStream();
        }
    }
}