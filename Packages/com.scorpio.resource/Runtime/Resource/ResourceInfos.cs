using System.Collections.Generic;
namespace Scorpio.Resource {
    //包体内的patch
    public class FixedPatchInfo {
        public class Asset {
            public string name;
            public string md5;
        }
        public List<Asset> Assets = new List<Asset>();
    }
    //更新文件
    public class FileList {
        public class Asset {
            public string size;
            public string md5;
            public string GetName(string file) {
                return $"{file}_{size}_{md5}";
            }
        }
        public Dictionary<string, Asset> Assets = new Dictionary<string, Asset>();
    }
    public enum AssetBundleType {
        Storage,        //本地文件
        Web,            //网络文件
    }
    //网络AssetBundle
    public class AssetBundleInfo {
        public AssetBundleInfo(AssetBundleType type) {
            this.type = type;
        }
        public AssetBundleType type;    //AB类型
        public string filePath;         //本地目录
        public string[] urls;           //下载链接
        public string version;          //文件版本号
        public bool queue;              //是否是队列下载
    }
}
