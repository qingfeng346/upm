using Newtonsoft.Json;
using Scorpio.Unity.Util;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using UnityEngine.Networking;

namespace Scorpio.Resource {
    public class DownloadAssets {
        private const double KB_LENGTH = 1024; //1KB 的字节数
        private const double MB_LENGTH = 1048576; //1MB 的字节数
        private const double GB_LENGTH = 1073741824; //1GB 的字节数
        public enum Status {
            None,                       //
            RequestAssets,              //开始请求资源列表
            CheckAssets,                //开始检测需要下载的资源列表
            CheckAssetsOver,            //检测资源列表完成
            PreDownload,                //准备好下载
            Download,                   //开始下载文件
            DownloadOver,               //下载所有文件完成
            Finished,                   //所有下载执行完成或者没有需要下载的文件
            Error,                      //下载失败，网络错误等错误
            Over,                       //结束
        }
        public class DownloadFileInfo : FileList.Asset {
            public string name;
        }
        private string[] fileListUrls;      //filelist 下载地址
        private string[] assetsUrls;        //assets 下载地址
        private FileList packageAssets;     //包内的资源列表
        private FileList storageAssets;     //本地的资源列表
        private FileList remoteAssets;      //远程资源列表
        private string downloadPath;        //下载保存目录
        private string storagePath;         //最终保存目录
        private bool isBlueprints;          //是否是Blueprints
        private bool isFixedPatch;
        private List<DownloadFileInfo> downloadFiles;   //需要下载的所有文件
        private HttpRequest downloadRequest;            //当前正在下载的网络链接
        private int downloadIndex;                      //当前下载的索引
        private long downloadSize;                      //已下载大小
        private long downloadTotal;                     //总下载大小
        private Status status;
        public DownloadAssets(string[] fileListUrls,
                              string[] assetsUrls,
                              FileList packageAssets,
                              FileList storageAssets,
                              string downloadPath,
                              string storagePath,
                              bool isBlueprints,
                              bool isFixedPatch) {
            this.fileListUrls = fileListUrls;
            this.assetsUrls = assetsUrls;
            this.packageAssets = packageAssets ?? new FileList();
            this.storageAssets = storageAssets ?? new FileList();
            this.downloadPath = downloadPath;
            this.storagePath = storagePath;
            this.isBlueprints = isBlueprints;
            this.isFixedPatch = isFixedPatch;
            this.downloadFiles = new List<DownloadFileInfo>();
        }
        public bool isOver => status == Status.Over;
        int downloadFileCount => downloadFiles.Count;
        public void SetStatus(Status status) {
            SetStatus(status, null);
        }
        public void SetStatus(Status status, string error) {
            if (this.status != status) {
            }
        }
        //运行出错
        void ShowError(string error) {
            logger.error("下载出错 : ${error}");
            SetStatus(Status.Error, error);
        }
        string GetMemory(long by) {
            if (by < MB_LENGTH)
                return string.Format("{0:f2} KB", Convert.ToDouble(by) / KB_LENGTH);
            else if (by < GB_LENGTH)
                return string.Format("{0:f2} MB", Convert.ToDouble(by) / MB_LENGTH);
            else
                return string.Format("{0:f2} GB", Convert.ToDouble(by) / GB_LENGTH);
        }
        void HttpGetFallback_impl(string[] urls, string file, int timeout, HttpListener listener, Action<HttpRequest> updateRequest) {
            var index = 0;
            var errorMsg = new StringBuilder();
            Action<string> httpGet = null;
            httpGet = (httpUrl) => {
                var request = HttpUtil.httpGet(httpUrl, (errorCode, error, bytes, url, handler) => {
                    if (errorCode != 0) {
                        if (index < urls.Length) {
                            errorMsg.AppendLine(error);
                            httpGet($"{urls[index++]}{file}");
                        } else {
                            errorMsg.AppendLine(error);
                            listener(errorCode, errorMsg.ToString(), bytes, url, handler);
                        }
                    } else {
                        listener(errorCode, error, bytes, url, handler);
                    }
                });
                if (timeout >= 0) {
                    request.Request.timeout = timeout;
                }
                updateRequest?.Invoke(request);
                request.Send();
            };
            httpGet($"{urls[index++]}{file}");
        }
        void HttpGetFallback(string[] urls, HttpListener listener) {
            HttpGetFallback_impl(urls, "", -1, listener, null);
        }
        void DownloadFileFallback(string[] urls, string file, string storeageFile, HttpListener listener, Action<HttpRequest> updateRequest) {
            HttpGetFallback_impl(urls, file, 0, listener, (request) => {
                request.Request.downloadHandler = new DownloadHandlerFile(storeageFile);
                updateRequest?.Invoke(request);
            });
        }
        public void Execute() {
            if (isOver) { return; }
            SetStatus(Status.RequestAssets);
            HttpGetFallback(fileListUrls, (errorCode, error, bytes, url, handler) => {
                if (isOver) { return; }
                if (errorCode != 0) {
                    ShowError("请求列表失败 : ${error}");
                    return;
                }
                var content = Encoding.UTF8.GetString(bytes, 0, bytes.Length);
                try {
                    remoteAssets = JsonConvert.DeserializeObject<FileList>(content);
                } catch (Exception) {
                    ShowError($"文件下载错误,json解析失败 {url} : {content}");
                    return;
                }
                try {
                    CheckAssets();
                } catch (Exception e) {
                    this.ShowError($"CheckAssets错误 : {e}");
                }
            });
        }
        void Finished() {
            storageAssets.Version = remoteAssets.Version;
            storageAssets.BuildID = remoteAssets.BuildID;
            this.SetStatus(Status.Finished);
        }
        void CheckAssets() {
            logger.info("检测资源下载");
            SetStatus(Status.CheckAssets);
            downloadFiles.Clear();     //需要下载的文件列表
            foreach (var pair in remoteAssets.Assets) {
                var file = pair.Key;
                var remoteAsset = pair.Value;
                var downloadFile = $"{downloadPath}/{file}";    //下载目录保存文件
                var storageFile = $"{storagePath}/{file}";      //最终目录保存文件
                if (packageAssets.Assets.TryGetValue(file, out var packageAsset)) {
                    if (packageAsset.md5 == remoteAsset.md5) {
                        storageAssets.Assets.Remove(file);
                        FileUtil.DeleteFile(downloadFile);
                        return;
                    }
                }
                if (storageAssets.Assets.TryGetValue(file, out var storageAsset)) {
                    if (storageAsset.md5 == remoteAsset.md5) {
                        FileUtil.DeleteFile(downloadFile);
                        return;
                    }
                }
                //比较已下载文件是否和服务器相同
                if (FileUtil.FileExist(downloadFile) && remoteAsset.md5 == FileUtil.GetMD5FromFile(downloadFile)) {
                    storageAssets.Assets[file] = new FileList.Asset() { md5 = remoteAsset.md5, size = remoteAsset.size };
                    return;
                }
                downloadFiles.Add(new DownloadFileInfo() { name = file, md5 = remoteAsset.md5, size = remoteAsset.size });
            }
            SetStatus(Status.CheckAssetsOver);
            if (downloadFileCount == 0) {
                Finished();
                return;
            }
            downloadIndex = 0;         //当前正在下载的文件索引
            downloadSize = 0;          //已经下载的文件总大小
            downloadTotal = 0;         //需要下载的总大小
            //如果是下载Blueprints并且下载数量超过20个,则直接下载blueprints.unity3d
            if (isBlueprints && remoteAssets.ABInfo != null && downloadFileCount > 20) {
                //this.isDownloadBlueprintsAB = true                 //是否是下载的blueprints.unity3d
                var blueprintsPath = $"{downloadPath}/blueprints.unity3d";
                //比较需要下载的blueprints.unity3d 和 已下载的 blueprints.unity3d 是否相同
                if (FileUtil.FileExist(blueprintsPath) && remoteAssets.ABInfo.md5 == FileUtil.GetMD5FromFileStream(blueprintsPath)) {
                    Finished();
                    return;
                }
                downloadFiles.Clear();
                downloadFiles.Add(new DownloadFileInfo() { name = "blueprints.unity3d", size = remoteAssets.ABInfo.size, md5 = remoteAssets.ABInfo.md5 });
            }
            downloadFiles.ForEach((value) => {
                downloadTotal += value.size;
            });
            logger.info($"需要下载文件数量 : ${downloadFileCount}  总大小 : ${GetMemory(downloadTotal)}");
            SetStatus(Status.PreDownload);
        }
        public void StartDownloadFile() {
            if (isOver) { return; }
            SetStatus(Status.Download);
            DownloadFile();
        }
        void DownloadFile() {
            if (isOver) { return; }
            if (downloadIndex >= downloadFileCount) {
                DownloadFilesFinished();
                return;
            }
            var downloadFileInfo = downloadFiles[downloadIndex];
            var filePath = $"{downloadPath}/{downloadFileInfo.name}";
            var fileUrl = downloadFileInfo.GetName(downloadFileInfo.name);
            logger.info("开始下载新文件:${file.name}  大小:${Util.GetMemory(file.size)}  MD5:${file.md5}  URL:${fileUrl}");
            DownloadFileFallback(assetsUrls, fileUrl, filePath, (errorCode, error, bytes, url, handler) => {
                if (isOver) { return; }
                downloadRequest = null;
                if (errorCode != 0) {
                    ShowError($"下载文件 {url} 失败 : {error}");
                    return;
                }
                var downloadMD5 = FileUtil.GetMD5FromFileStream(filePath);
                if (downloadFileInfo.md5 != downloadMD5) {
                    ShowError($"文件下载错误 {url} ,MD5比对错误 url:{downloadFileInfo.md5} -> download:{downloadMD5}");
                    return;
                }
                downloadSize += downloadFileInfo.size;
                DownloadFileOver(downloadFileInfo);
            },
            (request) => {
                downloadRequest = request;
            });
        }
        void DownloadFileOver(DownloadFileInfo file) {
            storageAssets.Assets[file.name] = new FileList.Asset() { md5 = file.md5, size = file.size };
            downloadIndex++;
            DownloadFile();
        }
        void DownloadFilesFinished() {
            SetStatus(Status.DownloadOver);
            Finished();
        }
    }
}
