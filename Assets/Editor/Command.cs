using UnityEditor;
using System;
using System.Diagnostics;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Scorpio.Unity.Command;
using System.IO;
using Scorpio.Unity.Util;
using FileUtil = Scorpio.Unity.Util.FileUtil;
[InitializeOnLoad]
public class Command
{
    static Command() {
        CommandBuild.AddCommand<string>("SyncScorpio", SyncScorpio);
        CommandBuild.AddCommand<string>("SyncScov", SyncScov);
        CommandBuild.AddCommand("Start", Start);
    }
    public static int StartProcess(string fileName, string arguments, string workingDirectory = null) {
        try {
            using (var process = new Process()) {
                process.StartInfo.FileName = fileName;
                if (!string.IsNullOrEmpty(workingDirectory)) {
                    process.StartInfo.WorkingDirectory = workingDirectory;
                } else {
                    process.StartInfo.WorkingDirectory = Path.GetFullPath("./");
                }
                if (arguments != null) {
                    process.StartInfo.Arguments = arguments;
                }
                process.StartInfo.WindowStyle = ProcessWindowStyle.Normal;
                process.StartInfo.CreateNoWindow = true;
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.RedirectStandardError = true;
                process.EnableRaisingEvents = true;
                logger.info($"StartProcess {fileName} {arguments}");
                process.Start();
                var output = process.StandardOutput.ReadToEnd();
                var error = process.StandardError.ReadToEnd();
                process.WaitForExit();
                if (!string.IsNullOrEmpty(output)) {
                    logger.info(output);
                }
                if (!string.IsNullOrEmpty(error)) {
                    logger.error(error);
                }
                return process.ExitCode;
            }
        } catch (Exception e) {
            logger.error("StartProcess Error : " + e.ToString());
        } finally {
            EditorUtility.ClearProgressBar();
        }
        return -1;
    }
    static string GetTempPath() {
        return Path.GetTempPath() + Guid.NewGuid().ToString();
    }
    static void SyncScorpio(string version) {
        var path = GetTempPath();
        StartProcess("git", $"clone -b v{version} https://github.com/qingfeng346/Scorpio-CSharp.git {path}");
        var packagePath = "Packages/com.scorpio.sco";
        FileUtil.SyncFolder($"{path}/Scorpio/src", $"{packagePath}/Runtime/Scorpio", new[] { "*.cs" }, true);
        FileUtil.SyncFolder($"{path}/ScorpioFastReflect/src", $"{packagePath}/Editor/ScorpioFastReflect", new[] { "*.cs" }, true);
        FileUtil.CopyFile($"{path}/README.md", $"{packagePath}/Documentation~/index.md", true);
        FileUtil.CopyFile($"{path}/README.md", $"{packagePath}/README.md", true);
        FileUtil.CopyFile($"{path}/ReleaseNotes.md", $"{packagePath}/CHANGELOG.md", true);
        FileUtil.CopyFile($"{path}/LICENSE", $"{packagePath}/LICENSE.md", true);
        AssetDatabase.Refresh();
        var file = $"{packagePath}/package.json";
        var package = (JObject)JsonConvert.DeserializeObject(FileUtil.GetFileString(file));
        package["version"] = version;
        FileUtil.CreateFile(file, JsonConvert.SerializeObject(package, Formatting.Indented));
    }
    static void SyncScov(string version) {
        var path = GetTempPath();
        StartProcess("git", $"clone -b v{version} https://github.com/qingfeng346/ScorpioConversion.git {path}");
        var packagePath = "Packages/com.scorpio.conversion.runtime";
        FileUtil.SyncFolder($"{path}/ScorpioProto/CSharp/Scorpio.Conversion.Runtime/src",     $"{packagePath}/Runtime/", new[] { "*.cs" }, true);
        FileUtil.CopyFile($"{path}/README.md",                                                $"{packagePath}/Documentation~/index.md", true);
        FileUtil.CopyFile($"{path}/README.md",                                                $"{packagePath}/README.md", true);
        AssetDatabase.Refresh();
        var file = $"{packagePath}/package.json";
        var package = (JObject)JsonConvert.DeserializeObject(FileUtil.GetFileString(file));
        package["version"] = version;
        FileUtil.CreateFile(file, JsonConvert.SerializeObject(package, Formatting.Indented));
    }
    static void Start() {
        //EditorUtil.Start("pull.bat");
        var result = EditorUtil.ExecutePowershell("pull.ps1");
        UnityEngine.Debug.Log(JsonConvert.SerializeObject(result));
    }
}
