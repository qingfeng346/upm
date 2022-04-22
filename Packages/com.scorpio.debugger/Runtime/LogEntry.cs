namespace Scorpio.Debugger {
    public enum LogType {
        Info = 1,
        Warn = 2,
        Error = 4
    }
    /// <summary>
    /// 一条日志
    /// </summary>
    public class LogEntry {
        public LogType logType;
        public string logString;
        public string stackTrace;
        public LogEntry(LogType logType, string logString, string stackTrace) {
            this.logType = logType;
            this.logString = logString;
            this.stackTrace = stackTrace;
        }
        public string LogString => logString.Length > ScorpioDebugger.LogStringMaxLength ? logString.Substring(0, ScorpioDebugger.LogStringMaxLength) : logString;
        public string StackTrace => stackTrace.Length > ScorpioDebugger.StackTraceMaxLength ? stackTrace.Substring(0, ScorpioDebugger.StackTraceMaxLength) : stackTrace;
        public string LogInfo => $@"[{logType}] : {LogString}
    {StackTrace}";
    }
}
