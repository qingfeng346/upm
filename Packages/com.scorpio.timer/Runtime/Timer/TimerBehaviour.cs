using UnityEngine;
namespace Scorpio.Timer {
    public class TimerBehaviour : MonoBehaviour {
        void LateUpdate() {
            LooperManager.Instance.OnUpdate();
            TimerManager.Instance.OnUpdate();
        }
    }
}
