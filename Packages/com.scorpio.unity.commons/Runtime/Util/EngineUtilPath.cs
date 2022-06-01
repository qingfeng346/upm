using UnityEngine;
public static partial class EngineUtil {
    public const string FileListJson = "FileList.json";
    //存储目录
    public static readonly string PersistentDataPath = Application.persistentDataPath + "/";
    private static string _InternalDataPath = null;
    public static string InternalDataPath {
        get {
#if UNITY_ANDROID && !UNITY_EDITOR
            if (string.IsNullOrEmpty(_InternalDataPath)) {
                using (AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer")) {
                    using (AndroidJavaObject currentActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity")) {
                        _InternalDataPath = currentActivity.Call<AndroidJavaObject>("getFilesDir").Call<string>("getAbsolutePath");
                    }
                }
            }
#else
            if (string.IsNullOrEmpty(_InternalDataPath)) {
                _InternalDataPath = PersistentDataPath;
            }
#endif
            return _InternalDataPath;
        }
    }
    //本地AssetBundles存放目录
    public static readonly string AssetBundlesPath = PersistentDataPath + "AssetBundles/";

    //存放主AssetBundles目录
    public static readonly string AssetBundlesMainPath = $"{AssetBundlesPath}/assetbundles/";

    public static readonly string AssetBundlesMainJson = $"{AssetBundlesMainPath}/{FileListJson}";

    //存放Patches目录
    public static readonly string AssetBundlesPatchPath = $"{AssetBundlesPath}/patches/";

    //存放Blueprints目录
    public static readonly string BlueprintsPath = $"{AssetBundlesPath}/blueprints/";

    public static readonly string BlueprintsJson = $"{BlueprintsPath}/{FileListJson}";

    //下载AssetBundles目录
    public static readonly string DownloadPath = PersistentDataPath + "Download/";

    //下载AssetBundles主目录
    public static readonly string DownloadAssetBundleMainPath = $"{DownloadPath}/assetbundles/";

    //下载AssetBundles分包目录
    public static readonly string DownloadAssetBundlesPatchPath = $"{DownloadPath}/patches/";

    //Blueprints下载目录
    public static readonly string DownloadBlueprintsPath = $"{DownloadPath}/blueprints/";

    //网络图片缓存目录
    public static readonly string ImageCachePath = PersistentDataPath + "ImageCache/";
}
