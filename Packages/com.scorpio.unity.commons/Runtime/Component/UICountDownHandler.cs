namespace UnityEngine.UI
{
    public abstract class UICountDownHandler : MonoBehaviour
    {
        public abstract void OnProgressChanged(float remainTime, float totalTime);
        public abstract void OnCompleted();
    }
}
