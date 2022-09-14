using UnityEngine.EventSystems;
public class ScriptSelectHandler : IScriptEventHandler<ScriptSelectHandler>, ISelectHandler {
	public void OnSelect(BaseEventData eventData) {
		onEvent?.Invoke(gameObject, eventData);
    }
}
