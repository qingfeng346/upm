using System;
using System.Collections.Generic;
using UnityEngine;
namespace Scorpio.Debugger {
    [Serializable]
    public class CommandHistory {
        private string key;
        private int selected;
        [SerializeField]
        public List<string> commands;
        public CommandHistory(string key) {
            this.key = key;
            this.commands = JsonUtility.FromJson<CommandHistory>(PlayerPrefs.GetString(key, ""))?.commands ?? new List<string>();
            this.selected = commands.Count;
        }
        void Save() {
            PlayerPrefs.SetString(key, JsonUtility.ToJson(this));
            PlayerPrefs.Save();
        }
        public void AddHistory(string command) {
            var index = commands.IndexOf(command);
            if (index >= 0) { commands.RemoveAt(index); }
            commands.Add(command);
            if (commands.Count > ScorpioDebugger.MaxHistoryNumber) { commands.RemoveAt(0); }
            selected = commands.Count;
            Save();
        }
        public string Last() {
            if (commands.Count > 0) {
                selected = Mathf.Max(0, selected - 1);
                return commands[selected];
            }
            return "";
        }
        public string Next() {
            if (commands.Count > 0) {
                selected = Mathf.Min(commands.Count, selected + 1);
                return selected == commands.Count ? "" : commands[selected];
            }
            return "";
        }
    }
}
