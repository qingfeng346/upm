using UnityEngine;
using UnityEngine.EventSystems;
public class ScriptPointerClickHandler : MonoBehaviour, IPointerClickHandler {
	static public void Register(UnityEngine.Object obj, ScriptEventHandlerDelegate onEvent) {
		EngineUtil.GetComponent<ScriptPointerClickHandler> (obj).onEvent = onEvent;
	}
	public ScriptEventHandlerDelegate onEvent;
    public void OnPointerClick(PointerEventData eventData) {
		if (onEvent != null) onEvent(gameObject, eventData);
    }
}
