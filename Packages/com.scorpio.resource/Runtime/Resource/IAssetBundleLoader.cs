using System;
using Object = UnityEngine.Object;
namespace Scorpio.Resource {
    //资源加载类
    public interface IAssetBundleLoader {
        void Initialize();
        bool IsLoadSuccessed { get; }
        IAssetBundleLoaderAsync LoadAssetBundle();
        Object LoadAsset(string name);
        Object LoadAsset(string name, Type type);
        T LoadAsset<T>(string name) where T : Object;
        IAssetBundleLoaderAsync LoadAssetAsync(string name);
        IAssetBundleLoaderAsync LoadAssetAsync(string name, Type type);
        IAssetBundleLoaderAsync LoadAssetAsync<T>(string name) where T : Object;
        void Unload(bool unloadAllLoadedObjects);
    }
}
