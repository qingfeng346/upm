using UnityEngine.UI;
using System;

namespace Scorpio.Debugger {
    public class OptionDropdown : OptionItemBase {
        public Dropdown dropdown;
        public Action<int> onValueChanged;
        void Awake() {
            dropdown.onValueChanged.AddListener((value) => {
                onValueChanged?.Invoke(value);
            });
        }
        internal override void SetEntry(object value) {
            var v = value as OptionValueDropdown;
            onValueChanged = v.action;
            if (v.options != null) {
                dropdown.options.Clear();
                foreach (var option in v.options) {
                    dropdown.options.Add(new Dropdown.OptionData(option));
                }
            }
            dropdown.SetValueWithoutNotify(v.value);
        }
    }
}