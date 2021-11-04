using System.Threading.Tasks;

namespace TcpClientApp
{
    public interface ITcpServer
    {
        void Start();

        Task SendDirect(int clientNum, string msg);

        Task Send(string msg);
    }
}