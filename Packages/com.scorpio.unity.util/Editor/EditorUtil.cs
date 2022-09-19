using System;
using System.Diagnostics;
using System.Text;
using UnityEngine;
using UnityEditor;

public static partial class EditorUtil {
    public class ProcessResult {
        public int exitCode;
        public string output;
        public string error;
        public ProcessResult(Process process) {
            exitCode = process.ExitCode;
            if (process.StartInfo.RedirectStandardOutput) {
                output = process.StandardOutput.ReadToEnd();
            }
            if (process.StartInfo.RedirectStandardError) {
                error = process.StandardError.ReadToEnd();
            }
        }
    }
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
    static void StartProcess(string fileName, string workingDirectory, string arguments, Action<Process> preStart, Action<Process> postStart) {
        using (var process = new Process()) {
            process.StartInfo.FileName = fileName;
            if (!string.IsNullOrEmpty(workingDirectory)) {
                process.StartInfo.WorkingDirectory = workingDirectory;
            }
            if (!string.IsNullOrEmpty(arguments)) {
                process.StartInfo.Arguments = arguments;
            }
            process.StartInfo.CreateNoWindow = true;
            process.StartInfo.UseShellExecute = true;
            process.StartInfo.WindowStyle = ProcessWindowStyle.Normal;
            preStart?.Invoke(process);
            process.Start();
            postStart?.Invoke(process);
        }
    }
    public static void Start(string fileName, string workingDirectory = null, string arguments = null) {
        StartProcess(fileName, workingDirectory, arguments, null, null);
    }
    public static void StartPowershell(string fileName, string workingDirectory = null, string arguments = null) {
        Start("pwsh", workingDirectory, $"-ExecutionPolicy Unrestricted {fileName} {arguments}");
    }
    public static void StartShell(string fileName, string workingDirectory = null, string arguments = null) {
        Start("sh", workingDirectory, $"{fileName} {arguments}");
    }
    public static ProcessResult Execute(string fileName, string workingDirectory = null, string arguments = null, bool showWindow = false) {
        ProcessResult processResult = null;
        StartProcess(fileName, workingDirectory, arguments, (process) => {
            if (showWindow) {
                process.StartInfo.CreateNoWindow = false;
                process.StartInfo.UseShellExecute = true;
            } else {
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.RedirectStandardError = true;
#if UNITY_EDITOR_WIN
                process.StartInfo.StandardOutputEncoding = Encoding.GetEncoding("GBK");
                process.StartInfo.StandardErrorEncoding = Encoding.GetEncoding("GBK");
#endif
            }
            process.EnableRaisingEvents = true;
        }, (process) => {
            process.WaitForExit();
            processResult = new ProcessResult(process);
        });
        return processResult;
    }
    public static ProcessResult ExecutePowershell(string fileName, string workingDirectory = null, string arguments = null, bool showWindow = false) {
        return Execute("pwsh", workingDirectory, $"-ExecutionPolicy Unrestricted {fileName} {arguments}", showWindow);
    }
    public static ProcessResult ExecuteShell(string fileName, string workingDirectory = null, string arguments = null) {
        return Execute("sh", workingDirectory, $"{fileName} {arguments}", false);
    }
    public static bool MessageBox(string title, string message, string ok, string cancel = "") {
        return EditorUtility.DisplayDialog(title, message, ok, cancel);
    }
    public static bool MessageBox(string message) {
        return EditorUtility.DisplayDialog("警告", message, "确定", "取消");
    }
    public static void ShowProgressBar(int i, int length) {
        ShowProgressBar("进度", "", i, length);
    }
    public static void ShowProgressBar(string message, int i, int length) {
        ShowProgressBar("进度", message, i, length);
    }
    public static void ShowProgressBar(string title, string message, int i, int length) {
        float index = i + 1;
        EditorUtility.DisplayProgressBar($"{title}:({index}/{length}) {Mathf.Floor(index / length * 100)}%", message, index / length);
    }
    public static void SetValue(this object assetImporter, string name, object value, ref bool changed) {
        var type = assetImporter.GetType();
        var propertyInfo = type.GetProperty(name);
        if (propertyInfo == null) {
            UnityEngine.Debug.LogError($"{type} 找不到属性 {name}");
            return;
        }
        if (!propertyInfo.GetValue(assetImporter).Equals(value)) {
            propertyInfo.SetValue(assetImporter, value);
            changed = true;
        }
    }
    //获取4的倍数的图片
    public static bool ChangeTextureSizeToMultipleOf4(this Texture2D texture, SpriteAlignment alignment, out Texture2D output, out bool transparent) {
        output = texture;
        transparent = true;
        var oldSize = new Vector2Int(texture.width, texture.height);
        var newSize = oldSize;
        bool isChange = false;
        var modWigth = oldSize.x % 4;
        var modHigh = oldSize.y % 4;
        if (modWigth != 0) {
            isChange = true;
            newSize.x = oldSize.x + 4 - modWigth;
        }
        if (modHigh != 0) {
            isChange = true;
            newSize.y = oldSize.y + 4 - modHigh;
        }
        if (!isChange) return false;
        transparent = HasTransparent(texture);
        var background = transparent ? Color.clear : Color.black;
        output = new Texture2D(newSize.x, newSize.y, TextureFormat.ARGB32, false);
        if (alignment == SpriteAlignment.Center) {
            CopyTexture(texture, output, background, (newSize.x - oldSize.x) / 2, (newSize.y - oldSize.y) / 2);
        } else if (alignment == SpriteAlignment.BottomCenter) {
            CopyTexture(texture, output, background, (newSize.x - oldSize.x) / 2, newSize.y - oldSize.y);
        } else if (alignment == SpriteAlignment.RightCenter) {
            CopyTexture(texture, output, background, newSize.x - oldSize.x, (newSize.y - oldSize.y) / 2);
        } else if (alignment == SpriteAlignment.LeftCenter) {
            CopyTexture(texture, output, background, 0, (newSize.y - oldSize.y) / 2);
        } else {
            throw new Exception($"不支持Pivot:{alignment} 的图片自动补充");
        }
        return true;
    }
    static bool HasTransparent(this Texture2D texture) {
        for (int x = 0; x < texture.width; x++) {
            for (int y = 0; y < texture.height; y++) {
                if (texture.GetPixel(x, y).a == 0) {
                    return true;
                }
            }
        }
        return false;
    }
    static void CopyTexture(Texture2D rescTexture, Texture2D descTexture, Color background, int startX, int startY) {
        descTexture.Fill(background);
        for (int x = 0; x < rescTexture.width; x++) {
            for (int y = 0; y < rescTexture.height; y++) {
                descTexture.SetPixel(x + startX, y + startY, rescTexture.GetPixel(x, y));
            }
        }
    }
    //填充颜色
    static void Fill(this Texture2D texture, Color color) {
        for (int x = 0; x < texture.width; x++) {
            for (int y = 0; y < texture.height; y++) {
                texture.SetPixel(x, y, color);
            }
        }
    }
}
