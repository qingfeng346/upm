using UnityEngine;
using UnityEngine.UI;

namespace Scorpio.Debugger {
    public class ConsoleLogItem : MonoBehaviour, IVirtualView {
        public RectTransform rectTransform { get; private set; }
        public Sprite spriteInfo, spriteWarn, spriteError;
        public Image imageLogType;
        public Text textLogStr;
        public ConsoleWindow consoleWindow;
        private LogEntry entry;
        void Awake() {
            rectTransform = transform as RectTransform;
            ScorpioDebugUtil.RegisterClick(gameObject, this.OnClick);
        }
        public void SetDataContext(object data) {
            var entry = data as LogEntry;
            SetLogEntry(entry);
        }
        public void SetParent(Transform parent) {
            rectTransform.SetParent(parent);
            rectTransform.transform.localScale = Vector3.one;
        }
        public void SetLogEntry(LogEntry entry) {
            this.entry = entry;
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
            //ScorpioDebugger.Instance.ShowLogInfo(entry);
        }
    }
}
