using UnityEngine.UI;

namespace Scorpio.Debugger {
    public class OptionToggle : OptionItemBase {
        public Toggle toggle;
        public Text label;
        private OptionValueToggle optionValue;
        void Awake() {
            toggle.onValueChanged.AddListener((value) => {
                optionValue.isOn = value;
                optionValue.action?.Invoke(value);
            });
        }
        internal override void SetEntry(object value) {
            optionValue = value as OptionValueToggle;
            label.text = optionValue.label;
            toggle.isOn = optionValue.isOn;
        }
    }
}