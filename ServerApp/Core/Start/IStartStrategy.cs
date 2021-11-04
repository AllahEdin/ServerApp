using System;

namespace Core
{
    public interface IStartStrategy
    {
        public event Action<ClientModel> OnNewSocketEvent;

        public void Start(Guid id);
    }
}