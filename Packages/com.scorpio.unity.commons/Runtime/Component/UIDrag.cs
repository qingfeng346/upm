using System;
using UnityEngine.EventSystems;

namespace UnityEngine.UI
{
    /// <summary>
    /// 限制拖拽方向的控件
    /// </summary>
    [AddComponentMenu("UGUI/UIDrag")]
    [DisallowMultipleComponent]
    public sealed class UIDrag : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IDropHandler
    {
        public static UIDrag Add(GameObject go, Transform dragParent, Camera camera, Action onBeginDrag, Action<Vector2> onDrop, Action onEndDrag)
        {
            var dragable = EngineUtil.GetComponent<UIDrag>(go, true);
            dragable.m_UICamera = camera;
            dragable.m_DragParent = dragParent as RectTransform;
            dragable.m_OnBeginDrag = onBeginDrag;
            dragable.m_OnDrop = onDrop;
            dragable.m_OnEndDrag = onEndDrag;
            return dragable;
        }

        /// <summary>
        /// 允许的拖拽方向
        /// </summary>
        public enum DragDirection
        {
            UpDown,
            LeftRight,
            All,
        }

        /// <summary>
        /// 是否允许拖拽
        /// </summary>
        public bool Enable
        {
            get;
            set;
        } = true;

        [SerializeField]
        private DragDirection m_Direction = DragDirection.All;
        public ScrollRect ScrollRect
        {
            get;
            set;
        } = null;

        private RectTransform m_DragParent = null;
        private Camera m_UICamera = null;
        private Action m_OnBeginDrag = null;
        private Action<Vector2> m_OnDrop = null;
        private Action m_OnEndDrag = null;

        private Transform m_OrginalParent = null;
        private bool m_IsDragging = false;
        private bool m_IsScrolling = false;

        private void Awake()
        {
            m_OrginalParent = transform.parent;
        }

        private void OnEnable()
        {
            m_IsDragging = false;
            m_IsScrolling = false;
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (eventData.button == PointerEventData.InputButton.Left)
            {
                if (DispatchEvent(eventData))
                {
                    m_IsScrolling = true;
                    ScrollRect.OnBeginDrag(eventData);
                }
                else
                {
                    m_IsDragging = true;
                    transform.SetParent(m_DragParent);
                    UpdateUI(eventData);
                    m_OnBeginDrag?.Invoke();
                }
            }
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (eventData.button == PointerEventData.InputButton.Left)
            {
                if (m_IsDragging)
                {
                    UpdateUI(eventData);
                }
                else if (m_IsScrolling)
                {
                    ScrollRect.OnDrag(eventData);
                }
            }
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (m_IsDragging)
            {
                m_IsDragging = false;
                UpdateUI(eventData);
                transform.SetParent(m_OrginalParent);
                m_OnEndDrag?.Invoke();
            }
            else if (m_IsScrolling)
            {
                m_IsScrolling = false;
                ScrollRect.OnEndDrag(eventData);
            }
        }

        public void OnDrop(PointerEventData eventData)
        {
            if (m_IsDragging)
            {
                m_OnDrop?.Invoke(eventData.position);
            }
        }

        private void UpdateUI(PointerEventData eventData)
        {
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(m_DragParent, eventData.position, m_UICamera, out var localPosition))
            {
                transform.localPosition = localPosition;
            }
        }

        /// <summary>
        /// 是否应该转发本次UI事件，即忽略拖拽
        /// </summary>
        private bool DispatchEvent(PointerEventData eventData)
        {
            if (!Enable)
            {
                return true;
            }
            var delta = eventData.delta;
            var moveDir = DetermineMoveDirection(delta.x, delta.y, 0.6f);
            switch (m_Direction)
            {
                case DragDirection.UpDown:
                    return moveDir == MoveDirection.Left || moveDir == MoveDirection.Right;
                case DragDirection.LeftRight:
                    return moveDir == MoveDirection.Up || moveDir == MoveDirection.Down;
                case DragDirection.All:
                    return false;
            }
            return false;
        }

        /// <summary>
        /// 计算鼠标移动方向
        /// </summary>
        private MoveDirection DetermineMoveDirection(float x, float y, float deadZone)
        {
            MoveDirection moveDir = MoveDirection.None;
            if (new Vector2(x, y).sqrMagnitude > deadZone * deadZone)
            {
                if (Mathf.Abs(x) > Mathf.Abs(y))
                {
                    moveDir = x > 0 ? MoveDirection.Right : MoveDirection.Left;
                }
                else
                {
                    moveDir = y > 0 ? MoveDirection.Up : MoveDirection.Down;
                }
            }
            return moveDir;
        }
    }
}
