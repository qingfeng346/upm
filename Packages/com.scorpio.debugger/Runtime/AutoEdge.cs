using UnityEngine;
using UnityEngine.EventSystems;
using System;

namespace Scorpio.Debugger
{
    public class AutoEdge : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerClickHandler
    {
        private Rect screen;
        private Vector2 size;
        private RectTransform parent;
        private RectTransform rectTransform;
        private bool isDrag = false;
        public Action onClick;
        void Start()
        {
            var screenSize = (GetComponentInParent<Canvas>().transform as RectTransform).sizeDelta;
            screen = new Rect(0, 0, screenSize.x, screenSize.y);
            rectTransform = transform as RectTransform;
            size = rectTransform.sizeDelta;
            parent = rectTransform.parent as RectTransform;
            screen = new Rect(-screenSize.x / 2 + size.x / 2, -screenSize.y / 2 + size.y / 2, screenSize.x - size.x, screenSize.y - size.y);
            CalcEdge();
        }
        public void OnBeginDrag(PointerEventData eventData)
        {
            isDrag = true;
        }
        public void OnDrag(PointerEventData eventData)
        {
            UpdatePosition(eventData.position);
        }
        public void OnEndDrag(PointerEventData eventData)
        {
            isDrag = false;
            CalcEdge();
        }
        public void OnPointerClick(PointerEventData eventData)
        {
            if (!isDrag)
            {
                onClick?.Invoke();
            }
        }
        void UpdatePosition(Vector2 position)
        {
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(parent, position, null, out var localPoint))
            {
                rectTransform.anchoredPosition = localPoint;
            }
        }
        void CalcEdge()
        {
            var position = rectTransform.anchoredPosition;
            position.x = Mathf.Clamp(position.x, screen.xMin, screen.xMax);
            position.y = Mathf.Clamp(position.y, screen.yMin, screen.yMax);
            var left = Mathf.Abs(position.x - screen.xMin);
            var right = Mathf.Abs(position.x - screen.xMax);
            var top = Mathf.Abs(position.y - screen.yMin);
            var bottom = Mathf.Abs(position.y - screen.yMax);
            var index = 0;
            var value = left;
            if (value > right)
            {
                value = right;
                index = 1;
            }
            if (value > top)
            {
                value = top;
                index = 2;
            }
            if (value > bottom)
            {
                value = bottom;
                index = 3;
            }
            switch (index)
            {
                case 0:
                    position.x = screen.xMin;
                    break;
                case 1:
                    position.x = screen.xMax;
                    break;
                case 2:
                    position.y = screen.yMin;
                    break;
                case 3:
                    position.y = screen.yMax;
                    break;
            }
            rectTransform.anchoredPosition = position;
        }
    }
}