namespace UnityEngine.UI
{
    [AddComponentMenu("UICountDown/Slider")]
    [RequireComponent(typeof(Slider))]
    public class UICountDownHandlerSlider : UICountDownHandler
    {
        [SerializeField]
        private bool m_From0To1 = true;
        private Slider m_Slider = null;

        private void Awake()
        {
            m_Slider = GetComponent<Slider>();
        }

        public override void OnProgressChanged(float remainTime, float totalTime)
        {
            var progress = remainTime / totalTime;
            if (m_From0To1)
            {
                m_Slider.value = 1 - remainTime / totalTime;
            }
            else
            {
                m_Slider.value = remainTime / totalTime;
            }
        }

        public override void OnCompleted()
        {
            m_Slider.value = 1.0f;
        }
    }
}
