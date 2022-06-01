using UnityEngine;
using UnityEngine.EventSystems;
public class ScriptCancelHandler : MonoBehaviour, ICancelHandler {
	static public void Register(UnityEngine.Object obj, ScriptEventHandlerDelegate onEvent) {
		EngineUtil.GetComponent<ScriptCancelHandler> (obj).onEvent = onEvent;
	}
    public ScriptEventHandlerDelegate onEvent;
	public void OnCancel(BaseEventData eventData) {
		if (onEvent != null) onEvent(gameObject, eventData);
    }
}
