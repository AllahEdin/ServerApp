using System;
using System.Net.Sockets;

namespace Core
{
    public interface IListenStrategyFactory
    {
        public IListenStrategy Create(Socket socket, Guid id);
    }
}