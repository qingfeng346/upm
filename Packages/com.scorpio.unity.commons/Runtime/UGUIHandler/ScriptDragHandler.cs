using UnityEngine;
using UnityEngine.EventSystems;
public class ScriptDragHandler : MonoBehaviour, IDragHandler {
	static public void Register(UnityEngine.Object obj, ScriptEventHandlerDelegate onEvent) {
		EngineUtil.GetComponent<ScriptDragHandler> (obj).onEvent = onEvent;
	}
    public ScriptEventHandlerDelegate onEvent;
	public void OnDrag(PointerEventData eventData) {
		if (onEvent != null) onEvent(gameObject, eventData);
    }
}
