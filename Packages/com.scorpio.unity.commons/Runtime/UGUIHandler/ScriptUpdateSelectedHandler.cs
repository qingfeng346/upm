using UnityEngine;
using UnityEngine.EventSystems;
public class ScriptUpdateSelectedHandler : MonoBehaviour, IUpdateSelectedHandler {
	static public void Register(UnityEngine.Object obj, ScriptEventHandlerDelegate onEvent) {
		EngineUtil.GetComponent<ScriptUpdateSelectedHandler> (obj).onEvent = onEvent;
	}
    public ScriptEventHandlerDelegate onEvent;
	public void OnUpdateSelected(BaseEventData eventData) {
		if (onEvent != null) onEvent(gameObject, eventData);
    }
}
