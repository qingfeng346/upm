using UnityEngine.UI;
using System;

namespace Scorpio.Debugger {
    public class OptionSlider : OptionItemBase {
        public Text label;
        public Slider slider;
        public string format;
        internal override void SetEntry(object value) {
            var v = value as OptionValueSlider;
            format = string.IsNullOrEmpty(v.format) ? "{0:f2}" : v.format;
            OnValueChanged(v.value);
        }
        public void OnValueChanged(float value) {
            this.label.text = string.Format(format, value);
        }
    }
}