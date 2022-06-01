using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Scorpio.Unity.Logger;
using Scorpio.Unity.Commons;
public partial class ResourceManager : Singleton<ResourceManager> {
    private Dictionary<string, IAssetBundleLoader> m_AssetBundle = new Dictionary<string, IAssetBundleLoader> (); //所有的AssetBundle
    //public Material GreySprite { get; private set; } //黑白材质
    //public Material DefaultSprite { get; private set; } //默认材质
    //public Font DefaultFont { get; private set; } //默认字体
    public AssetBundleManifest AssetBundleManifest { get; private set; } //AssetBundleManifest

    private HashSet<string> assetBundleUsage = new HashSet<string> (); //AssetBundle 是否使用硬盘资源
    private HashSet<string> blueprintsUsage = new HashSet<string> (); //Blueprints 是否使用硬盘资源
    //新版本首次启动，删除本地AssetBundle配置
    public void InitializeNewGame () {
        FileUtil.DeleteFolder(EngineUtil.AssetBundlesMainPath, null, true);
        FileUtil.DeleteFolder(EngineUtil.BlueprintsPath, null, true);
        AssetBundleManifest = null;
    }
    //复制下载的AssetBundles到AssetBundle路径
    public void CopyMainDownloadResource () {
        logger.info ("拷贝AssetBundles");
        FileUtil.MoveFolder (EngineUtil.DownloadAssetBundleMainPath, EngineUtil.AssetBundlesMainPath, null, true, true);
    }
    //复制下载的AssetBundles到AssetBundle路径
    public void CopyPatchDownloadResource (string patch) {
        logger.info ($"拷贝补丁AssetBundles : {patch}");
        FileUtil.MoveFolder ($"{EngineUtil.DownloadAssetBundlesPatchPath}/{patch}", $"{EngineUtil.AssetBundlesPatchPath}/{patch}", null, true, true);
    }
    //复制下载的Bluepritns到本地目录
    public void CopyBlueprints () {
        logger.info ("拷贝Blueprints");
        FileUtil.MoveFolder (EngineUtil.DownloadBlueprintsPath, EngineUtil.BlueprintsPath, null, true, true);
    }
    //AssetBundle是否使用硬盘资源
    void UpdateAssetsUsage () {
        {
            assetBundleUsage.Clear ();
            var value = FileUtil.GetFileString (EngineUtil.AssetBundlesMainJson);
            if (!string.IsNullOrEmpty (value)) {
                var datas = MiniJson.Json.Deserialize (value) as Dictionary<string, object>;
                if (datas.ContainsKey ("Assets")) {
                    foreach (var pair in datas["Assets"] as Dictionary<string, object>) {
                        assetBundleUsage.Add (pair.Key);
                    }
                }
            }
        } {
            blueprintsUsage.Clear ();
            var value = FileUtil.GetFileString (EngineUtil.BlueprintsJson);
            if (!string.IsNullOrEmpty (value)) {
                var datas = MiniJson.Json.Deserialize (value) as Dictionary<string, object>;
                if (datas.ContainsKey ("Assets")) {
                    foreach (var pair in datas["Assets"] as Dictionary<string, object>) {
                        blueprintsUsage.Add (pair.Key);
                    }
                }
            }
        }
    }
    public void Initialize () {
        UpdateAssetsUsage ();
#if !UNITY_EDITOR || UNITY_WEB_ASSETS
        if (AssetBundleManifest == null) {
            AssetBundleManifest = GetAssetBundle_impl ("assetbundles", false).LoadAsset<AssetBundleManifest> ("AssetBundleManifest");
        }
#endif
    }
    public void Shutdown () {
        Shutdown (true);
    }
    public void Shutdown (bool unloadAllLoadedObjects) {
        foreach (var pair in m_AssetBundle)
            pair.Value.Unload (unloadAllLoadedObjects);
        m_AssetBundle.Clear ();
        AssetBundleManifest = null;
        UnloadUnusedAssets ();
    }
    public void UnloadUnusedAssets () {
        System.GC.Collect ();
        Resources.UnloadUnusedAssets ();
    }
    public IAssetBundleLoader GetAssetBundle (string assetBundleName) {
        if (!assetBundleName.EndsWith (".unity3d")) { assetBundleName = $"{assetBundleName}.unity3d"; }
        assetBundleName = assetBundleName.ToLower ();
        if (assetBundleName.StartsWith ("patches/")) {
            return GetAssetBundle_impl (assetBundleName, true);
        } else {
            return GetAssetBundle_impl (assetBundleName, false);
        }
    }
    /// <summary> 获得一个AssetBundle </summary>
    IAssetBundleLoader GetAssetBundle_impl (string assetBundleName, bool patch) {
        IAssetBundleLoader loader;
        if (m_AssetBundle.TryGetValue (assetBundleName, out loader)) {
            return loader;
        }
#if UNITY_EDITOR && !UNITY_WEB_ASSETS
        loader = new AssetBundleLoaderEditor ();
#else
        if (patch || assetBundleUsage.Contains (assetBundleName)) {
            loader = new AssetBundleLoaderStorage ();
        } else {
            loader = new AssetBundleLoaderSA ();
        }
#endif
        m_AssetBundle[assetBundleName] = loader;
        if (AssetBundleManifest != null) {
            var names = AssetBundleManifest.GetAllDependencies (assetBundleName);
            foreach (var name in names) { GetAssetBundle (name); }
        }
        loader.Initialize (patch ? assetBundleName : $"assetbundles/{assetBundleName}");
        return loader;
    }
    public Object Load (string resourceName) {
        return Resources.Load (resourceName);
    }
    public Object Load (string resourceName, System.Type type) {
        return Resources.Load (resourceName, type);
    }
    public Object Load<T> (string resourceName) where T : Object {
        return Resources.Load<T> (resourceName);
    }
    public Object Instantiate (string resourceName) {
        var obj = Load (resourceName);
        if (obj == null) {
            throw new System.Exception($"Instantiate is null : {resourceName}");
        }
        return Object.Instantiate (obj);
    }
    /// <summary> Load资源(不实例化) </summary>
    public Object LoadResource (string assetBundleName, string resourceName) {
        return GetAssetBundle (assetBundleName).LoadAsset (resourceName);
    }
    /// <summary> Load资源(不实例化) </summary>
    public Object LoadResource (string assetBundleName, string resourceName, System.Type type) {
        return GetAssetBundle (assetBundleName).LoadAsset (resourceName, type);
    }
    /// <summary> Load资源(不实例化) </summary>
    public T LoadResource<T> (string assetBundleName, string resourceName) where T : UnityEngine.Object {
        return GetAssetBundle (assetBundleName).LoadAsset<T> (resourceName);
    }
    /// <summary> 异步Load资源 </summary>
    public AsyncOperation LoadResourceAsync (string assetBundleName, string resourceName) {
        return GetAssetBundle (assetBundleName).LoadAssetAsync (resourceName);
    }
    /// <summary> 异步Load资源 </summary>
    public AsyncOperation LoadResourceAsync (string assetBundleName, string resourceName, System.Type type) {
        return GetAssetBundle (assetBundleName).LoadAssetAsync (resourceName, type);
    }
    /// <summary> 异步Load资源 </summary>
    public AsyncOperation LoadResourceAsync<T> (string assetBundleName, string resourceName) where T : UnityEngine.Object {
        return GetAssetBundle (assetBundleName).LoadAssetAsync<T> (resourceName);
    }

    //load 二进制数据
    public byte[] LoadBytes (string assetBundleName, string resourceName) {
        return LoadResource<TextAsset> (assetBundleName, resourceName).bytes;
    }
    //load 文本
    public string LoadString (string assetBundleName, string resourceName) {
        return System.Text.Encoding.UTF8.GetString (LoadBytes (assetBundleName, resourceName));
    }
    //load 音频
    public AudioClip LoadAudio (string assetBundleName, string resourceName) {
        return LoadResource<AudioClip> (assetBundleName, resourceName);
    }
    //load 材质
    public Material LoadMaterial (string assetBundleName, string resourceName) {
        return LoadResource<Material> (assetBundleName, resourceName);
    }
    //load 图片
    public Texture LoadTexture (string assetBundleName, string resourceName) {
        return LoadResource<Texture> (assetBundleName, resourceName);
    }
    //load Sprite
    public Sprite LoadSprite (string assetBundleName, string resourceName) {
        return LoadResource<Sprite> (assetBundleName, resourceName);
    }
    //load Font     1.16版本之后，>1.16版本
    public Font LoadFont (string assetBundleName, string resourceName) {
        return LoadResource<Font> (assetBundleName, resourceName);
    }
    //load sprite from SpriteRenderer
    public Sprite LoadSpriteFromRenderer (string assetBundleName, string resourceName) {
        var gameObject = LoadResource (assetBundleName, resourceName) as GameObject;
        if (gameObject == null) { return null; }
        var renderer = gameObject.GetComponent<SpriteRenderer> ();
        if (renderer == null) { return null; }
        return renderer.sprite;
    }
    /// <summary> Load资源(实例化) </summary>
    public Object InstantiateResource (string assetBundleName, string resourceName) {
        var obj = LoadResource (assetBundleName, resourceName);
        if (obj == null) {
            throw new System.Exception($"InstantiateResource is null : {assetBundleName}/{resourceName}");
        }
        return Object.Instantiate (obj);
    }

    //==============================Patch 加载=====================================
    /// <summary> 获取补丁AssetBundle名字 </summary>
    public string GetPatchName (string patch, string assetBundleName) { return $"patches/{patch}/{assetBundleName}"; }
    /// <summary> Load补丁资源(不实例化) </summary>
    public Object LoadResource (string patch, string assetBundleName, string resourceName) {
        return LoadResource (GetPatchName (patch, assetBundleName), resourceName);
    }
    /// <summary> Load补丁资源(不实例化) </summary>
    public Object LoadResource (string patch, string assetBundleName, string resourceName, System.Type type) {
        return LoadResource (GetPatchName (patch, assetBundleName), resourceName, type);
    }
    /// <summary> 异步Load补丁资源 </summary>
    public AsyncOperation LoadResourceAsync (string patch, string assetBundleName, string resourceName) {
        return LoadResourceAsync (GetPatchName (patch, assetBundleName), resourceName);
    }
    /// <summary> 异步Load补丁资源 </summary>
    public AsyncOperation LoadResourceAsync (string patch, string assetBundleName, string resourceName, System.Type type) {
        return LoadResourceAsync (GetPatchName (patch, assetBundleName), resourceName, type);
    }

    /// <summary> Load资源(不实例化) </summary>
    public T LoadResource<T> (string patch, string assetBundleName, string resourceName) where T : UnityEngine.Object {
        return LoadResource<T> (GetPatchName (patch, assetBundleName), resourceName);
    }
    //load 二进制数据
    public byte[] LoadBytes (string patch, string assetBundleName, string resourceName) {
        return LoadResource<TextAsset> (patch, assetBundleName, resourceName).bytes;
    }
    //load 文本
    public string LoadString (string patch, string assetBundleName, string resourceName) {
        return System.Text.Encoding.UTF8.GetString (LoadBytes (patch, assetBundleName, resourceName));
    }
    //load 图片
    public Texture LoadTexture (string patch, string assetBundleName, string resourceName) {
        return LoadResource<Texture> (patch, assetBundleName, resourceName);
    }
    //load Sprite
    public Sprite LoadSprite (string patch, string assetBundleName, string resourceName) {
        return LoadResource<Sprite> (patch, assetBundleName, resourceName);
    }
    /// <summary> Load补丁资源(实例化) </summary>
    public Object InstantiateResource (string patch, string assetBundleName, string resourceName) {
        return InstantiateResource (GetPatchName (patch, assetBundleName), resourceName);
    }
    //==============================Patch 加载=====================================

    //加载Blueprints
    public byte[] LoadBlueprints (string file) {
        if (blueprintsUsage.Contains (file)) {
            return FileUtil.GetFileBuffer (EngineUtil.BlueprintsPath + file);
        } else {
            return LoadResource<TextAsset> ("blueprints", $"assets/assetbundles/assetbundles/blueprints/{file}").bytes;
        }
    }
}