using UnityEngine.UI;

namespace Scorpio.Debugger {
    public class OptionDropdown : OptionItemBase {
        public Dropdown dropdown;
        public OptionValueDropdown optionValue;
        void Awake() {
            dropdown.onValueChanged.AddListener((value) => {
                optionValue.value = value;
                optionValue.action?.Invoke(value);
            });
        }
        internal override void SetEntry(object value) {
            optionValue = value as OptionValueDropdown;
            if (optionValue.options != null) {
                dropdown.options.Clear();
                foreach (var option in optionValue.options) {
                    dropdown.options.Add(new Dropdown.OptionData(option));
                }
            }
            dropdown.SetValueWithoutNotify(optionValue.value);
        }
    }
}