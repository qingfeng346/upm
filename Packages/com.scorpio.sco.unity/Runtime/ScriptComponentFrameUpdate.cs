using UnityEngine;
namespace Scorpio.Unity {
    public class ScriptComponentFrameUpdate : ScriptComponentBase {
        public const string StringFrameUpdate = "FrameUpdate";
        public const string StringFrameUpdateValue = "FrameUpdateValue";
        public uint frame = 3;
        void Update() {
            if (Time.frameCount % frame == 0) {
                Call(StringFrameUpdate);
            }
        }
    }
}
