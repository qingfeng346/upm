using UnityEngine.EventSystems;
public class ScriptDeselectHandler : IScriptEventHandler<ScriptDeselectHandler>, IDeselectHandler {
	public void OnDeselect(BaseEventData eventData) {
		onEvent?.Invoke(gameObject, eventData);
    }
}
