using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;
public class ScriptEventHandler : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler, IPointerClickHandler, IInitializePotentialDragHandler, IBeginDragHandler, IDragHandler, IEndDragHandler, IDropHandler, IScrollHandler, IUpdateSelectedHandler, ISelectHandler, IDeselectHandler, IMoveHandler, ISubmitHandler, ICancelHandler {
    static public ScriptEventHandler Register(Object obj) {
        return EngineUtil.AddComponent<ScriptEventHandler>(obj);
    }

    public float LongPressStartTime = ScriptLongPressHandler.DefaultLongPressStartTime;
    public float LongPressInterval = ScriptLongPressHandler.DefaultLongPressInterval;
    private bool isPress = false;
    public ScriptEventHandlerDelegate onBeginDrag, onCancel, onDeselect, onDrag, onDrop, onEndDrag, onInitializePotentialDrag, onMove, onPointerClick,
            onPointerDown, onPointerEnter, onPointerExit, onPointerUp, onScroll, onSelect, onSubmit, onUpdateSelected, onLongPress;

    public void OnBeginDrag(PointerEventData eventData) { onBeginDrag?.Invoke(gameObject, eventData); }
    public void OnCancel(BaseEventData eventData) { onCancel?.Invoke(gameObject, eventData); }
    public void OnDeselect(BaseEventData eventData) { onDeselect?.Invoke(gameObject, eventData); }
    public void OnDrag(PointerEventData eventData) { onDrag?.Invoke(gameObject, eventData); }
    public void OnDrop(PointerEventData eventData) { onDrop?.Invoke(gameObject, eventData); }
    public void OnEndDrag(PointerEventData eventData) { onEndDrag?.Invoke(gameObject, eventData); }
    public void OnInitializePotentialDrag(PointerEventData eventData) { onInitializePotentialDrag?.Invoke(gameObject, eventData); }
    public void OnMove(AxisEventData eventData) { onMove?.Invoke(gameObject, eventData); }
    public void OnPointerClick(PointerEventData eventData) { onPointerClick?.Invoke(gameObject, eventData); }
    public void OnPointerDown(PointerEventData eventData) {
        isPress = true;
        StartCoroutine(OnLongPress());
        onPointerDown?.Invoke(gameObject, eventData);
    }
    public void OnPointerEnter(PointerEventData eventData) { onPointerEnter?.Invoke(gameObject, eventData); }
    public void OnPointerExit(PointerEventData eventData) { onPointerExit?.Invoke(gameObject, eventData); }
    public void OnPointerUp(PointerEventData eventData) {
        isPress = false;
        onPointerUp?.Invoke(gameObject, eventData);
    }
    IEnumerator OnLongPress() {
        yield return new WaitForSecondsRealtime(LongPressStartTime);
        while (isPress) {
            onLongPress?.Invoke(gameObject, null);
            if (LongPressInterval > 0) {
                yield return new WaitForSecondsRealtime(LongPressInterval);
            } else {
                yield return null;
            }
        }
    }
    public void OnScroll(PointerEventData eventData) { onScroll?.Invoke(gameObject, eventData); }
    public void OnSelect(BaseEventData eventData) { onSelect?.Invoke(gameObject, eventData); }
    public void OnSubmit(BaseEventData eventData) { onSubmit?.Invoke(gameObject, eventData); }
    public void OnUpdateSelected(BaseEventData eventData) { onUpdateSelected?.Invoke(gameObject, eventData); }
}
