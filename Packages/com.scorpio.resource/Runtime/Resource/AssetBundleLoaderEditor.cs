#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using System.Linq;
using Scorpio.Unity.Util;
using Object = UnityEngine.Object;
namespace Scorpio.Resource {
    public class UnityEditorLoadAssetAsync : IAssetBundleLoaderAsync {
        public UnityEditorLoadAssetAsync(Object asset) {
            this.asset = asset;
        }
        public Object asset { get; private set; }
        public bool isDone => true;
        public float progress => 1;
    }
    //编辑器加载
    public class AssetBundleLoaderEditor : IAssetBundleLoader {
        private Dictionary<Tuple<string, Type>, string> assetPathCache = new Dictionary<Tuple<string, Type>, string>();    //sprite缓存
        private Dictionary<string, string> fileCache = new Dictionary<string, string>();
        private static List<Tuple<Type, string[]>> Extensions = new List<Tuple<Type, string[]>>() {
        new Tuple<Type,string[]>(typeof(Material), new string[] { "mat" }),
        new Tuple<Type,string[]>(typeof(Texture), new string[] { "png", "jpg" }),
        new Tuple<Type,string[]>(typeof(Sprite), new string[] { "asset", "png", "jpg" }),
        new Tuple<Type,string[]>(typeof(TextAsset), new string[] { "bytes", "json", "txt" }),
        new Tuple<Type,string[]>(typeof(AudioClip), new string[] { "mp3", "ogg", "wav" }),
        new Tuple<Type,string[]>(typeof(Font), new string[] { "ttf", "otf" }),
        new Tuple<Type,string[]>(typeof(ScriptableObject), new string[] { "asset" }),
        new Tuple<Type,string[]>(typeof(ShaderVariantCollection), new string[] { "shadervariants" }),
    };
        private static string[] allExtensions = null;
        private static string[] AllExtensions {
            get {
                if (allExtensions == null) {
                    var extensions = new HashSet<string>();
                    extensions.Add("prefab");
                    Extensions.ForEach(extension => {
                        extensions.UnionWith(extension.Item2);
                    });
                    allExtensions = extensions.ToArray();
                }
                return allExtensions;
            }
        }
        private static string AssetsPath;
        public AssetBundleLoaderEditor(string assetsPath) {
            AssetsPath = assetsPath;
        }
        public void Initialize() {
            if (Directory.Exists(AssetsPath)) {
                foreach (var file in Directory.GetFiles(AssetsPath, "*", SearchOption.AllDirectories)) {
                    if (!file.EndsWith(".meta")) {
                        fileCache[Path.GetFileName(file).ToLower()] = file;
                    }
                }
            }
        }
        public bool IsLoadSuccessed => true;
        string FindAsset(string name, Type type) {
            var assetKey = new Tuple<string, Type>(name, type);
            if (assetPathCache.TryGetValue(assetKey, out var path)) {
                return path;
            }
            string[] extensions = null;
            foreach (var extension in Extensions) {
                if (extension.Item1.IsAssignableFrom(type)) {
                    extensions = extension.Item2;
                }
            }
            if (extensions == null) {
                extensions = AllExtensions;
            }
            name = Path.GetFileNameWithoutExtension(name).ToLower();
            for (var i = 0; i < extensions.Length; ++i) {
                var file = $"{name}.{extensions[i]}";
                if (fileCache.ContainsKey(file)) {
                    return assetPathCache[assetKey] = fileCache[file];
                }
            }
            return assetPathCache[assetKey] = null;
        }
        public IAssetBundleLoaderAsync LoadAssetBundle() {
            return new AssetBundleLoaderLoadSuccessAsync();
        }
        public Object LoadAsset(string name) {
            return LoadAsset(name, typeof(UnityEngine.Object));
        }
        public T LoadAsset<T>(string name) where T : Object {
            return LoadAsset(name, typeof(T)) as T;
        }
        public Object LoadAsset(string name, Type type) {
            if (string.IsNullOrEmpty(name)) {
                logger.error($"asset name is null '{AssetsPath} - {name}'");
                return null;
            }
            var file = FindAsset(name, type);
            if (string.IsNullOrEmpty(file)) {
                logger.error($"can't found asset '{AssetsPath} - {name}'");
                return null;
            }
            var asset = UnityEditor.AssetDatabase.LoadAssetAtPath(file, type);
            if (asset == null) {
                logger.error($"can't load asset '{AssetsPath} - {name}'");
                return null;
            }
            return asset;
        }
        public IAssetBundleLoaderAsync LoadAssetAsync(string name) {
            return LoadAssetAsync(name, typeof(Object));
        }
        public IAssetBundleLoaderAsync LoadAssetAsync<T>(string name) where T : Object {
            return LoadAssetAsync(name, typeof(T));
        }
        public IAssetBundleLoaderAsync LoadAssetAsync(string name, Type type) {
            return new UnityEditorLoadAssetAsync(LoadAsset(name, type));
        }
        public void Unload(bool unloadAllLoadedObjects) {
        }
    }
}
#endif