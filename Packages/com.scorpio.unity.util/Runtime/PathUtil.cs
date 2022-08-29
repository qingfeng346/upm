using UnityEngine;
namespace Scorpio.Unity.Util {
    public static class PathUtil {
        public static readonly string PersistentDataPath = Application.persistentDataPath + "/";
        private static string _InternalDataPath = null;
        public static string InternalDataPath {
            get {
#if UNITY_ANDROID && !UNITY_EDITOR
                if (string.IsNullOrEmpty(_InternalDataPath)) {
                    using (AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer")) {
                        using (AndroidJavaObject currentActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity")) {
                            _InternalDataPath = currentActivity.Call<AndroidJavaObject>("getFilesDir").Call<string>("getAbsolutePath");
                        }
                    }
                }
#else
                if (string.IsNullOrEmpty(_InternalDataPath)) {
                    _InternalDataPath = PersistentDataPath;
                }
#endif
                return _InternalDataPath;
            }
        }
    }
}

