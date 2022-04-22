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

        private List<LogEntry> logs;                            //所有的日志
        private List<CommandEntry> commands;                    //常用命令列表
        private List<LogOperate> logOperates;                   //每条log的操作列表
        public static bool LogEnabled { get; set; } = true;     //是否开启日志
        public event Action<LogEntry> logMessageReceived;       //日志回调
        public Action<string> executeCommand;                   //命令行执行
        public ScorpioDebugger() {
            logs = new List<LogEntry>();
            commands = new List<CommandEntry>();
            logOperates = new List<LogOperate>();
            Application.logMessageReceivedThreaded += OnLogReceived;
        }
        public void Show() {
            //if (consoleWindow == null) {
            //    GameObject.Instantiate(Resources.Load("ScorpioConsoleWindow"));
            //}
            //consoleWindow.gameObject.SetActive(true);
        }
        public void Hide() {
            //if (consoleWindow != null) {
            //    consoleWindow.gameObject.SetActive(false);
            //}
        }
        public bool IsShow() {
                //return consoleWindow != null && consoleWindow.gameObject.activeSelf;
                return false;
        }
        public void SetVisiable(bool visiable) {
            //if (consoleWindow != null) {
            //    consoleWindow.SetVisiable(visiable);
            //}
        }
        //public void SetConsoleWindow(ScorpioConsoleWindow window) {
        //    consoleWindow = window;
        //    GameObject.DontDestroyOnLoad(consoleWindow.gameObject);
        //}

        public void OnLogReceived(string logString, string stackTrace, UnityEngine.LogType logType) {
            if (!LogEnabled) { return; }
            var debugLogType = LogType.Info;
            if (logType == UnityEngine.LogType.Assert || logType == UnityEngine.LogType.Error || logType == UnityEngine.LogType.Exception) {
                debugLogType = LogType.Error;
            } else if (logType == UnityEngine.LogType.Warning) {
                debugLogType = LogType.Warn;
            }
            var entry = new LogEntry(debugLogType, logString, stackTrace);
            logs.Add(entry);
            while (logs.Count > MaxEntryNumber) {
                logs.RemoveAt(0);
            }
            logMessageReceived?.Invoke(entry);
        }
        public List<LogEntry> All() { return new List<LogEntry>(logs); }
        public List<LogEntry> FindAll(Predicate<LogEntry> match) { return logs.FindAll(match); }
        public int Count(LogType logType) { return logs.FindAll((entry) => entry.logType == logType).Count; }
        public void Clear() { logs.Clear(); }
        public void ExecuteCommand(string command) {
            executeCommand?.Invoke(command);
        }
        public void AddLogOperate(string label, Action<LogEntry> action) {
            logOperates.Add(new LogOperate() { label = label, action = action });
        }
        public void AddCommand(string labelEN, string labelCN, string labelParam, string command) {
            var entry = new CommandEntry() {
                labelCN = labelCN,
                labelEN = labelEN,
                labelParam = labelParam,
                command = command
            };
            commands.Add(entry);
        }
    }
}