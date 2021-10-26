using System;

namespace Core
{
    public abstract class ClientBase
    {
        public abstract ConsoleColor TextColor { get; }

        public void WriteLine(string msg)
        {
            //Console.ForegroundColor = TextColor;
            //Console.WriteLine(msg);
        }
    }
}
