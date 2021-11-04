using System.Collections.Concurrent;

namespace Core
{
    public static class MessageHub
    {
        private static ConcurrentQueue<string> _queue;

        public static ConcurrentQueue<string> Queue => _queue ??= new ConcurrentQueue<string>();
    }
}