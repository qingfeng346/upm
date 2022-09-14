using UnityEngine.EventSystems;
public class ScriptMoveHandler : IScriptEventHandler<ScriptMoveHandler>, IMoveHandler {
	public void OnMove(AxisEventData eventData) {
		onEvent?.Invoke(gameObject, eventData);
    }
}
