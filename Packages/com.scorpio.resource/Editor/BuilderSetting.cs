using UnityEditor;
using UnityEngine;

namespace Scorpio.Resource.Editor {
    [CreateAssetMenu(menuName = "Scorpio/AssetBundleBuilderSetting")]
    public class BuilderSetting : ScriptableObject {
        public string Output = "AssetBundlesOutputs";
        public BuildAssetBundleOptions BuildOptions = BuildAssetBundleOptions.ChunkBasedCompression | BuildAssetBundleOptions.DeterministicAssetBundle;
        [SerializeField] private BuildTarget buildTarget = BuildTarget.NoTarget;
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
    }
}
