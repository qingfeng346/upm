using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace Scorpio.Debugger {
    public class ConsoleWindow : MonoBehaviour {
        public InputField inputCommand;
        public InputField inputSearch;
        public VirtualVerticalLayoutGroup listView;
        public ConsoleLogInfo logInfo;
        public ConsoleCommands commands;
        public ConsoleLogFilter infoFilter, warnFilter, errorFilter;
        private Queue<LogEntry> addEntries;
        private string search = null;
        void Awake() {
            inputCommand.onValidateInput += OnValidateInput;
            inputSearch.onEndEdit.AddListener(OnSearchChanged);
            addEntries = new Queue<LogEntry>();
            ScorpioDebugger.Instance.logMessageReceived += LogMessageReceived;
            UpdateAllLogs();
        }
        char OnValidateInput(string text, int charIndex, char addedChar) {
            if (addedChar == '\n') {
                ExecuteCommand(text);
                return '\0';
            }
            return addedChar;
        }
        void OnSearchChanged(string text) {
            search = text;
            UpdateAllLogs();
        }
        public void UpdateAllLogs() {
            listView.ClearItems();
            addEntries.Clear();
            infoFilter.Count = 0;
            warnFilter.Count = 0;
            errorFilter.Count = 0;
            IEnumerable<LogEntry> entries = string.IsNullOrEmpty(search) ? ScorpioDebugger.Instance.LogEntries : ScorpioDebugger.Instance.LogEntries.Where(_ => _.logString.Contains(search));
            foreach (var entry in entries) {
                if (IsMatch(entry)) {
                    addEntries.Enqueue(entry);
                }
            }
        }
        bool IsMatch(LogEntry logEntry) {
            switch (logEntry.logType) {
                case LogType.Error: {
                    return errorFilter.isOn;
                }
                case LogType.Warn: {
                    return warnFilter.isOn;
                }
                case LogType.Info: {
                    return infoFilter.isOn;
                }
            }
            return false;
        }
        void LogMessageReceived(LogEntry logEntry) {
            if (!string.IsNullOrEmpty(search) && !logEntry.logString.Contains(search)) {
                return;
            }
            if (IsMatch(logEntry)) {
                addEntries.Enqueue(logEntry);
            }
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
            inputCommand.Select();
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
                int count = 0;
                while (addEntries.Count > 0 && count < 20) {
                    var entry = addEntries.Dequeue();
                    if (entry.logType == LogType.Info) {
                        infoFilter.Count += 1;
                    } else if (entry.logType == LogType.Warn) {
                        warnFilter.Count += 1;
                    } else if (entry.logType == LogType.Error) {
                        errorFilter.Count += 1;
                    }
                    listView.AddItem(entry);
                    count++;
                }
            }
        }
        public void OnClickClear() {
            listView.ClearItems();
            addEntries.Clear();
            infoFilter.Count = 0;
            warnFilter.Count = 0;
            errorFilter.Count = 0;
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
            //listView.StickToBottom = true;
            listView.ScrollRect.verticalNormalizedPosition = 0;
        }
        public void OnClickEnter() {
            ExecuteCommand(inputCommand.text);
        }
    }
}