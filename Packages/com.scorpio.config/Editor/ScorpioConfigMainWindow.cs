using UnityEngine;
using UnityEditor;

namespace Scorpio.Config {
	public enum ConfigType {
		Game,
		Local,
	}
	public class MainWindow : EditorWindow {
        private readonly GUILayoutOption ButtonWidth = GUILayout.Width(100);
        private readonly GUILayoutOption KeyWidth = GUILayout.Width(200);
        [MenuItem("Scorpio/Config")]
		public static void ShowMainWindow() {
			EditorWindow.GetWindow<MainWindow>("Config编辑器");
		}
		ConfigType configType = ConfigType.Game;
		string newKey = "", newValue = "";
		void OnEnable() {
			LocalGlobalConfig.Initialize();
        }
		void OnGUI() {
			GUILayout.BeginHorizontal(EditorStyles.toolbar);
			configType = (ConfigType)EditorGUILayout.EnumPopup(configType, EditorStyles.toolbarPopup);
			var config = configType == ConfigType.Game ? GameConfig.Config : LocalGlobalConfig.Config;
			if (GUILayout.Button("Delete All", EditorStyles.toolbarButton, ButtonWidth)) {
				if (EditorUtility.DisplayDialog ("提示", "确定全部删除,此操作不可恢复!", "确定", "取消")) {
					config.Clear();
					Save(configType);
				}
			}
            GUILayout.EndHorizontal();
			var section = config.GetSection();
			if (section != null) {
				foreach (var pair in section.Datas) {
					GUILayout.BeginHorizontal();
					GUILayout.TextField(pair.Key, KeyWidth);
					var str = GUILayout.TextField(pair.Value.value);
					if (str != pair.Value.value) {
						pair.Value.Set(str, "");
						Save(configType);
						return;
					}
					var color = GUI.color;
					GUI.color = Color.red;
					if (GUILayout.Button("Del", ButtonWidth)) {
						config.Remove(pair.Key);
						Save(configType);
						return;
					}
					GUILayout.EndHorizontal();
					GUI.color = color;
				}
			}
			GUILayout.BeginHorizontal();
			newKey = GUILayout.TextField(newKey, KeyWidth);
			newValue = GUILayout.TextField(newValue);
			if (GUILayout.Button("Add", ButtonWidth)) {
				if (!string.IsNullOrEmpty(newKey)) {
					config.Set(newKey, newValue);
					Save(configType);
				}
			}
			GUILayout.EndHorizontal();
		}
		void Save(ConfigType configType) {
			if (configType == ConfigType.Game) {
                GameConfig.Save();
				AssetDatabase.Refresh();
			} else {
                LocalGlobalConfig.Config.SaveToFile();
			}
		}
	}
}
