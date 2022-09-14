using UnityEngine.EventSystems;
public class ScriptDragHandler : IScriptEventHandler<ScriptDragHandler>, IDragHandler {
	public void OnDrag(PointerEventData eventData) {
		onEvent?.Invoke(gameObject, eventData);
    }
}
