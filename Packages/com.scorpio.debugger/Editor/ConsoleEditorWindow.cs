using UnityEngine;
using UnityEditor;

namespace Scorpio.Debugger.Editor {
    public class ConsoleEditorWindow : EditorWindow {
        [MenuItem("Debugger/DebuggerConsole")]
        static void Open() {
            GetWindow<ConsoleEditorWindow>("DebuggerConsole").Show();
        }
        private string text = "";
        private bool historyFoldout = true;

        private bool commandFoldout = true;
        private Vector2 commandScroll = Vector2.zero;

        void OnGUI() {
            var e = Event.current;
            if (e.type == EventType.KeyDown) {
                switch (e.keyCode) {
                    case KeyCode.Return:
                    case KeyCode.KeypadEnter:
                        ScorpioDebugger.Instance.ExecuteCommand(text);
                        text = "";
                        Repaint();
                        break;
                    case KeyCode.UpArrow:
                        text = ScorpioDebugger.Instance.LastCommand;
                        Repaint();
                        break;
                    case KeyCode.DownArrow:
                        text = ScorpioDebugger.Instance.NextCommand;
                        Repaint();
                        break;
                }
            }
            GUILayout.Label("当前命令");
            var newText = GUILayout.TextField(text);
            if (newText != text) {
                text = newText;
            }
            if (historyFoldout = EditorGUILayout.Foldout(historyFoldout, "历史记录", true)) {
                var commands = ScorpioDebugger.Instance.CommandHistory.commands;
                for (var i = commands.Count - 1; i >= 0; i--) {
                    GUILayout.BeginHorizontal();
                    if (GUILayout.Button(commands[i])) {
                        text = commands[i];
                    }
                    if (GUILayout.Button("执行", GUILayout.Width(100))) {
                        ScorpioDebugger.Instance.ExecuteCommand(commands[i]);
                    }
                    GUILayout.EndHorizontal();
                }
            }
            if (commandFoldout = EditorGUILayout.Foldout(commandFoldout, "命令列表", true)) {
                commandScroll = GUILayout.BeginScrollView(commandScroll);
                foreach (var command in ScorpioDebugger.Instance.CommandEntries) {
                    if (GUILayout.Button($"{command.labelCN}({command.labelParam})")) {
                        text = command.command;
                    }
                }
                GUILayout.EndScrollView();
            }
        }
    }
}
