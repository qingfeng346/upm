using System;
using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace Scorpio.Unity.Util {
    public delegate void HttpListener(int errorCode, string error, byte[] bytes, string url, DownloadHandler handler);
    public enum HttpMethod {
        GET,
        POST
    }
    public static partial class HttpUtil {
        public static int DefaultTimeout = 15;
        const string DefaultEncoding = "UTF-8";
        private static MonoBehaviour __RequestBehaviour = null;
        internal static MonoBehaviour RequestBehaviour {
            get {
                if (__RequestBehaviour == null) {
                    var gameObject = new GameObject("____HttpRequest");
                    GameObject.DontDestroyOnLoad(gameObject);
                    __RequestBehaviour = gameObject.AddComponent<MonoBehaviour>();
                }
                return __RequestBehaviour;
            }
        }
        public static void Shutdown() {
            if (__RequestBehaviour != null) {
                GameObject.DestroyImmediate(__RequestBehaviour.gameObject);
                __RequestBehaviour = null;
            }
        }
        public static void StopAll() {
            RequestBehaviour.StopAllCoroutines();
        }
        /// <summary> 网络字符转码 </summary>
        public static string urlencode(string data) { return urlencode(data, DefaultEncoding); }
        /// <summary> 网络字符转码 </summary>
        public static string urlencode(string data, string encoding) {
            try {
                return UriTranscoder.URLEncode(data, Encoding.GetEncoding(encoding));
            } catch (Exception e) {
                logger.error("urlencode is error data : {0}  {1}", data, e.ToString());
            }
            return "";
        }
        /// <summary> 网络字符解码 </summary>
        public static string urldecode(string data) { return urldecode(data, DefaultEncoding); }
        /// <summary> 网络字符解码 </summary>
        public static string urldecode(string data, string encoding) {
            try {
                return UriTranscoder.URLDecode(data, Encoding.GetEncoding(encoding));
            } catch (Exception e) {
                logger.error("urldecode is error data : {0}  {1}", data, e.ToString());
            }
            return "";
        }

        /// <summary> 网络字符转码 </summary>
        public static string qpencode(string data) { return qpencode(data, DefaultEncoding); }
        /// <summary> 网络字符转码 </summary>
        public static string qpencode(string data, string encoding) {
            try {
                return UriTranscoder.QPEncode(data, Encoding.GetEncoding(encoding));
            } catch (Exception e) {
                logger.error("qpencode is error data : {0}  {1}", data, e.ToString());
            }
            return "";
        }
        /// <summary> 网络字符解码 </summary>
        public static string qpdecode(string data) { return qpdecode(data, DefaultEncoding); }
        /// <summary> 网络字符解码 </summary>
        public static string qpdecode(string data, string encoding) {
            try {
                return UriTranscoder.QPDecode(data, Encoding.GetEncoding(encoding));
            } catch (Exception e) {
                logger.error("qpdecode is error data : {0}  {1}", data, e.ToString());
            }
            return "";
        }

        public static HttpRequest httpGet(string url, HttpListener listener) {
            return new HttpRequest().httpGet(url, listener);
        }
        public static HttpRequest httpPost(string url, string body, HttpListener listener) {
            return httpPost(url, body, DefaultEncoding, listener);
        }
        public static HttpRequest httpPost(string url, string body, string encoding, HttpListener listener) {
            return httpPost(url, Encoding.GetEncoding(encoding).GetBytes(body), listener);
        }
        public static HttpRequest httpPost(string url, byte[] body, HttpListener listener) {
            return new HttpRequest().httpPost(url, body, listener);
        }
        public static HttpRequest httpPost(string url, WWWForm formData, HttpListener listener) {
            return new HttpRequest().httpPost(url, formData, listener);
        }
    }
    public class HttpCertificate : CertificateHandler {
        protected override bool ValidateCertificate(byte[] certificateData) { return true; }
    }
    public class HttpRequest {
        public string Url { get; private set; }
        public HttpMethod Method { get; private set; }
        public HttpListener Listener { get; private set; }
        public UnityWebRequest Request { get; private set; }
        public bool IsDone { get; private set; }
        public float downloadProgress { get { return IsDone ? 1 : Request.downloadProgress; } }
        public long downloadedBytes { get { return IsDone ? -1 : Convert.ToInt64(Request.downloadedBytes); } }
        private void InitRequest(string url, HttpMethod method, HttpListener listener) {
            Url = url;
            Listener = listener;
            Method = method;
            IsDone = false;
            Request = new UnityWebRequest(url, Method.ToString());
            Request.timeout = HttpUtil.DefaultTimeout;
            Request.disposeDownloadHandlerOnDispose = true;
            Request.disposeUploadHandlerOnDispose = true;
            Request.disposeCertificateHandlerOnDispose = true;
            Request.SetRequestHeader("charset", "utf-8");
            Request.SetRequestHeader("Content-Type", "application/json");
            if (url.StartsWith("https")) {
                Request.certificateHandler = new HttpCertificate();
            }
            Request.downloadHandler = new DownloadHandlerBuffer();
        }
        public HttpRequest httpGet(string url, HttpListener listener) {
            InitRequest(url, HttpMethod.GET, listener);
            return this;
        }
        public HttpRequest httpPost(string url, byte[] postData, HttpListener listener) {
            InitRequest(url, HttpMethod.POST, listener);
            Request.uploadHandler = new UploadHandlerRaw(postData);
            return this;
        }
        public HttpRequest httpPost(string url, WWWForm formData, HttpListener listener) {
            InitRequest(url, HttpMethod.POST, listener);
            Request.uploadHandler = new UploadHandlerRaw(formData.data);
            foreach (var pair in formData.headers) {
                Request.SetRequestHeader(pair.Key, pair.Value);
            }
            return this;
        }
        public HttpRequest Send() {
#if UNITY_EDITOR
            if (!Application.isPlaying) {
                EditorCoroutineUtility.Start(Send_impl());
                return this;
            }
#endif
            HttpUtil.RequestBehaviour.StartCoroutine(Send_impl());
            return this;
        }
        IEnumerator Send_impl() {
            using (Request) {
                yield return Request.SendWebRequest();
                IsDone = true;
                if (Listener != null) {
                    try {
                        if (Request.isNetworkError) {
                            Listener(1, $"{Url} : {Request.error}", null, Url, Request.downloadHandler);
                        } else if (Request.downloadHandler is DownloadHandlerBuffer) {
                            if (Request.isHttpError || Request.responseCode != 200) {
                                Listener(2, $"{Url} responseCode : {Request.responseCode}", Request.downloadHandler.data, Url, Request.downloadHandler);
                            } else {
                                Listener(0, "", Request.downloadHandler.data, Url, Request.downloadHandler);
                            }
                        } else {
                            if (Request.isHttpError || Request.responseCode != 200) {
                                Listener(2, $"{Url} responseCode : {Request.responseCode}", null, Url, Request.downloadHandler);
                            } else {
                                Listener(0, "", null, Url, Request.downloadHandler);
                            }
                        }
                    } catch (Exception e) {
                        Listener(-1, $"{Url} : {e}", null, Url, Request.downloadHandler);
                    } finally {
                        Listener = null;
                    }
                }
            }
        }
    }
}