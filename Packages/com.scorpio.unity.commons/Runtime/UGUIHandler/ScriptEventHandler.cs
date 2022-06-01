using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;
public class ScriptEventHandler : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler, IPointerClickHandler, IInitializePotentialDragHandler, IBeginDragHandler, IDragHandler, IEndDragHandler, IDropHandler, IScrollHandler, IUpdateSelectedHandler, ISelectHandler, IDeselectHandler, IMoveHandler, ISubmitHandler, ICancelHandler {
    static public ScriptEventHandler Register(UnityEngine.Object obj) {
		return EngineUtil.AddComponent<ScriptEventHandler>(obj);
	}

    public float LongPressStartTime = ScriptLongPressHandler.DefaultLongPressStartTime;
    public float LongPressInterval = ScriptLongPressHandler.DefaultLongPressInterval;
    private bool isPress = false;
    public ScriptEventHandlerDelegate onBeginDrag, onCancel, onDeselect, onDrag, onDrop, onEndDrag, onInitializePotentialDrag, onMove, onPointerClick,
            onPointerDown, onPointerEnter, onPointerExit, onPointerUp, onScroll, onSelect, onSubmit, onUpdateSelected, onLongPress;

    public void OnBeginDrag(PointerEventData eventData) { if (onBeginDrag != null) onBeginDrag(gameObject, eventData); }
    public void OnCancel(BaseEventData eventData) { if (onCancel != null) onCancel(gameObject, eventData); }
    public void OnDeselect(BaseEventData eventData) { if (onDeselect != null) onDeselect(gameObject, eventData); }
    public void OnDrag(PointerEventData eventData) { if (onDrag != null) onDrag(gameObject, eventData); }
    public void OnDrop(PointerEventData eventData) { if (onDrop != null) onDrop(gameObject, eventData); }
    public void OnEndDrag(PointerEventData eventData) { if (onEndDrag != null) onEndDrag(gameObject, eventData); }
    public void OnInitializePotentialDrag(PointerEventData eventData) { if (onInitializePotentialDrag != null) onInitializePotentialDrag(gameObject, eventData); }
    public void OnMove(AxisEventData eventData) { if (onMove != null) onMove(gameObject, eventData); }
    public void OnPointerClick(PointerEventData eventData) { if (onPointerClick != null) onPointerClick(gameObject, eventData); }
    public void OnPointerDown(PointerEventData eventData) { 
        isPress = true;
        StartCoroutine(OnLongPress());
        if (onPointerDown != null) onPointerDown(gameObject, eventData); 
    }
    public void OnPointerEnter(PointerEventData eventData) { if (onPointerEnter != null) onPointerEnter(gameObject, eventData); }
    public void OnPointerExit(PointerEventData eventData) { if (onPointerExit != null) onPointerExit(gameObject, eventData); }
    public void OnPointerUp(PointerEventData eventData) { 
        isPress = false;
        if (onPointerUp != null) onPointerUp(gameObject, eventData);
    }
    IEnumerator OnLongPress() {
        yield return new WaitForSecondsRealtime(LongPressStartTime);
        while (isPress) {
            if (onLongPress != null) onLongPress(gameObject, null);
            if (LongPressInterval > 0) {
                yield return new WaitForSecondsRealtime(LongPressInterval);
            } else {
                yield return null;
            }
        }
    }
    public void OnScroll(PointerEventData eventData) { if (onScroll != null) onScroll(gameObject, eventData); }
    public void OnSelect(BaseEventData eventData) { if (onSelect != null) onSelect(gameObject, eventData); }
    public void OnSubmit(BaseEventData eventData) { if (onSubmit != null) onSubmit(gameObject, eventData); }
    public void OnUpdateSelected(BaseEventData eventData) { if (onUpdateSelected != null) onUpdateSelected(gameObject, eventData); }
}
