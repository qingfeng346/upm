using UnityEngine.EventSystems;
public class ScriptSubmitHandler : IScriptEventHandler<ScriptSubmitHandler>, ISubmitHandler {
	public void OnSubmit(BaseEventData eventData) {
		onEvent?.Invoke(gameObject, eventData);
    }
}
