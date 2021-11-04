using System;
using System.Net.Sockets;
using System.Threading.Tasks;
using SimpleTcp;
using Stateless;

namespace WebServer.Services.Impl
{
    public static class StateMachineMethods
    {
        private static StateMachine<ServerState, ServerTrigger>.TriggerWithParameters<TcpClient> _clientConnected;

        private static StateMachine<ServerState, ServerTrigger> _stateMachine;
        
        private static SimpleTcpServer _server;

        public static StateMachine<ServerState, ServerTrigger> CreateServerStateMachine()
        {
            _stateMachine =
                new StateMachine<ServerState, ServerTrigger>(ServerState.Start);
            
            _clientConnected = _stateMachine.SetTriggerParameters<TcpClient>(ServerTrigger.NewSocketAccepted);

            _stateMachine.Configure(ServerState.Start)
                .Permit(ServerTrigger.StartAccept, ServerState.WaitForAccept);

            _stateMachine.Configure(ServerState.WaitForAccept)
                .OnEntryAsync(BeginAccept)
                .Permit(ServerTrigger.NewSocketAccepted, ServerState.StartListen);

            _stateMachine.Configure(ServerState.StartListen)
                .OnEntryFromAsync(_clientConnected, client => StartListen(client));

            _stateMachine.OnTransitioned(t => Console.WriteLine($"OnTransitioned: {t.Source} -> {t.Destination} via {t.Trigger}({string.Join(", ", t.Parameters)})"));

            return _stateMachine;
        }

        private static async Task BeginAccept()
        {
            Console.WriteLine("Begin accept");

            Task.Delay(TimeSpan.FromSeconds(3)).GetAwaiter().GetResult();

            await _stateMachine.FireAsync(_clientConnected, new TcpClient());
        }

        private static async Task StartListen(TcpClient tcpClient)
        {
            Console.WriteLine("Starting to listen socket");
        }


        public static async Task OnMessageReceived(Guid id, string msg)
        {

        }

        #region Internal 


        private static void DataReceived(object? sender, DataReceivedEventArgs e)
        {
            throw new NotImplementedException();
        }

        private static void ClientDisconnected(object? sender, ClientDisconnectedEventArgs e)
        {
            throw new NotImplementedException();
        }

        private static void ClientConnected(object? sender, ClientConnectedEventArgs e)
        {

        }

        #endregion
    }
}