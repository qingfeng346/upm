using UnityEngine;
using UnityEngine.EventSystems;
public class ScriptPointerUpHandler : MonoBehaviour, IPointerUpHandler {
	static public void Register(UnityEngine.Object obj, ScriptEventHandlerDelegate onEvent) {
		EngineUtil.GetComponent<ScriptPointerUpHandler> (obj).onEvent = onEvent;
	}
    public ScriptEventHandlerDelegate onEvent;
	public void OnPointerUp(PointerEventData eventData) {
		if (onEvent != null) onEvent(gameObject, eventData);
    }
}
