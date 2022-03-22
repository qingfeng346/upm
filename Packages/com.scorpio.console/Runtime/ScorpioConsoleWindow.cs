using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
// using SuperScrollView;
using System;

namespace Scorpio.Console {

//日志过滤
public class LogFilter {
    public static Color FilterNormalColor = new Color (0.31f, 0.31f, 0.31f, 1); //未过滤颜色
    public static Color FilterSelectedColor = new Color (0.5f, 0.5f, 0.5f, 1); //过滤颜色

    public Image imageFilter; //背景
    public Text textCount; //数量
    public bool selected { get; private set; } = true; //是否选中
    private int count = 0; //数量
    public void Init (GameObject gameObject, System.Action filter, int count) {
        this.imageFilter = ScorpioDebugUtil.GetComponent<Image> (gameObject);
        this.textCount = ScorpioDebugUtil.FindChild<Text> (gameObject, "Count");
        this.count = count;
        SetSelected (true);
        ScorpioDebugUtil.RegisterClick (gameObject, () => {
            SetSelected (!selected);
            filter ();
        });
        this.UpdateText();
    }
    void SetSelected (bool selected) {
        this.selected = selected;
        imageFilter.color = selected ? FilterSelectedColor : FilterNormalColor;
    }
    public void AddCount () {
        ++count;
        UpdateText();
    }
    public void Clear () {
        count = 0;
        UpdateText();
    }
    public void UpdateText() {
        textCount.text = count.ToString();
    }
}

public class ScorpioConsoleWindow : MonoBehaviour {
    private const string CommandHistoryKey = "__command_history";
    public GameObject commandItem;                          //命令行列表元素
    public GameObject buttonItem;                           //日志操作Button

    public GameObject toolbar;                             //工具栏
    public InputField commandInput;                        //命令行输入框
    public InputField searchInput;                         //搜索输入框
    public VirtualVerticalLayoutGroup listView;            //
    public GameObject debugLogInfo;                        //日志详情界面
    public Text debugLogInfoText;                          //日志详情信息
    public GameObject logButtons;                          //日志操作列表
    private Dictionary<string, GameObject> logObjects;      //日志操作列表
    private DebugLogEntry debugLog;                         //当前查看的日志
    public GameObject debugLogWindow;
    public GameObject buttonBottom;                        //到底部按钮
    private List<DebugLogEntry> entrys;                     //所有日志
    private Queue<DebugLogEntry> addEntrys;                 //需要添加的日志

    public GameObject commandList;                          //命令行列表界面
    public GameObject commandGrid;                          //命令行Grid
    private Dictionary<string, GameObject> commandObjects;  //命令行模板
    private List<string> commandHistory;                    //命令行历史纪录
    private int commandHistoryIndex;                        //命令行历史记录位置
    private LogFilter filterInfo = new LogFilter ();        //Info过滤
    private LogFilter filterWarn = new LogFilter ();        //Warn过滤
    private LogFilter filterError = new LogFilter ();       //Error过滤
    private bool autoToBottom;                              //是否自动到底部
    void Awake()
    {
        ScorpioConsole.Instance.SetConsoleWindow(this);

        ScorpioDebugUtil.RegisterClick(debugLogInfo, "Collider", () => debugLogInfo.SetActive(false));
        commandInput.onValidateInput += (text, charIndex, addedChar) => {
            if (addedChar == '\n') {
                ExecuteCommand(text);
                return '\0';
            }
            return addedChar;
        };
        ScorpioDebugUtil.RegisterClick(buttonBottom, () => { this.SetAutoToBottom(true); });
        //显示隐藏命令行列表
        ScorpioDebugUtil.RegisterClick(debugLogWindow, "ButtonCommands", () => { 
            commandList.SetActive(!commandList.activeSelf);
        });
        //最小化按钮
        ScorpioDebugUtil.RegisterClick(debugLogWindow, "ButtonVisiable", () => {
            SetVisiable(!toolbar.activeSelf);
        });
        //执行命令行按钮
        ScorpioDebugUtil.RegisterClick(debugLogWindow, "ButtonEnter", () => ExecuteCommand(commandInput.text));
        ScorpioDebugUtil.RegisterClick(debugLogWindow, "ButtonUploadLog", () => ExecuteCommand("uploadLog"));
        //搜索
        ScorpioDebugUtil.RegisterSubmit (searchInput = ScorpioDebugUtil.FindChild<InputField> (toolbar, "InputSearch"), (str) => UpdateListView());
        //隐藏命令行界面
        ScorpioDebugUtil.RegisterClick(toolbar, "HideButton", () => ScorpioConsole.Instance.Hide());
        //清除所有日志
        ScorpioDebugUtil.RegisterClick(toolbar, "ClearButton", () => {
            ScorpioConsole.Instance.Clear ();
            filterInfo.Clear ();
            filterWarn.Clear ();
            filterError.Clear ();
            UpdateListView ();
        });
        //过滤日志类型
        filterInfo.Init(ScorpioDebugUtil.FindChild(toolbar, "FilterInfoButton"), this.OnFilterChanged, ScorpioConsole.Instance.Count(DebugLogType.Info));
        filterWarn.Init(ScorpioDebugUtil.FindChild(toolbar, "FilterWarnButton"), this.OnFilterChanged, ScorpioConsole.Instance.Count(DebugLogType.Warn));
        filterError.Init(ScorpioDebugUtil.FindChild(toolbar, "FilterErrorButton"), this.OnFilterChanged, ScorpioConsole.Instance.Count(DebugLogType.Error));

        addEntrys = new Queue<DebugLogEntry>();
        ScorpioConsole.Instance.logMessageReceived += entry => addEntrys.Enqueue(entry);
        commandHistory = new List<string>();
        if (PlayerPrefs.HasKey(CommandHistoryKey)) {
            // foreach (var command in MiniJson.Json.Deserialize(PlayerPrefs.GetString(CommandHistoryKey)) as List<object>) {
            //     commandHistory.Add(command.ToString());
            // }
            commandHistoryIndex = commandHistory.Count;
        }
        commandGrid = ScorpioDebugUtil.FindChild (debugLogWindow, "CommandList/Mask/Items");
        commandObjects = new Dictionary<string, GameObject> ();
        logObjects = new Dictionary<string, GameObject>();
        foreach (var pair in ScorpioConsole.Instance.operates) { AddOperate (pair.Key, pair.Value); }
        foreach (var entry in ScorpioConsole.Instance.commands) { AddCommand (entry); }
        // logListView.InitListView(0, (listView, index) => {
        //     if (index < 0 || index >= entrys.Count) { return null; }
        //     LoopListViewItem2 item = listView.NewListViewItem("DebugLogItem");
        //     item.GetComponent<DebugLogItem>().SetLogEntry(entrys[index]);
        //     return item;
        // });
        // logListView.ScrollRect.onValueChanged.AddListener((pos) => {
        //     if (autoToBottom && pos.y > 0.01f) {
        //         SetAutoToBottom(false);
        //     }
        // });
        UpdateListView ();
        SetAutoToBottom(true);
    }
    //添加一个日志操作
    public void AddOperate (string label, Action<DebugLogEntry> action) {
        GameObject item;
        if (!logObjects.TryGetValue(label, out item)) {
            item = Instantiate(this.buttonItem) as GameObject;
            item.transform.SetParent(logButtons.transform);
            item.transform.localScale = Vector3.one;
            logObjects[label] = item;
            ScorpioDebugUtil.FindChild<Text> (item, "Text").text = label;
        }
        ScorpioDebugUtil.RegisterClick (item, () => {
            if (action != null) action(debugLog);
        });
    }
    //添加一个模板命令
    public void AddCommand (CommandEntry entry) {
        GameObject item;
        if (!commandObjects.TryGetValue (entry.labelCN, out item)) {
            item = Instantiate (this.commandItem) as GameObject;
            item.transform.SetParent (commandGrid.transform);
            item.transform.localScale = Vector3.one;
            commandObjects[entry.labelCN] = item;
            ScorpioDebugUtil.FindChild<Text> (item, "Image/Desc_CN").text = entry.labelCN;
            ScorpioDebugUtil.FindChild<Text> (item, "Image/Desc_EN").text = entry.labelEN;
            ScorpioDebugUtil.FindChild<Text> (item, "Image/Desc_Param").text = entry.labelParam;
            ScorpioDebugUtil.RegisterClick (item, () => {
                commandList.SetActive (false);
                commandInput.ActivateInputField();
                commandInput.text = entry.command;
                commandInput.MoveTextEnd(true);
            });
        }
    }
    //显示日志详情
    public void ShowLogInfo (DebugLogEntry entry) {
        debugLogInfo.SetActive (true);
        debugLog = entry;
        // this.debugLogInfoText.text = entry.LogInfo;
    }
    //是否显示主界面
    public void SetVisiable(bool visiable) {
        // logListView.gameObject.SetActive(visiable);
        toolbar.SetActive(visiable);
    }
    //是否自动滚到底部
    void SetAutoToBottom(bool enable) {
        if (enable) {
            autoToBottom = true;
            buttonBottom.SetActive(false);
            // logListView.MovePanelToItemIndex(entrys.Count - 1, 0);
        } else {
            autoToBottom = false;
            buttonBottom.SetActive(true);
        }
    }
    //命令行列表
    void OnClickCommandList () {
        commandList.SetActive (!commandList.activeSelf);
    }
    //日志过滤改变
    void OnFilterChanged () {
        UpdateListView ();
    }
    //检查日志是否显示
    bool CheckLogEntry (DebugLogEntry entry) {
        var search = searchInput.text;
        if (!filterInfo.selected && entry.logType == DebugLogType.Info) { return false; }
        if (!filterWarn.selected && entry.logType == DebugLogType.Warn) { return false; }
        if (!filterError.selected && entry.logType == DebugLogType.Error) { return false; }
        if (!string.IsNullOrEmpty (search) && !entry.logString.ToLower().Contains (search.ToLower())) { return false; }
        return true;
    }
    //更新日志列表
    void UpdateListView () {
        var search = searchInput.text;
        if (string.IsNullOrEmpty (search) && filterInfo.selected && filterWarn.selected && filterError.selected) {
            entrys = ScorpioConsole.Instance.All();
        } else {
            entrys = ScorpioConsole.Instance.FindAll(CheckLogEntry);
        }
        listView.ClearItems();
        foreach (var entry in entrys) {
            listView.AddItem(entry);
        }
        // logListView.SetListItemCount(entrys.Count, false);
        // if (autoToBottom) {
        //     logListView.MovePanelToItemIndex(entrys.Count - 1, 0);
        // }
    }
    //执行命令行
    void ExecuteCommand(string text) {
        commandInput.text = "";
        if (text.Length > 0) {
            AddCommandHistory(text);
            ScorpioConsole.Instance.ExecuteCommand (text);
        }
    }
    //添加一条历史记录
    void AddCommandHistory(string text) {
        var index = commandHistory.IndexOf(text);
        if (index >= 0) { commandHistory.RemoveAt(index); }
        commandHistory.Add(text);
        if (commandHistory.Count > ScorpioConsole.MaxHistoryNumber) { commandHistory.RemoveAt(0); }
        commandHistoryIndex = commandHistory.Count;
        // PlayerPrefs.SetString(CommandHistoryKey, MiniJson.Json.Serialize(commandHistory));
        // PlayerPrefs.Save();
    }
    void LateUpdate() {
        if (commandInput.isFocused) {
            if (Input.GetKeyDown(KeyCode.UpArrow)) {
                if (commandHistory.Count > 0) {
                    commandHistoryIndex = Mathf.Max(0, commandHistoryIndex - 1);
                    commandInput.text = commandHistory[commandHistoryIndex];
                    commandInput.caretPosition = commandInput.text.Length;
                }
            } else if (Input.GetKeyDown(KeyCode.DownArrow)) {
                if (commandHistory.Count > 0) {
                    commandHistoryIndex = Mathf.Min(commandHistory.Count, commandHistoryIndex + 1);
                    commandInput.text = commandHistoryIndex == commandHistory.Count ? "" : commandHistory[commandHistoryIndex];
                    commandInput.caretPosition = commandInput.text.Length;
                }
            }
        }
        if (addEntrys.Count > 0) {
            var entry = addEntrys.Dequeue();
            if (entry.logType == DebugLogType.Info) {
                filterInfo.AddCount ();
            } else if (entry.logType == DebugLogType.Warn) {
                filterWarn.AddCount ();
            } else if (entry.logType == DebugLogType.Error) {
                filterError.AddCount ();
            }
            listView.AddItem(entry);
            // UpdateListView();
        }
    }
}
}