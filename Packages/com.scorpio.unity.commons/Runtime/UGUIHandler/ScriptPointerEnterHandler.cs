using UnityEngine.EventSystems;
public class ScriptPointerEnterHandler : IScriptEventHandler<ScriptPointerEnterHandler>, IPointerEnterHandler {
	public void OnPointerEnter(PointerEventData eventData) {
		onEvent?.Invoke(gameObject, eventData);
    }
}
