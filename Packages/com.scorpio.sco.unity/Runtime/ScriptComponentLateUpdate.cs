namespace Scorpio.Unity {
    public class ScriptComponentLateUpdate : ScriptComponentBase {
        public const string StringLateUpdate = "LateUpdate";
        void LateUpdate() {
            Call(StringLateUpdate);
        }
    }
}
