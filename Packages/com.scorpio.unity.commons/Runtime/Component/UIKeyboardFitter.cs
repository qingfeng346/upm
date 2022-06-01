namespace UnityEngine.UI
{
    /// <summary>
    /// 自动适配移动端弹出键盘高度
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    [DisallowMultipleComponent]
    public sealed class UIKeyboardFitter : MonoBehaviour
    {
        private const int c_UpdateInterval = 10;

        public static void Add(GameObject go)
        {
            var component = go.GetComponent<UIKeyboardFitter>();
            if (component == null)
                go.AddComponent<UIKeyboardFitter>();
        }

        private RectTransform m_RectTranform = null;
        private Vector2 m_OrginalAnchoredPosition = Vector2.zero;

        private void Awake()
        {
            m_RectTranform = transform as RectTransform;
            m_OrginalAnchoredPosition = m_RectTranform.anchoredPosition;
        }

        private void LateUpdate()
        {
#if !UNITY_EDITOR
            if (Time.frameCount % c_UpdateInterval != 0)
            {
                return;
            }

            if (TouchScreenKeyboard.visible)
            {
                float height = EngineUtil.GetKeyboardHeight(true);
                height = Mathf.Max(m_OrginalAnchoredPosition.y, height);
                m_RectTranform.anchoredPosition = new Vector2(m_OrginalAnchoredPosition.x, height);
            }
            else
            {
                m_RectTranform.anchoredPosition = m_OrginalAnchoredPosition;
            }
#endif
        }
    }
}