using System;
using System.Diagnostics;
using System.Text;
using UnityEditor;

public static partial class EditorUtil {
    public static BuildTarget BuildTarget {
        get {
#if UNITY_ANDROID
            return BuildTarget.Android;
#elif UNITY_IOS
            return BuildTarget.iOS;
#elif UNITY_WEBGL
            return BuildTarget.WebGL;
#elif UNITY_UWP || UNITY_WSA
            return BuildTarget.WSAPlayer;
#elif UNITY_STANDALONE_WIN
            return BuildTarget.StandaloneWindows;
#elif UNITY_STANDALONE_OSX
            return BuildTarget.StandaloneOSX;
#elif UNITY_STANDALONE_LINUX
            return BuildTarget.StandaloneLinuxUniversal;
#else
            return BuildTarget.StandaloneWindows;
#endif
        }
    }
    public static BuildTargetGroup BuildTargetGroup {
        get {
#if UNITY_ANDROID
            return BuildTargetGroup.Android;
#elif UNITY_IOS
            return BuildTargetGroup.iOS;
#elif UNITY_WEBGL
            return BuildTargetGroup.WebGL;
#elif UNITY_UWP || UNITY_WSA
            return BuildTargetGroup.WSA;
#else
            return BuildTargetGroup.Standalone;
#endif
        }
    }
    public static string GetBuildID(this DateTime time) {
        return time.ToString("yyyyMMdd-HHmmss");
    }
    //public static bool StartPowershell(string fileName, string workingDirectory, string arguments) {
    //    return StartPowershell(fileName, workingDirectory, arguments, out var output);
    //}
    //public static bool StartPowershell(string fileName, string workingDirectory, string arguments, out string output) {
    //    //#if UNITY_EDITOR_WIN
    //    return StartProcess("pwsh", workingDirectory, $"-ExecutionPolicy Unrestricted {fileName} {arguments}", out output);
    //    //#else
    //    //        return StartProcess("pwsh", workingDirectory, $"-ExecutionPolicy Unrestricted {fileName} {arguments}", out output);
    //    //#endif
    //}
    static void Start(string fileName, string workingDirectory, string arguments, Action<Process> preStart, Action<Process> postStart) {
        using (var process = new Process()) {
            process.StartInfo.FileName = fileName;
            process.StartInfo.CreateNoWindow = true;
            process.StartInfo.UseShellExecute = true;
            process.StartInfo.WindowStyle = ProcessWindowStyle.Normal;
            if (!string.IsNullOrEmpty(workingDirectory)) {
                process.StartInfo.WorkingDirectory = workingDirectory;
            }
            if (!string.IsNullOrEmpty(arguments)) {
                process.StartInfo.Arguments = arguments;
            }
            preStart?.Invoke(process);
            process.Start();
            postStart?.Invoke(process);
        }
    }
    public static void Start(string fileName, string workingDirectory, string arguments) {
        Start(fileName, workingDirectory, arguments, null, null);
    }
}
