using UnityEngine.UI;
using System;

namespace Scorpio.Debugger {
    public class OptionImage : OptionItemBase {
        public Image image;
        internal override void SetEntry(object value) {
            var v = value as OptionValueImage;
            image.sprite = v.sprite;
        }
    }
}