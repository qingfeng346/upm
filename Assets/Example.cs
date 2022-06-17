using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Scorpio.Timer;
using Scorpio.Debugger;
using Scorpio.Ini;
using Scorpio.Prompt;
public class Example : MonoBehaviour
{
    public Sprite sprite;
    public Texture texture;
    void Start() {
        //        var ini = new ScorpioIni();
        //        ini.Set("aaa", @"111
        //222
        //333");
        //        System.IO.File.WriteAllText(@"C:\Users\while\Desktop\test.ini", ini.BuilderString());
        //        ini.InitFormFile(@"C:\Users\while\Desktop\test.ini", System.Text.Encoding.UTF8);
        //        Debug.Log(ini.Get("aaa"));
        for (var i = 0; i < 10; ++i) {
            ScorpioDebugger.Instance.AddCommandEntry("command : cn " + i, "command : en  " + i, "command : param " + i, "command " + i);
        }
        for (var i = 0; i < 10; ++i) {
            var a = i;
            ScorpioDebugger.Instance.AddOptionButton("测试测试", "button" + i, () =>
            {
                Debug.Log("====== " + a);
            });
        }
        var title = "控件测试";
        for (var i = 0; i <2; ++i) {
            ScorpioDebugger.Instance.AddOptionToggle(title, 0, 0, "label " + i, true, (isOn) => {
                Debug.Log("toggle : " + isOn);
            });
        }
        for (var i = 0; i < 2; ++i) {
            ScorpioDebugger.Instance.AddOptionDropdown(title, new string[] {"111","222","3333"}, 1, (value) => {
                Debug.Log("dropdown : " + value);
            });
        }
        for (var i = 0; i < 2; ++i) {
            ScorpioDebugger.Instance.AddOptionInput(title, i.ToString(), (value) => {
                Debug.Log("input : " + value);
            });
        }
        for (var i = 0; i < 2; ++i) {
            var a = i;
            ScorpioDebugger.Instance.AddOptionButton(title, "button" + i, () =>
            {
                Debug.Log("button : " + a);
            });
        }
        ScorpioDebugger.Instance.AddOptionLabel(title, "labellabe");
        ScorpioDebugger.Instance.AddOptionSlider(title, 0.5f, 10, 100, null, (value) => {
            Debug.Log("slider : " + value);
        });
        ScorpioDebugger.Instance.AddOptionImage(title, sprite);
        ScorpioDebugger.Instance.AddOptionRawImage(title, texture, new Rect(0, 0, 1, 1));
        for (var i = 0; i < 5; ++i) {
            ScorpioDebugger.Instance.AddLogOperation("单条日志:" + i, (logEntry) => {
                Debug.Log("------- " + logEntry.logString);
            });
        }
        ScorpioDebugger.Instance.executeCommand += (command) =>
        {
            Debug.Log("运行命令 : " + command);
        };
    }
    void OnGUI() {
        if (GUI.Button(new Rect(100, 100, 100, 100), "打开Debug")) {
            ScorpioDebugger.Instance.Initialize();
            ScorpioDebugger.Instance.Show();
        }
        if (GUI.Button(new Rect(100, 200, 100, 100), "Debug Shutdown")) {
            ScorpioDebugger.Instance.Shutdown();
        }
        if (GUI.Button(new Rect(100, 300, 100, 100), "测试Timer")) {
            Debug.Log("start : " + Time.time);
            TimerManager.Instance.AddGameTimerMS(2000, (timer, args, fixedArgs) => {
                Debug.Log("end : " + Time.time);
            });
        }
        if (GUI.Button(new Rect(100, 400, 100, 100), "测试PromptLabel")) {
            //PromptManager.Instance.ShowLabel("fawefawefw");
            PromptManager.Instance.ShowToast("fawefawefw");
        }
    }
}
