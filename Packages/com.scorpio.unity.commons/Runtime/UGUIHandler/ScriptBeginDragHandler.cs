using UnityEngine.EventSystems;
public class ScriptBeginDragHandler : IScriptEventHandler<ScriptBeginDragHandler>, IBeginDragHandler {
	public void OnBeginDrag(PointerEventData eventData) {
		onEvent?.Invoke(gameObject, eventData);
    }
}
