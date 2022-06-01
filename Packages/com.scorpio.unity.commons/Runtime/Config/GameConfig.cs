using UnityEngine;
using System.Text;
using Scorpio.Ini;
namespace Scorpio.Config {
    //保存在Resource 的只读配置
    public class GameConfig {
        static GameConfig() {
            var text = Resources.Load("GameConfig") as TextAsset;
            if (text != null) {
                Config = new ScorpioIni(text.bytes, Encoding.UTF8);
            } else {
                Config = new ScorpioIni();
            }
        }
        public static ScorpioIni Config { get; private set; }
        public static string Get(string key) {
            return Get(key, null);
        }
        public static string Get(string key, string def) {
            return Config.GetDef(key, def);
        }
#if UNITY_EDITOR
        public static void Set(string key, string value) {
            Config.Set(key, value);
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
