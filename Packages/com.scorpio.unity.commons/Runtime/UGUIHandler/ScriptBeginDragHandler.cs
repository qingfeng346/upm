using UnityEngine;
using UnityEngine.EventSystems;
public class ScriptBeginDragHandler : MonoBehaviour, IBeginDragHandler {
	static public void Register(UnityEngine.Object obj, ScriptEventHandlerDelegate onEvent) {
		EngineUtil.AddComponent<ScriptBeginDragHandler>(obj).onEvent = onEvent;
	}
    public ScriptEventHandlerDelegate onEvent;
	public void OnBeginDrag(PointerEventData eventData) {
		if (onEvent != null) onEvent(gameObject, eventData);
    }
}
