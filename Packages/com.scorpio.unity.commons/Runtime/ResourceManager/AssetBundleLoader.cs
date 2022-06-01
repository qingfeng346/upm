using System;
using UnityEngine;
using Scorpio.Unity.Commons;
//资源加载类
public abstract class IAssetBundleLoader {
    public string AssetBundleName { get; protected set; }
    public void Initialize (string name) {
        AssetBundleName = name;
        Initialize_impl ();
    }
    protected abstract void Initialize_impl ();
    public abstract UnityEngine.Object LoadAsset (string name);
    public abstract UnityEngine.Object LoadAsset (string name, Type type);
    public abstract T LoadAsset<T> (string name) where T : UnityEngine.Object;
    public abstract AsyncOperation LoadAssetAsync (string name);
    public abstract AsyncOperation LoadAssetAsync (string name, Type type);
    public abstract AsyncOperation LoadAssetAsync<T> (string name) where T : UnityEngine.Object;
    public abstract void Unload (bool unloadAllLoadedObjects);
}
//AssetBundle 加载
public abstract class AssetBundleLoaderAB : IAssetBundleLoader {
    public AssetBundle AssetBundle { get; protected set; }
    public override UnityEngine.Object LoadAsset (string name) {
        return AssetBundle.LoadAsset (name.ToLower ());
    }
    public override UnityEngine.Object LoadAsset (string name, Type type) {
        return AssetBundle.LoadAsset (name.ToLower (), type);
    }
    public override T LoadAsset<T> (string name) {
        return AssetBundle.LoadAsset<T> (name.ToLower ());
    }
    public override AsyncOperation LoadAssetAsync (string name) {
        return AssetBundle.LoadAssetAsync (name.ToLower ());
    }
    public override AsyncOperation LoadAssetAsync (string name, Type type) {
        return AssetBundle.LoadAssetAsync (name.ToLower (), type);
    }
    public override AsyncOperation LoadAssetAsync<T> (string name) {
        return AssetBundle.LoadAssetAsync<T> (name.ToLower ());
    }

    public override void Unload (bool unloadAllLoadedObjects) {
        AssetBundle.Unload (unloadAllLoadedObjects);
    }
}
//StreamingAssets 加载
public class AssetBundleLoaderSA : AssetBundleLoaderAB {
    protected override void Initialize_impl () {
        AssetBundle = UnityEngine.AssetBundle.LoadFromFile ($"{Application.streamingAssetsPath}/{EngineUtil.PlatformType}/{AssetBundleName}");
    }
}
//硬盘加载
public class AssetBundleLoaderStorage : AssetBundleLoaderAB {
    protected override void Initialize_impl () {
        AssetBundle = UnityEngine.AssetBundle.LoadFromFile ($"{EngineUtil.AssetBundlesPath}{AssetBundleName}");
    }
}
#if UNITY_EDITOR
public class UnityEditorLoadAssetAsync : AsyncOperation {
    public UnityEditorLoadAssetAsync (UnityEngine.Object asset) {
        this.asset = asset;
    }
    public UnityEngine.Object asset { get; private set; }
}
//编辑器加载
public class AssetBundleLoaderEditor : IAssetBundleLoader {
    private System.Collections.Generic.Dictionary<string, string> assetPathCache = new System.Collections.Generic.Dictionary<string, string>();    //sprite缓存
    private System.Collections.Generic.Dictionary<string, string> fileCache = new System.Collections.Generic.Dictionary<string, string>();
    private string[] materalExtensions = new string[] { "mat" };
    private string[] textureExtensions = new string[] { "asset", "png", "jpg" };
    private string[] textAssetExtensions = new string[] { "bytes", "json", "txt" };
    private string[] audioExtensions = new string[] { "mp3", "ogg", "wav" };
    private string[] fontExtensions = new string[] { "ttf", "otf" };
    private string[] otherExtensions = new string[] { "prefab", "ttf", "otf", "asset" };
    protected override void Initialize_impl () {
        AssetBundleName = FileUtil.RemoveExtension (AssetBundleName);
        foreach (var file in System.IO.Directory.GetFiles($"Assets/AssetBundles/{AssetBundleName}/", "*", System.IO.SearchOption.AllDirectories)) {
            if (!file.EndsWith(".meta")) {
                fileCache[System.IO.Path.GetFileName(file).ToLower()] = file;
            }
        }
    }
    string FindAsset (string name, Type type) {
        var assetKey = name + type.Name;
        if(assetPathCache.TryGetValue(assetKey, out var path)) {
            return path;
        }
        string[] extension;
        if (type == typeof(Material)) {
            extension = materalExtensions;
        } else if (type == typeof(Texture) || type.IsSubclassOf(typeof(Texture))
            || type == typeof(Sprite) || type.IsSubclassOf(typeof(Sprite))) {
            extension = textureExtensions;
        } else if (type == typeof(TextAsset)) {
            extension = textAssetExtensions;
        } else if (type == typeof(AudioClip)) {
            extension = audioExtensions;
        } else if (type == typeof(ScriptableObject) || type.IsSubclassOf(typeof(ScriptableObject))) {
            extension = new string[] { "asset" };
        } else if (type == typeof(Font)) {
            extension = fontExtensions;
        } else if (type == typeof(ShaderVariantCollection)) {
            extension = new string[] { "shadervariants" };
        } else {
            extension = otherExtensions;
        }
        name = System.IO.Path.GetFileNameWithoutExtension(name).ToLower();
        for (var i = 0; i < extension.Length; ++i) {
            var file = $"{name}.{extension[i]}";
            if (fileCache.ContainsKey(file)) {
                return assetPathCache[assetKey] = fileCache[file];
            }
        }
        return assetPathCache[assetKey] = null;
    }
    public override UnityEngine.Object LoadAsset (string name) {
        return LoadAsset (name, typeof (UnityEngine.Object));
    }
    public override T LoadAsset<T> (string name) {
        return LoadAsset (name, typeof (T)) as T;
    }
    public override UnityEngine.Object LoadAsset (string name, Type type) {
        var file = FindAsset(name, type);
        if (string.IsNullOrEmpty(file))
        {
            logger.warn($"can't load asset '{AssetBundleName}/{name}'");
            return null;
        }
        var asset = UnityEditor.AssetDatabase.LoadAssetAtPath(file, type);
        if (asset == null)
        {
            logger.warn($"can't load asset '{AssetBundleName}/{name}'");
            return null;
        }
        return asset;
    }
    public override AsyncOperation LoadAssetAsync (string name) {
        return LoadAssetAsync (name, typeof (UnityEngine.Object));
    }
    public override AsyncOperation LoadAssetAsync<T> (string name) {
        return LoadAssetAsync (name, typeof (T));
    }
    public override AsyncOperation LoadAssetAsync (string name, Type type) {
        return new UnityEditorLoadAssetAsync (LoadAsset (name, type));
    }
    public override void Unload (bool unloadAllLoadedObjects) {

    }
}
#endif