using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using Scorpio.Commons;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using FileUtil = Scorpio.Commons.FileUtil;
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
    [MenuItem("Assets/Execute")]
    static void Execute() {
        var command = ParseCommand();
        var version = command.GetValue("-version");
        var path = command.GetValue("-path");
        FileUtil.SyncFolder($"../{path}/Scorpio/src", "Assets/com.scorpio.unity.sco/Runtime/Scorpio", new[] { "*.cs" }, true);
        FileUtil.SyncFolder($"../{path}/ScorpioReflect/src", "Assets/com.scorpio.unity.sco.fastreflect/Editor/ScorpioFastReflect", new[] { "*.cs" }, true);
        FileUtil.CopyFile($"../{path}/README.md",       "Assets/com.scorpio.unity.sco/Documentation~/index.md", true);
        FileUtil.CopyFile($"../{path}/ReleaseNotes.md", "Assets/com.scorpio.unity.sco/CHANGELOG.md", true);
        FileUtil.CopyFile($"../{path}/LICENSE",         "Assets/com.scorpio.unity.sco/LICENSE.md", true);
        FileUtil.CopyFile($"../{path}/README.md",       "Assets/com.scorpio.unity.sco.fastreflect/Documentation~/index.md", true);
        FileUtil.CopyFile($"../{path}/ReleaseNotes.md", "Assets/com.scorpio.unity.sco.fastreflect/CHANGELOG.md", true);
        FileUtil.CopyFile($"../{path}/LICENSE",         "Assets/com.scorpio.unity.sco.fastreflect/LICENSE.md", true);
        AssetDatabase.Refresh();
        {
            var file = "Assets/com.scorpio.unity.sco/package.json";
            var package = (JObject)JsonConvert.DeserializeObject(FileUtil.GetFileString(file));
            package["version"] = version;
            FileUtil.CreateFile(file, JsonConvert.SerializeObject(package, Formatting.Indented));
        }
        {
            var file = "Assets/com.scorpio.unity.sco.fastreflect/package.json";
            var package = (JObject)JsonConvert.DeserializeObject(FileUtil.GetFileString(file));
            package["version"] = version;
            ((JObject)package["dependencies"])["com.scorpio.unity.sco"] = version;
            FileUtil.CreateFile(file, JsonConvert.SerializeObject(package, Formatting.Indented));
        }
    }
}
