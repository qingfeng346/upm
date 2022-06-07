using UnityEngine.UI;

namespace Scorpio.Debugger {
    public class OptionSlider : OptionItemBase {
        public Text label;
        public Slider slider;
        public string format;
        public OptionValueSlider optionValue;
        void Awake() {
            slider.onValueChanged.AddListener((value) => {
                optionValue.value = value;
                OnValueChanged(value);
                optionValue.action?.Invoke(value);
            });
        }
        internal override void SetEntry(object value) {
            optionValue = value as OptionValueSlider;
            format = string.IsNullOrEmpty(optionValue.format) ? "{0:0.##}" : optionValue.format;
            slider.value = optionValue.value;
            OnValueChanged(optionValue.value);
        }
        float length => optionValue.max - optionValue.min;
        void OnValueChanged(float value) {
            var v = optionValue.min + length * value;
            this.label.text = string.Format(format, v);
        }
    }
}