using UnityEditor;
namespace Scorpio.Unity.Editor {
    [CustomEditor(typeof(ScriptComponent))]
    public class ScriptComponentInspector : UnityEditor.Editor {
        private ScriptInstanceDrawer drawer = new ScriptInstanceDrawer();
        public override void OnInspectorGUI() {
            var com = target as ScriptComponent;
            EditorGUILayout.TextField("Name", com.Name);
            var table = (target as ScriptComponent).Table;
            if (table == null) { return; }
            drawer.SetObject(table);
            drawer.Draw();
        }
    }
}
