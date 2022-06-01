using System.Collections.Generic;
using Scorpio.Unity.Logger;
/// <summary> 日志类 </summary>
public static class logger {
    private static List<ILogger> iloggers = new List<ILogger>();
    static logger() {
        AddLogger(UnityLogger.Instance);
    }
    /// <summary> 设置日志对象 </summary>
    public static void AddLogger(ILogger ilogger) {
        iloggers.Add(ilogger);
    }
    /// <summary> debug输出 </summary>
    public static void debug(object msg) {
        debug($"{msg}");
    }
    /// <summary> debug输出 </summary>
    public static void debug(string msg) {
        iloggers.ForEach(i => i.debug(msg));
    }
    /// <summary> debug输出 </summary>
    public static void debug(string format, params object[] args) {
        var msg = string.Format(format, args);
        iloggers.ForEach(i => i.debug(msg));
    }
    /// <summary> info输出 </summary>
    public static void info(object msg) {
        info($"{msg}");
    }
    /// <summary> info输出 </summary>
    public static void info(string msg) {
        iloggers.ForEach(i => i.info(msg));
    }
    /// <summary> info输出 </summary>
    public static void info(string format, params object[] args) {
        var msg = string.Format(format, args);
        iloggers.ForEach(i => i.info(msg));
    }
    /// <summary> warn输出 </summary>
    public static void warn(object msg) {
        warn($"{msg}");
    }
    /// <summary> warn输出 </summary>
    public static void warn(string msg) {
        iloggers.ForEach(i => i.info(msg));
    }
    /// <summary> 警告输出 </summary>
    public static void warn(string format, params object[] args) {
        var msg = string.Format(format, args);
        iloggers.ForEach(i => i.warn(msg));
    }
    /// <summary> error输出 </summary>
    public static void error(object msg) {
        warn($"{msg}");
    }
    /// <summary> error输出 </summary>
    public static void error(string msg) {
        iloggers.ForEach(i => i.error(msg));
    }
    /// <summary> 错误输出 </summary>
    public static void error(string format, params object[] args) {
        var msg = string.Format(format, args);
        iloggers.ForEach(i => i.error(msg));
    }
}