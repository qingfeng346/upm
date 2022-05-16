using System;
namespace Scorpio.Debugger {
    /// <summary>
    /// 日志操作
    /// </summary>
    public class LogOperation {
        public string label;
        public Action<LogEntry> action;
    }
}
