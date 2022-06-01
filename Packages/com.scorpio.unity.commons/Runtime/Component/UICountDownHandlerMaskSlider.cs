namespace UnityEngine.UI
{
    [AddComponentMenu("UICountDown/MaskSlider")]
    [RequireComponent(typeof(UIMaskSlider))]
    public class UICountDownHandlerMaskSlider : UICountDownHandler
    {
        [SerializeField]
        private bool m_From0To1 = true;
        private UIMaskSlider m_MaskSlider = null;

        private void Awake()
        {
            m_MaskSlider = GetComponent<UIMaskSlider>();
        }

        public override void OnProgressChanged(float remainTime, float totalTime)
        {
            var progress = remainTime / totalTime;
            if (m_From0To1)
            {
                m_MaskSlider.Value = 1 - remainTime / totalTime;
            }
            else
            {
                m_MaskSlider.Value = remainTime / totalTime;
            }
        }

        public override void OnCompleted()
        {
            m_MaskSlider.Value = 1.0f;
        }
    }
}
