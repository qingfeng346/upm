using Newtonsoft.Json;

namespace Scorpio.Resource.Editor {
    public static class BuilderUtil {
        public static readonly JsonSerializerSettings JsonSerializerSettings = new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore };
        public static bool IsInvalidFile(this string file) {
            file = file.ToLower();
            if (file.EndsWith(".meta") || file.EndsWith(".ds_store") || file.EndsWith(".tpsheet"))
                return true;
            return false;
        }
        public static string ToJson(this FileList fileList) {
            return JsonConvert.SerializeObject(fileList, JsonSerializerSettings);
        }
    }
}
