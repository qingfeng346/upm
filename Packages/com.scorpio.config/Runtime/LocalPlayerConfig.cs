using UnityEngine;
using System.IO;
namespace Scorpio.Config {
    public class LocalPlayerConfig {
        public static readonly string ConfigPath = Application.persistentDataPath + "/PlayerConfig/";
        public static void Initialize(string playerId) {
            PlayerId = playerId;
            if (!Directory.Exists(ConfigPath)) { Directory.CreateDirectory(ConfigPath); }
            Config = new StorageConfig();
            Config.InitFormFile($"{ConfigPath}/{playerId}.ini", System.Text.Encoding.UTF8);
        }
        public static StorageConfig Config { get; private set; }
        public static string PlayerId { get; private set; }

        public static float GetFloat(string key) { return Config.GetFloat(key); }
        public static float GetFloat(string key, float defaultValue) { return Config.GetFloat(key, defaultValue); }

        public static int GetInt(string key) { return Config.GetInt(key); }
        public static int GetInt(string key, int defaultValue) { return Config.GetInt(key, defaultValue); }

        public static bool GetBool(string key) { return Config.GetBool(key); }
        public static bool GetBool(string key, bool defaultValue) { return Config.GetBool(key, defaultValue); }

        public static string GetString(string key) { return Config.GetString(key); }
        public static string GetString(string key, string defaultValue) { return Config.GetString(key, defaultValue); }

        public static bool HasKey(string key) { return Config.HasKey(key); }

        public static void SetFloat(string key, float value) { Config.SetFloat(key, value); }
        public static void SetInt(string key, int value) { Config.SetInt(key, value); }
        public static void SetBool(string key, bool value) { Config.SetBool(key, value); }
        public static void SetString(string key, string value) { Config.SetString(key, value); }
        public static void DeleteKey(string key) { Config.DeleteKey(key); }
        public static void DeleteAll() { Config.DeleteSection(PlayerId); }
    }
}
