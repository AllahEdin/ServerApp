using System.Collections.Generic;

namespace ServerApp
{
    public static class MessageHub
    {
        private static Queue<string> _queue;

        public static Queue<string> Queue => _queue ??= new Queue<string>();
    }
}