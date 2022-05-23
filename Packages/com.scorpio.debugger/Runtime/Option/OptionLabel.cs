using UnityEngine.UI;
using System;

namespace Scorpio.Debugger {
    public class OptionLabel : OptionItemBase {
        public Text label;
        internal override void SetEntry(object value) {
            var v = value as OptionValueLabel;
            label.text = v.label;
        }
    }
}