using UnityEngine;
using UnityEngine.UI;

namespace Scorpio.Debugger {
    public class ConsoleCommandItem : MonoBehaviour {
        public Text labelEN;
        public Text lableCN;
        public Text labelParam;
        public ConsoleWindow consoleWindow;
        private CommandEntry commandEntry;
        void Awake() {
            ScorpioDebugUtil.RegisterClick(this, OnClick);
        }
        public void SetCommandEntry(CommandEntry commandEntry) {
            this.commandEntry = commandEntry;
            labelEN.text = commandEntry.labelEN;
            lableCN.text = commandEntry.labelCN;
            labelParam.text = commandEntry.labelParam;
        }
        void OnClick() {
            consoleWindow.SelectCommand(commandEntry);
        }
    }
}