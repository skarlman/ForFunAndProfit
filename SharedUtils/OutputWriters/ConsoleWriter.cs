using System;

namespace SharedUtils.OutputWriters
{
    public class ConsoleWriter : IOutputWriter
    {
        public void Write(string text, params object[] arg)
        {
            lock (Console.Out)
            {
                Console.Write(text, arg);
            }
        }

        public void Write(ConsoleColor foregroundColor, string text, params object[] arg)
        {
            lock (Console.Out)
            {
                var oldColor = Console.ForegroundColor;
                Console.ForegroundColor = foregroundColor;
                Console.Write(text, arg);
                Console.ForegroundColor = oldColor;
            }
        }

        public void WriteLine(string text, params object[] arg)
        {
            lock (Console.Out)
            {
                if (arg.Length == 0)
                {
                    Console.WriteLine(text);
                }
                else
                {
                    Console.WriteLine(text, arg);
                }
            }
        }

        public void WriteLine(ConsoleColor foregroundColor, string text, params object[] arg)
        {
            lock (Console.Out)
            {
                var oldColor = Console.ForegroundColor;
                Console.ForegroundColor = foregroundColor;
                Console.WriteLine(text, arg);
                Console.ForegroundColor = oldColor;
            }
        }

        public void WriteLine()
        {
            Console.WriteLine();
        }
    }
}