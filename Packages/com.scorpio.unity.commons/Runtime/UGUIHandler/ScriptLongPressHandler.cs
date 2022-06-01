using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;
public class ScriptLongPressHandler : MonoBehaviour, IPointerDownHandler, IPointerUpHandler {
    public static float DefaultLongPressStartTime = 0.3f;
    public static float DefaultLongPressInterval = 0.05f;
    static public ScriptLongPressHandler Register(UnityEngine.Object obj, ScriptEventHandlerDelegate onEvent) {
        var handler = EngineUtil.AddComponent<ScriptLongPressHandler>(obj);
        handler.onEvent = onEvent;
        return handler;
	}

	public float LongPressStartTime = DefaultLongPressStartTime;
    public float LongPressInterval = DefaultLongPressInterval;
    private bool isPress = false;
    public ScriptEventHandlerDelegate onEvent;

	public void OnPointerDown(PointerEventData eventData) { 
        isPress = true;
        StartCoroutine(OnLongPress());
    }
    public void OnPointerUp(PointerEventData eventData) { 
        isPress = false;
        StopAllCoroutines();
    }
    IEnumerator OnLongPress() {
        yield return new WaitForSecondsRealtime(LongPressStartTime);
        while (isPress) {
            if (onEvent != null) onEvent(gameObject, null);
            if (LongPressInterval > 0) {
                yield return new WaitForSecondsRealtime(LongPressInterval);
            } else {
                yield return null;
            }
        }
    }
}
