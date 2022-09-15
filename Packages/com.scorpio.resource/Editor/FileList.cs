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
            public Asset() { }
            public Asset(string file) {
                this.file = file;
                this.size = new FileInfo(file).Length;
                this.md5 = FileUtil.GetMD5FromFile(file);
            }
        }
        public string ID;               //根据所有assets的size和md5计算一个MD5
        public string UnityVersion;     //Unity版本
        public Asset ABInfo;            //Blueprints专用,保存Blueprints.unity3d的信息
        public Dictionary<string, Asset> Assets = new Dictionary<string, Asset>();      //所有文件
        public virtual Asset AddAsset(string name, string file) {
            return Assets[name] = new Asset(file);
        }
        public virtual void Process() { }
    }
}

