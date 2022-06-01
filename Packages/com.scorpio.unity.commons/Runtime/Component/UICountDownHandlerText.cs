namespace UnityEngine.UI
{
    [AddComponentMenu("UICountDown/Text")]
    [RequireComponent(typeof(Text))]
    public sealed class UICountDownHandlerText : UICountDownHandler
    {
        private static readonly string s_HourFormatter = "{0:D2}:{1:D2}:{2:D2}";
        private static readonly string s_MinuteFormatter = "{0:D2}:{1:D2}";

        private Text m_Text = null;
        private int m_LastHour, m_LastMinute, m_LastSecond;

        private void Awake()
        {
            m_Text = GetComponent<Text>();
            m_LastHour = m_LastMinute = m_LastSecond = -1;
        }

        public override void OnProgressChanged(float remainTime, float totalTime)
        {
            var second = Mathf.Min(Mathf.CeilToInt(remainTime % 60), 59);
            if (remainTime > 3600)
            {
                var hour = Mathf.FloorToInt(remainTime / 3600);
                var minute = Mathf.FloorToInt(remainTime % 3600 / 60);
                if (m_LastHour != hour || m_LastMinute != minute || m_LastSecond != second)
                {
                    m_LastHour = hour;
                    m_LastMinute = minute;
                    m_LastSecond = second;
                    m_Text.text = string.Format(s_HourFormatter, hour, minute, second);
                }
            }
            else
            {
                var minute = Mathf.FloorToInt(remainTime / 60);
                if (m_LastMinute != minute || m_LastSecond != second)
                {
                    m_LastMinute = minute;
                    m_LastSecond = second;
                    m_Text.text = string.Format(s_MinuteFormatter, minute, second);
                }
            }
        }

        public override void OnCompleted()
        {
            m_Text.text = string.Format(s_MinuteFormatter, 0, 0);
        }
    }
}
