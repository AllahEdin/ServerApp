using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Core
{
    public class CommonSendStrategy : ISendStrategy
    {
        private class SocketWithEvent
        {
            public Socket Socket { get; set; }

            public ManualResetEvent SendDone { get; set; }
        }

        private readonly List<Socket> _list = new List<Socket>();

        public void OnNewSocket(ClientModel client)
        {
            _list.Add(client.Socket);
        }

        public async Task SendBroadcast(TransferMessage msg)
        {
            var sendDoneEvents = new List<ManualResetEvent>();

            foreach (var socket in _list)
            {
                ManualResetEvent sendDone = new ManualResetEvent(false);

                SocketWithEvent swe = new SocketWithEvent()
                {
                    Socket = socket,
                    SendDone = sendDone
                };

                sendDoneEvents.Add(sendDone);

                var json =
                    JsonSerializer.Serialize(msg) + ConfigurationConstants.MessageSeparator;

                byte[] byteData = Encoding.ASCII.GetBytes(json);

                socket.BeginSend(byteData, 0, byteData.Length, 0,
                    new AsyncCallback(SendCallback), swe);
            }

            await Task.Run(() =>
            {
                foreach (var manualResetEvent in sendDoneEvents)
                {
                    manualResetEvent.WaitOne();
                }

                sendDoneEvents.Clear();
            });
        }


        private void SendCallback(IAsyncResult ar)
        {
            try
            {
                // Retrieve the client from the state object.  
                var swe = (SocketWithEvent)ar.AsyncState;

                // Complete sending the data to the remote device.  
                int bytesSent = swe.Socket.EndSend(ar);

                swe.SendDone.Set();
            }
            catch (Exception e)
            {
                throw;
            }
        }
    }


    public class SendWithConfirmStrategy : ISendStrategy
    {
        private class SocketWithEvent
        {
            public Socket Socket { get; set; }

            public ManualResetEvent SendDone { get; set; }
        }

        private class SocketConfig
        {
            public Socket Socket { get; set; }

            public Guid Id { get; set; }
        }

        private class EventWithId
        {
            public ManualResetEvent SendDone { get; set; }

            public Guid Id { get; set; }
        }


        private string _message;
        private object _locker = new object();
        private readonly List<SocketConfig> _listClients = new List<SocketConfig>();
        List<EventWithId> _sendDoneEvents = new List<EventWithId>();
        private Guid _guid;

        public SendWithConfirmStrategy()
        {
            listenStrategy.OnNewMessage += ListenStrategyOnOnNewMessage;
        }

        private void ListenStrategyOnOnNewMessage(MessageEventModel msg)
        {
            lock (_locker)
            {
                var msgStr = _message + msg.Message;

                if (msgStr.IndexOf(ConfigurationConstants.MessageSeparator) > -1)
                {
                    var json = msg.Message.Replace(ConfigurationConstants.MessageSeparator, "");
                    var tm = JsonSerializer.Deserialize<TransferMessage>(json);

                    _sendDoneEvents
                        .First(w => w.Id == tm.Id)
                        .SendDone
                        .Set();

                    _message = "";
                }
            }
        }

        public void OnNewSocket(ClientModel client)
        {
            _listClients.Add(new SocketConfig()
            {
                Id = client.Id,
                Socket = client.Socket
            });
        }

        public async Task SendBroadcast(TransferMessage msg)
        {
            _sendDoneEvents = new List<EventWithId>();

            _guid = msg.Id;

            foreach (var socket in _listClients)
            {
                ManualResetEvent sendDone = new ManualResetEvent(false);

                SocketWithEvent swe = new SocketWithEvent()
                {
                    Socket = socket.Socket,
                    SendDone = sendDone
                };

                _sendDoneEvents.Add(new EventWithId()
                {
                    SendDone = sendDone,
                    Id = socket.Id
                });

                var json =
                    JsonSerializer.Serialize(msg) + ConfigurationConstants.MessageSeparator;

                byte[] byteData = Encoding.ASCII.GetBytes(json);

                socket.Socket.BeginSend(byteData, 0, byteData.Length, 0,
                    new AsyncCallback(SendCallback), swe);
            }

            await Task.Run(() =>
            {
                foreach (var manualResetEvent in _sendDoneEvents)
                {
                    manualResetEvent.SendDone.WaitOne();
                }

                _sendDoneEvents.Clear();
            });
        }


        private void SendCallback(IAsyncResult ar)
        {
            try
            {
                // Retrieve the client from the state object.  
                var swe = (SocketWithEvent)ar.AsyncState;

                // Complete sending the data to the remote device.  
                int bytesSent = swe.Socket.EndSend(ar);
            }
            catch (Exception e)
            {
                throw;
            }
        }

    }
}