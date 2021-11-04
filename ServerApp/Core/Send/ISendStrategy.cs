using System.Net.Sockets;
using System.Threading.Tasks;

namespace Core
{
    public interface ISendStrategy
    {
        public void OnNewSocket(ClientModel client);

        public Task SendBroadcast(TransferMessage msg);
    }
}