using System;
using System.Diagnostics;
namespace Scorpio.Unity.Util {
    public class LogRecord : IDisposable {
        private Stopwatch stopwatch;
        private string message;
        public LogRecord(string message) {
            logger.info($"开始 : {message}");
            this.message = message;
            this.stopwatch = Stopwatch.StartNew();
        }
        public void Dispose() {
            logger.info($"结束 : {message}, 耗时:{stopwatch.ElapsedMilliseconds / 1000}s");
        }
    }
}
