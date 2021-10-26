using System;
using System.Threading.Tasks;

namespace Core
{
    public abstract class ClientBase2 : ClientBase
    {
        public abstract IStartStrategy StartStrategy { get; }

        public abstract IListenStrategy ListenStrategy { get; }

        public abstract ISendStrategy SendStrategy { get; }

        public async Task Start()
        {
            StartStrategy.OnNewSocketEvent
                += socket => ListenStrategy.StartListen(socket);

            StartStrategy.OnNewSocketEvent
                += SendStrategy.OnNewSocket;

            StartStrategy.Start();
        }

        public async Task Send(string msg)
        {
            SendStrategy.SendBroadcast(msg);
        }

        public override ConsoleColor TextColor => ConsoleColor.Cyan;
    }
}