using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;
public class ScriptLongPressHandler : MonoBehaviour, IPointerDownHandler, IPointerUpHandler {
    public static float DefaultLongPressStartTime = 0.3f;
    public static float DefaultLongPressInterval = 0.05f;

    private bool isPress = false;
    public float startTime = DefaultLongPressStartTime;
    public float interval = DefaultLongPressInterval;
    public ScriptEventHandlerDelegate onEvent;
    static public void Register(Object obj, ScriptEventHandlerDelegate onEvent) {
        Register(obj, onEvent, DefaultLongPressStartTime, DefaultLongPressInterval);
    }
    static public void Register(Object obj, ScriptEventHandlerDelegate onEvent, float startTime, float interval) {
        var handler = EngineUtil.GetComponent<ScriptLongPressHandler>(obj);
        handler.onEvent = onEvent;
        handler.startTime = startTime;
        handler.interval = interval;
    }

    public void OnPointerDown(PointerEventData eventData) { 
        isPress = true;
        StartCoroutine(OnLongPress());
    }
    public void OnPointerUp(PointerEventData eventData) { 
        isPress = false;
        StopAllCoroutines();
    }
    IEnumerator OnLongPress() {
        yield return new WaitForSecondsRealtime(startTime);
        while (isPress) {
            onEvent?.Invoke(gameObject, null);
            if (interval > 0) {
                yield return new WaitForSecondsRealtime(interval);
            } else {
                yield return null;
            }
        }
    }
}
