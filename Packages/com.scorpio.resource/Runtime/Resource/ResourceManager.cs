using System.Collections.Generic;
using UnityEngine;
using System.Text;
using Scorpio.Unity.Util;
using Newtonsoft.Json;
using System.IO;
using UnityEngine.Networking;

namespace Scorpio.Resource {
    public partial class ResourceManager {
        private static readonly string AssetBundleDataPath = Application.persistentDataPath + "/AssetBundles/";
        private static readonly string AssetBundleDownloadPath = Application.persistentDataPath + "/AssetBundlesDownload/";
        public static readonly string FileListName = "FileList.json";

        public static readonly string AssetBundleWebPath = Application.persistentDataPath + "/AssetBundlesWeb/";
        public static readonly string AssetBundlesMainPath = $"{AssetBundleDataPath}assetbundles/";
        public static readonly string AssetBundlesPatchPath = $"{AssetBundleDataPath}patches/";
        public static readonly string BlueprintsPath = $"{AssetBundleDataPath}blueprints/";

        public static readonly string DownloadAssetBundlesMainPath = $"{AssetBundleDownloadPath}assetbundles/";
        public static readonly string DownloadAssetBundlesPatchPath = $"{AssetBundleDownloadPath}patches/";
        public static readonly string DownloadBlueprintsPath = $"{AssetBundleDownloadPath}blueprints/";
        public static ResourceManager Instance { get; } = new ResourceManager();

        public ABInfo ABInfo { get; private set; }                                              //资源总信息
        public Dictionary<string, FixedPatchInfo> FixedPatchInfos => ABInfo.FixedPatchInfos;    //包体内的patch
        public HashSet<string> FixedPatchAssets { get; set; }                                   //包体内的assets

        public AssetBundleManifest AssetBundleManifest { get; private set; }        //AssetBundleManifest


        private Dictionary<string, AssetBundleInfo> m_AssetBundleInfos = new Dictionary<string, AssetBundleInfo>();     //AB文件信息
        private Dictionary<string, IAssetBundleLoader> m_AssetBundles = new Dictionary<string, IAssetBundleLoader>();   //所有的AssetBundle
        private Dictionary<string, FileList.Asset> assetBundleUsage = new Dictionary<string, FileList.Asset>();         //AssetBundle 是否使用硬盘资源
        private Dictionary<string, FileList.Asset> blueprintsUsage = new Dictionary<string, FileList.Asset>();          //Blueprints 是否使用硬盘资源
       

        public ResourceManager() {
            var bytes = LoadStreamingAssets("ABInfo.json");
            if (bytes != null) {
                ABInfo = JsonConvert.DeserializeObject<ABInfo>(Encoding.UTF8.GetString(bytes));
                FixedPatchAssets = new HashSet<string>();
                foreach (var pair in FixedPatchInfos) {
                    foreach (var asset in pair.Value.Assets) {
                        FixedPatchAssets.Add($"patches/{pair.Key}/{asset.name}");
                    }
                }
            }
        }
        //新版本首次启动，删除本地AssetBundle配置
        public void InitializeNewGame() {
            //新删除所有主AB和Blueprints,保留patch
            FileUtil.DeleteFolder(AssetBundlesMainPath);
            FileUtil.DeleteFolder(BlueprintsPath);
            //删除包内的patch
            foreach (var pair in FixedPatchInfos) {
                FileUtil.DeleteFolder($"{AssetBundlesPatchPath}{pair.Key}");
            }
            AssetBundleManifest = null;
        }
        //复制下载的AssetBundles到AssetBundle路径
        public void CopyMainDownloadResource() {
            logger.info("拷贝AssetBundles");
            FileUtil.MoveFolder(DownloadAssetBundlesMainPath, AssetBundlesMainPath);
        }
        //复制下载的AssetBundles到AssetBundle路径
        public void CopyPatchDownloadResource(string patch) {
            logger.info($"拷贝补丁AssetBundles : {patch}");
            FileUtil.MoveFolder($"{DownloadAssetBundlesPatchPath}/{patch}", $"{AssetBundlesPatchPath}/{patch}");
        }
        //复制下载的Bluepritns到本地目录
        public void CopyBlueprints() {
            logger.info("拷贝Blueprints");
            FileUtil.MoveFolder(DownloadBlueprintsPath, BlueprintsPath);
        }
        //AssetBundle是否使用硬盘资源
        void UpdateAssetsUsage() {
            {
                assetBundleUsage.Clear();
                var value = FileUtil.GetFileString($"{AssetBundlesMainPath}/{FileListName}");
                if (!string.IsNullOrEmpty(value)) {
                    foreach (var pair in JsonConvert.DeserializeObject<FileList>(value).Assets) {
                        assetBundleUsage[$"assetbundles/{pair.Key}"] = pair.Value;
                    }
                }
            }
            {
                blueprintsUsage.Clear();
                var value = FileUtil.GetFileString($"{BlueprintsPath}/{FileListName}");
                if (!string.IsNullOrEmpty(value)) {
                    blueprintsUsage = JsonConvert.DeserializeObject<FileList>(value).Assets;
                }
            }
        }
        public void Initialize() {
            UpdateAssetsUsage();
#if !UNITY_EDITOR || UNITY_WEB_ASSETS
            if (!string.IsNullOrEmpty(ABInfo.ManifestName) && AssetBundleManifest == null) {
                AssetBundleManifest = GetAssetBundle_impl($"assetbundles/{ABInfo.ManifestName}").LoadAsset<AssetBundleManifest>("AssetBundleManifest");
            }
#endif
        }
        public void Shutdown() {
            Shutdown(true);
            m_AssetBundleInfos.Clear();
        }
        public void Shutdown(bool unloadAllLoadedObjects) {
            foreach (var pair in m_AssetBundles)
                pair.Value.Unload(unloadAllLoadedObjects);
            m_AssetBundles.Clear();
            AssetBundleManifest = null;
            UnloadUnusedAssets();
        }
        public void Unload(string assetBundleName, bool unloadAllLoadedObjects) {
            assetBundleName = GetABName(assetBundleName);
            if (m_AssetBundles.TryGetValue(assetBundleName, out var loader)) {
                m_AssetBundles.Remove(assetBundleName);
                loader.Unload(unloadAllLoadedObjects);
                UnloadUnusedAssets();
            }
        }
        public void AddAssetBundleLoader(string assetBundleName, IAssetBundleLoader loader) {
            assetBundleName = GetABName(assetBundleName);
            if (!m_AssetBundles.ContainsKey(assetBundleName)) {
                m_AssetBundles[assetBundleName] = loader;
            }
        }
        public FixedPatchInfo GetFixedPatchInfo(string patchName) {
            if (FixedPatchInfos.TryGetValue(patchName, out var info)) {
                return info;
            }
            return null;
        }
        public string GetABName(string assetBundleName) {
            assetBundleName = assetBundleName.ToLowerInvariant();
            if (!assetBundleName.EndsWith(ABInfo.Expansion)) { assetBundleName = $"{assetBundleName}{ABInfo.Expansion}"; }
            if (assetBundleName.StartsWith("patches/")) {
                return assetBundleName;
            } else {
                return $"assetbundles/{assetBundleName}";
            }
        }
        public string GetStorageABPath(string assetBundleName) {
            return $"{AssetBundleDataPath}{assetBundleName}";
        }
        public string GetStreamingABPath(string assetBundleName) {
            return $"{Application.streamingAssetsPath}/{ABInfo.StreamingPath}/{assetBundleName}";
        }
        public void AddWebAssetBundle(string assetBundleName, string filePath, string url, string version) {
            AddWebAssetBundle(assetBundleName, filePath, new[] { url }, version, false);
        }
        public void AddWebAssetBundle(string assetBundleName, string filePath, string url, string version, bool queue) {
            AddWebAssetBundle(assetBundleName, filePath, new[] { url }, version, queue);
        }
        public void AddWebAssetBundle(string assetBundleName, string filePath, string[] urls, string version, bool queue) {
            m_AssetBundleInfos[GetABName(assetBundleName)] = new AssetBundleInfo(AssetBundleType.Web) { filePath = filePath, urls = urls, version = version, queue = queue };
        }
        public void AddStorageAssetBundle(string assetBundleName, string filePath) {
            m_AssetBundleInfos[GetABName(assetBundleName)] = new AssetBundleInfo(AssetBundleType.Storage) { filePath = filePath };
        }
        
        public IAssetBundleLoader GetAssetBundleLoader(string assetBundleName) {
            if (m_AssetBundles.TryGetValue(GetABName(assetBundleName), out var loader)) {
                return loader;
            }
            return null;
        }
        public void UnloadUnusedAssets() {
            System.GC.Collect();
            Resources.UnloadUnusedAssets();
        }
        public IAssetBundleLoader GetAssetBundle(string assetBundleName) {
            return GetAssetBundle_impl(GetABName(assetBundleName));
        }
        public IAssetBundleLoaderAsync LoadAssetBundle(string assetBundleName) {
            return GetAssetBundle(assetBundleName).LoadAssetBundle();
        }
        /// <summary> 获得一个AssetBundle </summary>
        IAssetBundleLoader GetAssetBundle_impl(string assetBundleName) {
            if (m_AssetBundles.TryGetValue(assetBundleName, out var loader)) {
                return loader;
            }
            if (m_AssetBundleInfos.TryGetValue(assetBundleName, out var assetBundleInfo)) {
                if (assetBundleInfo.type == AssetBundleType.Storage) {
                    return m_AssetBundles[assetBundleName] = new AssetBundleLoaderABFile(assetBundleInfo.filePath);
                } else {
                    if (assetBundleInfo.queue) {
                        return m_AssetBundles[assetBundleName] = new AssetBundleLoaderWebQueue(assetBundleInfo.urls, assetBundleInfo.filePath, assetBundleInfo.version);
                    } else {
                        return m_AssetBundles[assetBundleName] = new AssetBundleLoaderWebSingle(assetBundleInfo.urls, assetBundleInfo.filePath, assetBundleInfo.version);
                    }
                }
            }
            var patch = assetBundleName.StartsWith("patches/");
#if UNITY_EDITOR && !UNITY_WEB_ASSETS
            loader = new AssetBundleLoaderEditor($"Assets/AssetBundles/{FileUtil.RemoveExtension(assetBundleName)}/");
#else
            if (assetBundleUsage.ContainsKey(assetBundleName)) {
                loader = new AssetBundleLoaderABFile(GetStorageABPath(assetBundleName));
            } else if (patch) {
                if (!FixedPatchAssets.Contains(assetBundleName) || FileUtil.FileExist(GetStorageABPath(assetBundleName))) {
                    loader = new AssetBundleLoaderABFile(GetStorageABPath(assetBundleName));
                } else {
                    loader = new AssetBundleLoaderABFile(GetStreamingABPath(assetBundleName));
                }
            } else {
                loader = new AssetBundleLoaderABFile(GetStreamingABPath(assetBundleName));
            }
#endif
            m_AssetBundles[assetBundleName] = loader;
            if (AssetBundleManifest != null && !patch) {
                var mainAbName = FileUtil.GetFileName(assetBundleName);
                var names = AssetBundleManifest.GetAllDependencies(mainAbName);
                foreach (var name in names) {
                    GetAssetBundle(name);
                }
            }
            loader.Initialize();
            return loader;
        }

        public Object Load(string resourceName) {
            return Resources.Load(resourceName);
        }
        public Object Load(string resourceName, System.Type type) {
            return Resources.Load(resourceName, type);
        }
        public Object Load<T>(string resourceName) where T : Object {
            return Resources.Load<T>(resourceName);
        }
        public Object Instantiate(string resourceName) {
            var obj = Load(resourceName);
            if (obj == null) {
                throw new System.Exception($"Instantiate is null : {resourceName}");
            }
            return Object.Instantiate(obj);
        }
        public byte[] LoadStreamingAssets(string path) {
            var uri = new System.Uri(Path.Combine(Application.streamingAssetsPath, path));
            var request = UnityWebRequest.Get(uri);
            request.SendWebRequest();
            while (request.downloadHandler.isDone) { }
            return string.IsNullOrEmpty(request.error) ? request.downloadHandler.data : null;
        }

        /// <summary> Load资源(不实例化) </summary>
        public Object LoadResource(string assetBundleName, string resourceName) {
            return GetAssetBundle(assetBundleName).LoadAsset(resourceName);
        }
        /// <summary> Load资源(不实例化) </summary>
        public Object LoadResource(string assetBundleName, string resourceName, System.Type type) {
            return GetAssetBundle(assetBundleName).LoadAsset(resourceName, type);
        }
        /// <summary> Load资源(不实例化) </summary>
        public T LoadResource<T>(string assetBundleName, string resourceName) where T : UnityEngine.Object {
            return GetAssetBundle(assetBundleName).LoadAsset<T>(resourceName);
        }
        /// <summary> 异步Load资源 </summary>
        public IAssetBundleLoaderAsync LoadResourceAsync(string assetBundleName, string resourceName) {
            return GetAssetBundle(assetBundleName).LoadAssetAsync(resourceName);
        }
        /// <summary> 异步Load资源 </summary>
        public IAssetBundleLoaderAsync LoadResourceAsync(string assetBundleName, string resourceName, System.Type type) {
            return GetAssetBundle(assetBundleName).LoadAssetAsync(resourceName, type);
        }
        /// <summary> 异步Load资源 </summary>
        public IAssetBundleLoaderAsync LoadResourceAsync<T>(string assetBundleName, string resourceName) where T : UnityEngine.Object {
            return GetAssetBundle(assetBundleName).LoadAssetAsync<T>(resourceName);
        }

        //load 二进制数据
        public byte[] LoadBytes(string assetBundleName, string resourceName) {
            return LoadResource<TextAsset>(assetBundleName, resourceName).bytes;
        }
        //load 文本
        public string LoadString(string assetBundleName, string resourceName) {
            return System.Text.Encoding.UTF8.GetString(LoadBytes(assetBundleName, resourceName));
        }
        //load 音频
        public AudioClip LoadAudio(string assetBundleName, string resourceName) {
            return LoadResource<AudioClip>(assetBundleName, resourceName);
        }
        //load 材质
        public Material LoadMaterial(string assetBundleName, string resourceName) {
            return LoadResource<Material>(assetBundleName, resourceName);
        }
        //load 图片
        public Texture LoadTexture(string assetBundleName, string resourceName) {
            return LoadResource<Texture>(assetBundleName, resourceName);
        }
        //load Sprite
        public Sprite LoadSprite(string assetBundleName, string resourceName) {
            return LoadResource<Sprite>(assetBundleName, resourceName);
        }
        //load Font
        public Font LoadFont(string assetBundleName, string resourceName) {
            return LoadResource<Font>(assetBundleName, resourceName);
        }
        //load sprite from SpriteRenderer
        public Sprite LoadSpriteFromRenderer(string assetBundleName, string resourceName) {
            var gameObject = LoadResource(assetBundleName, resourceName) as GameObject;
            if (gameObject == null) { return null; }
            var renderer = gameObject.GetComponent<SpriteRenderer>();
            if (renderer == null) { return null; }
            return renderer.sprite;
        }
        /// <summary> Load资源(实例化) </summary>
        public Object InstantiateResource(string assetBundleName, string resourceName) {
            var obj = LoadResource(assetBundleName, resourceName);
            if (obj == null) {
                throw new System.Exception($"InstantiateResource is null : {assetBundleName}/{resourceName}");
            }
            return Object.Instantiate(obj);
        }

        //==============================Patch 加载=====================================
        /// <summary> 获取补丁AssetBundle名字 </summary>
        public string GetPatchName(string patch, string assetBundleName) { return $"patches/{patch}/{assetBundleName}"; }
        /// <summary> Load补丁资源(不实例化) </summary>
        public Object LoadResource(string patch, string assetBundleName, string resourceName) {
            return LoadResource(GetPatchName(patch, assetBundleName), resourceName);
        }
        /// <summary> Load补丁资源(不实例化) </summary>
        public Object LoadResource(string patch, string assetBundleName, string resourceName, System.Type type) {
            return LoadResource(GetPatchName(patch, assetBundleName), resourceName, type);
        }
        /// <summary> Load资源(不实例化) </summary>
        public T LoadResource<T>(string patch, string assetBundleName, string resourceName) where T : UnityEngine.Object {
            return LoadResource<T>(GetPatchName(patch, assetBundleName), resourceName);
        }
        /// <summary> 异步Load补丁资源 </summary>
        public IAssetBundleLoaderAsync LoadResourceAsync(string patch, string assetBundleName, string resourceName) {
            return LoadResourceAsync(GetPatchName(patch, assetBundleName), resourceName);
        }
        /// <summary> 异步Load补丁资源 </summary>
        public IAssetBundleLoaderAsync LoadResourceAsync(string patch, string assetBundleName, string resourceName, System.Type type) {
            return LoadResourceAsync(GetPatchName(patch, assetBundleName), resourceName, type);
        }
        /// <summary> 异步Load补丁资源 </summary>
        public IAssetBundleLoaderAsync LoadResourceAsync<T>(string patch, string assetBundleName, string resourceName) where T : UnityEngine.Object {
            return LoadResourceAsync<T>(GetPatchName(patch, assetBundleName), resourceName);
        }


        //load 二进制数据
        public byte[] LoadBytes(string patch, string assetBundleName, string resourceName) {
            return LoadResource<TextAsset>(patch, assetBundleName, resourceName).bytes;
        }
        //load 文本
        public string LoadString(string patch, string assetBundleName, string resourceName) {
            return System.Text.Encoding.UTF8.GetString(LoadBytes(patch, assetBundleName, resourceName));
        }
        //load 图片
        public Texture LoadTexture(string patch, string assetBundleName, string resourceName) {
            return LoadResource<Texture>(patch, assetBundleName, resourceName);
        }
        //load Sprite
        public Sprite LoadSprite(string patch, string assetBundleName, string resourceName) {
            return LoadResource<Sprite>(patch, assetBundleName, resourceName);
        }
        //load Font
        public Font LoadFont(string patch, string assetBundleName, string resourceName) {
            return LoadResource<Font>(patch, assetBundleName, resourceName);
        }
        /// <summary> Load补丁资源(实例化) </summary>
        public Object InstantiateResource(string patch, string assetBundleName, string resourceName) {
            return InstantiateResource(GetPatchName(patch, assetBundleName), resourceName);
        }
        //==============================Patch 加载=====================================

        //加载Blueprints
        public byte[] LoadBlueprints(string file) {
            if (blueprintsUsage.TryGetValue(file, out var asset)) {
                return FileUtil.GetFileBuffer(BlueprintsPath + asset.GetName(file));
            } else {
                return LoadResource<TextAsset>("blueprints", $"assets/assetbundles/assetbundles/blueprints/{file}").bytes;
            }
        }
    }
}