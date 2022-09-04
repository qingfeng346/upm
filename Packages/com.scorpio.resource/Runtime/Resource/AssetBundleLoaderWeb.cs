using System;
using UnityEngine;
using Scorpio.Unity.Util;
using Object = UnityEngine.Object;
namespace Scorpio.Resource {
    public class AssetBundleLoaderWebLoadAsync : IAssetBundleLoaderAsync {
        private AssetBundleLoaderWeb loader;
        public AssetBundleLoaderWebLoadAsync(AssetBundleLoaderWeb loader) {
            this.loader = loader;
        }
        public bool isDone => loader.IsLoadSuccessed;
        public float progress => loader.IsLoadSuccessed ? 1 : 0;
        public Object asset => null;
    }
    public class AssetBundleLoaderWebAsync : IAssetBundleLoaderAsync {
        private AssetBundleLoaderWeb loader;
        private string name;
        private Type type;
        private AssetBundleRequest assetBundleRequest;
        public AssetBundleLoaderWebAsync(AssetBundleLoaderWeb loader, string name, Type type) {
            this.loader = loader;
            this.name = name;
            this.type = type;
        }
        public AssetBundleRequest AssetBundleRequest {
            get {
                if (loader.IsLoadSuccessed) {
                    if (assetBundleRequest == null) {
                        assetBundleRequest = type == null ? loader.AssetBundle.LoadAssetAsync(name) : loader.AssetBundle.LoadAssetAsync(name, type);
                    }
                    return assetBundleRequest;
                }
                return null;
            }
        }
        public Object asset => AssetBundleRequest?.asset;
        public bool isDone => loader.IsLoadSuccessed ? AssetBundleRequest.isDone : false;
        public float progress => loader.IsLoadSuccessed ? AssetBundleRequest.progress : 0;
    }
    //编辑器加载
    public abstract class AssetBundleLoaderWeb : IAssetBundleLoader {
        private enum Status {
            None,
            Downloading,
            Success,
            Unload,
        }
        public AssetBundle AssetBundle { get; protected set; }
        public string[] Urls { get; private set; }
        public string FilePath { get; private set; }
        public string Version { get; private set; }
        public string VersionFile { get; private set; }
        private Status status = Status.None;
        public AssetBundleLoaderWeb(string url, string filePath, string version) : this(new[] { url }, filePath, version) { }
        public AssetBundleLoaderWeb(string[] urls, string filePath, string version) {
            Urls = urls;
            Version = version;
            FilePath = filePath;
            VersionFile = $"{FilePath}.v";
            Initialize();
        }
        public void Initialize() {
            if (status != Status.None) { return; }
            if (FileUtil.FileExist(FilePath) && FileUtil.GetFileString(VersionFile) == Version) {
                DownloadSuccess();
            } else {
                status = Status.Downloading;
                Download();
            }
        }
        public bool IsLoadSuccessed => AssetBundle != null;
        protected abstract void Download();
        protected void DownloadSuccess() {
            if (status == Status.Unload) { return; }
            status = Status.Success;
            AssetBundle = AssetBundle.LoadFromFile(FilePath);
        }
        public IAssetBundleLoaderAsync LoadAssetBundle() {
            return new AssetBundleLoaderWebLoadAsync(this);
        }
        public Object LoadAsset(string name) {
            if (AssetBundle == null) {
                logger.error("AssetBundle 还未下载完成,webab建议使用异步加载");
                return null;
            }
            return AssetBundle.LoadAsset(name.ToLower());
        }
        public Object LoadAsset(string name, Type type) {
            if (AssetBundle == null) {
                logger.error("AssetBundle 还未下载完成,webab建议使用异步加载");
                return null;
            }
            return AssetBundle.LoadAsset(name.ToLower(), type);
        }
        public T LoadAsset<T>(string name) where T : Object {
            if (AssetBundle == null) {
                logger.error("AssetBundle 还未下载完成,webab建议使用异步加载");
                return null;
            }
            return AssetBundle.LoadAsset<T>(name.ToLower());
        }
        public IAssetBundleLoaderAsync LoadAssetAsync(string name) {
            return new AssetBundleLoaderWebAsync(this, name.ToLower(), null);
        }
        public IAssetBundleLoaderAsync LoadAssetAsync(string name, Type type) {
            return new AssetBundleLoaderWebAsync(this, name.ToLower(), type);
        }
        public IAssetBundleLoaderAsync LoadAssetAsync<T>(string name) where T : Object {
            return new AssetBundleLoaderWebAsync(this, name.ToLower(), typeof(T));
        }
        public void Unload(bool unloadAllLoadedObjects) {
            this.status = Status.Unload;
            if (AssetBundle != null) {
                AssetBundle.Unload(unloadAllLoadedObjects);
            }
        }
    }
    public class AssetBundleLoaderWebSingle : AssetBundleLoaderWeb {
        public AssetBundleLoaderWebSingle(string url, string filePath, string version) : base(url, filePath, version) { }
        public AssetBundleLoaderWebSingle(string[] urls, string filePath, string version) : base(urls, filePath, version) { }
        private Downloader downloader;
        protected override void Download() {
            if (downloader == null) {
                downloader = new Downloader() { urls = Urls, filePath = FilePath, versionPath = VersionFile, version = Version };
            }
            downloader.StartDownload((success) => {
                if (success) {
                    DownloadSuccess();
                } else {
                    //出错了需要一直重试,5秒后重试
                    DownloaderManager.Instance.AddTimer(5, () => {
                        Download();
                    });
                }
            });
        }
    }
    public class AssetBundleLoaderWebQueue : AssetBundleLoaderWeb {
        public AssetBundleLoaderWebQueue(string url, string filePath, string version) : base(url, filePath, version) { }
        public AssetBundleLoaderWebQueue(string[] urls, string filePath, string version) : base(urls, filePath, version) { }
        protected override void Download() {
            DownloaderManager.Instance.Download(Urls, FilePath, Version, DownloadSuccess);
        }
    }
}