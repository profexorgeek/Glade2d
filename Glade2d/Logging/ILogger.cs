namespace Glade2d.Logging
{
    public enum LogLevel
    {
        Trace = 0,
        Debug = 1,
        Info = 2,
        Warn = 3,
        Error = 4
    }

    public interface ILogger
    {
        LogLevel Level { get; set; }
        void Trace(string msg);
        void Debug(string msg);
        void Info(string msg);
        void Warn(string msg);
        void Error(string msg);
    }
}
