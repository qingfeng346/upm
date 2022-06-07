using UnityEngine.UI;

namespace Scorpio.Debugger {
    public class OptionInput : OptionItemBase {
        public InputField input;
        public OptionValueInput optionValue;
        void Awake() {
            input.onEndEdit.AddListener((value) => {
                optionValue.value = value;
                optionValue.action?.Invoke(value);
            });
        }
        internal override void SetEntry(object value) {
            optionValue = value as OptionValueInput;
            input.text = optionValue.value;
        }
    }
}