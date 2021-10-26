using System;
using System.Threading.Tasks;
using ServerApp;

namespace Manager
{
    class Program
    {
        [STAThread]
        static async Task Main(string[] args)
        {
            var server = new AsynchronousSocketListener3();
            var client = new AsynchronousClient3();
            var client2 = new AsynchronousClient3();

            Task.Run(server.Start);

            await Task.Delay(TimeSpan.FromSeconds(1));

            Task.Run(client.Start);
            Task.Run(client2.Start);

            while (true)
            {
                Parallel.For(0, 10, i => client.Send($"test test {i} <EOF>") );

                Parallel.For(0, 10, i => client2.Send($"test test {i} <EOF>"));

                Parallel.For(0, 10, i => server.Send($"AAAA {i} <EOF>"));
                await Task.Delay(TimeSpan.FromSeconds(0.1));
            }
        }
    }
}
