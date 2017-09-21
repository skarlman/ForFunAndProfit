using System;
using System.Diagnostics;
using System.IO;

namespace SharedUtils.OutputWriters
{
    public class FileLogger : IOutputWriter
    {
        readonly string _workingFolder = System.AppDomain.CurrentDomain.BaseDirectory + "Logs\\";
        private string FileName => $"{_workingFolder}{System.AppDomain.CurrentDomain.FriendlyName}{DateTime.Now.ToString("yyyy-MM-dd")}-{Process.GetCurrentProcess().Id}.log";

        private object FileLock = new object();

        public FileLogger()
        {
            if (!Directory.Exists(_workingFolder))
            {
                Directory.CreateDirectory(_workingFolder);
            }
        }

        public void Write(string text, params object[] arg)
        {
            lock (FileLock)
            {
                File.AppendAllText(FileName, string.Format(text, arg));
            }
        }

        public void Write(ConsoleColor foregroundColor, string text, params object[] arg)
        {
            lock (FileLock)
            {
                File.AppendAllText(FileName, string.Format(text, arg));
            }
        }

        public void WriteLine(string text, params object[] arg)
        {
            lock (FileLock)
            {
                var message = arg.Length == 0 ? text : string.Format(text, arg);
                File.AppendAllLines(FileName, new[] {message});
            }
        }

        public void WriteLine(ConsoleColor foregroundColor, string text, params object[] arg)
        {
            lock (FileLock)
            {
                File.AppendAllLines(FileName, new[] {string.Format(text, arg)});
            }
        }

        public void WriteLine()
        {
            lock (FileLock)
            {
                File.AppendAllLines(FileName, new[] {""});
            }
        }
    }
}