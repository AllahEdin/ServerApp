using System;
using System.Text;
using System.Threading.Tasks;
using SimpleTcp;

namespace WebClient.Services.Impl
{
    public class Client
    {
        private SimpleTcpClient _client;

        public void Start()
        {
            // instantiate
            _client = new SimpleTcpClient("127.0.0.1:9000");

            // set events
            _client.Events.Connected += Connected;
            _client.Events.Disconnected += Disconnected;
            _client.Events.DataReceived += DataReceived;

            // let's go!
            _client.Connect();
        }

        public async Task Send(string msg)
        {
            await _client.SendAsync(msg);
        }

        static void Connected(object sender, EventArgs e)
        {
            Console.WriteLine("*** Server connected");
        }

        static void Disconnected(object sender, EventArgs e)
        {
            Console.WriteLine("*** Server disconnected");
        }

        static void DataReceived(object sender, DataReceivedEventArgs e)
        {
            Console.WriteLine("[" + e.IpPort + "] " + Encoding.UTF8.GetString(e.Data));
        }
    }
}