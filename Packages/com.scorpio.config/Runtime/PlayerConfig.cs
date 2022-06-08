using UnityEngine;
//玩家单独配置 保存在 PlayerPrefs
namespace Scorpio.Config {
    public static class PlayerConfig {
        public static string PlayerId { get; set; }
        private static string GetPlayerKey(string key) {
            return string.Format("{0}_{1}", PlayerId, key);
        }
        public static void SetString(string key, string value) {
            PlayerPrefs.SetString(GetPlayerKey(key), value);
            PlayerPrefs.Save();
        }
        public static void SetInt(string key, int value) {
            PlayerPrefs.SetInt(GetPlayerKey(key), value);
            PlayerPrefs.Save();
        }
        public static void SetFloat(string key, float value) {
            PlayerPrefs.SetFloat(GetPlayerKey(key), value);
            PlayerPrefs.Save();
        }
        public static void SetBool(string key, bool value) {
            PlayerPrefs.SetInt(GetPlayerKey(key), value ? 1 : 0);
            PlayerPrefs.Save();
        }

        public static string GetString(string key) {
            return PlayerPrefs.GetString(GetPlayerKey(key));
        }
        public static string GetString(string key, string defaultValue) {
            return PlayerPrefs.GetString(GetPlayerKey(key), defaultValue);
        }
        public static int GetInt(string key) {
            return PlayerPrefs.GetInt(GetPlayerKey(key));
        }
        public static int GetInt(string key, int defaultValue) {
            return PlayerPrefs.GetInt(GetPlayerKey(key), defaultValue);
        }
        public static float GetFloat(string key) {
            return PlayerPrefs.GetFloat(GetPlayerKey(key));
        }
        public static float GetFloat(string key, float defaultValue) {
            return PlayerPrefs.GetFloat(GetPlayerKey(key), defaultValue);
        }
        public static bool GetBool(string key) {
            return PlayerPrefs.GetInt(GetPlayerKey(key), 0) == 1;
        }
        public static bool GetBool(string key, bool defaultValue) {
            return PlayerPrefs.GetInt(GetPlayerKey(key), defaultValue ? 1 : 0) == 1;
        }
        public static void DeleteKey(string key) {
            PlayerPrefs.DeleteKey(GetPlayerKey(key));
            PlayerPrefs.Save();
        }
        public static bool HasKey(string key) {
            return PlayerPrefs.HasKey(GetPlayerKey(key));
        }
    }
}
