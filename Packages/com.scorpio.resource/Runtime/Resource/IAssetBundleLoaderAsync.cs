using UnityEngine;
namespace Scorpio.Resource {
    public interface IAssetBundleLoaderAsync {
        bool isDone { get; }
        float progress { get; }
        Object asset { get; }
    }
    public class AssetBundleLoaderLoadSuccessAsync : IAssetBundleLoaderAsync {
        public bool isDone => true;
        public float progress => 1;
        public Object asset => null;
    }
}