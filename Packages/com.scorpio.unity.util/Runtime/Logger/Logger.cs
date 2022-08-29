namespace Scorpio.Unity.Util {
    public enum LoggerLevel {
        Debug = 1 << 0,
        Info = 1 << 1,
        Warn = 1 << 2,
        Error = 1 << 3,
        All = Debug | Info | Warn | Error,
    }
    /// <summary> 日志类 </summary>
    public static class logger {
        static logger() {
            ILogger = new UnityLogger();
        }
        public static int loggerLevel = (int)LoggerLevel.All;
        private static ILogger log = null;
        /// <summary> 设置日志对象 </summary>
        public static ILogger ILogger {
            get { return log; }
            set { log = value; }
        }
        /// <summary> debug输出 </summary>
        public static void debug(object format) {
            if (log == null || (loggerLevel & (int)LoggerLevel.Debug) == 0) return;
            log.debug($"{format}");
        }
        /// <summary> debug输出 </summary>
        public static void debug(string format) {
            if (log == null || (loggerLevel & (int)LoggerLevel.Debug) == 0) return;
            log.debug(format);
        }
        /// <summary> debug输出 </summary>
        public static void debug(string format, params object[] args) {
            if (log == null || (loggerLevel & (int)LoggerLevel.Debug) == 0) return;
            log.debug(string.Format(format, args));
        }
        
        /// <summary> info输出 </summary>
        public static void info(object format) {
            if (log == null || (loggerLevel & (int)LoggerLevel.Info) == 0) return;
            log.info($"{format}");
        }
        /// <summary> info输出 </summary>
        public static void info(string format) {
            if (log == null || (loggerLevel & (int)LoggerLevel.Info) == 0) return;
            log.info(format);
        }
        /// <summary> info输出 </summary>
        public static void info(string format, params object[] args) {
            if (log == null || (loggerLevel & (int)LoggerLevel.Info) == 0) return;
            log.info(string.Format(format, args));
        }

        /// <summary> warn输出 </summary>
        public static void warn(object format) {
            if (log == null || (loggerLevel & (int)LoggerLevel.Warn) == 0) return;
            log.warn($"{format}");
        }
        /// <summary> warn输出 </summary>
        public static void warn(string format) {
            if (log == null || (loggerLevel & (int)LoggerLevel.Warn) == 0) return;
            log.warn(format);
        }
        /// <summary> warn输出 </summary>
        public static void warn(string format, params object[] args) {
            if (log == null || (loggerLevel & (int)LoggerLevel.Warn) == 0) return;
            log.warn(string.Format(format, args));
        }
        
        /// <summary> error输出 </summary>
        public static void error(object format) {
            if (log == null || (loggerLevel & (int)LoggerLevel.Error) == 0) return;
            log.error($"{format}");
        }
        /// <summary> error输出 </summary>
        public static void error(string format) {
            if (log == null || (loggerLevel & (int)LoggerLevel.Error) == 0) return;
            log.error(format);
        }
        /// <summary> error输出 </summary>
        public static void error(string format, params object[] args) {
            if (log == null || (loggerLevel & (int)LoggerLevel.Error) == 0) return;
            log.error(string.Format(format, args));
        }
    }
}