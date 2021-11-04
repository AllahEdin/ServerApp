using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Core
{
    public abstract class ClientBase2
    {
        private readonly List<IListenStrategy> _listenStrategyList = new List<IListenStrategy>();

        public abstract Guid Id { get; }

        public abstract IListenStrategyFactory ListenStrategyFactory { get; }

        public abstract IStartStrategy StartStrategy { get; }

        public abstract ISendStrategy SendStrategy { get; }

        public abstract ILogStrategy LogStrategy { get; }

        public event Action<string> OnNewMessage;

        public async Task Start()
        {
            StartStrategy.OnNewSocketEvent
                += socket =>
                {
                    var listener = 
                        ListenStrategyFactory.Create(socket.Socket, Id);
                    
                    _listenStrategyList.Add(listener);

                    listener.OnNewMessage += msg =>
                    {
                        MessageHub.Queue.Enqueue(msg.Message);
                        LogStrategy.Log(msg.Message);
                    };

                    listener.OnNewMessage += msg => OnNewMessage?.Invoke(msg.Message);

                    listener.StartListen();
                };

            StartStrategy.OnNewSocketEvent
                += SendStrategy.OnNewSocket;

            StartStrategy.Start(Id);
        }

        public async Task Send(TransferMessage msg)
        {
            await SendStrategy.SendBroadcast(msg);
        }
    }
}