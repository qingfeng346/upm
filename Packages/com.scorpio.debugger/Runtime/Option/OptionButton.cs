using UnityEngine;
using UnityEngine.UI;
using System;

namespace Scorpio.Debugger {
    public class OptionButton : OptionItemBase {
        public Text text;
        private Action action;
        public override void SetValue(object value)
        {
            var v = value as OptionValueButton;
            text.text = v.label;
            action = v.action;
        }
        public void OnClick() {
            action?.Invoke();
        }
    }
}