namespace UnityEngine.UI
{
    [AddComponentMenu("UICountDown/SlicedFilledImage")]
    [RequireComponent(typeof(SlicedFilledImage))]
    public class UICountDownHandlerSlicedFilledImage : UICountDownHandler
    {
        [SerializeField]
        private bool m_From0To1 = true;
        private SlicedFilledImage m_Image = null;

        private void Awake()
        {
            m_Image = GetComponent<SlicedFilledImage>();
        }

        public override void OnProgressChanged(float remainTime, float totalTime)
        {
            var progress = remainTime / totalTime;
            if (m_From0To1)
            {
                m_Image.fillAmount = 1 - remainTime / totalTime;
            }
            else
            {
                m_Image.fillAmount = remainTime / totalTime;
            }
        }

        public override void OnCompleted()
        {
            m_Image.fillAmount = 1.0f;
        }
    }
}
