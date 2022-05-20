using UnityEngine.UI;
using System;

namespace Scorpio.Debugger {
    public class OptionToggle : OptionItemBase {
        public Toggle toggle;
        public Text label;
        public Action<bool> onValueChanged;
        void Awake() {
            toggle.onValueChanged.AddListener((value) => {
                onValueChanged?.Invoke(value);
            });
        }
        internal override void SetEntry(object value) {
            var v = value as OptionValueToggle;
            label.text = v.label;
            toggle.isOn = v.isOn;
            onValueChanged = v.action;
        }
    }
}