using UnityEngine;
using System.Text;
namespace Scorpio.Config {
    //保存在Resource 的只读配置
    public class GameConfig {
        static GameConfig() {
            var text = Resources.Load("GameConfig") as TextAsset;
            if (text != null) {
                Config = new StorageConfig();
                Config.InitFormBuffer(text.bytes, Encoding.UTF8);
            } else {
                Config = new StorageConfig();
            }
        }
        private static string internalDataPath = null;
        public static string InternalDataPath {
            get {
                if (string.IsNullOrEmpty(internalDataPath)) {
#if UNITY_ANDROID && !UNITY_EDITOR
                    using (var unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer")) {
                        using (var currentActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity")) {
                            internalDataPath = currentActivity.Call<AndroidJavaObject>("getFilesDir").Call<string>("getAbsolutePath");
                        }
                    }
#else
                    internalDataPath = Application.persistentDataPath;
#endif
                }
                return internalDataPath;
            }
        }
        public static StorageConfig Config { get; private set; }

        public static float GetFloat(string key) { return Config.GetFloat(key); }
        public static float GetFloat(string key, float defaultValue) { return Config.GetFloat(key, defaultValue); }

        public static int GetInt(string key) { return Config.GetInt(key); }
        public static int GetInt(string key, int defaultValue) { return Config.GetInt(key, defaultValue); }

        public static bool GetBool(string key) { return Config.GetBool(key); }
        public static bool GetBool(string key, bool defaultValue) { return Config.GetBool(key, defaultValue); }

        public static string GetString(string key) { return Config.GetString(key); }
        public static string GetString(string key, string defaultValue) { return Config.GetString(key, defaultValue); }

        public static bool HasKey(string key) { return Config.HasKey(key); }

        public static string Get(string key) {
            return GetString(key);
        }
        public static string Get(string key, string def) {
            return GetString(key, def);
        }
#if UNITY_EDITOR
        public static void Set(string key, string value) {
            Config.SetString(key, value);
            Save();
        }
        public static void Save() {
            var resourcePath = Application.dataPath + "/Resources/";
            if (!System.IO.Directory.Exists(resourcePath)) System.IO.Directory.CreateDirectory(resourcePath);
            System.IO.File.WriteAllBytes(resourcePath + "GameConfig.txt", Encoding.UTF8.GetBytes(Config.BuilderString()));
        }
#endif
    }
}
