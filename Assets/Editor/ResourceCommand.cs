using Scorpio.Resource;
using Scorpio.Resource.Editor;
using Scorpio.Unity.Command;
using UnityEditor;
[InitializeOnLoad]
public class ResourceCommand {
    static ResourceCommand() {
        CommandBuild.AddCommand("BuildBlueprints", BuildBlueprints);
    }
    static void BuildBlueprints() {
        var builder = new AssetBundleBuilder<FileList>(AssetDatabase.LoadAssetAtPath<BuilderSetting>("Assets/Commands/BuilderSetting.asset"));
        builder.BuildBlueprints(null);
    }
}