using System;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Core
{
    public class AsyncListenStrategy : ListenStrategyBase,IListenStrategy
    {
        private readonly Guid _id;

        private object _locker = new object();
        private object _locker2 = new object();
        private readonly ManualResetEvent _endReceived = new ManualResetEvent(false);

        private string _last;

        private string MessageSeparator => ConfigurationConstants.MessageSeparator;

        public event Action<MessageEventModel> OnNewMessage;

        public AsyncListenStrategy(Socket socket, Guid id):base(socket)
        {
            _id = id;
        }

        public void StartListen()
        {
            // Incoming data from the client.    
            string data = null;
            byte[] bytes = null;

            Task.Run(() =>
            {
                lock (_locker)
                {
                    _endReceived.Reset();

                    // Create the state object.  
                    StateObject state = new StateObject();
                    state.workSocket = Socket;
                    Socket.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
                        new AsyncCallback(ReadCallback), state);

                    _endReceived.WaitOne();
                }
            });
        }

        public void ReadCallback(IAsyncResult ar)
        {
            lock (_locker2)
            {
                // Retrieve the state object and the handler socket  
                // from the asynchronous state object.  
                StateObject state = (StateObject)ar.AsyncState;
                Socket handler = state.workSocket;

                // Read data from the client socket.
                int bytesRead = handler.EndReceive(ar);

                if (bytesRead > 0)
                {
                    //InStream.WriteAsync(state.buffer, 0, bytesRead);

                    using (var sw = new StreamWriter(File.Open(FileName, FileMode.Append, FileAccess.Write)))
                    {
                        sw.WriteLine(Encoding.ASCII.GetString(
                            state.buffer, 0, bytesRead));
                    }

                    var res =
                        _last + Encoding.ASCII.GetString(
                            state.buffer, 0, bytesRead);

                    OnNewMessage?.Invoke(new MessageEventModel()
                    {
                        Id = _id,
                        Message = res
                    });
                    state.sb = new StringBuilder();
                    _endReceived.Set();

                    handler.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
                        new AsyncCallback(ReadCallback), state);
                }
                else
                {
                    handler.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
                        new AsyncCallback(ReadCallback), state);
                }
            }
        }

    }
}