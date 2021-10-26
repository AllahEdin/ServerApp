using System;
using System.Threading.Tasks;

namespace ServerApp
{
    class Program
    {
        [STAThread]
        static async Task Main(string[] args)
        {
            new AsynchronousSocketListener2().StartListening();
        }
    }
}
