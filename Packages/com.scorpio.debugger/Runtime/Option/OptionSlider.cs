using UnityEngine.UI;
using System;

namespace Scorpio.Debugger {
    public class OptionSlider : OptionItemBase {
        public Text label;
        public Slider slider;
        public float min, max;
        public string format;
        public Action<float> action;
        void Awake() {
            slider.onValueChanged.AddListener((value) => {
                OnValueChanged(value);
                action?.Invoke(value);
            });
        }
        internal override void SetEntry(object value) {
            var v = value as OptionValueSlider;
            format = string.IsNullOrEmpty(v.format) ? "{0:#.##}" : v.format;
            min = v.min;
            max = v.max;
            slider.value = v.value;
            action = v.action;
            OnValueChanged(v.value);
        }
        float length => max - min;
        public void OnValueChanged(float value) {
            var v = min + length * value;
            this.label.text = string.Format(format, v);
        }
    }
}