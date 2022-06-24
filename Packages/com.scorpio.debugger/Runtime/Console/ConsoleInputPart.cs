using UnityEngine;
using UnityEngine.UI;
namespace Scorpio.Debugger {
    public abstract class ConsoleInputPart : MonoBehaviour {
        public InputField inputCommand;
        public ConsoleCommands commands;
        protected virtual void Awake() {
            inputCommand.onValidateInput += OnValidateInput;
        }
        protected char OnValidateInput(string text, int charIndex, char addedChar) {
            if (addedChar == '\n') {
                ExecuteCommand(text);
                return '\0';
            }
            return addedChar;
        }
        void LastHistory() {
            inputCommand.text = ScorpioDebugger.Instance.LastCommand;
            inputCommand.caretPosition = inputCommand.text.Length;
        }
        void NextHistory() {
            inputCommand.text = ScorpioDebugger.Instance.NextCommand;
            inputCommand.caretPosition = inputCommand.text.Length;
        }
        void ExecuteCommand(string text) {
            inputCommand.text = "";
            if (text.Length > 0) {
                ScorpioDebugger.Instance.ExecuteCommand(text);
            }
        }
        internal void SelectCommand(CommandEntry commandEntry) {
            commands.gameObject.SetActive(false);
            inputCommand.text = commandEntry.command;
            inputCommand.Select();
        }
        protected virtual void LateUpdate() {
            if (inputCommand.isFocused) {
                if (Input.GetKeyDown(KeyCode.UpArrow)) {
                    LastHistory();
                } else if (Input.GetKeyDown(KeyCode.DownArrow)) {
                    NextHistory();
                }
            }
        }
        public void OnClickCommands() {
            commands.gameObject.SetActive(!commands.gameObject.activeSelf);
        }
        public void OnClickNext() {
            NextHistory();
        }
        public void OnClickLast() {
            LastHistory();
        }
        public void OnClickEnter() {
            ExecuteCommand(inputCommand.text);
        }
    }
}
