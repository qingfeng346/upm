using UnityEngine;
using System.IO;
namespace Scorpio.Config {
    public class LocalGlobalConfig {
        public static string ConfigFile = GameConfig.InternalDataPath + "/LocalConfig.ini";
        public static void Initialize() {
            Config = new StorageConfig();
            Config.InitFormFile(ConfigFile, System.Text.Encoding.UTF8);
        }
        public static StorageConfig Config { get; private set; }

        public static float GetFloat(string key) { return Config?.GetFloat(key) ?? 0; }
        public static float GetFloat(string key, float defaultValue) { return Config?.GetFloat(key, defaultValue) ?? defaultValue; }

        public static int GetInt(string key) { return Config?.GetInt(key) ?? 0; }
        public static int GetInt(string key, int defaultValue) { return Config?.GetInt(key, defaultValue) ?? defaultValue; }

        public static bool GetBool(string key) { return Config?.GetBool(key) ?? false; }
        public static bool GetBool(string key, bool defaultValue) { return Config?.GetBool(key, defaultValue) ?? defaultValue; }

        public static string GetString(string key) { return Config?.GetString(key) ?? ""; }
        public static string GetString(string key, string defaultValue) { return Config?.GetString(key, defaultValue) ?? defaultValue; }

        public static bool HasKey(string key) { return Config?.HasKey(key) ?? false; }

        public static void SetFloat(string key, float value) { Config?.SetFloat(key, value); }
        public static void SetInt(string key, int value) { Config?.SetInt(key, value); }
        public static void SetBool(string key, bool value) { Config?.SetBool(key, value); }
        public static void SetString(string key, string value) { Config?.SetString(key, value); }
        public static void DeleteKey(string key) { Config?.DeleteKey(key); }
        public static void DeleteAll() { Config?.DeleteAll(); }
        public static void DeleteFile() {
            if (Config == null) { return; }
            if (File.Exists(Config.File)) { File.Delete(Config.File); }
            Config = null;
        }
    }
}
