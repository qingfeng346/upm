using System.Collections.Generic;
using UnityEngine.EventSystems;

namespace UnityEngine.UI
{
    [AddComponentMenu("UGUI/UITouchPenetrate")]
    [DisallowMultipleComponent]
    public sealed class UITouchPenetrate : MonoBehaviour, IPointerClickHandler
    {
        private static readonly List<RaycastResult> s_HitResults = new List<RaycastResult>();

        [SerializeField]
        private bool m_PenetrateOneLayer = true;

        public void OnPointerClick(PointerEventData eventData)
        {
            PassEvent(eventData, ExecuteEvents.pointerClickHandler);
        }

        public void PassEvent<T>(PointerEventData data, ExecuteEvents.EventFunction<T> function) where T : IEventSystemHandler
        {
            s_HitResults.Clear();
            EventSystem.current.RaycastAll(data, s_HitResults);
            GameObject current = data.pointerCurrentRaycast.gameObject;
            for (int i = 0; i < s_HitResults.Count; i++)
            {
                if (current != s_HitResults[i].gameObject)
                {
                    var go = s_HitResults[i].gameObject;
                    ExecuteEvents.Execute(s_HitResults[i].gameObject, data, function);
                    if (m_PenetrateOneLayer)
                    {
                        break;
                    }
                }
            }
            s_HitResults.Clear();
        }
    }
}
