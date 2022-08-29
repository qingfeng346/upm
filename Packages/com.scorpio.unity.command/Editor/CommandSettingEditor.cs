using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
namespace Scorpio.Unity.Command {
    [CustomEditor(typeof(CommandSetting))]
    public class CommandSettingEditor : Editor {
        public override void OnInspectorGUI() {
            base.OnInspectorGUI();
            if (GUILayout.Button("模拟运行")) {
                var setting = target as CommandSetting;
                var args = new List<string>();
                args.Add("--args");
                args.Add("-executeType");
                args.Add(setting.command);
                args.Add("-result");
                args.Add(setting.result);
                args.Add("-time");
                args.Add(setting.time);
                args.AddRange(setting.args.Split(' '));
                args.Remove(null);
                CommandBuild.ExecuteWithArgs(args.ToArray());
            }
        }
    }
}
