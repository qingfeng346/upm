using UnityEngine;
using Scorpio.Unity.Logger;
using Scorpio.Unity.Commons;
namespace Scorpio.Config {
    public class LocalPlayerConfig {
        public static readonly string ConfigFile = Application.persistentDataPath + "/PlayerConfig.ini";
        static LocalPlayerConfig() {
            try {
                Initialize();
            } catch (System.Exception e) {
                FileUtil.DeleteFile(ConfigFile);
                Initialize();
                logger.error("LocalPlayerConfig init error : " + e.ToString());
            }
        }
        public static void Initialize() {
            Config = new StorageConfig(ConfigFile);
        }
        public static StorageConfig Config { get; private set; }
        public static string PlayerId { get; set; } = "";

        public static float GetFloat(string key) { return Config.GetFloat(PlayerId, key); }
        public static float GetFloat(string key, float defaultValue) { return Config.GetFloat(PlayerId, key, defaultValue); }

        public static int GetInt(string key) { return Config.GetInt(PlayerId, key); }
        public static int GetInt(string key, int defaultValue) { return Config.GetInt(PlayerId, key, defaultValue); }

        public static bool GetBool(string key) { return Config.GetBool(PlayerId, key); }
        public static bool GetBool(string key, bool defaultValue) { return Config.GetBool(PlayerId, key, defaultValue); }

        public static string GetString(string key) { return Config.GetStringSection(PlayerId, key); }
        public static string GetString(string key, string defaultValue) { return Config.GetStringSection(PlayerId, key, defaultValue); }

        public static bool HasKey(string key) { return Config.HasKey(PlayerId, key); }

        public static void SetFloat(string key, float value) { Config.SetFloat(PlayerId, key, value); }
        public static void SetInt(string key, int value) { Config.SetInt(PlayerId, key, value); }
        public static void SetBool(string key, bool value) { Config.SetBool(PlayerId, key, value); }
        public static void SetString(string key, string value) { Config.SetString(PlayerId, key, value); }
        public static void DeleteKey(string key) { Config.DeleteKey(PlayerId, key); }
        public static void DeleteAll() { Config.DeleteSection(PlayerId); }
    }
}
