using UnityEngine.EventSystems;
public class ScriptPointerExitHandler : IScriptEventHandler<ScriptPointerExitHandler>, IPointerExitHandler {
	public void OnPointerExit(PointerEventData eventData) {
		onEvent?.Invoke(gameObject, eventData);
    }
}
