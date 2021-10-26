using Core;

public class AsynchronousClient3 : ClientBase2
{
    private IStartStrategy _startStrategy;
    private IListenStrategy _listenStrategy;
    private ISendStrategy _sendStrategy;

    public override IStartStrategy StartStrategy => _startStrategy ??= new StartWithConnectStrategy();
    
    public override IListenStrategy ListenStrategy => _listenStrategy ??= new CommonListenStrategy();

    public override ISendStrategy SendStrategy => _sendStrategy ??= new CommonSendStrategy();
}
