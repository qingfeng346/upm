using UnityEngine;
using System;
using System.Collections.Generic;

namespace Scorpio.Debugger {

    public enum DebugLogType {
        Info = 1,
        Warn = 2,
        Error = 4
    }

    public class DebugLogEntry {
        public int index;
        public DebugLogType logType;
        public string logString;
        public string stackTrace;
        public DebugLogEntry(int index, DebugLogType logType, string logString, string stackTrace) {
            this.index = index;
            this.logType = logType;
            this.logString = logString;
            this.stackTrace = stackTrace;
        }
        public string LogString => logString.Length > ScorpioDebugger.LogStringMaxLength ? logString.Substring(0, ScorpioDebugger.LogStringMaxLength) : logString;
        public string StackTrace => stackTrace.Length > ScorpioDebugger.StackTraceMaxLength ? stackTrace.Substring(0, ScorpioDebugger.StackTraceMaxLength) : stackTrace;
        public string LogInfo => $@"[{logType}] : {LogString}
    {StackTrace}";

    }
    public class CommandEntry {
        public string labelCN;
        public string labelEN;
        public string labelParam;
        public string command;
    }
    public class ScorpioDebugger {
        public static int MaxEntryNumber = 20000;                                   //保存的最大日志条数 默认 20000 条
        public static int MaxHistoryNumber = 20;                                    //命令行历史纪录最大条数
        public static int LogStringMaxLength = 256;                                 //日志显示最大字符数
        public static int StackTraceMaxLength = 1024;                               //日志堆栈最大字符数
        public static ScorpioDebugger Instance { get; } = new ScorpioDebugger();      //单例
        private List<DebugLogEntry> logs = new List<DebugLogEntry>();               //所有的日志
        //private ScorpioConsoleWindow consoleWindow;                                 //控制台窗口
        public Dictionary<string, Action<DebugLogEntry>> operates = new Dictionary<string, Action<DebugLogEntry>>();         //单条日志操作
        public List<CommandEntry> commands = new List<CommandEntry>();              //常用命令列表
        public static bool LogEnabled { get; set; } = true;                         //是否开启日志
        public event Action<DebugLogEntry> logMessageReceived;                      //日志回调
        public Action<string> executeCommand;                                       //命令行执行
        private int index = 0;                                                      //日志索引
        public void Initialize() {
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

        public void OnLogReceived(string logString, string stackTrace, LogType logType) {
            if (!LogEnabled) { return; }
            var debugLogType = DebugLogType.Info;
            if (logType == LogType.Assert || logType == LogType.Error || logType == LogType.Exception) {
                debugLogType = DebugLogType.Error;
            } else if (logType == LogType.Warning) {
                debugLogType = DebugLogType.Warn;
            }
            var entry = new DebugLogEntry(index++, debugLogType, logString, stackTrace);
            logs.Add(entry);
            while (logs.Count > MaxEntryNumber) {
                logs.RemoveAt(0);
            }
            if (logMessageReceived != null) logMessageReceived(entry);
        }
        public List<DebugLogEntry> All() { return new List<DebugLogEntry>(logs); }
        public List<DebugLogEntry> FindAll(Predicate<DebugLogEntry> match) { return logs.FindAll(match); }
        public int Count(DebugLogType logType) { return logs.FindAll((entry) => entry.logType == logType).Count; }
        public void Clear() { logs.Clear(); }
        public void ExecuteCommand(string command) {
            if (executeCommand != null) { 
                try {
                    executeCommand(command);
                } catch (System.Exception e) {
                    // logger.error($"执行命令行出错 : {e.ToString()}");
                }
            }
        }
        public void AddOperate(string label, Action<DebugLogEntry> action) {
            operates[label] = action;
            //if (consoleWindow != null) { consoleWindow.AddOperate(label, action); }
        }
        public void AddCommand(string labelEN, string labelCN, string labelParam, string command) {
            var entry = new CommandEntry() {
                labelCN = labelCN,
                labelEN = labelEN,
                labelParam = labelParam,
                command = command
            };
            commands.Add(entry);
            //if (consoleWindow != null) { consoleWindow.AddCommand(entry); }
        }
        public void ShowLogInfo(DebugLogEntry entry) {
            //if (consoleWindow != null) { consoleWindow.ShowLogInfo(entry); }
        }
    }
}