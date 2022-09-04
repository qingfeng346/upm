using UnityEngine;
using System;
using System.IO;
using Object = UnityEngine.Object;
namespace Scorpio.Resource {
    public class AssetBundleLoaderABAsync : IAssetBundleLoaderAsync {
        private AssetBundleRequest assetBundleRequest;
        public AssetBundleLoaderABAsync(AssetBundleRequest assetBundleRequest) {
            this.assetBundleRequest = assetBundleRequest;
        }
        public bool isDone => assetBundleRequest.isDone;
        public float progress => assetBundleRequest.progress;
        public Object asset => assetBundleRequest.asset;
    }
    //AssetBundle 加载
    public abstract class AssetBundleLoaderAB : IAssetBundleLoader {
        public AssetBundle AssetBundle { get; protected set; }
        public abstract void Initialize();
        public bool IsLoadSuccessed => AssetBundle != null;
        public IAssetBundleLoaderAsync LoadAssetBundle() {
            return new AssetBundleLoaderLoadSuccessAsync();
        }
        public Object LoadAsset(string name) {
            return AssetBundle.LoadAsset(name.ToLower());
        }
        public Object LoadAsset(string name, Type type) {
            return AssetBundle.LoadAsset(name.ToLower(), type);
        }
        public T LoadAsset<T>(string name) where T : Object {
            return AssetBundle.LoadAsset<T>(name.ToLower());
        }
        public IAssetBundleLoaderAsync LoadAssetAsync(string name) {
            return new AssetBundleLoaderABAsync(AssetBundle.LoadAssetAsync(name.ToLower()));
        }
        public IAssetBundleLoaderAsync LoadAssetAsync(string name, Type type) {
            return new AssetBundleLoaderABAsync(AssetBundle.LoadAssetAsync(name.ToLower(), type));
        }
        public IAssetBundleLoaderAsync LoadAssetAsync<T>(string name) where T : Object {
            return new AssetBundleLoaderABAsync(AssetBundle.LoadAssetAsync<T>(name.ToLower()));
        }
        public void Unload(bool unloadAllLoadedObjects) {
            if (AssetBundle != null) {
                AssetBundle.Unload(unloadAllLoadedObjects);
            }
        }
    }
    public class AssetBundleLoaderABFile : AssetBundleLoaderAB {
        public string FilePath { get; private set; }
        public AssetBundleLoaderABFile(string filePath) {
            FilePath = filePath;
        }
        public override void Initialize() {
            AssetBundle = AssetBundle.LoadFromFile(FilePath);
        }
    }
    public class AssetBundleLoaderABStream : AssetBundleLoaderAB {
        public Stream stream { get; private set; }
        public AssetBundleLoaderABStream(Stream stream) {
            this.stream = stream;
        }
        public override void Initialize() {
            AssetBundle = AssetBundle.LoadFromStream(stream);
        }
    }
    public class AssetBundleLoaderABMemory : AssetBundleLoaderAB {
        public byte[] memory { get; private set; }
        public AssetBundleLoaderABMemory(byte[] memory) {
            this.memory = memory;
        }
        public override void Initialize() {
            AssetBundle = AssetBundle.LoadFromMemory(memory);
            memory = null;
        }
    }
}