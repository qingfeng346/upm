using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Scorpio.Unity.Util;
using UnityEditor;
using UnityEngine;

using Object = UnityEngine.Object;
using FileUtil = Scorpio.Unity.Util.FileUtil;

public static class AssetBundleBuilder {
    public const string MainAssetBundlesPath = "Assets/AssetBundles/assetbundles/";
    public const string PatchAssetBundlesPath = "Assets/AssetBundles/patches/";
    public static Dictionary<string, HashSet<string>> AssetBundleAssets { get; private set; }       //所有AB文件
    public static Dictionary<string, string> AllAssets { get; private set; }                        //所有打进AB的资源文件
    public static Dictionary<string, HashSet<string>> CommonAssets { get; private set; }            //多个AB引用的资源
    public static List<AssetBundleBuild> AssetBundleBuilds { get; private set; }                    //ABBuilds
    public static BuildTarget BuildTarget {
        get {
#if UNITY_ANDROID
            return BuildTarget.Android;
#elif UNITY_IOS
            return BuildTarget.iOS;
#elif UNITY_WEBGL
            return BuildTarget.WebGL;
#elif UNITY_UWP || UNITY_WSA
            return BuildTarget.WSAPlayer;
#elif UNITY_STANDALONE_WIN
            return BuildTarget.StandaloneWindows;
#elif UNITY_STANDALONE_OSX
            return BuildTarget.StandaloneOSX;
#elif UNITY_STANDALONE_LINUX
            return BuildTarget.StandaloneLinuxUniversal;
#else
            return BuildTarget.StandaloneWindows;
#endif
        }
    }
    static bool IsInvalidFile(this string file) {
        file = file.ToLower();
        if (file.EndsWith(".meta") || file.EndsWith(".ds_store") || file.EndsWith(".tpsheet"))
            return true;
        return false;
    }
    static string GetMainABName(string file) {
        var name = FileUtil.GetRelativePath(file, MainAssetBundlesPath);
        return name.Substring(0, name.IndexOf('/'));
    }
    static void GenerateBuildInfo(string path, Func<string, bool> check, Func<string, string> getABName) {
        GenerateBuildInfo(new List<string>(Directory.GetDirectories(path, "*", SearchOption.TopDirectoryOnly).Where(dir => check?.Invoke(dir) ?? true)), getABName);
    }
    static void GenerateBuildInfo(List<string> dirs, Func<string, string> getABName) {
        using (var logRecord = new LogRecord("计算AB配置")) {
            AssetBundleAssets = new Dictionary<string, HashSet<string>>();
            AllAssets = new Dictionary<string, string>();
            foreach (var dir in dirs) {
                foreach (var _ in Directory.GetFiles(dir, "*", SearchOption.AllDirectories)) {
                    var file = _.Replace("\\", "/");
                    if (file.IsInvalidFile()) { continue; }
                    var abName = getABName(file);
                    if (!AssetBundleAssets.TryGetValue(abName, out var value)) {
                        AssetBundleAssets[abName] = (value = new HashSet<string>());
                    }
                    value.Add(file);
                    AllAssets[file] = abName;
                }
            }
        }
    }
    static void CollectCommonBundle() {
        try {
            CommonAssets = new Dictionary<string, HashSet<string>>();
            var length = AllAssets.Count;
            var index = 0f;
            var phase = length / 15;
            foreach (var pair in AllAssets) {
                ++index;
                if (index % phase == 0 || index == length) {
                    EditorUtility.DisplayProgressBar($"正在计算资源引用 {index}/{length} {Mathf.Floor(index / length * 100)}%", "", index / length);
                }
                var assetPath = pair.Key;
                var asset = AssetDatabase.LoadAssetAtPath<Object>(assetPath);
                if (asset == null) { continue; }
                foreach (var dependentObj in EditorUtility.CollectDependencies(new Object[1] { asset })) {
                    if (dependentObj == null || dependentObj is DefaultAsset) { continue; }
                    var dependentPath = AssetDatabase.GetAssetPath(dependentObj);
                    if (dependentPath.StartsWith("Assets/AssetBundles/") || 
                        !assetPath.StartsWith("Assets/") || 
                        assetPath.EndsWith(".cs") || 
                        assetPath.EndsWith(".dll")) { continue; }
                    if (!CommonAssets.TryGetValue(dependentPath, out var value)) {
                        CommonAssets[dependentPath] = (value = new HashSet<string>());
                    }
                    value.Add(pair.Value);
                }
            }
        } finally {
            EditorUtility.ClearProgressBar();
        }
    }
    static void CreateAssetBundleBuilds() {
        AssetBundleBuilds = new List<AssetBundleBuild>();
        foreach (var pair in AssetBundleAssets) {
            var abBuild = new AssetBundleBuild();
            abBuild.assetBundleName = $"{pair.Key}.unity3d";
            abBuild.assetNames = pair.Value.Where(x => !string.IsNullOrEmpty(x)).ToArray();
            AssetBundleBuilds.Add(abBuild);
        }
    }
    public static AssetBundleManifest BuildMainAssetBundle(string output, BuildAssetBundleOptions buildAssetBundleOptions, Action preProcess) {
        GenerateBuildInfo(MainAssetBundlesPath, null, GetMainABName);
        CollectCommonBundle();
        preProcess?.Invoke();
        CreateAssetBundleBuilds();
        EditorUtility.UnloadUnusedAssetsImmediate();
        return BuildPipeline.BuildAssetBundles(output, AssetBundleBuilds.ToArray(), buildAssetBundleOptions, BuildTarget);
    }
    //public static AssetBundleManifest BuildPatchAssetBundle(List<string> dirs, string output) {
    //    GenerateBuildInfo(dirs, GetMainABName);
    //}
}
