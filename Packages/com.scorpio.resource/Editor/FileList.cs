using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using Scorpio.Unity.Util;
namespace Scorpio.Resource.Editor {
    public class FileList {
        public class Asset {
            [JsonIgnore] public string file;
            public long size;
            public string md5;
        }
        public string ID;               //根据所有assets的size和md5计算一个MD5
        public string UnityVersion;     //Unity版本
        public Dictionary<string, Asset> Assets = new Dictionary<string, Asset>();
        public virtual Asset AddAsset(string name, string file) {
            var info = new Asset();
            info.file = file;
            info.size = new FileInfo(file).Length;
            info.md5 = FileUtil.GetMD5FromFile(file);
            return Assets[name] = info;
        }
        public virtual void Process() { }
    }
}

