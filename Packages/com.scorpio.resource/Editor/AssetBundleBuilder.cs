using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Scorpio.Unity.Util;
using UnityEditor;
using UnityEngine;
using System.Text;

using Object = UnityEngine.Object;
using FileUtil = Scorpio.Unity.Util.FileUtil;
using Newtonsoft.Json;
using UnityEditor.Build.Content;

namespace Scorpio.Resource.Editor {
    public class AssetBundleBuilder<T> where T : FileList, new() {
        public const string MainAssetBundlesPath = "Assets/AssetBundles/assetbundles";                  //��ABĿ¼
        public const string PatchAssetBundlesPath = "Assets/AssetBundles/patches";                      //patchabĿ¼
        public const string BlueprintsPath = "Assets/AssetBundles/assetbundles/blueprints";             //blueprintsĿ¼
        
        private static readonly string[] TextExtensions = new[] { "*.txt", "*.xml", "*.bytes", "*.json", "*.csv", "*.yaml", "*.html" };
        private static readonly string[] ABExtensions = new[] { "*.unity3d" };
        private static readonly string[] ABJsonExtensions = new[] { "*.unity3d", "*.json" };
        private static readonly string[] AllExtensions = TextExtensions.Concat(ABExtensions).ToArray();
        public Dictionary<string, HashSet<string>> AssetBundleAssets { get; private set; }      //����AB�ļ�
        public Dictionary<string, string> AllAssets { get; private set; }                       //���д��AB����Դ�ļ�
        public Dictionary<string, HashSet<string>> CommonAssets { get; private set; }           //���AB���õ���Դ
        public List<AssetBundleBuild> AssetBundleBuilds { get; private set; }                   //ABBuilds
        public Func<string, string> GetPatchUUID;                                               //��ȡPatchUUID
        public Func<string> GetBlueprintsOutput, GetAssetBundlesOutput, GetPatchesOutput, GetStreamingABPath;       //

        public string BlueprintsOutputPath => GetBlueprintsOutput?.Invoke() ?? $"{BuilderSetting.OutputExport}/blueprints";
        public string AssetBundlesOutputPath => GetAssetBundlesOutput?.Invoke() ?? $"{BuilderSetting.OutputExport}/assetbundles";
        public string PatchesOutputPath => GetPatchesOutput?.Invoke() ?? $"{BuilderSetting.OutputExport}/patches";
        public string StreamingABPath => GetStreamingABPath?.Invoke() ?? $"{Application.streamingAssetsPath}/AB";

        private string BlueprintsInABFileList => $"{BlueprintsPath}/BlueprintsFileList.json";

        public BuilderSetting BuilderSetting { get; set; }
        public AssetBundleBuilder(BuilderSetting builderSetting) {
            BuilderSetting = builderSetting;
        }
        void GenerateBuildInfo(string path, Func<string, bool> check, Func<string, string, string> getABName) {
            GenerateBuildInfo(new List<string>(Directory.GetDirectories(path, "*", SearchOption.TopDirectoryOnly).Where(dir => check?.Invoke(dir) ?? true)), getABName);
        }
        void GenerateBuildInfo(List<string> dirs, Func<string, string, string> getABName) {
            using (var logRecord = new LogRecord("����AB����")) {
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
                        logger.error($"{file} > {abName} �Ѱ���������AB�ļ��� {pair.Key}");
                    }
                    return;
                }
            }
            if (!AssetBundleAssets.TryGetValue(abName, out var value)) {
                AssetBundleAssets[abName] = (value = new HashSet<string>());
            }
            value.Add(file);
        }
        void CollectCommonBundle() {
            try {
                CommonAssets = new Dictionary<string, HashSet<string>>();
                var length = AllAssets.Count;
                var index = 0f;
                var phase = length / 15;
                foreach (var pair in AllAssets) {
                    ++index;
                    if (index % phase == 0 || index == length) {
                        EditorUtility.DisplayProgressBar($"���ڼ�����Դ���� {index}/{length} {Mathf.Floor(index / length * 100)}%", "", index / length);
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
        //����AssetBundleBuild
        void CreateAssetBundleBuilds() {
            AssetBundleBuilds = new List<AssetBundleBuild>();
            foreach (var pair in AssetBundleAssets) {
                var abBuild = new AssetBundleBuild();
                abBuild.assetBundleName = $"{pair.Key}.unity3d";
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
                throw new Exception("BuildAssetBundles ʧ��");
            }
            return manifest;
        }
        public void CheckDeleteFiles(AssetBundleManifest assetBundleManifest, string path) {
            var allABs = assetBundleManifest.GetAllAssetBundles();
            foreach (var file in FileUtil.GetFiles(path, ABExtensions, SearchOption.TopDirectoryOnly)) {
                if (!file.EndsWith("assetbundles.unity3d") && !allABs.Contains(FileUtil.GetRelativePath(file, BuilderSetting.AssetBundlesBuildPath))) {
                    FileUtil.DeleteFile(file);
                    FileUtil.DeleteFile($"{file}.manifest");
                    logger.info($"ɾ������AB�ļ�:{file}");
                }
            }
        }
        public AssetBundleManifest BuildBlueprints() {
            using (var logRecord = new LogRecord("���Blueprints")) {
                EditorUtility.UnloadUnusedAssetsImmediate();
                FileUtil.DeleteFolder(BuilderSetting.BlueprintsBuildPath);     //�������
                FileUtil.DeleteFile(BlueprintsInABFileList);
                FileUtil.CopyFolder(BlueprintsPath, BuilderSetting.BlueprintsBuildPath, TextExtensions, true);
                var fileList = GenerateFileList(BuilderSetting.BlueprintsBuildPath, TextExtensions);
                FileUtil.CreateFile(BlueprintsInABFileList, fileList.ToJson());
                AssetDatabase.Refresh();
                var assetBundleBuild = new AssetBundleBuild();
                assetBundleBuild.assetBundleName = "blueprints.unity3d";
                assetBundleBuild.assetNames = FileUtil.GetFiles(BlueprintsPath, TextExtensions).ToArray();
                EditorSettings.spritePackerMode = SpritePackerMode.Disabled;
                var manifest = BuildPipeline.BuildAssetBundles(BuilderSetting.BlueprintsBuildPath, new[] { assetBundleBuild }, BuilderSetting.BuildOptions, BuilderSetting.BuildTarget);
                if (manifest == null) {
                    throw new Exception("���Blueprints.unity3dʧ��");
                }
                FileUtil.DeleteFile($"{BuilderSetting.BlueprintsBuildPath}/Blueprints");
                FileUtil.DeleteFolder(BuilderSetting.BlueprintsBuildPath, new[] { "*.manifest" }, false);
                fileList.ABInfo = BuilderUtil.GetAsset($"{BuilderSetting.BlueprintsBuildPath}/blueprints.unity3d");
                FileUtil.CreateFile($"{BuilderSetting.BlueprintsBuildPath}/FileList.json", fileList.ToJson());
                return manifest;
            }
        }
        public AssetBundleManifest BuildMainAssetBundle(Func<string, bool> check = null, Action preBuild = null) {
            using (var logRecord = new LogRecord("���MainAssetBundles")) {
                EditorUtility.UnloadUnusedAssetsImmediate();
                GenerateBuildInfo(MainAssetBundlesPath, check, (dir, file) => {
                    return FileUtil.GetFileNameWithoutExtension(dir);
                });
                CollectCommonBundle();
                preBuild?.Invoke();
                var manifest = BuildAssetBundles();
                CheckDeleteFiles(manifest, BuilderSetting.AssetBundlesBuildPath);
                FileUtil.MoveFile($"{BuilderSetting.AssetBundlesBuildPath}/assetbundles", $"{BuilderSetting.AssetBundlesBuildPath}/assetbundles.unity3d", true);
                CreateFileList(BuilderSetting.AssetBundlesBuildPath, ABExtensions, new[] { "blueprints.unity3d" });
                return manifest;
            }
        }
        public AssetBundleManifest BuildPatch(string patchName, bool force = false, Action preBuild = null) {
            return BuildPatch(patchName, () => {
                GenerateBuildInfo($"{PatchAssetBundlesPath}/{patchName}", null, (dir, file) => {
                    return $"patches/{patchName}/{Path.GetFileNameWithoutExtension(dir)}";
                });
            }, force, preBuild);
        }
        public AssetBundleManifest BuildPatch(string patchName, Action generateBuildInfo, bool force = false, Action preBuild = null) {
            using (var logRecord = new LogRecord($"���patch : {patchName}")) {
                var uuid = GetPatchUUID?.Invoke(patchName) ?? null;
                if (force) {
                    FileUtil.DeleteFolder(BuilderSetting.GetPatchBuildPath(patchName));
                } else if (BuilderSetting.CheckPatchUUID(patchName, uuid)) {
                    return null;
                }
                EditorUtility.UnloadUnusedAssetsImmediate();
                generateBuildInfo();
                CollectCommonBundle();
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
            FileUtil.SyncFolder(BuilderSetting.BlueprintsBuildPath, BlueprintsOutputPath, AllExtensions, true);
        }
        public void SyncAssetBundlesToOutput() {
            FileUtil.SyncFolder(BuilderSetting.AssetBundlesBuildPath, AssetBundlesOutputPath, ABJsonExtensions, false);
        }
        public void SyncPatchesToOutput() {
            FileUtil.SyncFolder(BuilderSetting.PatchesBuildPath, PatchesOutputPath, ABJsonExtensions, true);
        }
        public void SyncToStreaming() {
            FileUtil.SyncFolder(BuilderSetting.AssetBundlesBuildPath, $"{StreamingABPath}/assetbundles", ABJsonExtensions, false);
            var fixedPatchInfos = new Dictionary<string, FixedPatchInfo>();
            foreach (var patchName in BuilderSetting.InPackagePatches) {
                FileUtil.SyncFolder($"{BuilderSetting.PatchesBuildPath}/{patchName}", $"{StreamingABPath}/patches/{patchName}", ABJsonExtensions, true);
                var fileList = JsonConvert.DeserializeObject<T>(FileUtil.GetFileString($"{BuilderSetting.PatchesBuildPath}/{patchName}/FileList.json"));
                var fixedPatchInfo = new FixedPatchInfo();
                foreach (var pair in fileList.Assets) {
                    fixedPatchInfo.Assets.Add(new FixedPatchInfo.Asset() { name = pair.Key, md5 = pair.Value.md5 });
                }
                fixedPatchInfos[patchName] = fixedPatchInfo;
            }
            FileUtil.CreateFile($"{StreamingABPath}/FixedPatches.json", fixedPatchInfos.ToJson());
        }
    }
}