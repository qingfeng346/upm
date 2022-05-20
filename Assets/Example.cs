using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Scorpio.Timer;
using Scorpio.Debugger;
public class Example : MonoBehaviour
{
    void Start() {
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
        for (var i = 0; i <2; ++i) {
            ScorpioDebugger.Instance.AddOptionToggle("测试Toggle", 100,100,"label " + i, true, (isOn) => {
                Debug.Log("isOn : " + isOn);
            });
        }
        for (var i = 0; i < 2; ++i) {
            ScorpioDebugger.Instance.AddOptionDropdown("测试Dropdown", new string[] {"111","222","3333"}, 1, (value) => {
                Debug.Log("dropdown : " + value);
            });
        }
        for (var i = 0; i < 2; ++i) {
            ScorpioDebugger.Instance.AddOptionInput("测试input", i.ToString(), (value) => {
                Debug.Log("input : " + value);
            });
        }
        for (var i = 0; i < 10; ++i) {
            var a = i;
            ScorpioDebugger.Instance.AddOptionButton("测试测试22", "button" + i, () =>
            {
                Debug.Log("======2222 " + a);
            });
        }
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
        if (GUI.Button(new Rect(100,100,100,100), "打开Debug")) {
            ScorpioDebugger.Instance.Show();
        }
        if (GUI.Button(new Rect(100,200,100,100), "测试Timer")) {
            Debug.Log("start : " + Time.time);
            TimerManager.Instance.AddGameTimerMS(2000, (timer, args, fixedArgs) => {
                Debug.Log("end : " + Time.time);
            });
        }
    }
}
