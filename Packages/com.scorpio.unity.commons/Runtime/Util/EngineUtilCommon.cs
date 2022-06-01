using System;
using System.Collections;
using System.Collections.Generic;
using System.IO.Compression;
using System.Security.Cryptography;
using System.Text;
using Scorpio.Config;
using UnityEngine;
using System.Globalization;
using System.Threading;
using Scorpio.Unity.Commons;

public static partial class EngineUtil {
    private const double KB_LENGTH = 1024; //1KB 的字节数
    private const double MB_LENGTH = 1048576; //1MB 的字节数
    private const double GB_LENGTH = 1073741824; //1GB 的字节数
    const string base32 = "23456789ABCDEFGHJKLMNPQRSTUVWXYZ";
    const int division = 32;

    public static int SortByName (Transform a, Transform b) { return string.Compare (a.name, b.name); } //transform 名字排序
    public static int ReverseSortByName (Transform a, Transform b) { return string.Compare (b.name, a.name); } //transform 名字倒序


    public static void InvariantCulture() {
        var culture = CultureInfo.InvariantCulture;
        CultureInfo.DefaultThreadCurrentCulture = culture;
        CultureInfo.DefaultThreadCurrentUICulture = culture;
        Thread.CurrentThread.CurrentCulture = culture;
        Thread.CurrentThread.CurrentUICulture = culture;
    }
    public static string GetMemory (long by) {
        if (by < MB_LENGTH)
            return string.Format ("{0:f2} KB", Convert.ToDouble (by) / KB_LENGTH);
        else if (by < GB_LENGTH)
            return string.Format ("{0:f2} MB", Convert.ToDouble (by) / MB_LENGTH);
        else
            return string.Format ("{0:f2} GB", Convert.ToDouble (by) / GB_LENGTH);
    }
    //是否是横屏
    public static bool IsLandscape { get; } = Screen.width >= Screen.height;
    //是否是竖屏
    public static bool IsPortrait { get; } = Screen.width <= Screen.height;
    //获取应用版本号
    public static string Version { get; } = Application.version;
    //应用编译版本
    public static string BuildVersion { get; } = GameConfig.Get("BuildID");
    //根据 versionName 算出来的一个数字类型的 version
    //数字位数 2 . 2 . 4
    public static int GetVersionNumber(string versionName) {
        var versions = versionName.Split('.');
        return int.Parse(versions[0]) * 1000000 + int.Parse(versions[1]) * 10000 + int.Parse(versions[2]);
    }
    //获得万能accessToken
    public static string GetSuperAccessToken() {
#if DRAGON_PRODUCT
        return "";
#else
        var now = (DateTime.UtcNow.Ticks - TimeUtil.BaseTime.Ticks) / 10000 / 60000;
        return FileUtil.GetMD5FromString($"{now}_dragon_adventure");
#endif
    }
    public static Vector2 GetCanvasSize () {
        var screen = new Vector2 (Screen.width, Screen.height);
        return new Vector2 (screen.x / screen.y * 750f, 750);
    }

    //transform 子节点排序
    public static void SortTransform (UnityEngine.Object obj) {
        SortTransform (obj, SortByName);
    }
    //transform 子节点排序
    public static void SortTransform (UnityEngine.Object obj, Comparison<Transform> comparison) {
        var transform = GetTransform (obj);
        if (transform == null) { return; }
        var list = new List<Transform> ();
        for (int i = 0; i < transform.childCount; ++i)
            list.Add (transform.GetChild (i));
        list.Sort (comparison);
        for (int i = 0; i < list.Count; ++i)
            list[i].SetAsLastSibling ();
    }
    //压缩文件夹
    public static void Compression (string sourceDirectoryName, string destinationArchiveFileName, bool includeBaseDirectory) {
        ZipFile.CreateFromDirectory (sourceDirectoryName, destinationArchiveFileName, System.IO.Compression.CompressionLevel.Optimal, includeBaseDirectory);
    }
    public static void SetObjectValue (UnityEngine.Object obj, object value) {
        if (value is int) {
            EngineUtil.GetComponent<ObjectValue> (obj).numberValue = (int) value;
        } else if (value is string) {
            EngineUtil.GetComponent<ObjectValue> (obj).stringValue = (string) value;
        } else {
            EngineUtil.GetComponent<ObjectValue> (obj).value = value;
        }
    }
    public static object GetObjectValue (UnityEngine.Object obj) {
        return EngineUtil.GetComponent<ObjectValue> (obj).value;
    }
    public static string GetObjectStringValue (UnityEngine.Object obj) {
        return EngineUtil.GetComponent<ObjectValue> (obj).stringValue;
    }
    public static int GetObjectNumberValue (UnityEngine.Object obj) {
        return EngineUtil.GetComponent<ObjectValue> (obj).numberValue;
    }
    public static string GenerateGUID () {
        return System.Convert.ToBase64String (System.Guid.NewGuid().ToByteArray ());
    }
    public static string ChangeLongToBase32 (long value) {
        var sb = new System.Text.StringBuilder (13);
        do {
            sb.Insert (0, base32[(byte) (value % division)]);
            value /= division;
        } while (value != 0);
        return sb.ToString ();
    }

    public static long ChangeBase32ToLong (string text) {
        long number = 0;
        foreach (char c in text.ToUpper ()) {
            var index = base32.IndexOf (c);
            if (index < 0) {
                return -1;
            }
            number = number * division + index;
        }
        return number;
    }

    // 获取移动设备上（Android/iOS）键盘区域高度
    private static AndroidJavaObject s_UnityPlayer = null;
    private static AndroidJavaObject s_AndroidRect = null;
    public static int GetKeyboardHeight(bool includeInput)
    {
#if UNITY_ANDROID
        if (s_UnityPlayer == null)
        {
            var unityClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            s_UnityPlayer = unityClass.GetStatic<AndroidJavaObject>("currentActivity").Get<AndroidJavaObject>("mUnityPlayer");
        }
        var view = s_UnityPlayer.Call<AndroidJavaObject>("getView");
        var dialog = s_UnityPlayer.Get<AndroidJavaObject>("b");

        if (view == null || dialog == null)
            return 0;

        var decorHeight = 0;
        if (includeInput)
        {
            var decorView = dialog.Call<AndroidJavaObject>("getWindow").Call<AndroidJavaObject>("getDecorView");
            if (decorView != null)
                decorHeight = decorView.Call<int>("getHeight");
        }

        if (s_AndroidRect == null)
        {
            s_AndroidRect = new AndroidJavaObject("android.graphics.Rect");
        }
        view.Call("getWindowVisibleDisplayFrame", s_AndroidRect);
        return Display.main.systemHeight - s_AndroidRect.Call<int>("height") + decorHeight;
#else
        var height = Mathf.RoundToInt(TouchScreenKeyboard.area.height);
        return height >= Display.main.systemHeight ? 0 : height;
#endif
    }
    static IEnumerator CoroutineLoop(Func<bool> condition, Action loop) {
        while (true) {
            if (!condition()) { yield break; }
            loop();
            yield return null;
        }
    }

    /// <summary> AES 算法加密 </summary>
    /// <param name="encrypt">数据</param>
    /// <param name="key">密钥</param>
    public static byte[] AesEncrypt(byte[] encrypt, byte[] key, byte[] iv) {
        RijndaelManaged rDel = new RijndaelManaged();
        rDel.Key = key;
        rDel.IV = iv;
        rDel.Mode = CipherMode.CBC;
        rDel.Padding = PaddingMode.PKCS7;
        ICryptoTransform cTransform = rDel.CreateEncryptor();
        return cTransform.TransformFinalBlock(encrypt, 0, encrypt.Length);
    }
    /// <summary> AES 算法解密 </summary>
    /// <param name="decrypt">数据</param>
    /// <param name="key">密钥</param>
    public static byte[] AesDecrypt(byte[] decrypt, byte[] key, byte[] iv) {
        RijndaelManaged rDel = new RijndaelManaged();
        rDel.Key = key;
        rDel.IV = iv;
        rDel.Mode = CipherMode.CBC;
        rDel.Padding = PaddingMode.PKCS7;
        ICryptoTransform cTransform = rDel.CreateDecryptor();
        return cTransform.TransformFinalBlock(decrypt, 0, decrypt.Length);
    }
    public static string ByteToString(byte[] buffer) {
        var builder = new StringBuilder();
        foreach (byte b in buffer)
            builder.Append(b.ToString("x2"));
        return builder.ToString();
    }

    // 设备震动
    public static void Vibrate() {
        if (SystemInfo.supportsVibration) {
            Handheld.Vibrate();
        }
    }
}