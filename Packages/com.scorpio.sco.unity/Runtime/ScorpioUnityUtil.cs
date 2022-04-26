using UnityEngine;
using UnityEngine.UI;

namespace Scorpio.Unity {
    public static class ScorpioUnityUtil {
        public static System.Action<GameObject> PreClicked = null;          //所有按钮点击的前置event
        public static System.Action<GameObject> PostClicked = null;         //所有按钮点击的后置event
        public static ScriptInstance AddComponent(this GameObject gameObject, ScriptInstance instance) {
            if (gameObject == null || instance == null) return null;
            return AddComponent(gameObject, instance, "");
        }
        public static ScriptInstance AddComponent(this GameObject gameObject, ScriptInstance instance, string name) {
            if (gameObject == null || instance == null) return null;
            gameObject.AddComponent<ScriptComponent>().Initialize(instance, name);
            return instance;
        }
        public static ScriptInstance GetComponent(this GameObject gameObject) {
            if (gameObject == null) return null;
            var component = gameObject.GetComponent<ScriptComponentBase>();
            if (component == null) return null;
            return component.Table;
        }
        public static ScriptInstance GetComponent(this GameObject gameObject, string name) {
            if (gameObject == null) return null;
            foreach (var component in gameObject.GetComponents<ScriptComponentBase>()) {
                if (component.Name == name)
                    return component.Table;
            }
            return null;
        }
        public static ScriptInstance GetOrAddComponent(this GameObject gameObject, ScriptInstance instance, string name) {
            if (gameObject == null) return null;
            foreach (var component in gameObject.GetComponents<ScriptComponentBase>()) {
                if (component.Name == name)
                    return component.Table;
            }
            return gameObject.AddComponent<ScriptComponent>().Initialize(instance, name);
        }

        public static void DelComponent(this GameObject gameObject) {
            if (gameObject == null) return;
            var component = gameObject.GetComponent<ScriptComponentBase>();
            if (component == null) return;
            Object.Destroy(component);
        }
        public static void DelComponent(this GameObject gameObject, string name) {
            if (gameObject == null) return;
            foreach (var component in gameObject.GetComponents<ScriptComponentBase>()) {
                if (component.Name == name) {
                    Object.Destroy(component);
                }
            }
        }
        public static GameObject FindChild(this GameObject gameObject, string path) {
            if (gameObject == null) return null;
            if (string.IsNullOrEmpty(path)) return null;
            var transform = gameObject.transform.Find(path);
            return transform == null ? null : transform.gameObject;
        }
        public static Component FindChild(this GameObject gameObject, string path, System.Type type) {
            var obj = gameObject.FindChild(path);
            return obj == null ? null : obj.GetComponent(type);
        }
        public static void CallScript(this Component component, string func, params object[] args) {
            if (component == null) { return; }
            component.gameObject.CallScript(func, args);
        }
        public static void CallScript(this GameObject gameObject, string func, params object[] args) {
            if (gameObject == null) { return; }
            foreach (var component in gameObject.GetComponents<ScriptComponent>()) {
                component.Call(func, args);
            }
        }
        public static void RegisterOnClick(this Component component, ScriptFunction func) {
            if (component == null) { return; }
            RegisterOnClick(component.gameObject, func);
        }
        public static void RegisterOnClick(this GameObject gameObject, ScriptFunction func) {
            if (gameObject == null) return;
            if (func == null) return;
            var button = gameObject.GetComponent<Button>();
            if (button == null) {
                button = gameObject.AddComponent<Button>();
            }
            button.onClick = new Button.ButtonClickedEvent();
            button.onClick.AddListener(() => {
                PreClicked?.Invoke(gameObject);
                func.Call(ScriptValue.Null, ScriptValue.EMPTY, 0);
                PostClicked?.Invoke(gameObject);
            });
        }
    }
}