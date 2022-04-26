using UnityEditor;
using System.Collections.Generic;

namespace Scorpio.Unity.Editor {
    public class ScriptInstanceDrawer {
        private Dictionary<string, bool> folder = new Dictionary<string, bool>();
        private ScriptInstance instance = null;
        public void SetObject(ScriptInstance instance) {
            this.instance = instance;
        }
        public void Draw() {
            if (instance == null) { return; }
            DrawInstance(0, instance);
        }
        string GetPostfix(ScriptObject scriptValue) {
            if (scriptValue is ScriptMap) {
                return (scriptValue as ScriptMap).Count().ToString();
            } else if (scriptValue is ScriptArray) {
                return (scriptValue as ScriptArray).Length().ToString();
            }
            return "";
        }
        void DrawInstance(int layer, ScriptInstance scriptValue) {
            if (scriptValue is ScriptMapObject) {
                foreach (var pair in (ScriptMapObject)scriptValue) {
                    Label(layer, pair.Key.ToString(), pair.Value);
                }
            } else if (scriptValue is ScriptMapString) {
                foreach (var pair in (ScriptMapString)scriptValue) {
                    Label(layer, pair.Key.ToString(), pair.Value);
                }
            } else if (scriptValue is ScriptArray) {
                var array = scriptValue as ScriptArray;
                for (var i = 0; i < array.Length(); ++i) {
                    Label(layer, i.ToString(), array[i]);
                }
            } else {
                foreach (var pair in (ScriptInstance)scriptValue) {
                    Label(layer, pair.Key, pair.Value);
                }
            }
        }
        void Label(int layer, string name, ScriptValue value) {
            if (string.IsNullOrEmpty(name)) { return; }
            var prefix = "";
            for (var i = 0; i < layer; ++i) { prefix += "    "; }
            var path = prefix + name;
            if (value.valueType != ScriptValue.scriptValueType) {
                EditorGUILayout.TextField(prefix + name, value.ToString());
            } else {
                var scriptValue = value.scriptValue;
                if (scriptValue is ScriptInstance && !(scriptValue is ScriptFunction)) {
                    if (!folder.ContainsKey(path)) { folder[path] = false; }
                    var label = folder[path] ? $"{prefix}{name}({GetPostfix(scriptValue)})" : $"{prefix}{name}";
                    if ((folder[path] = EditorGUILayout.Foldout(folder[path], label)) == true) {
                        DrawInstance(layer + 1, scriptValue as ScriptInstance);
                    }
                } else {
                    var obj = value.Value as UnityEngine.Object;
                    if (obj != null) {
                        EditorGUILayout.ObjectField(prefix + name, obj, obj.GetType(), false);
                    }
                }
            }
        }
    }
}