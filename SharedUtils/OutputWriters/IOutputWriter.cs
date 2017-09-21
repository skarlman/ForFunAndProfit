using System;

namespace SharedUtils.OutputWriters
{
    internal interface IOutputWriter
    {
        void Write(string text, params object[] arg);

        void Write(ConsoleColor foregroundColor, string text, params object[] arg);

        void WriteLine(string text, params object[] arg);

        void WriteLine(ConsoleColor foregroundColor, string text, params object[] arg);

        void WriteLine();
    }
}