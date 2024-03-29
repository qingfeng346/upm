using UnityEngine;
using System;
using System.Collections.Generic;
namespace Scorpio.Debugger {
    public class ScorpioDebugger {
        public static ScorpioDebugger Instance { get; } = new ScorpioDebugger();    //单例
        private static ScorpioDebuggerWindow windowInstance = null;
        public static ScorpioDebuggerWindow WindowInstance {
            get {
                if (!windowInstance) {
                    windowInstance = UnityEngine.Object.FindObjectOfType<ScorpioDebuggerWindow>();
                    if (windowInstance == null) {
                        var gameObject = UnityEngine.Object.Instantiate(Resources.Load("ScorpioDebugger")) as GameObject;
                        UnityEngine.Object.DontDestroyOnLoad(gameObject);
                        gameObject.SetActive(false);
                        windowInstance = gameObject.GetComponent<ScorpioDebuggerWindow>();
                    }
                }
                return windowInstance;
            }
        }

        public int MaxEntryNumber { get; set; } = 20000;                    //保存的最大日志条数 默认 20000 条
        public int MaxHistoryNumber { get; set; } = 20;                     //命令行历史纪录最大条数
        public int LogStringMaxLength { get; set; } = 256;                  //日志显示最大字符数
        public int StackTraceMaxLength { get; set; } = 1024;                //日志堆栈最大字符数
        public int MinimizeType { get; set; } = 0;                          //最小化类型
        public List<LogEntry> LogEntries { get; set; }                      //所有的日志
        public List<CommandEntry> CommandEntries { get; set; }              //常用命令列表
        public List<OptionEntry> OptionEntries { get; set; }                //操作列表
        public List<LogOperationEntry> LogOperationEntries { get; set; }    //每条log的操作列表
        public bool LogEnabled { get; set; } = true;                        //是否开启日志
        internal event Action<LogEntry> logMessageReceived;                 //日志回调
        internal event Action<CommandEntry> addCommandEntry;                //添加命令列表
        internal event Action<OptionEntry> addOption;                       //添加一个控件
        internal event Action<LogOperationEntry> addLogOperation;           //添加单条日志操作
        internal event Action<Rect> safeAreaChanged;                        //屏幕安全区域改变
        public event Action<string> executeCommand;                         //命令行执行
        private CommandHistory commandHistory;                              //命令历史记录
        private bool isInitialize = false;
        public ScorpioDebugger() {
            LogEntries = new List<LogEntry>();
            CommandEntries = new List<CommandEntry>();
            OptionEntries = new List<OptionEntry>();
            LogOperationEntries = new List<LogOperationEntry>();
            commandHistory = new CommandHistory("Scorpio.Debugger.CommandHistory");
        }
        public void Initialize() {
            if (isInitialize) { return; }
            isInitialize = true;
            Application.logMessageReceivedThreaded += OnLogReceived;
        }
        public void Shutdown() {
            isInitialize = false;
            Application.logMessageReceivedThreaded -= OnLogReceived;
            LogEntries.Clear();
            CommandEntries.Clear();
            OptionEntries.Clear();
            LogOperationEntries.Clear();
            logMessageReceived = null;
            addCommandEntry = null;
            addOption = null;
            addLogOperation = null;
            safeAreaChanged = null;
            executeCommand = null;
            if (windowInstance != null) {
                GameObject.Destroy(windowInstance.gameObject);
                windowInstance = null;
            }
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
        public void SetVisiable(bool visiable) {
            if (visiable) {
                Show();
            } else {
                Hide();
            }
        }
        public void SetTabVisiable(bool visiable) {
            WindowInstance.SetTabVisiable(visiable);
        }
        public void Minimize() {
            WindowInstance.OnClickMinimize();
        }
        public void Maximize() {
            WindowInstance.OnClickMaximize();
        }
        public void SetAutoSafeArea(bool minimizeWidth, bool minimizeHeight, bool maximizeWidth, bool maximizeHeight) {
            WindowInstance.SetAutoSafeArea(minimizeWidth, minimizeHeight, maximizeWidth, maximizeHeight);
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
        public CommandHistory CommandHistory => commandHistory;
        public string LastCommand => commandHistory.Last();
        public string NextCommand => commandHistory.Next();
        public void ExecuteCommand(string command) {
            commandHistory.AddHistory(command);
            executeCommand?.Invoke(command);
        }
        public void SafeAreaChanged(Rect safeArea) {
            safeAreaChanged?.Invoke(safeArea);
        }
        public void AddLogOperation(string label, Action<LogEntry> action) {
            var entry = new LogOperationEntry() { label = label, action = action };
            LogOperationEntries.Add(entry);
            addLogOperation?.Invoke(entry);
        }
        /// <summary> Label </summary>
        public OptionEntry AddOptionLabel(string title, string label) {
            return AddOptionLabel(title, 0, 0, label);
        }
        public OptionEntry AddOptionLabel(string title, int width, int height, string label) {
            return AddOptionLabel(title, new OptionValueLabel() { width = width, height = height, label = label });
        }
        public OptionEntry AddOptionLabel(string title, OptionValueLabel value) {
            return AddOption(title, OptionType.Label, value);
        }
        /// <summary> Image </summary>
        public OptionEntry AddOptionImage(string title, Sprite sprite) {
            return AddOptionImage(title, 0, 0, sprite);
        }
        public OptionEntry AddOptionImage(string title, int width, int height, Sprite sprite) {
            return AddOptionImage(title, new OptionValueImage() { width = width, height = height, sprite = sprite });
        }
        public OptionEntry AddOptionImage(string title, OptionValueImage value) {
            return AddOption(title, OptionType.Image, value);
        }
        /// <summary> RawImage </summary>
        public OptionEntry AddOptionRawImage(string title, Texture texture, Rect uvRect) {
            return AddOptionRawImage(title, 0, 0, texture, uvRect);
        }
        public OptionEntry AddOptionRawImage(string title, int width, int height, Texture texture, Rect uvRect) {
            return AddOptionRawImage(title, new OptionValueRawImage() { width = width, height = height, texture = texture, uvRect = uvRect });
        }
        public OptionEntry AddOptionRawImage(string title, OptionValueRawImage value) {
            return AddOption(title, OptionType.RawImage, value);
        }
        /// <summary> Button </summary>
        public OptionEntry AddOptionButton(string title, string label, Action action) {
            return AddOptionButton(title, 0, 0, label, action);
        }
        public OptionEntry AddOptionButton(string title, int width, int height, string label, Action action) {
            return AddOptionButton(title, new OptionValueButton() { width = width, height = height, label = label, action = action });
        }
        public OptionEntry AddOptionButton(string title, OptionValueButton value) {
            return AddOption(title, OptionType.Button, value);
        }
        /// <summary> Toggle </summary>
        public OptionEntry AddOptionToggle(string title, string label, bool isOn, Action<bool> action) {
            return AddOptionToggle(title, 0, 0, label, isOn, action);
        }
        public OptionEntry AddOptionToggle(string title, int width, int height, string label, bool isOn, Action<bool> action) {
            return AddOptionToggle(title, new OptionValueToggle() { width = width, height = height, label = label, isOn = isOn, action = action });
        }
        public OptionEntry AddOptionToggle(string title, OptionValueToggle value) {
            return AddOption(title, OptionType.Toggle, value);
        }
        /// <summary> Dropdown </summary>
        public OptionEntry AddOptionDropdown(string title, string[] options, int value, Action<int> action) {
            return AddOptionDropdown(title, 0, 0, options, value, action);
        }
        public OptionEntry AddOptionDropdown(string title, int width, int height, string[] options, int value, Action<int> action) {
            return AddOptionDropdown(title, new OptionValueDropdown() { width = width, height = height, options = options, value = value, action = action });
        }
        public OptionEntry AddOptionDropdown(string title, OptionValueDropdown value) {
            return AddOption(title, OptionType.Dropdown, value);
        }
        /// <summary> Input </summary>
        public OptionEntry AddOptionInput(string title, string value, Action<string> action) {
            return AddOptionInput(title, 0, 0, value, action);
        }
        public OptionEntry AddOptionInput(string title, int width, int height, string value, Action<string> action) {
            return AddOptionInput(title, new OptionValueInput() { width = width, height = height, value = value, action = action });
        }
        public OptionEntry AddOptionInput(string title, OptionValueInput value) {
            return AddOption(title, OptionType.Input, value);
        }
        /// <summary> Slider </summary>
        public OptionEntry AddOptionSlider(string title, float value, float min, float max, string format, Action<float> action) {
            return AddOptionSlider(title, 0, 0, value, min, max, format, action);
        }
        public OptionEntry AddOptionSlider(string title, int width, int height, float value, float min, float max, string format, Action<float> action) {
            return AddOptionSlider(title, new OptionValueSlider() { width = width, height = height, value = value, min = min, max = max, format = format, action = action });
        }
        public OptionEntry AddOptionSlider(string title, OptionValueSlider value) {
            return AddOption(title, OptionType.Slider, value);
        }
        internal OptionEntry AddOption(string title, OptionType type, OptionValueBase value) {
            var entry = new OptionEntry() { title = title, type = type, value = value };
            OptionEntries.Add(entry);
            addOption?.Invoke(entry);
            return entry;
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