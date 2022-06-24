namespace Scorpio.Debugger {
    public class MiniCommandWindow : ConsoleInputPart {
        public void OnClickMaximize() {
            ScorpioDebugger.Instance.Maximize();
        }
    }
}
