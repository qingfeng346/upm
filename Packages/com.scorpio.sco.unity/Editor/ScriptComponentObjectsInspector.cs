using UnityEditor;
using Scorpio.Unity;
using UnityEngine;
[CustomEditor(typeof(ScriptComponentObjects))]
public class ScriptComponentObjectsInspector : Editor {
    public override void OnInspectorGUI() {
        var com = target as ScriptComponentObjects;
        if (GUILayout.Button("���¼���")) {
            com.Parse();
            EditorUtility.SetDirty(target);
        }
        EditorGUILayout.HelpBox("���е�λ", MessageType.Info);
        foreach (var obj in com.values) {
            if (obj.value != null) {
                GUILayout.BeginHorizontal();
                EditorGUILayout.ObjectField(obj.value, obj.value.GetType(), true);
                GUILayout.TextField(obj.name, GUILayout.Width(150));
                GUILayout.EndHorizontal();
            }
        }
        EditorGUILayout.HelpBox("����Button", MessageType.Info);
        foreach (var obj in com.clicks) {
            if (obj.button != null) {
                GUILayout.BeginHorizontal();
                EditorGUILayout.ObjectField(obj.button, obj.button.GetType(), true);
                GUILayout.TextField(obj.click, GUILayout.Width(150));
                GUILayout.EndHorizontal();
            }
        }
    }

    
}
