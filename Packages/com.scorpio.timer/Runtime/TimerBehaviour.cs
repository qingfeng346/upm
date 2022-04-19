using UnityEngine;
namespace Scorpio.Timer {
    public class TimerBehaviour : MonoBehaviour {
        private static TimerBehaviour instance = null;
        public static void Initialize() {
            if (instance != null) { return; }
            var gameObject = new GameObject("__Timer");
            DontDestroyOnLoad(gameObject);
            instance = gameObject.AddComponent<TimerBehaviour>();
        }
        void LateUpdate() {
            LooperManager.Instance.OnUpdate();
            TimerManager.Instance.OnUpdate();
        }
    }
}
