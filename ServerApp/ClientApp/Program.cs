﻿using System;
using System.Threading.Tasks;

namespace ClientApp
{
    class Program
    {
        [STAThread]
        static async Task Main(string[] args)
        {
            await AsynchronousClient2.StartClient();
        }
    }
}
