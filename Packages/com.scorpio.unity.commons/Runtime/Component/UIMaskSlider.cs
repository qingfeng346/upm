namespace UnityEngine.UI
{
    [AddComponentMenu("UGUI/UIMaskSlider")]
    [DisallowMultipleComponent]
    public sealed class UIMaskSlider : MonoBehaviour
    {
        [Range(0, 1)]
        [SerializeField]
        private float m_Value = 0.0f;
        public float Value
        {
            get
            {
                return m_Value;
            }
            set
            {
                if (m_Value != value)
                {
                    m_Value = value;
                    OnValueChanged();
                }
            }
        }

        [SerializeField]
        private RectTransform m_MaskRect = null;
        [SerializeField]
        private float m_MinValue = 0.0f;
        [SerializeField]
        private float m_MaxValue = 100.0f;

        private void OnEnable()
        {
            OnValueChanged();
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (m_MaskRect == null)
                return;
            OnValueChanged();
        }
#endif

        private void OnValueChanged()
        {
            var newSizeDelta = m_MaskRect.sizeDelta;
            newSizeDelta.x = m_MinValue + (m_MaxValue - m_MinValue) * m_Value;
            m_MaskRect.sizeDelta = newSizeDelta;
        }
    }
}
