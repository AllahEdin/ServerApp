using System;

namespace Core
{
    public class TransferMessage
    {
        public Guid Id { get; set; }

        public string Message { get; set; }

        public int Number { get; set; }
    }
}