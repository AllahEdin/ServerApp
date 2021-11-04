using System;
using System.Diagnostics;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Core
{
    public class CommonListenStrategy : ListenStrategyBase,IListenStrategy
    {
        private object _locker = new object();

        private string MessageSeparator => ConfigurationConstants.MessageSeparator;

        public event Action<MessageEventModel> OnNewMessage;

        public void StartListen()
        {
            // Incoming data from the client.    
            string data = null;
            byte[] bytes = null;

            Task.Run(() =>
            {
                lock (_locker)
                {
                    while (true)
                    {
                        bytes = new byte[1024];
                        int bytesRec = Socket.Receive(bytes);
                        data += Encoding.ASCII.GetString(bytes, 0, bytesRec);
                        if (data.IndexOf("<EOF>", StringComparison.Ordinal) > -1)
                        {
                            bool messagesLeft = true;

                            while (messagesLeft)
                            {
                                var first = data.IndexOf(MessageSeparator, StringComparison.Ordinal);
                                var message = data.Substring(0, first);

                                OnNewMessage?.Invoke(new MessageEventModel()
                                {
                                    Message = message
                                });

                                data = data.Substring(first + MessageSeparator.Length,
                                    data.Length - (first + MessageSeparator.Length));

                                if (data.Length <= 0 ||
                                    data.IndexOf("<EOF>", StringComparison.Ordinal) <= -1)
                                {
                                    messagesLeft = false;
                                }
                            }

                            data = String.Empty;

                            Stopwatch sw = new Stopwatch();

                            sw.Reset();
                            sw.Start();

                            while (sw.ElapsedTicks < 1000)
                            {
                            }

                            sw.Stop();
                        }
                    }
                }
            });
        }

        public CommonListenStrategy(Socket socket) : base(socket)
        {
        }
    }
}