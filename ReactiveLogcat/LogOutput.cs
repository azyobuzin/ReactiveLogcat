using System;

namespace Azyobuzi.ReactiveLogcat
{
    public class LogOutput
    {
        public DateTime Time { get; set; }
        public LogPriority Priority { get; set; }
        public string Tag { get; set; }
        public string Message { get; set; }
    }

    public enum LogPriority
    {
        Verbose,
        Debug,
        Info,
        Warning,
        Error,
        Fatal,
        Silent
    }
}
