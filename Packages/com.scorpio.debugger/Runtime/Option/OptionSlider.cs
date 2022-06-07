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
                OnValueChanged();
                optionValue.action?.Invoke(value);
            });
        }
        internal override void SetEntry(object value) {
            optionValue = value as OptionValueSlider;
            format = string.IsNullOrEmpty(optionValue.format) ? "{0:0.##}" : optionValue.format;
            slider.value = optionValue.value;
            OnValueChanged();
        }
        void OnValueChanged() {
            this.label.text = string.Format(format, optionValue.Value);
        }
    }
}