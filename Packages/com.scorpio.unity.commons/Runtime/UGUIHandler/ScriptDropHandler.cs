using UnityEngine;
using UnityEngine.EventSystems;
public class ScriptDropHandler : MonoBehaviour, IDropHandler {
	static public void Register(UnityEngine.Object obj, ScriptEventHandlerDelegate onEvent) {
		EngineUtil.GetComponent<ScriptDropHandler> (obj).onEvent = onEvent;
	}
    public ScriptEventHandlerDelegate onEvent;
	public void OnDrop(PointerEventData eventData) {
		if (onEvent != null) onEvent(gameObject, eventData);
    }
}
