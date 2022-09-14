using UnityEngine.EventSystems;
public class ScriptCancelHandler : IScriptEventHandler<ScriptCancelHandler>, ICancelHandler {
	public void OnCancel(BaseEventData eventData) {
		onEvent?.Invoke(gameObject, eventData);
    }
}
