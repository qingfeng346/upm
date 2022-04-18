using UnityEngine;
using UnityEngine.UI;

namespace Scorpio.Console {
    public class DebugLogItem : MonoBehaviour, IVirtualView {
        public RectTransform rectTransform { get; private set; }
        public Sprite spriteInfo, spriteWarn, spriteError;
        public Image imageLogType;
        public Text textLogStr;
        private DebugLogEntry entry;
        void Awake() {
            rectTransform = transform as RectTransform;
            ScorpioDebugUtil.RegisterClick(gameObject, this.OnClick);
        }
        public void SetDataContext(object data) {
            var entry = data as DebugLogEntry;
            SetLogEntry(entry);
        }
        public void SetParent(Transform parent) {
            rectTransform.SetParent(parent);
            rectTransform.transform.localScale = Vector3.one;
        }
        public void SetLogEntry(DebugLogEntry entry) {
            this.entry = entry;
            this.textLogStr.text = entry.LogString;
            switch (entry.logType) {
                case DebugLogType.Error:
                    imageLogType.sprite = spriteError;
                    break;
                case DebugLogType.Warn:
                    imageLogType.sprite = spriteWarn;
                    break;
                case DebugLogType.Info:
                    imageLogType.sprite = spriteInfo;
                    break;
            }
        }
        void OnClick() {
            ScorpioConsole.Instance.ShowLogInfo(entry);
        }
    }
}
