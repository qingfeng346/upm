using UnityEngine;
using UnityEngine.UI;
namespace Scorpio.Debugger {
    public class ConsoleLogFilter : MonoBehaviour {
        public ScorpioToggle toggle;
        public Text count;
        public ConsoleWindow consoleWindow;
        private int _count = 0;
        void Awake() {
            toggle.onValueChanged = OnValueChanged;
        }
        void OnValueChanged(bool isOn, ScorpioToggle toggle) {
            consoleWindow.UpdateAllLogs();
        }
        public bool isOn => toggle.isOn;
        public int Count {
            get {
                return _count;
            }
            set {
                _count = value;
                count.text = _count.ToString();
            }
        }
    }
}
