using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using Scorpio.Unity.Logger;

public delegate void HttpListener (int errorCode, string error, byte[] bytes, string url);

public enum HttpMethod {
    GET,
    POST
}
public static partial class HttpUtil {
    public static int DefaultTimeout = 15;
    const string DefaultEncoding = "UTF-8";
    private static MonoBehaviour __Request = null;
    private static MonoBehaviour Request {
        get {
            if (__Request == null) {
                var gameObject = new GameObject ("____HttpRequest");
                GameObject.DontDestroyOnLoad (gameObject);
                __Request = gameObject.AddComponent<HttpRequestManager> ();
            }
            return __Request;
        }
    }
    /// <summary> 网络字符转码 </summary>
    public static string urlencode (string data) { return urlencode (data, DefaultEncoding); }
    /// <summary> 网络字符转码 </summary>
    public static string urlencode (string data, string encoding) {
        try {
            return UriTranscoder.URLEncode (data, Encoding.GetEncoding (encoding));
        } catch (Exception e) {
            logger.error ("urlencode is error data : {0}  {1}", data, e.ToString ());
        }
        return "";
    }
    /// <summary> 网络字符解码 </summary>
    public static string urldecode (string data) { return urldecode (data, DefaultEncoding); }
    /// <summary> 网络字符解码 </summary>
    public static string urldecode (string data, string encoding) {
        try {
            return UriTranscoder.URLDecode (data, Encoding.GetEncoding (encoding));
        } catch (Exception e) {
            logger.error ("urldecode is error data : {0}  {1}", data, e.ToString ());
        }
        return "";
    }

    /// <summary> 网络字符转码 </summary>
    public static string qpencode (string data) { return qpencode (data, DefaultEncoding); }
    /// <summary> 网络字符转码 </summary>
    public static string qpencode (string data, string encoding) {
        try {
            return UriTranscoder.QPEncode (data, Encoding.GetEncoding (encoding));
        } catch (Exception e) {
            logger.error ("qpencode is error data : {0}  {1}", data, e.ToString ());
        }
        return "";
    }
    /// <summary> 网络字符解码 </summary>
    public static string qpdecode (string data) { return qpdecode (data, DefaultEncoding); }
    /// <summary> 网络字符解码 </summary>
    public static string qpdecode (string data, string encoding) {
        try {
            return UriTranscoder.QPDecode (data, Encoding.GetEncoding (encoding));
        } catch (Exception e) {
            logger.error ("qpdecode is error data : {0}  {1}", data, e.ToString ());
        }
        return "";
    }

    public static HttpRequest httpGet (string url, HttpListener listener) {
        return new HttpRequest (Request).httpGet (url, listener);
    }
    public static HttpRequest httpPost (string url, string body, HttpListener listener) {
        return httpPost (url, body, DefaultEncoding, listener);
    }
    public static HttpRequest httpPost (string url, string body, string encoding, HttpListener listener) {
        return httpPost (url, Encoding.GetEncoding (encoding).GetBytes (body), listener);
    }
    public static HttpRequest httpPost (string url, byte[] body, HttpListener listener) {
        return new HttpRequest (Request).httpPost (url, body, listener);
    }
    public static HttpRequest httpPost (string url, WWWForm formData, HttpListener listener) {
        return new HttpRequest (Request).httpPost (url, formData, listener);
    }
}
public class HttpRequestManager : MonoBehaviour { }
public class HttpCertificate : CertificateHandler {  
    protected override bool ValidateCertificate (byte[] certificateData) { return true; }
}
public class HttpRequest {
    public string Url { get; private set; }
    public HttpMethod Method { get; private set; }
    public HttpListener Listener { get; private set; }
    public UnityWebRequest Request { get; private set; }
    public bool IsDone { get; private set; }
    public float downloadProgress { get { return IsDone ? 1 : Request.downloadProgress; } }
    public long downloadedBytes { get { return IsDone ? -1 : Convert.ToInt64 (Request.downloadedBytes); } }
    private MonoBehaviour coroutine;
    public HttpRequest (MonoBehaviour coroutine) {
        this.coroutine = coroutine;
    }
    private void InitRequest (string url, HttpMethod method, HttpListener listener) {
        Url = url;
        Listener = listener;
        Method = method;
        IsDone = false;
        Request = new UnityWebRequest (url, Method.ToString ());
        Request.timeout = HttpUtil.DefaultTimeout;
        Request.SetRequestHeader ("charset", "utf-8");
        Request.SetRequestHeader ("Content-Type", "application/json");
        if (url.StartsWith("https")) {
            Request.certificateHandler = new HttpCertificate();
        }
        Request.downloadHandler = new DownloadHandlerBuffer ();
    }
    public HttpRequest httpGet (string url, HttpListener listener) {
        InitRequest (url, HttpMethod.GET, listener);
        return this;
    }
    public HttpRequest httpPost (string url, byte[] postData, HttpListener listener) {
        InitRequest (url, HttpMethod.POST, listener);
        Request.uploadHandler = new UploadHandlerRaw (postData);
        return this;
    }
    public HttpRequest httpPost (string url, WWWForm formData, HttpListener listener) {
        InitRequest (url, HttpMethod.POST, listener);
        Request.uploadHandler = new UploadHandlerRaw (formData.data);
        foreach (var pair in formData.headers) {
            Request.SetRequestHeader(pair.Key, pair.Value);
        }
        return this;
    }
    public HttpRequest Send () {
        coroutine.StartCoroutine (Send_impl ());
        return this;
    }
    IEnumerator Send_impl () {
        Request.disposeDownloadHandlerOnDispose = true;         //需要手动释放Download
        Request.disposeUploadHandlerOnDispose = true;
        Request.disposeCertificateHandlerOnDispose = true;
        using (Request) {
            yield return Request.SendWebRequest ();
            IsDone = true;
            if (Listener != null) {
                try {
                    if (Request.isNetworkError) {
                        Listener (1, $"{Url} : {Request.error}", null, Url);
                    } else if (Request.isHttpError || Request.responseCode != 200) {
                        Listener (2, $"{Url} responseCode : {Request.responseCode}", Request.downloadHandler.data, Url);
                    } else {
                        Listener (0, "", Request.downloadHandler.data, Url);
                    }
                } catch (Exception e) {
                    logger.error ($"HttpRequest is error Url : {Url}  {e}");
                }
                Listener = null;
            }
        }
    }
}