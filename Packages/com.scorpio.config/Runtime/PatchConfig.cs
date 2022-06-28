using UnityEngine;
using System.IO;
namespace Scorpio.Config {
    public class PatchConfig {
        public static string ConfigPath = Application.persistentDataPath + "/PatchConfig/";
        public PatchConfig(string patch) {
            Patch = patch;
            if (!Directory.Exists(ConfigPath)) { Directory.CreateDirectory(ConfigPath); }
            Config = new StorageConfig();
            Config.InitFormFile($"{ConfigPath}/{patch}.ini", System.Text.Encoding.UTF8);
        }
        public string Patch { get; private set; }
        public StorageConfig Config { get; private set; }

        public float GetFloat(string key) { return Config?.GetFloat(key) ?? 0; }
        public float GetFloat(string key, float defaultValue) { return Config?.GetFloat(key, defaultValue) ?? defaultValue; }

        public int GetInt(string key) { return Config?.GetInt(key) ?? 0; }
        public int GetInt(string key, int defaultValue) { return Config?.GetInt(key, defaultValue) ?? defaultValue; }

        public bool GetBool(string key) { return Config?.GetBool(key) ?? false; }
        public bool GetBool(string key, bool defaultValue) { return Config?.GetBool(key, defaultValue) ?? defaultValue; }

        public string GetString(string key) { return Config?.GetString(key) ?? ""; }
        public string GetString(string key, string defaultValue) { return Config?.GetString(key, defaultValue) ?? defaultValue; }

        public bool HasKey(string key) { return Config?.HasKey(key) ?? false; }

        public void SetFloat(string key, float value) { Config?.SetFloat(key, value); }
        public void SetInt(string key, int value) { Config?.SetInt(key, value); }
        public void SetBool(string key, bool value) { Config?.SetBool(key, value); }
        public void SetString(string key, string value) { Config?.SetString(key, value); }
        public void DeleteKey(string key) { Config?.DeleteKey(key); }
        public void DeleteAll() { Config?.DeleteAll(); }
        public void DeleteFile() {
            if (Config == null) { return; }
            if (File.Exists(Config.File)) { File.Delete(Config.File); }
            Config = null;
        }
    }
}
