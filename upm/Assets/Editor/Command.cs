using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using Scorpio.Commons;
public class Command
{
    static CommandLine ParseCommand () {
        var args = new List<string> ();
        var first = false;
        Array.ForEach (Environment.GetCommandLineArgs (), (value) => {
            if (value == "--args") {
                first = true;
            } else if (first) {
                args.Add (value);
            }
        });
        return CommandLine.Parse (args.ToArray ());
    }
    static void Execute() {
        var command = ParseCommand();
        Scorpio.Commons.FileUtil.SyncFolder("../temp", "Assets/com.scorpio.unity.sco/Runtime/Scorpio", new[] { "*.cs" }, true);
    }
}
