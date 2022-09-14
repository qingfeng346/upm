using UnityEngine.EventSystems;
public class ScriptPointerUpHandler : IScriptEventHandler<ScriptPointerUpHandler>, IPointerUpHandler {
	public void OnPointerUp(PointerEventData eventData) {
		onEvent?.Invoke(gameObject, eventData);
    }
}
