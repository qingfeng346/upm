using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
public class UIClickPenetrate : MonoBehaviour, IPointerClickHandler {
    public bool penetrate = true;
    static public void Register(UnityEngine.Object obj, ScriptEventHandlerDelegate onEvent) {
		EngineUtil.GetComponent<UIClickPenetrate> (obj).onEvent = onEvent;
	}
	public ScriptEventHandlerDelegate onEvent;
    public void OnPointerClick(PointerEventData eventData) {
        if (onEvent != null) onEvent(gameObject, eventData);
        if (penetrate) {
            List<RaycastResult> results = new List<RaycastResult>();
            EventSystem.current.RaycastAll(eventData, results);
            for (int i = 0; i < results.Count; i++) {
                if (eventData.pointerCurrentRaycast.gameObject != results[i].gameObject) {
                    ExecuteEvents.Execute(results[i].gameObject, eventData, ExecuteEvents.pointerClickHandler);
                    return;
                }
            }
        }
    }
}
