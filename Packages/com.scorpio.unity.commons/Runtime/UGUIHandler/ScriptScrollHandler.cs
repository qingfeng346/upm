using UnityEngine;
using UnityEngine.EventSystems;
public class ScriptScrollHandler : MonoBehaviour, IScrollHandler {
	static public void Register(UnityEngine.Object obj, ScriptEventHandlerDelegate onEvent) {
		EngineUtil.GetComponent<ScriptScrollHandler> (obj).onEvent = onEvent;
	}
    public ScriptEventHandlerDelegate onEvent;
	public void OnScroll(PointerEventData eventData) {
		if (onEvent != null) onEvent(gameObject, eventData);
    }
}
