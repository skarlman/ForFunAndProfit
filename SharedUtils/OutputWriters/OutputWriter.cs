using System;
using System.Collections.Generic;

namespace SharedUtils.OutputWriters
{
    public static class OutputWriter
    {
        private static readonly List<IOutputWriter> outputWriters = new List<IOutputWriter>();


        static OutputWriter()
        {
            outputWriters.Add(new ConsoleWriter());
            outputWriters.Add(new FileLogger());
        }

        private static string CurrentTimestamp => "[" + DateTimeOffset.Now + "]";

        public static void Write(string text, params object[] arg)
        {
            outputWriters.ForEach(writer => writer.Write(text, arg));
        }

        public static void Write(ConsoleColor foregroundColor, string text, params object[] arg)
        {
            outputWriters.ForEach(writer => writer.Write(foregroundColor, text, arg));
        }

        public static void WriteLine(string text, params object[] arg)
        {
            outputWriters.ForEach(writer => writer.WriteLine(CurrentTimestamp + text, arg));
        }

        public static void WriteLine(ConsoleColor foregroundColor, string text, params object[] arg)
        {
            outputWriters.ForEach(writer => writer.WriteLine(foregroundColor, CurrentTimestamp + text, arg));
        }

        public static void WriteLine()
        {
            outputWriters.ForEach(writer => writer.WriteLine());
        }
    }
}