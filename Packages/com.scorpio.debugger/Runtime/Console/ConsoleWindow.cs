using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace Scorpio.Debugger {
    public class ConsoleWindow : ConsoleInputPart {
        public InputField inputSearch;
        public VirtualVerticalLayoutGroup listView;
        public ConsoleLogInfo logInfo;
        public ConsoleLogFilter infoFilter, warnFilter, errorFilter;
        private Queue<LogEntry> addEntries;
        private string search = null;
        protected override void Awake() {
            base.Awake();
            inputSearch.onEndEdit.AddListener(OnSearchChanged);
            addEntries = new Queue<LogEntry>();
            ScorpioDebugger.Instance.logMessageReceived += LogMessageReceived;
            UpdateAllLogs();
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
        internal void ShowLogEntry(LogEntry logEntry) {
            logInfo.gameObject.SetActive(true);
            logInfo.SetLogEntry(logEntry);
        }
        protected override void LateUpdate() {
            base.LateUpdate();
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
        public void OnClickStickToBottom() {
            //listView.StickToBottom = true;
            listView.ScrollRect.verticalNormalizedPosition = 0;
        }
    }
}