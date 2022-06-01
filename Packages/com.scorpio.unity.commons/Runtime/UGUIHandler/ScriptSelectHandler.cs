using UnityEngine;
using UnityEngine.EventSystems;
public class ScriptSelectHandler : MonoBehaviour, ISelectHandler {
	static public void Register(UnityEngine.Object obj, ScriptEventHandlerDelegate onEvent) {
		EngineUtil.GetComponent<ScriptSelectHandler> (obj).onEvent = onEvent;
	}
    public ScriptEventHandlerDelegate onEvent;
	public void OnSelect(BaseEventData eventData) {
		if (onEvent != null) onEvent(gameObject, eventData);
    }
}
