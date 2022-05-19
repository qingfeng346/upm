using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
// using SuperScrollView;
using System;

namespace Scorpio.Debugger {
    public class ConsoleWindow : MonoBehaviour {
        public InputField inputCommand;
        public InputField inputSearch;
        public VirtualVerticalLayoutGroup listView;
        public ConsoleLogInfo logInfo;
        public ConsoleCommands commands;
        private Queue<LogEntry> addEntries;
        void Awake() {
            inputCommand.onValidateInput += OnValidateInput;
            ScorpioDebugger.Instance.logMessageReceived += LogMessageReceived;
            addEntries = new Queue<LogEntry>(ScorpioDebugger.Instance.LogEntries);
        }
        char OnValidateInput(string text, int charIndex, char addedChar) {
            if (addedChar == '\n') {
                ExecuteCommand(text);
                return '\0';
            }
            return addedChar;
        }
        void LogMessageReceived(LogEntry logEntry) {
            addEntries.Enqueue(logEntry);
        }
        //执行命令行
        void ExecuteCommand(string text) {
            inputCommand.text = "";
            if (text.Length > 0) {
                ScorpioDebugger.Instance.ExecuteCommand(text);
            }
        }
        internal void SelectCommand(CommandEntry commandEntry) {
            commands.gameObject.SetActive(false);
            inputCommand.text = commandEntry.command;
        }
        internal void ShowLogEntry(LogEntry logEntry) {
            logInfo.gameObject.SetActive(true);
            logInfo.SetLogEntry(logEntry);
        }
        void LastHistory() {
            inputCommand.text = ScorpioDebugger.Instance.LastCommand;
            inputCommand.caretPosition = inputCommand.text.Length;
        }
        void NextHistory() {
            inputCommand.text = ScorpioDebugger.Instance.NextCommand;
            inputCommand.caretPosition = inputCommand.text.Length;
        }
        void LateUpdate() {
            if (inputCommand.isFocused) {
                if (Input.GetKeyDown(KeyCode.UpArrow)) {
                    LastHistory();
                } else if (Input.GetKeyDown(KeyCode.DownArrow)) {
                    NextHistory();
                }
            }
            if (addEntries.Count > 0) {
                var entry = addEntries.Dequeue();
                //if (entry.logType == DebugLogType.Info) {
                //    filterInfo.AddCount();
                //} else if (entry.logType == DebugLogType.Warn) {
                //    filterWarn.AddCount();
                //} else if (entry.logType == DebugLogType.Error) {
                //    filterError.AddCount();
                //}
                listView.AddItem(entry);
                // UpdateListView();
            }
        }
        public void OnClickClear() {
            listView.ClearItems();
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
        public void OnClickStickToBottom() {
            listView.StickToBottom = true;
            listView.ScrollRect.verticalNormalizedPosition = 0;
        }
        public void OnClickEnter() {
            ExecuteCommand(inputCommand.text);
        }
    }
}