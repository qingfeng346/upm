//Editor 下使用的函数 
using System.Diagnostics;
using System.IO;
using System.Text;
using UnityEngine;
using Scorpio.Unity.Commons;
public static partial class EngineUtil {
#if UNITY_EDITOR
    public static void HideInHierarchy (this UnityEngine.Object obj, string str) {
        var gameObject = string.IsNullOrEmpty (str) ? EngineUtil.GetGameObject (obj) : EngineUtil.FindChild (obj, str);
        gameObject.hideFlags |= HideFlags.HideInHierarchy;
    }
    public static void ShowInHierarchy (this UnityEngine.Object obj, string str) {
        var gameObject = string.IsNullOrEmpty (str) ? EngineUtil.GetGameObject (obj) : EngineUtil.FindChild (obj, str);
        if ((gameObject.hideFlags & HideFlags.HideInHierarchy) != 0) {
            gameObject.hideFlags ^= HideFlags.HideInHierarchy;
        }
    }
    public static void SetInHierarchy (this UnityEngine.Object obj, bool visiable, string str) {
        if (visiable) {
            ShowInHierarchy (obj, str);
        } else {
            HideInHierarchy (obj, str);
        }
    }
#endif
}