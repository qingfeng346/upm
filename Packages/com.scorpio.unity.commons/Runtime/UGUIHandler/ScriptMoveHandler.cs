using UnityEngine;
using UnityEngine.EventSystems;
public class ScriptMoveHandler : MonoBehaviour, IMoveHandler {
	static public void Register(UnityEngine.Object obj, ScriptEventHandlerDelegate onEvent) {
		EngineUtil.GetComponent<ScriptMoveHandler> (obj).onEvent = onEvent;
	}
    public ScriptEventHandlerDelegate onEvent;
	public void OnMove(AxisEventData eventData) {
		if (onEvent != null) onEvent(gameObject, eventData);
    }
}
