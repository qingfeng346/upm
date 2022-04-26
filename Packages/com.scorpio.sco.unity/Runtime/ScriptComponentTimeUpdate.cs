using UnityEngine;
namespace Scorpio.Unity {
    public class ScriptComponentTimeUpdate : ScriptComponentBase {
        public const string StringTimeUpdate = "TimeUpdate";
        public const string StringTimeUpdateValue = "TimeUpdateValue";
        public float time = 1;
        private float lastTime = 0;
        void Update() {
            if (Time.time - lastTime > time) {
                lastTime = Time.time;
                Call(StringTimeUpdate);
            }
        }
    }
}
