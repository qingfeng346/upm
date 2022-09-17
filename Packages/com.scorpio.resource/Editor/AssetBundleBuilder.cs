using Newtonsoft.Json;
using Scorpio.Unity.Util;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;
using FileUtil = Scorpio.Unity.Util.FileUtil;
using Object = UnityEngine.Object;

namespace Scorpio.Resource.Editor {
    public class AssetBundleBuilder<T> where T : FileList, new() {
        public const string MainAssetBundlesPath = "Assets/AssetBundles/assetbundles";                  //主AB目录
        public const string PatchAssetBundlesPath = "Assets/AssetBundles/patches";                      //patchab目录
        public const string BlueprintsPath = "Assets/AssetBundles/assetbundles/blueprints";             //blueprints目录

        private string[] ABExtensions => new[] { $"*.{BuilderSetting.Expansion}" };
        private string[] ABJsonExtensions => new[] { $"*.{BuilderSetting.Expansion}", "*.json" };
        private string[] TextExtensions => new[] { "*.txt", "*.xml", "*.bytes", "*.json", "*.csv", "*.yaml", "*.html" };
        private string[] ABTextExtensions => TextExtensions.Concat(ABExtensions).ToArray();
        
        public Dictionary<string, HashSet<string>> AssetBundleAssets { get; private set; }      //所有AB文件
        public Dictionary<string, string> AllAssets { get; private set; }                       //所有打进AB的资源文件
        public Dictionary<string, HashSet<string>> CommonAssets { get; private set; }           //多个AB引用的资源
        public List<AssetBundleBuild> AssetBundleBuilds { get; private set; }                   //ABBuilds
        public Func<string, string> GetPatchUUID;                                               //获取PatchUUID
        public Func<string> GetBlueprintsOutput, GetAssetBundlesOutput, GetPatchesOutput, GetStreamingABPath;       //

        public string BlueprintsOutputPath => GetBlueprintsOutput?.Invoke() ?? $"{BuilderSetting.OutputExport}/blueprints";
        public string AssetBundlesOutputPath => GetAssetBundlesOutput?.Invoke() ?? $"{BuilderSetting.OutputExport}/assetbundles";
        public string PatchesOutputPath => GetPatchesOutput?.Invoke() ?? $"{BuilderSetting.OutputExport}/patches";
        public string StreamingABPath => GetStreamingABPath?.Invoke() ?? "AB";

        private string BlueprintsInABFileList => $"{BlueprintsPath}/BlueprintsFileList.json";
        private string BlueprintsABFile => $"blueprints.{BuilderSetting.Expansion}";

        public BuilderSetting BuilderSetting { get; set; }
        public AssetBundleBuilder(BuilderSetting builderSetting) {
            BuilderSetting = builderSetting;
        }
        void GenerateBuildInfo(string path, Func<string, bool> check, Func<string, string, string> getABName) {
            GenerateBuildInfo(new List<string>(Directory.GetDirectories(path, "*", SearchOption.TopDirectoryOnly).Where(dir => check?.Invoke(dir) ?? true)), getABName);
        }
        void GenerateBuildInfo(List<string> dirs, Func<string, string, string> getABName) {
            using (var logRecord = new LogRecord("计算AB配置")) {
                AssetBundleAssets = new Dictionary<string, HashSet<string>>();
                AllAssets = new Dictionary<string, string>();
                foreach (var dir in dirs) {
                    foreach (var _ in Directory.GetFiles(dir, "*", SearchOption.AllDirectories)) {
                        var file = _.Replace("\\", "/");
                        if (file.IsInvalidFile()) { continue; }
                        var abName = getABName(dir, file);
                        InsertBundleAsset(abName, file);
                        AllAssets[file] = abName;
                    }
                }
            }
        }
        void InsertBundleAsset(string abName, string file) {
            foreach (var pair in AssetBundleAssets) {
                if (pair.Value.Contains(file)) {
                    if (pair.Key != abName) {
                        logger.error($"{file} > {abName} 已包含在其他AB文件内 {pair.Key}");
                    }
                    return;
                }
            }
            if (!AssetBundleAssets.TryGetValue(abName, out var value)) {
                AssetBundleAssets[abName] = (value = new HashSet<string>());
            }
            value.Add(file);
        }
        void CollectCommonBundle(Func<string, bool> checkDependenValid) {
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
                        if (!assetPath.StartsWith("Assets/") ||
                            assetPath.EndsWith(".cs") ||
                            assetPath.EndsWith(".dll") ||
                            !checkDependenValid(dependentPath)) { continue; }
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
        //生成AssetBundleBuild
        void CreateAssetBundleBuilds() {
            AssetBundleBuilds = new List<AssetBundleBuild>();
            foreach (var pair in AssetBundleAssets) {
                var abBuild = new AssetBundleBuild();
                abBuild.assetBundleName = $"{pair.Key}.{BuilderSetting.Expansion}";
                abBuild.assetNames = pair.Value.Where(x => !string.IsNullOrEmpty(x)).ToArray();
                AssetBundleBuilds.Add(abBuild);
            }
        }
        public T GenerateFileList(string rootPath, string[] searchPatterns, string[] ignoreNames = null) {
            var filelist = new T();
            var idBuilder = new StringBuilder();
            var ignores = new HashSet<string>();
            if (ignoreNames != null) {
                ignores.UnionWith(ignoreNames);
            }
            foreach (var file in FileUtil.GetFiles(rootPath, searchPatterns, SearchOption.TopDirectoryOnly)) {
                var name = FileUtil.GetRelativePath(file, rootPath);
                if (ignores.Contains(name)) { continue; }
                var asset = BuilderUtil.GetAsset(file);
                filelist.Assets[name] = asset;
                idBuilder.Append($"{name}:{asset.md5}_{asset.size},");
            }
            filelist.ID = FileUtil.GetMD5FromString(idBuilder.ToString());
            filelist.Process();
            return filelist;
        }
        public void CreateFileList(string rootPath, string[] searchPatterns, string[] ignoreNames = null) {
            var fileList = GenerateFileList(rootPath, searchPatterns, ignoreNames);
            FileUtil.CreateFile($"{rootPath}/FileList.json", fileList.ToJson());
        }
        public AssetBundleManifest BuildAssetBundles() {
            FileUtil.CreateDirectory(BuilderSetting.AssetBundlesBuildPath);
            CreateAssetBundleBuilds();
            EditorUtility.UnloadUnusedAssetsImmediate();
            var manifest = BuildPipeline.BuildAssetBundles(BuilderSetting.AssetBundlesBuildPath, AssetBundleBuilds.ToArray(), BuilderSetting.BuildOptions, BuilderSetting.BuildTarget);
            if (manifest == null) {
                throw new Exception("BuildAssetBundles 失败");
            }
            return manifest;
        }
        public void CheckDeleteFiles(AssetBundleManifest assetBundleManifest, string path) {
            var allABs = assetBundleManifest.GetAllAssetBundles();
            foreach (var file in FileUtil.GetFiles(path, ABExtensions, SearchOption.TopDirectoryOnly)) {
                if (!file.EndsWith($"assetbundles.{BuilderSetting.Expansion}") && !allABs.Contains(FileUtil.GetRelativePath(file, BuilderSetting.AssetBundlesBuildPath))) {
                    FileUtil.DeleteFile(file);
                    FileUtil.DeleteFile($"{file}.manifest");
                    logger.info($"删除无用AB文件:{file}");
                }
            }
        }
        public AssetBundleManifest BuildBlueprints() {
            using (var logRecord = new LogRecord("打包Blueprints")) {
                EditorUtility.UnloadUnusedAssetsImmediate();
                FileUtil.DeleteFolder(BuilderSetting.BlueprintsBuildPath);     //清楚缓存
                FileUtil.DeleteFile(BlueprintsInABFileList);
                FileUtil.CopyFolder(BlueprintsPath, BuilderSetting.BlueprintsBuildPath, TextExtensions, true);
                var fileList = GenerateFileList(BuilderSetting.BlueprintsBuildPath, TextExtensions);
                FileUtil.CreateFile(BlueprintsInABFileList, fileList.ToJson());
                AssetDatabase.Refresh();
                var assetBundleBuild = new AssetBundleBuild();
                assetBundleBuild.assetBundleName = BlueprintsABFile;
                assetBundleBuild.assetNames = FileUtil.GetFiles(BlueprintsPath, TextExtensions).ToArray();
                EditorSettings.spritePackerMode = SpritePackerMode.Disabled;
                var manifest = BuildPipeline.BuildAssetBundles(BuilderSetting.BlueprintsBuildPath, new[] { assetBundleBuild }, BuilderSetting.BuildOptions, BuilderSetting.BuildTarget);
                if (manifest == null) {
                    throw new Exception($"打包{BlueprintsABFile}失败");
                }
                FileUtil.DeleteFile($"{BuilderSetting.BlueprintsBuildPath}/Blueprints");
                FileUtil.DeleteFolder(BuilderSetting.BlueprintsBuildPath, new[] { "*.manifest" }, false);
                fileList.ABInfo = BuilderUtil.GetAsset($"{BuilderSetting.BlueprintsBuildPath}/{BlueprintsABFile}");
                FileUtil.CreateFile($"{BuilderSetting.BlueprintsBuildPath}/FileList.json", fileList.ToJson());
                return manifest;
            }
        }
        public AssetBundleManifest BuildMainAssetBundle(Func<string, bool> check = null, Action preBuild = null) {
            using (var logRecord = new LogRecord("打包MainAssetBundles")) {
                EditorUtility.UnloadUnusedAssetsImmediate();
                GenerateBuildInfo(MainAssetBundlesPath, check, (dir, file) => {
                    return FileUtil.GetFileNameWithoutExtension(dir);
                });
                CollectCommonBundle((path) => {
                    return !path.StartsWith(MainAssetBundlesPath);
                });
                preBuild?.Invoke();
                var manifest = BuildAssetBundles();
                CheckDeleteFiles(manifest, BuilderSetting.AssetBundlesBuildPath);
                FileUtil.MoveFile($"{BuilderSetting.AssetBundlesBuildPath}/assetbundles", $"{BuilderSetting.AssetBundlesBuildPath}/assetbundles.{BuilderSetting.Expansion}", true);
                CreateFileList(BuilderSetting.AssetBundlesBuildPath, ABExtensions, new[] { BlueprintsABFile });
                return manifest;
            }
        }
        public AssetBundleManifest BuildPatch(string patchName, bool force = false, Action preBuild = null) {
            var patchPath = $"{PatchAssetBundlesPath}/{patchName}";
            return BuildPatch(patchName, () => {
                GenerateBuildInfo(patchPath, null, (dir, file) => {
                    return $"patches/{patchName}/{Path.GetFileNameWithoutExtension(dir)}";
                });
            }, (path) => {
                return !path.StartsWith(patchPath);
            }, force, preBuild);
        }
        public AssetBundleManifest BuildPatch(string patchName, Action generateBuildInfo, Func<string, bool> checkDependenValid, bool force = false, Action preBuild = null) {
            using (var logRecord = new LogRecord($"打包patch : {patchName}")) {
                var uuid = GetPatchUUID?.Invoke(patchName) ?? null;
                if (force) {
                    FileUtil.DeleteFolder(BuilderSetting.GetPatchBuildPath(patchName));
                } else if (BuilderSetting.CheckPatchUUID(patchName, uuid)) {
                    return null;
                }
                EditorUtility.UnloadUnusedAssetsImmediate();
                generateBuildInfo();
                CollectCommonBundle(checkDependenValid);
                foreach (var pair in CommonAssets) {
                    if (pair.Value.Count > 1) {
                        InsertBundleAsset($"patches/{patchName}/{patchName}_common", pair.Key);
                    }
                }
                preBuild?.Invoke();
                var manifest = BuildAssetBundles();
                var patchPath = BuilderSetting.GetPatchBuildPath(patchName);
                CheckDeleteFiles(manifest, patchPath);
                CreateFileList(patchPath, ABExtensions);
                BuilderSetting.SetPatchUUID(patchName, uuid);
                return manifest;
            }
        }
        public void SyncBlueprintsToOutput() {
            FileUtil.SyncFolder(BuilderSetting.BlueprintsBuildPath, BlueprintsOutputPath, ABTextExtensions, true);
        }
        public void SyncAssetBundlesToOutput() {
            FileUtil.SyncFolder(BuilderSetting.AssetBundlesBuildPath, AssetBundlesOutputPath, ABJsonExtensions, false);
        }
        public void SyncPatchesToOutput() {
            FileUtil.SyncFolder(BuilderSetting.PatchesBuildPath, PatchesOutputPath, ABJsonExtensions, true);
        }
        public void SyncToStreaming() {
            var streamingPath = $"{Application.streamingAssetsPath}/{StreamingABPath}";
            FileUtil.SyncFolder(BuilderSetting.AssetBundlesBuildPath, $"{streamingPath}/assetbundles", ABJsonExtensions, false);
            var abInfo = new ABInfo();
            abInfo.Expansion = $".{BuilderSetting.Expansion}";
            abInfo.StreamingPath = StreamingABPath;
            abInfo.ManifestName = $"assetbundles.{BuilderSetting.Expansion}";
            foreach (var patchName in BuilderSetting.InPackagePatches) {
                FileUtil.SyncFolder($"{BuilderSetting.PatchesBuildPath}/{patchName}", $"{StreamingABPath}/patches/{patchName}", ABJsonExtensions, true);
                var fileList = JsonConvert.DeserializeObject<T>(FileUtil.GetFileString($"{BuilderSetting.PatchesBuildPath}/{patchName}/FileList.json"));
                var fixedPatchInfo = new FixedPatchInfo();
                foreach (var pair in fileList.Assets) {
                    fixedPatchInfo.Assets.Add(new FixedPatchInfo.Asset() { name = pair.Key, md5 = pair.Value.md5 });
                }
                abInfo.FixedPatchInfos[patchName] = fixedPatchInfo;
            }
            FileUtil.CreateFile($"{Application.streamingAssetsPath}/ABInfo.json", abInfo.ToJson());
        }
    }
}