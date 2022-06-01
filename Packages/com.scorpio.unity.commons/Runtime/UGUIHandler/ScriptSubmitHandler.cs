using UnityEngine;
using UnityEngine.EventSystems;
public class ScriptSubmitHandler : MonoBehaviour, ISubmitHandler {
	static public void Register(UnityEngine.Object obj, ScriptEventHandlerDelegate onEvent) {
		EngineUtil.GetComponent<ScriptSubmitHandler> (obj).onEvent = onEvent;
	}
    public ScriptEventHandlerDelegate onEvent;
	public void OnSubmit(BaseEventData eventData) {
		if (onEvent != null) onEvent(gameObject, eventData);
    }
}
