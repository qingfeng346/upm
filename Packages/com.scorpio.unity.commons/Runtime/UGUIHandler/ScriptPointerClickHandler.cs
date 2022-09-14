using UnityEngine.EventSystems;
public class ScriptPointerClickHandler : IScriptEventHandler<ScriptPointerClickHandler>, IPointerClickHandler {
    public void OnPointerClick(PointerEventData eventData) {
		onEvent?.Invoke(gameObject, eventData);
    }
}
