using System;
using Core;

public class AsynchronousClient3 : ClientBase2
{
    private IStartStrategy _startStrategy;
    private IListenStrategyFactory _listenStrategyFactory;
    private ISendStrategy _sendStrategy;
    private ILogStrategy _logStrategy;
    private Guid? _id;

    public override IStartStrategy StartStrategy => _startStrategy ??= new StartWithConnectStrategy();

    public override IListenStrategyFactory ListenStrategyFactory =>
        _listenStrategyFactory ??= new ListenStrategyFactory();

    public override Guid Id => _id ??= Guid.NewGuid();

    public override ISendStrategy SendStrategy => _sendStrategy ??= new SendWithConfirmStrategy();

    public override ILogStrategy LogStrategy => _logStrategy ??= new ColoredLogStrategy(ConsoleColor.DarkCyan, $"Client No {Guid.NewGuid().ToString()}");
}
