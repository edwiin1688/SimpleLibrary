using Autofac;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;

namespace SimpleLibrary.Logger
{
    public enum LogLevel
    {
        Debug,
        Info,
        Warning,
        Error
    }

    public class PrintLogger
    {
        private List<ILogger> _Logger = new List<ILogger>() { new ConsoleLogger() };
        public LogLevel MinLevel { get; set; } = LogLevel.Debug;

        public void AddLogger(ILogger log)
        {
            if (log != null)
            {
                _Logger.Add(log);
            }
        }

        protected void Print(string msg, Color color, LogLevel level = LogLevel.Info)
        {
            if (level < MinLevel) return;

            string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
            string formattedMsg = $"[{timestamp}] [{level}] {msg}";

            _Logger.ForEach(x => x.Print(formattedMsg, color));
        }

        protected void Debug(string msg) => Print(msg, Color.Gray, LogLevel.Debug);
        protected void Info(string msg) => Print(msg, Color.White, LogLevel.Info);
        protected void Warning(string msg) => Print(msg, Color.Yellow, LogLevel.Warning);
        protected void Error(string msg) => Print(msg, Color.Red, LogLevel.Error);

        protected ILogger InitLogger(ContainerBuilder builder)
        {
            if (builder != null)
            {
                IContainer container_ = builder.Build();
                ILogger log_ = container_.Resolve<ILogger>();
                AddLogger(log_);
                return log_;
            }
            return null;
        }
    }

    public interface ILogger
    {
        void Print(string msg, Color color);
    }

    public class ConsoleLogger : ILogger
    {
        public bool ShowTimestamp { get; set; } = false;

        public void Print(string msg, Color color)
        {
            string output = ShowTimestamp ? msg : msg.Contains("[") && msg.Contains("]") 
                ? msg.Substring(msg.IndexOf("]") + 2) 
                : msg;
            System.Console.WriteLine(output);
        }
    }

    public class ColorfulLogger : ILogger
    {
        public bool ShowTimestamp { get; set; } = false;

        public void Print(string msg, Color color)
        {
            string output = ShowTimestamp ? msg : msg.Contains("[") && msg.Contains("]") 
                ? msg.Substring(msg.IndexOf("]") + 2) 
                : msg;
            Colorful.Console.WriteLine(output, color);
        }
    }

    public class FileLogger : ILogger
    {
        private readonly string _filePath;
        private readonly object _lock = new object();

        public FileLogger(string filePath)
        {
            _filePath = filePath;
            string directory = Path.GetDirectoryName(filePath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
        }

        public void Print(string msg, Color color)
        {
            lock (_lock)
            {
                File.AppendAllText(_filePath, msg + Environment.NewLine);
            }
        }
    }
}
