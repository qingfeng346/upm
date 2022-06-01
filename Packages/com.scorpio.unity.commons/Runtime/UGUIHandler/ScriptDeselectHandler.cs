using UnityEngine;
using UnityEngine.EventSystems;
public class ScriptDeselectHandler : MonoBehaviour, IDeselectHandler {
	static public void Register(UnityEngine.Object obj, ScriptEventHandlerDelegate onEvent) {
		EngineUtil.GetComponent<ScriptDeselectHandler> (obj).onEvent = onEvent;
	}
    public ScriptEventHandlerDelegate onEvent;
	public void OnDeselect(BaseEventData eventData) {
		if (onEvent != null) onEvent(gameObject, eventData);
    }
}
