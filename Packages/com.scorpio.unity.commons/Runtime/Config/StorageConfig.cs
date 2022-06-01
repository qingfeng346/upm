using System.IO;
using Scorpio.Ini;
using System.Text;

public class StorageConfig : ScorpioIni {
    public StorageConfig(string file) {
        InitFormFile(file, Encoding.UTF8);
    }
    public float GetFloat(string key) { return GetFloat(key, 0); }
    public float GetFloat(string key, float defaultValue) { return GetFloat(DefaultSection, key, defaultValue); }
    public float GetFloat(string section, string key) { return GetFloat(section, key, 0); }
    public float GetFloat(string section, string key, float defaultValue) {
        if (Has(section, key)) {
            float ret;
            if (float.TryParse(Get(section, key), out ret)) {
                return ret;
            }
            return defaultValue;
        } else {
            return defaultValue;
        }
    }

    public int GetInt(string key) { return GetInt(key, 0); }
    public int GetInt(string key, int defaultValue) { return GetInt(DefaultSection, key, defaultValue); }
    public int GetInt(string section, string key) { return GetInt(section, key, 0); }
    public int GetInt(string section, string key, int defaultValue) {
        if (Has(section, key)) {
            int ret;
            if (int.TryParse(Get(section, key), out ret)) {
                return ret;
            }
            return defaultValue;
        } else {
            return defaultValue;
        }
    }
    
    public bool GetBool(string key) { return GetBool(key, false); }
    public bool GetBool(string key, bool defaultValue) { return GetBool(DefaultSection, key, defaultValue); }
    public bool GetBool(string section, string key) { return GetBool(section, key, false); }
    public bool GetBool(string section, string key, bool defaultValue) {
        if (Has(section, key)) {
            bool ret;
            if (bool.TryParse(Get(section, key), out ret)) {
                return ret;
            }
            return defaultValue;
        } else {
            return defaultValue;
        }
    }

    public string GetString(string key) { return GetString(key, ""); }
    public string GetString(string key, string defaultValue) {
        return GetStringSection(DefaultSection, key, defaultValue);
    }
    public string GetStringSection(string section, string key) {
        return GetStringSection(section, key, "");
    }
    public string GetStringSection(string section, string key, string defaultValue) {
        return Has(section, key) ? Get(section, key) : defaultValue;
    }
    
    public bool HasKey(string key) {
        return Has(key);
    }
    public bool HasKey(string section, string key) {
        return Has(section, key);
    }

    public void SetFloat(string key, float value) {
        SetString(key, value.ToString());
    }
    public void SetFloat(string section, string key, float value) {
        SetString(section, key, value.ToString());
    }
    
    public void SetInt(string key, int value) {
        SetString(key, value.ToString());
    }
    public void SetInt(string section, string key, int value) {
        SetString(section, key, value.ToString());
    }

    public void SetBool(string key, bool value) {
        SetString(key, value.ToString());
    }
    public void SetBool(string section, string key, bool value) {
        SetString(section, key, value.ToString());
    }
    
    public void SetString(string key, string value) {
        SetString(DefaultSection, key, value);
    }
    public void SetString(string section, string key, string value) {
        Set(section, key, value);
        SaveToFile();
    }
    
    public void DeleteKey(string key) {
        DeleteKey(DefaultSection, key);
    }
    public void DeleteKey(string section, string key) {
        Remove(section, key);
        SaveToFile();
    }
    
    public void DeleteSection(string section) {
        RemoveSection(section);
        SaveToFile();
    }
    public void DeleteAll() {
        Clear();
        SaveToFile();
    }
}
