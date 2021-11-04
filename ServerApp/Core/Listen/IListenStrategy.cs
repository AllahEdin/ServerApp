using System;

namespace Core
{
    public class MessageEventModel
    {
        public Guid Id { get; set; }

        public string Message { get; set; }
    }

    public interface IListenStrategy
    {
        public event Action<MessageEventModel> OnNewMessage;

        public void StartListen();
    }
}