using UnityEngine;
using System;
using System.Collections.Generic;
namespace Scorpio.Debugger {
    public class ScorpioDebugger {
        public static int MaxEntryNumber = 20000;                                   //保存的最大日志条数 默认 20000 条
        public static int MaxHistoryNumber = 20;                                    //命令行历史纪录最大条数
        public static int LogStringMaxLength = 256;                                 //日志显示最大字符数
        public static int StackTraceMaxLength = 1024;                               //日志堆栈最大字符数
        public static ScorpioDebugger Instance { get; } = new ScorpioDebugger();    //单例
        private static ScorpioDebuggerWindow windowInstance = null;
        public static ScorpioDebuggerWindow WindowInstance {
            get {
                if (!windowInstance) {
                    windowInstance = UnityEngine.Object.FindObjectOfType<ScorpioDebuggerWindow>();
                    if (windowInstance == null) {
                        var gameObject = UnityEngine.Object.Instantiate(Resources.Load("ScorpioDebugger")) as GameObject;
                        UnityEngine.Object.DontDestroyOnLoad(gameObject);
                        windowInstance = gameObject.GetComponent<ScorpioDebuggerWindow>();
                    }
                }
                return windowInstance;
            }
        }
        
        public List<LogEntry> LogEntries { get; set; }                  //所有的日志
        public List<CommandEntry> CommandEntries { get; set; }          //常用命令列表
        public List<OptionEntry> OptionEntries { get; set; }            //操作列表
        public List<LogOperation> LogOperations { get; set; }           //每条log的操作列表
        public bool LogEnabled { get; set; } = true;                    //是否开启日志
        internal event Action<LogEntry> logMessageReceived;             //日志回调
        internal event Action<CommandEntry> addCommandEntry;            //添加命令列表
        internal event Action<OptionEntry> addOption;                   //添加一个控件
        internal event Action<LogOperation> addLogOperation;            //添加单条日志操作
        public event Action<string> executeCommand;                     //命令行执行
        private CommandHistory commandHistory;                          //命令历史记录
        public ScorpioDebugger() {
            LogEntries = new List<LogEntry>();
            CommandEntries = new List<CommandEntry>();
            OptionEntries = new List<OptionEntry>();
            LogOperations = new List<LogOperation>();
            commandHistory = new CommandHistory("Scorpio.Debugger.CommandHistory");
            Application.logMessageReceivedThreaded += OnLogReceived;
        }
        public void Show() {
            WindowInstance.gameObject.SetActive(true);
        }
        public void Hide() {
            if (windowInstance != null) {
                windowInstance.gameObject.SetActive(false);
            }
        }
        public bool IsShow() {
            return windowInstance != null && windowInstance.gameObject.activeSelf;
        }
        public void OnLogReceived(string logString, string stackTrace, UnityEngine.LogType logType) {
            if (!LogEnabled) { return; }
            var debugLogType = LogType.Info;
            if (logType == UnityEngine.LogType.Assert || logType == UnityEngine.LogType.Error || logType == UnityEngine.LogType.Exception) {
                debugLogType = LogType.Error;
            } else if (logType == UnityEngine.LogType.Warning) {
                debugLogType = LogType.Warn;
            }
            var logEntry = new LogEntry(debugLogType, logString, stackTrace);
            LogEntries.Add(logEntry);
            while (LogEntries.Count > MaxEntryNumber) {
                LogEntries.RemoveAt(0);
            }
            logMessageReceived?.Invoke(logEntry);
        }
        public List<LogEntry> FindAll(Predicate<LogEntry> match) { return LogEntries.FindAll(match); }
        public int Count(LogType logType) { return LogEntries.FindAll((entry) => entry.logType == logType).Count; }
        public void Clear() { LogEntries.Clear(); }
        public void ClearExecuteCommand() { executeCommand = null; }
        public string LastCommand => commandHistory.Last();
        public string NextCommand => commandHistory.Next();
        public void ExecuteCommand(string command) {
            commandHistory.AddHistory(command);
            executeCommand?.Invoke(command);
        }
        public void AddLogOperation(string label, Action<LogEntry> action) {
            var entry = new LogOperation() { label = label, action = action };
            LogOperations.Add(entry);
            addLogOperation?.Invoke(entry);
        }
        public void AddOptionButton(string title, string label, Action action) {
            AddOption(title, OptionType.Button, new OptionValueButton() { label = label, action = action });
        }
        internal void AddOption(string title, OptionType type, object value) {
            var entry = new OptionEntry() { title = title, type = type, value = value };
            OptionEntries.Add(entry);
            addOption?.Invoke(entry);
        }
        public CommandEntry AddCommandEntry(string labelEN, string labelCN, string labelParam, string command) {
            var commandEntry = new CommandEntry() {
                labelCN = labelCN,
                labelEN = labelEN,
                labelParam = labelParam,
                command = command
            };
            CommandEntries.Add(commandEntry);
            addCommandEntry?.Invoke(commandEntry);
            return commandEntry;
        }
    }
}