using UnityEngine;
using UnityEngine.UI;

namespace Scorpio.Debugger {
    public class ConsoleLogItem : MonoBehaviour, IVirtualView {
        public RectTransform rectTransform { get; private set; }
        public Sprite spriteInfo, spriteWarn, spriteError;
        public Image imageLogType;
        public Text textLogStr;
        public ConsoleWindow consoleWindow;
        private LogEntry logEntry;
        void Awake() {
            rectTransform = transform as RectTransform;
            ScorpioDebugUtil.RegisterClick(gameObject, this.OnClick);
        }
        public void SetDataContext(object data) {
            var entry = data as LogEntry;
            SetLogEntry(entry);
        }
        public void SetLogEntry(LogEntry entry) {
            this.logEntry = entry;
            this.textLogStr.text = entry.LogString;
            switch (entry.logType) {
                case LogType.Error:
                    imageLogType.sprite = spriteError;
                    break;
                case LogType.Warn:
                    imageLogType.sprite = spriteWarn;
                    break;
                case LogType.Info:
                    imageLogType.sprite = spriteInfo;
                    break;
            }
        }
        void OnClick() {
            consoleWindow.ShowLogEntry(logEntry);
        }
    }
}
