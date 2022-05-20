using UnityEngine.UI;
using System;

namespace Scorpio.Debugger {
    public class OptionInput : OptionItemBase {
        public InputField input;
        public Action<string> action;
        void Awake() {
            input.onEndEdit.AddListener((value) => {
                action?.Invoke(value);
            });
        }
        internal override void SetEntry(object value) {
            var v = value as OptionValueInput;
            action = v.action;
            input.text = v.value;
        }
    }
}