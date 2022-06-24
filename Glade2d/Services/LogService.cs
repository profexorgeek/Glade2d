using Glade2d.Logging;

namespace Glade2d.Services
{
    public static class LogService
    {
        private static ILogger log;

        public static ILogger Log
        {
            get
            {
                if (log == null)
                {
                    log = new ConsoleLogger();
                    log.Level = LogLevel.Trace;
                }
                return log;
            }
            set
            {
                log = value;
            }
        }
    }
}
