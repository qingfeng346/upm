﻿using System.Collections;
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
