using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using SimpleTcp;

namespace TcpClientApp
{
    public class Server : ITcpServer
    {
        private SimpleTcpServer _server;
        private List<string> _clientIds;

        public void Start()
        {
            _clientIds = new List<string>();

            // instantiate
            _server = new SimpleTcpServer("127.0.0.1:9000");

            // set events
            _server.Events.ClientConnected += ClientConnected;
            _server.Events.ClientDisconnected += ClientDisconnected;
            _server.Events.DataReceived += DataReceived;

            // let's go!
            _server.Start();
        }

        public async Task SendDirect(int clientNum, string msg)
        {
            await _server.SendAsync(_clientIds[clientNum], msg);
        }

        public async Task Send(string msg)
        {
            foreach (var clientId in _clientIds)
            {
                await _server.SendAsync(clientId, msg);
            }
        }

        private void ClientConnected(object sender, ClientConnectedEventArgs e)
        {
            _clientIds.Add(e.IpPort);
            Console.WriteLine("[" + e.IpPort + "] client connected");
        }

        private void ClientDisconnected(object sender, ClientDisconnectedEventArgs e)
        {
            Console.WriteLine("[" + e.IpPort + "] client disconnected: " + e.Reason.ToString());
        }

        private void DataReceived(object sender, DataReceivedEventArgs e)
        {
            Console.WriteLine("[" + e.IpPort + "]: " + Encoding.UTF8.GetString(e.Data));
        }
    }
}