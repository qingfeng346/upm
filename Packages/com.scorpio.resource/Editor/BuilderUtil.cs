using Newtonsoft.Json;
using System.IO;
using Scorpio.Unity.Util;

namespace Scorpio.Resource.Editor {
    public static class BuilderUtil {
        public static readonly JsonSerializerSettings JsonSerializerSettings = new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore };
        public static bool IsInvalidFile(this string file) {
            file = file.ToLower();
            if (file.EndsWith(".meta") || file.EndsWith(".ds_store") || file.EndsWith(".tpsheet"))
                return true;
            return false;
        }
        public static string ToJson(this object obj) {
            return JsonConvert.SerializeObject(obj, JsonSerializerSettings);
        }
        public static FileList.Asset GetAsset(string file) {
            return new FileList.Asset() { size = new FileInfo(file).Length, md5 = FileUtil.GetMD5FromFile(file) };
        }
    }
}
