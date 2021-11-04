using System;
using Core;

namespace ServerApp
{
    public class AsynchronousSocketListener3 : ClientBase2
    {
        private IStartStrategy _startStrategy;
        private IListenStrategyFactory _listenStrategyFactory;
        private ISendStrategy _sendStrategy;
        private ILogStrategy _logStrategy;
        private Guid? _id;

        public override IStartStrategy StartStrategy => _startStrategy ??= new StartWithListenerOnManySocketsStrategy();

        public override Guid Id => _id ??= Guid.NewGuid();

        public override IListenStrategyFactory ListenStrategyFactory =>
            _listenStrategyFactory ??= new ListenStrategyFactory();

        public override ISendStrategy SendStrategy => _sendStrategy ??= new CommonSendStrategy();

        public override ILogStrategy LogStrategy =>
            _logStrategy ??= new ColoredLogStrategy(ConsoleColor.DarkRed, "Server");
    }
}