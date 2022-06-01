using UnityEngine;
//全局配置 保存在 PlayerPrefs
public static class GlobalConfig {
    public static void SetString(string key, string value) {
        PlayerPrefs.SetString(key, value);
        PlayerPrefs.Save();
    }
    public static void SetInt(string key, int value) {
        PlayerPrefs.SetInt(key, value);
        PlayerPrefs.Save();
    }
    public static void SetFloat(string key, float value) {
        PlayerPrefs.SetFloat(key, value);
        PlayerPrefs.Save();
    }
    public static void SetBool(string key, bool value) {
        PlayerPrefs.SetInt(key, value ? 1 : 0);
        PlayerPrefs.Save();
    }

    public static string GetString(string key) {
        return PlayerPrefs.GetString(key);
    }
    public static string GetString(string key, string defaultValue) {
        return PlayerPrefs.GetString(key, defaultValue);
    }
    public static int GetInt(string key) {
        return PlayerPrefs.GetInt(key);
    }
    public static int GetInt(string key, int defaultValue) {
        return PlayerPrefs.GetInt(key, defaultValue);
    }
    public static float GetFloat(string key) {
        return PlayerPrefs.GetFloat(key);
    }
    public static float GetFloat(string key, float defaultValue) {
        return PlayerPrefs.GetFloat(key, defaultValue);
    }
    public static bool GetBool(string key) {
        return PlayerPrefs.GetInt(key, 0) == 1;
    }
    public static bool GetBool(string key, bool defaultValue) {
        return PlayerPrefs.GetInt(key, defaultValue ? 1 : 0) == 1;
    }
    public static void DeleteKey(string key) {
        PlayerPrefs.DeleteKey(key);
        PlayerPrefs.Save();
    }
    public static bool HasKey(string key) {
        return PlayerPrefs.HasKey(key);
    }
    public static void DeleteAll() {
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();
    }
}
