using UnityEngine.EventSystems;
public class ScriptPointerDownHandler : IScriptEventHandler<ScriptPointerDownHandler>, IPointerDownHandler {
	public void OnPointerDown(PointerEventData eventData) {
		onEvent?.Invoke(gameObject, eventData);
    }
}
