using UnityEngine.EventSystems;
public class ScriptUpdateSelectedHandler : IScriptEventHandler<ScriptUpdateSelectedHandler>, IUpdateSelectedHandler {
	public void OnUpdateSelected(BaseEventData eventData) {
		onEvent?.Invoke(gameObject, eventData);
    }
}
