namespace Scorpio.Debugger {
    public enum LogType {
        Info = 1,
        Warn = 2,
        Error = 4
    }
    public class LogEntry {
        public int index;
        public LogType logType;
        public string logString;
        public string stackTrace;
        public LogEntry(int index, LogType logType, string logString, string stackTrace) {
            this.index = index;
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
