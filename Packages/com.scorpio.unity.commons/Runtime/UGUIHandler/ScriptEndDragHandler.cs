using UnityEngine;
using UnityEngine.EventSystems;
public class ScriptEndDragHandler : MonoBehaviour, IEndDragHandler {
	static public void Register(UnityEngine.Object obj, ScriptEventHandlerDelegate onEvent) {
		EngineUtil.GetComponent<ScriptEndDragHandler> (obj).onEvent = onEvent;
	}
    public ScriptEventHandlerDelegate onEvent;
	public void OnEndDrag(PointerEventData eventData) {
		if (onEvent != null) onEvent(gameObject, eventData);
    }
}
