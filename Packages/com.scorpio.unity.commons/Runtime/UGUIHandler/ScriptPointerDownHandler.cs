using UnityEngine;
using UnityEngine.EventSystems;
public class ScriptPointerDownHandler : MonoBehaviour, IPointerDownHandler {
	static public void Register(UnityEngine.Object obj, ScriptEventHandlerDelegate onEvent) {
		EngineUtil.GetComponent<ScriptPointerDownHandler> (obj).onEvent = onEvent;
	}
    public ScriptEventHandlerDelegate onEvent;
	public void OnPointerDown(PointerEventData eventData) {
		if (onEvent != null) onEvent(gameObject, eventData);
    }
}
