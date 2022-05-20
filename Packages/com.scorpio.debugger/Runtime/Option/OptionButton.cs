using UnityEngine.UI;
using System;

namespace Scorpio.Debugger {
    public class OptionButton : OptionItemBase {
        public Text text;
        public Action action;
        internal override void SetEntry(object value) {
            var v = value as OptionValueButton;
            text.text = v.label;
            action = v.action;
        }
        public void OnClick() {
            action?.Invoke();
        }
    }
}