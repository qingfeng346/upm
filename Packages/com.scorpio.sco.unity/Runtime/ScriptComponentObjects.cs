using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
namespace Scorpio.Unity {
    public sealed class ScriptComponentObjects : MonoBehaviour {
        [Serializable]
        public class Value {
            public string name;
            public UnityEngine.Object value;
        }
        [Serializable]
        public class Click {
            public Button button;
            public string click;
        }
        public List<Value> values = new List<Value>();
        public List<Click> clicks = new List<Click>();
    }
}