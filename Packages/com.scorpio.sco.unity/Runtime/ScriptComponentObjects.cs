using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;
namespace Scorpio.Unity {
    [DisallowMultipleComponent]
    public sealed class ScriptComponentObjects : MonoBehaviour {
        [Serializable]
        public class Value {
            public string name;
            public Object value;
        }
        [Serializable]
        public class Click {
            public GameObject button;
            public string click;
        }
        public List<Value> values = new List<Value>();
        public List<Click> clicks = new List<Click>();
#if UNITY_EDITOR
        public static Func<Transform, string, Tuple<UnityEngine.Object, string>> ParseComponent;
        private static char[] Keyword = new[] { '@', '#' };
        private static Dictionary<string, Type> keyValuePairs = new Dictionary<string, Type>() {
            {"image", typeof(Image) },
            {"text", typeof(Text) },
            {"trans", typeof(Transform) },
            {"transform", typeof(Transform) },
        };
        public void Parse() {
            values.Clear();
            clicks.Clear();
            Parse(transform);
        }
        void Parse(Transform transform) {
            var name = transform.name;
            var index = name.IndexOfAny(Keyword);
            if (index > 0) {
                var objName = name.Substring(0, index);
                name = name.Substring(index);
                while (true) {
                    var key = name[0];
                    index = name.IndexOfAny(Keyword, 1);
                    if (key == '@') {
                        var value = name.Substring(1, (index > 0 ? index : name.Length) - 1);
                        var result = ParseComponent == null ? DefaultParseComponent(transform, value) : ParseComponent(transform, value);
                        if (result?.Item1 != null) {
                            values.Add(new Value() { name = $"{result.Item2}{objName}", value = result.Item1 });
                        }
                    } else if (key == '#') {
                        clicks.Add(new Click() { button = transform.gameObject, click = $"OnClick{objName}" });
                    }
                    if (index > 0) {
                        name = name.Substring(index);
                    } else {
                        break;
                    }
                }
            }
            foreach (Transform trans in transform) {
                if (trans.GetComponent<ScriptComponentObjects>() == null) {
                    Parse(trans);
                }
            }
        }
        static Tuple<Object, string> DefaultParseComponent(Transform transform, string type) {
            if (type.ToLowerInvariant() == "gameobject" || type.ToLowerInvariant() == "object") {
                return new Tuple<Object, string>(transform.gameObject, "object");
            }
            if (keyValuePairs.TryGetValue(type.ToLowerInvariant(), out var pair)) {
                if (transform.TryGetComponent(pair, out var component)) {
                    return new Tuple<Object, string>(component, type);
                }
            }
            return new Tuple<Object, string>(null, null);
        }
#endif
    }
}