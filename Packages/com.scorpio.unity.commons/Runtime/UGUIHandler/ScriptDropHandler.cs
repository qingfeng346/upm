using UnityEngine.EventSystems;
public class ScriptDropHandler : IScriptEventHandler<ScriptDropHandler>, IDropHandler {
	public void OnDrop(PointerEventData eventData) {
		onEvent?.Invoke(gameObject, eventData);
    }
}
