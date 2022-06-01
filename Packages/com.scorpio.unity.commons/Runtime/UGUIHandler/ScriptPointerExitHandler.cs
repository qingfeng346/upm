using UnityEngine;
using UnityEngine.EventSystems;
public class ScriptPointerExitHandler : MonoBehaviour, IPointerExitHandler {
	static public void Register(UnityEngine.Object obj, ScriptEventHandlerDelegate onEvent) {
		EngineUtil.GetComponent<ScriptPointerExitHandler> (obj).onEvent = onEvent;
	}
    public ScriptEventHandlerDelegate onEvent;
	public void OnPointerExit(PointerEventData eventData) {
		if (onEvent != null) onEvent(gameObject, eventData);
    }
}
