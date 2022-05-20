using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
public class ScorpioDebugUtil {

    public static GameObject GetGameObject(Object obj) { 
        return obj is Component ? (obj as Component).gameObject : obj as GameObject; 
    }
    public static GameObject FindChild(Object obj, string str) {
        var gameObject = GetGameObject(obj);
        if (gameObject == null) { return null; }
        if (string.IsNullOrEmpty(str)) return gameObject;
        var transform = gameObject.transform.Find(str);
        return transform == null ? null : transform.gameObject;
    }
    public static T FindChild<T>(Object obj, string str) where T : Component {
        var gameObject = FindChild(obj, str);
        return gameObject == null ? null : gameObject.GetComponent<T>();
    }
    public static T GetComponent<T>(Object obj) where T : Component {
        var gameObject = GetGameObject(obj);
        if (gameObject == null) { return null; }
        return gameObject.GetComponent<T>();
    }
    public static void RegisterClick(Object obj, UnityAction action) {
        RegisterClick(obj, null, action);
    }
    public static void RegisterClick(Object obj, string path, UnityAction action) {
        var button = FindChild<Button>(obj, path);
        if (button == null) { return; }
        var buttonEvent = new Button.ButtonClickedEvent();
        buttonEvent.AddListener(action);
        button.onClick = buttonEvent;
    }
    public static void RegisterSubmit(Object obj, UnityAction<string> submit) {
        RegisterSubmit(obj, null, submit);
    }
    public static void RegisterSubmit(Object obj, string path, UnityAction<string> submit) {
        var input = FindChild<InputField>(obj, path);
        if (input == null) { return; }
        input.onEndEdit.RemoveAllListeners();
        input.onEndEdit.AddListener(submit);
    }
}