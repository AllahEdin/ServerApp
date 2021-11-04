using System;
using System.Net.Sockets;

namespace Core
{
    public class ListenStrategyFactory : IListenStrategyFactory
    {
        public IListenStrategy Create(Socket socket, Guid id)
        {
            return new AsyncListenStrategy(socket, id);
        }
    }
}