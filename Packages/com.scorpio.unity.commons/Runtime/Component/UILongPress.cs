using System;
using System.Collections;
using UnityEngine.EventSystems;

namespace UnityEngine.UI
{
    public sealed class UILongPress : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler, IBeginDragHandler
    {
        private static readonly float s_PressTime = 0.5f;
        private static readonly float s_TriggerTime = 0.2f;
        private static readonly WaitForSeconds s_WaitPressSeconds = new WaitForSeconds(s_PressTime);
        private static readonly WaitForSeconds s_WaitTriggerSeconds = new WaitForSeconds(s_TriggerTime);

        public static void Add(GameObject go, Action onTrigger)
        {
            if (go == null)
                return;
            var handler = EngineUtil.GetComponent<UILongPress>(go, true);
            handler.onTrigger = onTrigger;
        }

        public Action onTrigger = null;

        private Coroutine m_Coroutine = null;

        public void OnPointerDown(PointerEventData eventData)
        {
            m_Coroutine = StartCoroutine(CoroutineWait());
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            StopTrigger();
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            StopTrigger();
        }

        private void OnDisable()
        {
            StopTrigger();
        }

        public void StopTrigger()
        {
            if (m_Coroutine != null)
            {
                StopCoroutine(m_Coroutine);
                m_Coroutine = null;
            }
        }

        private IEnumerator CoroutineWait()
        {
            yield return s_WaitPressSeconds;
            do
            {
                onTrigger?.Invoke();
                yield return s_WaitTriggerSeconds;
            } while (true);
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            StopTrigger();
        }
    }
}
