namespace UnityEngine.UI
{
    using Events;
    using EventSystems;

    [AddComponentMenu("UGUI/UIEmptyWithClick")]
    [DisallowMultipleComponent]
    public sealed class UIEmptyWithClick : UIEmpty, IPointerClickHandler
    {
        [SerializeField]
        private UnityEvent m_OnClick = new UnityEvent();

        public UnityEvent OnClick
        {
            get { return m_OnClick; }
            set { m_OnClick = value; }
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (!IsActive())
                return;
            m_OnClick.Invoke();
        }

        public static void Register(GameObject go, UnityAction handler)
        {
            if (go == null || handler == null)
                return;
            var component = go.GetComponent<UIEmptyWithClick>();
            if (component != null)
            {
                component.OnClick.AddListener(handler);
            }
        }

        public static void Unregister(GameObject go, UnityAction handler)
        {
            if (go == null || handler == null)
                return;
            var component = go.GetComponent<UIEmptyWithClick>();
            if (component != null)
            {
                component.OnClick.RemoveListener(handler);
            }
        }
    }
}
