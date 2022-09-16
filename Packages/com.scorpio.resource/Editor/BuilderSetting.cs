using Newtonsoft.Json;
using Scorpio.Unity.Util;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using FileUtil = Scorpio.Unity.Util.FileUtil;

namespace Scorpio.Resource.Editor {
    [CreateAssetMenu(menuName = "Scorpio/AssetBundleBuilderSetting")]
    public class BuilderSetting : ScriptableObject {
        [Tooltip("导出目录")]
        public string ExportPath = "AssetBundlesOutputs";
        [Tooltip("AB打包选项")]
        public BuildAssetBundleOptions BuildOptions = BuildAssetBundleOptions.ChunkBasedCompression | BuildAssetBundleOptions.DeterministicAssetBundle;
        [Tooltip("AB目标平台")]
        [SerializeField] private BuildTarget buildTarget = BuildTarget.NoTarget;
        [Tooltip("打进主包的Patch")]
        public string[] InPackagePatches = new string[0];
        private Dictionary<string, PatchUUID> patchUUID;
        public BuildTarget BuildTarget {
            get {
                if (buildTarget != BuildTarget.NoTarget) {
                    return buildTarget;
                }
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
        public string BuildExport => $"{ExportPath}/Build";        //Build目录
        public string OutputExport => $"{ExportPath}/Output";      //最终产出目录
        public string InfoExport => $"{ExportPath}/Info";          //

        public string PatchUUIDFile => $"{InfoExport}/PatchUUID.json";
        
        public string BlueprintsBuildPath => $"{BuildExport}/blueprints";
        public string AssetBundlesBuildPath => $"{BuildExport}/assetbundles";
        public string PatchesBuildPath => $"{BuildExport}/assetbundles/patches";
        public string GetPatchBuildPath(string patchName) {
            return $"{AssetBundlesBuildPath}/patches/{patchName}";
        }

        void LoadPatchBuildInfos() {
            if (patchUUID == null) {
                if (FileUtil.FileExist(PatchUUIDFile)) {
                    patchUUID = JsonConvert.DeserializeObject<Dictionary<string, PatchUUID>>(FileUtil.GetFileString(PatchUUIDFile));
                } else {
                    patchUUID = new Dictionary<string, PatchUUID>();
                }
            }
        }
        public bool CheckPatchUUID(string patchName, string uuid) {
            if (string.IsNullOrEmpty(uuid)) { return false; }
            LoadPatchBuildInfos();
            if (patchUUID.TryGetValue(patchName, out var info)) {
                if (info.uuid == uuid) {
                    return true;
                }
            }
            return false;
        }
        public void SetPatchUUID(string patchName, string uuid) {
            if (string.IsNullOrEmpty(uuid)) { return; }
            LoadPatchBuildInfos();
            patchUUID[patchName] = new PatchUUID() { uuid = uuid, date = TimeUtil.GetNowDateString() };
            FileUtil.CreateFile(PatchUUIDFile, JsonConvert.SerializeObject(patchUUID, Formatting.Indented));
        }
    }
}
