using UnityEngine;
using UnityEngine.EventSystems;
public class ScriptPointerEnterHandler : MonoBehaviour, IPointerEnterHandler {
	static public void Register(UnityEngine.Object obj, ScriptEventHandlerDelegate onEvent) {
		EngineUtil.GetComponent<ScriptPointerEnterHandler> (obj).onEvent = onEvent;
	}
    public ScriptEventHandlerDelegate onEvent;
	public void OnPointerEnter(PointerEventData eventData) {
		if (onEvent != null) onEvent(gameObject, eventData);
    }
}
