using UnityEngine.UI;

namespace Scorpio.Debugger {
    public class OptionRawImage : OptionItemBase {
        public RawImage rawImage;
        internal override void SetEntry(object value) {
            var v = value as OptionValueRawImage;
            rawImage.texture = v.texture;
            rawImage.uvRect = v.uvRect;
        }
    }
}