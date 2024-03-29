﻿using System;

namespace Glade2d.Logging
{
    public class ConsoleLogger : ILogger
    {
        public LogLevel Level { get; set; } = LogLevel.Debug;

        public void Trace(string msg)
        {
            Write(LogLevel.Trace, msg);
        }

        public void Debug(string msg)
        {
            Write(LogLevel.Debug, msg);
        }

        public void Info(string msg)
        {
            Write(LogLevel.Info, msg);
        }

        public void Warn(string msg)
        {
            Write(LogLevel.Warn, msg);
        }

        public void Error(string msg)
        {
            Write(LogLevel.Error, msg);
        }

        public void Error(string msg, Exception exception)
        {
            var message = $"{msg}\n\n{exception}";
            Write(LogLevel.Error, message);
        }

        void Write(LogLevel level, string msg)
        {
            if (Level <= level)
            {
                msg = $"{level.ToString().ToUpper()}: {msg}";
                Console.WriteLine(msg);
            }
        }
    }
}
