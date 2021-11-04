using System;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Core;
using ServerApp;

namespace Manager
{
    class Program
    {
        private static object _locker = new object();

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

            await Task.Delay(TimeSpan.FromSeconds(3));

            int cc = 0;

            for (int i = 0; i < 10000; i++)
            {

                await client.Send(new TransferMessage()
                {
                    Id = Guid.NewGuid(),
                    Message = "Message of Reeeeeeeeeeeeeeeeeeeeealy long message",
                    Number = cc
                });

                //tasks.Add(Task.Run(async () =>
                //{
                //    var task1 = client.Send($"{cc} <EOF>");
                //    //var task2 = client2.Send($"test client1 123123 <EOF>");

                //    await Task.WhenAll(new[]
                //    {
                //        task1,
                //        //task2,
                //    });
                //}));

                Interlocked.Increment(ref cc);
            }

            //await Task.Delay(TimeSpan.FromSeconds(10));

            int countEntries = 0;
            int countEntries2 = 0;
            var numbers = Enumerable.Range(0, 20000).Select(i => true).ToList();
            string total = "";
            string last = "";
            while (MessageHub.Queue.TryDequeue(out var msg))
            {
                msg = last + msg;

                //total += msg;
                var res =
                    msg.Split("<EOF>") ?? new string[0];

                if (res[^1] == "")
                {
                    foreach (var entry in res.Take(res.Length - 1))
                    {
                        //if (entry == "test client1 123123 ")
                        //{
                        //    countEntries++;
                        //}
                        var tm =
                            JsonSerializer.Deserialize<TransferMessage>(entry);

                        if (numbers[tm.Number])
                        {
                            throw new Exception();
                        }

                        countEntries2++;
                    }
                }
                else
                {
                    last = res[^1];
                }
            }

            countEntries2 = total.Split("<EOF>").Length;

            //using (var sw = new StreamWriter(File.OpenWrite(Path.GetTempFileName())))
            //{
            //    await sw.WriteLineAsync(total);
            //}

            await Task.Delay(TimeSpan.FromSeconds(1000));
        }
    }
}
