using UnityEngine;
public delegate void ScriptEventHandlerDelegate(GameObject obj, object eventData);
public abstract class IScriptEventHandler<T> : MonoBehaviour where T : IScriptEventHandler<T> {
    static public void Register(UnityEngine.Object obj, ScriptEventHandlerDelegate onEvent) {
		EngineUtil.GetComponent<T> (obj).onEvent = onEvent;
	}
	public ScriptEventHandlerDelegate onEvent;
}