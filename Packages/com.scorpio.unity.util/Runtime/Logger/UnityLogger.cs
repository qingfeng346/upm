using UnityEngine;
namespace Scorpio.Unity.Util {
    public class UnityLogger : ILogger {
        public void debug(string format) {
            Debug.Log(format);
        }
        public void info(string format) {
            Debug.Log(format);
        }
        public void warn(string format) {
            Debug.LogWarning(format);
        }
        public void error(string format) {
            Debug.LogError(format);
        }
    }
}
