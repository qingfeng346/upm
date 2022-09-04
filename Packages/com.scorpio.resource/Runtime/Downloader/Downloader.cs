using System;
using UnityEngine.Networking;
using Scorpio.Unity.Util;
namespace Scorpio.Resource {
    public class Downloader {
        public static int RequestTimeout = 120;
        public string[] urls;
        public string filePath;
        public string versionPath;
        public string version;
        public Action success;
        public void StartDownload(Action<bool> callback) {
            //需要下载就要删掉version文件，避免回滚配置 File 有下载未完的情况
            FileUtil.DeleteFile(versionPath);
            FileUtil.CreateDirectoryByFile(filePath);
            Download(0, callback);
        }
        void Download(int index, Action<bool> callback) {
            var request = HttpUtil.httpGet(urls[index], (errorCode, error, bytes, url, handler) => {
                if (errorCode == 0) {
                    FileUtil.CreateFile(versionPath, version);
                    callback(true);
                } else if (index < urls.Length - 1) {
                    Download(++index, callback);
                } else {
                    callback(false);
                }
            });
            FileUtil.DeleteFile(filePath);
            request.Request.timeout = RequestTimeout;
            request.Request.downloadHandler = new DownloadHandlerFile(filePath);
            request.Send();
        }
    }
}