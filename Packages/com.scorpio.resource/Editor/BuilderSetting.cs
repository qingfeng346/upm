using Newtonsoft.Json;
using Scorpio.Unity.Util;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using FileUtil = Scorpio.Unity.Util.FileUtil;

namespace Scorpio.Resource.Editor {
    [CreateAssetMenu(menuName = "Scorpio/AssetBundleBuilderSetting")]
    public class BuilderSetting : ScriptableObject {
        public string ExportPath = "AssetBundlesOutputs";
        public BuildAssetBundleOptions BuildOptions = BuildAssetBundleOptions.ChunkBasedCompression | BuildAssetBundleOptions.DeterministicAssetBundle;
        [SerializeField] private BuildTarget buildTarget = BuildTarget.NoTarget;
        private Dictionary<string, PatchBuildInfo> patchBuildInfos;
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
        private string BuildExport => $"{ExportPath}/Build";        //Build目录
        private string OutputExport => $"{ExportPath}/Output";      //最终产出目录
        private string InfoExport => $"{ExportPath}/Info";          //

        public string PatchBuildInfosFile => $"{InfoExport}/PatchBuildInfos.json";
        
        public string BlueprintsBuildPath => $"{BuildExport}/Blueprints";
        public string AssetBundlesBuildPath => $"{BuildExport}/assetbundles";


        public string AssetBundlesOutputPath => $"{OutputExport}/assetbundles";
        public string GetPatchBuildPath(string patchName) {
            return $"{AssetBundlesBuildPath}/patches/{patchName}";
        }
        void LoadPatchBuildInfos() {
            if (patchBuildInfos == null) {
                if (FileUtil.FileExist(PatchBuildInfosFile)) {
                    patchBuildInfos = JsonConvert.DeserializeObject<Dictionary<string, PatchBuildInfo>>(FileUtil.GetFileString(PatchBuildInfosFile));
                } else {
                    patchBuildInfos = new Dictionary<string, PatchBuildInfo>();
                }
            }
        }
        public bool CheckPatchUUID(string patchName, string uuid) {
            if (string.IsNullOrEmpty(uuid)) { return false; }
            LoadPatchBuildInfos();
            if (patchBuildInfos.TryGetValue(patchName, out var info)) {
                if (info.uuid == uuid) {
                    return true;
                }
            }
            return false;
        }
        public void SetPatchBuildUUID(string patchName, string uuid) {
            if (string.IsNullOrEmpty(uuid)) { return; }
            LoadPatchBuildInfos();
            patchBuildInfos[patchName] = new PatchBuildInfo() { uuid = uuid, date = TimeUtil.GetNowDateString() };
            FileUtil.CreateFile(PatchBuildInfosFile, JsonConvert.SerializeObject(patchBuildInfos, Formatting.Indented));
        }
    }
}
