namespace Scorpio.Unity {
    public class ScriptComponentOnGUI : ScriptComponentBase {
        public const string StringOnGUI = "OnGUI";
        void OnGUI() {
            Call(StringOnGUI);
        }
    }
}