using UnityEngine.EventSystems;
public class ScriptScrollHandler : IScriptEventHandler<ScriptScrollHandler>, IScrollHandler {
	public void OnScroll(PointerEventData eventData) {
		onEvent?.Invoke(gameObject, eventData);
    }
}
