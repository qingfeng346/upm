using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.IO;
namespace Scorpio.Unity.Command {
    public class CommandRecord : ScriptableObject {
        public static CommandRecord GetInstance(string assetPath) {
            var instance = AssetDatabase.LoadAssetAtPath<CommandRecord>(assetPath);
            if (instance == null) {
                var path = Path.GetDirectoryName(assetPath);
                if (!Directory.Exists(path)) {
                    Directory.CreateDirectory(path);
                    AssetDatabase.Refresh();
                }
                instance = CreateInstance<CommandRecord>();
                AssetDatabase.CreateAsset(instance, assetPath);
            }
            return instance;
        }
        [System.Serializable]
        public class Record {
            public string name;
            public string time;
        }
        public List<Record> records = new List<Record>();
        public void UpdateCommandTime(string name, string time) {
            var data = records.Find(_ => _.name == name);
            if (data == null) {
                records.Add(data = new Record() { name = name });
            }
            data.time = time;
            EditorUtility.SetDirty(this);
        }
    }
}