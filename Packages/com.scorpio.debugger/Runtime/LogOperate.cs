using System;
namespace Scorpio.Debugger {
    /// <summary>
    /// 日志操作
    /// </summary>
    public class LogOperate {
        public string label;
        public Action<LogEntry> action;
    }
}
