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
        var versions = command.GetValues("-version");
        ExecSco("tmp/sco", versions[0]);
        ExecScov("tmp/scov", versions[1]);
    }
    static void ExecSco(string path, string version) {
        var packagePath = "Packages/com.scorpio.sco";
        FileUtil.SyncFolder($"./{path}/Scorpio/src",           $"{packagePath}/Runtime/Scorpio", new[] { "*.cs" }, true);
        FileUtil.SyncFolder($"./{path}/ScorpioReflect/src",    $"{packagePath}/Editor/ScorpioFastReflect", new[] { "*.cs" }, true);
        FileUtil.CopyFile($"./{path}/README.md",               $"{packagePath}/Documentation~/index.md", true);
        FileUtil.CopyFile($"./{path}/README.md",               $"{packagePath}/README.md", true);
        FileUtil.CopyFile($"./{path}/ReleaseNotes.md",         $"{packagePath}/CHANGELOG.md", true);
        FileUtil.CopyFile($"./{path}/LICENSE",                 $"{packagePath}/LICENSE.md", true);
        AssetDatabase.Refresh();
        var file = $"{packagePath}/package.json";
        var package = (JObject)JsonConvert.DeserializeObject(FileUtil.GetFileString(file));
        package["version"] = version;
        FileUtil.CreateFile(file, JsonConvert.SerializeObject(package, Formatting.Indented));
    }
    static void ExecScov(string path, string version) {
        var packagePath = "Packages/com.scorpio.conversion.runtime";
        FileUtil.SyncFolder($"./{path}/ScorpioProto/CSharp/Scorpio.Conversion.Runtime/src",     $"{packagePath}/Runtime/", new[] { "*.cs" }, true);
        FileUtil.CopyFile($"./{path}/README.md",                                                $"{packagePath}/Documentation~/index.md", true);
        FileUtil.CopyFile($"./{path}/README.md",                                                $"{packagePath}/README.md", true);
        AssetDatabase.Refresh();
        var file = $"{packagePath}/package.json";
        var package = (JObject)JsonConvert.DeserializeObject(FileUtil.GetFileString(file));
        package["version"] = version;
        FileUtil.CreateFile(file, JsonConvert.SerializeObject(package, Formatting.Indented));
    }
}
