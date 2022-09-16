using Scorpio.Resource;
using Scorpio.Resource.Editor;
using Scorpio.Unity.Command;
using UnityEditor;
[InitializeOnLoad]
public class ResourceCommand {
    static ResourceCommand() {
        CommandBuild.AddCommand("BuildBlueprints", BuildBlueprints);
        CommandBuild.AddCommand("BuildAssetBundles", BuildAssetBundles);
        CommandBuild.AddCommand<string[]>("BuildPatch", BuildPatch);
    }
    static void BuildBlueprints() {
        var builder = new AssetBundleBuilder<FileList>(AssetDatabase.LoadAssetAtPath<BuilderSetting>("Assets/Commands/BuilderSetting.asset"));
        builder.BuildBlueprints();
    }
    static void BuildAssetBundles() {
        var builder = new AssetBundleBuilder<FileList>(AssetDatabase.LoadAssetAtPath<BuilderSetting>("Assets/Commands/BuilderSetting.asset"));
        builder.GetAssetBundlesOutput = () => {
            return $"{builder.BuilderSetting.OutputExport}/Android/assetbundles";
        };
        builder.GetPatchesOutput = () => {
            return $"{builder.BuilderSetting.OutputExport}/Android/patches";
        };
        builder.BuildMainAssetBundle();
        builder.SyncBlueprintsToOutput();
        builder.SyncAssetBundlesToOutput();
        builder.SyncPatchesToOutput();
        builder.SyncToStreaming();
    }
    static void BuildPatch(string[] names) {
        var builder = new AssetBundleBuilder<FileList>(AssetDatabase.LoadAssetAtPath<BuilderSetting>("Assets/Commands/BuilderSetting.asset"));
        builder.GetPatchUUID = (patchName) => {
            return "";
        };
        foreach (var name in names) {
            builder.BuildPatch(name);
        }
    }
}