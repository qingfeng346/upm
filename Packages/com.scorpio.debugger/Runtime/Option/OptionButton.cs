using UnityEngine.UI;

namespace Scorpio.Debugger {
    public class OptionButton : OptionItemBase {
        public Text text;
        public OptionValueButton optionValue;
        internal override void SetEntry(object value) {
            optionValue = value as OptionValueButton;
            text.text = optionValue.label;
        }
        public void OnClick() {
            optionValue.action?.Invoke();
        }
    }
}