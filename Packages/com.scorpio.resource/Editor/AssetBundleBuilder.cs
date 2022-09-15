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
using System.IO.Compression;

namespace Scorpio.Resource.Editor {
    public class AssetBundleBuilder<T> where T : FileList, new() {
        public const string MainAssetBundlesPath = "Assets/AssetBundles/assetbundles/";                 //��ABĿ¼
        public const string PatchAssetBundlesPath = "Assets/AssetBundles/patches/";                     //patchabĿ¼
        public const string BlueprintsPath = "Assets/AssetBundles/assetbundles/blueprints/";            //blueprintsĿ¼
        private readonly string[] TextExtensions = new[] { "*.txt", "*.xml", "*.bytes", "*.json", "*.csv", "*.yaml", "*.html" };
        private readonly string[] ABExtensions = new[] { "*.unity3d" }; 
        public Dictionary<string, HashSet<string>> AssetBundleAssets { get; private set; }       //����AB�ļ�
        public Dictionary<string, string> AllAssets { get; private set; }                        //���д��AB����Դ�ļ�
        public Dictionary<string, HashSet<string>> CommonAssets { get; private set; }            //���AB���õ���Դ
        public List<AssetBundleBuild> AssetBundleBuilds { get; private set; }                    //ABBuilds
        public Func<string, string> GetPatchUUID;
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
                        if (!AssetBundleAssets.TryGetValue(abName, out var value)) {
                            AssetBundleAssets[abName] = (value = new HashSet<string>());
                        }
                        value.Add(file);
                        AllAssets[file] = abName;
                    }
                }
            }
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
        public FileList GenerateFileList(string rootPath, string[] searchPatterns) {
            var filelist = new T();
            var idBuilder = new StringBuilder();
            foreach (var file in FileUtil.GetFiles(rootPath, searchPatterns, SearchOption.TopDirectoryOnly)) {
                var name = FileUtil.GetRelativePath(file, rootPath);
                var asset = filelist.AddAsset(name, file);
                idBuilder.Append($"{name}:{asset.md5}_{asset.size},");
            }
            filelist.ID = FileUtil.GetMD5FromString(idBuilder.ToString());
            filelist.UnityVersion = Application.unityVersion;
            filelist.Process();
            return filelist;
        }
        void CreateFileList(string rootPath, string[] searchPatterns) {
            var fileList = GenerateFileList(rootPath, searchPatterns);
            FileUtil.CreateFile($"{rootPath}/FileList.json", fileList.ToJson());
        }
        public AssetBundleManifest BuildAssetBundles() {
            CreateAssetBundleBuilds();
            EditorUtility.UnloadUnusedAssetsImmediate();
            var manifest = BuildPipeline.BuildAssetBundles(BuilderSetting.AssetBundlesBuildPath, AssetBundleBuilds.ToArray(), BuilderSetting.BuildOptions, BuilderSetting.BuildTarget);
            if (manifest == null) {
                throw new Exception("BuildAssetBundles ʧ��");
            }
            return manifest;
        }
        public AssetBundleManifest BuildBlueprints(Action<FileList> postBuild) {
            using (var logRecord = new LogRecord("���Blueprints")) {
                EditorUtility.UnloadUnusedAssetsImmediate();
                FileUtil.DeleteFolder(BuilderSetting.BlueprintsBuildPath);     //�������
                FileUtil.CopyFolder(BlueprintsPath, BuilderSetting.BlueprintsBuildPath, TextExtensions, true);
                var fileList = GenerateFileList(BuilderSetting.BlueprintsBuildPath, TextExtensions);
                FileUtil.CreateFile($"{BlueprintsPath}/BlueprintsFileList.json", fileList.ToJson());
                AssetDatabase.Refresh();
                var guids = AssetDatabase.FindAssets("t:TextAsset", new string[] { BlueprintsPath });
                var assetBundleBuild = new AssetBundleBuild();
                assetBundleBuild.assetBundleName = "blueprints.unity3d";
                assetBundleBuild.assetNames = Array.ConvertAll(guids, guid => AssetDatabase.GUIDToAssetPath(guid).ToLower());
                EditorSettings.spritePackerMode = SpritePackerMode.Disabled;
                var manifest = BuildPipeline.BuildAssetBundles(BuilderSetting.BlueprintsBuildPath, new[] { assetBundleBuild }, BuilderSetting.BuildOptions, BuilderSetting.BuildTarget);
                if (manifest == null) {
                    throw new Exception("���Blueprints.unity3dʧ��");
                }
                FileUtil.DeleteFile($"{BuilderSetting.BlueprintsBuildPath}/Blueprints");
                FileUtil.DeleteFolder(BuilderSetting.BlueprintsBuildPath, new[] { "*.manifest" }, false);
                fileList.ABInfo = new FileList.Asset($"{BuilderSetting.BlueprintsBuildPath}/blueprints.unity3d");
                postBuild?.Invoke(fileList);
                FileUtil.CreateFile($"{BuilderSetting.BlueprintsBuildPath}/FileList.json", fileList.ToJson());
                return manifest;
            }
        }
        public AssetBundleManifest BuildMainAssetBundle(Func<string, bool> check, Action preBuild) {
            using (var logRecord = new LogRecord("���MainAssetBundles")) {
                EditorUtility.UnloadUnusedAssetsImmediate();
                GenerateBuildInfo(MainAssetBundlesPath, check, (dir, file) => {
                    return FileUtil.GetFileNameWithoutExtension(dir);
                });
                CollectCommonBundle();
                preBuild?.Invoke();
                var manifest = BuildAssetBundles();
                CreateFileList(BuilderSetting.AssetBundlesBuildPath, new[] { "*.unity3d", "assetbundles" });
                return manifest;
            }
        }
        public AssetBundleManifest BuildPatch(string patchName, bool force, Func<string, bool> check, Action preBuild) {
            using (var logRecord = new LogRecord($"���patch : {patchName}")) {
                var uuid = force ? null : (GetPatchUUID?.Invoke(patchName) ?? null);
                if (BuilderSetting.CheckPatchUUID(patchName, uuid)) {
                    return null;
                }
                EditorUtility.UnloadUnusedAssetsImmediate();
                GenerateBuildInfo($"{PatchAssetBundlesPath}/{patchName}", check, (dir, file) => {
                    return $"patches/{patchName}/{Path.GetFileNameWithoutExtension(dir)}";
                });
                CollectCommonBundle();
                preBuild?.Invoke();
                var manifest = BuildAssetBundles();
                CreateFileList(BuilderSetting.GetPatchBuildPath(patchName), ABExtensions);
                BuilderSetting.SetPatchBuildUUID(patchName, uuid);
                return manifest;
            }
        }
    }
}