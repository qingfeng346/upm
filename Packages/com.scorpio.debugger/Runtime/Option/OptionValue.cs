using System;
using System.Collections.Generic;
using UnityEngine;
namespace Scorpio.Debugger {
    public class OptionValueBase {
        public int width;
        public int height;
    }
    public class OptionValueLabel : OptionValueBase {
        public string label;
    }
    public class OptionValueImage : OptionValueBase {
        public Sprite sprite;
    }
    public class OptionValueRawImage : OptionValueBase {
        public Texture texture;
        public Rect uvRect;
    }
    public class OptionValueButton : OptionValueBase {
        public string label;
        public Action action;
    }
    public class OptionValueDropdown : OptionValueBase {
        public IEnumerable<string> options;
        public int value;
        public Action<int> action;
    }
    public class OptionValueToggle : OptionValueBase {
        public string label;
        public bool isOn;
        public Action<bool> action;
    }
    public class OptionValueInput : OptionValueBase {
        public string value;
        public Action<string> action;
    }
    public class OptionValueSlider : OptionValueBase {
        public string format;
        public float min;
        public float max;
        public float value;
        public Action<float> action;
    }
}