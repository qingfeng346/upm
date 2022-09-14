using UnityEngine.EventSystems;
public class ScriptEndDragHandler : IScriptEventHandler<ScriptEndDragHandler>, IEndDragHandler {
	public void OnEndDrag(PointerEventData eventData) {
		onEvent?.Invoke(gameObject, eventData);
    }
}
