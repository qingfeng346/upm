using System;
using System.Collections.Generic;
using Scorpio.Unity.Util;
using UnityEngine;

namespace Scorpio.Resource {
    public class DownloaderManager : MonoBehaviour {
        public class Timer {
            public float end;
            public Action callback;
        }
        private static DownloaderManager instance = null;
        public static DownloaderManager Instance {
            get {
                if (instance == null) {
                    var obj = new GameObject("__DownloaderManager");
                    DontDestroyOnLoad(obj);
                    instance = obj.AddComponent<DownloaderManager>();
                }
                return instance;
            }
        }
        public static int MaxDownloadQueue = 8;
        private List<Downloader> downloaders = new List<Downloader>();
        private List<Timer> timers = new List<Timer>();
        private int version = 0;
        private int downloadCount = 0;          //正在下载的数量
        public void Shutdown() {
            downloaders.Clear();
            version++;
        }
        public void Download(string[] urls, string filePath, string version, Action success) {
            var versionFile = $"{filePath}.v";
            if (FileUtil.FileExist(filePath) && FileUtil.GetFileString(versionFile) == version) {
                success?.Invoke();
            } else {
                downloaders.Insert(0, new Downloader() { urls = urls, filePath = filePath, versionPath = versionFile, version = version, success = success });
                CheckDownload();
            }
        }
        void CheckDownload() {
            if (downloaders.Count == 0 || downloadCount > MaxDownloadQueue) { return; }
            ++downloadCount;
            var downloader = downloaders[0];
            downloaders.RemoveAt(0);
            int version = this.version;
            downloader.StartDownload((success) => {
                if (version != this.version) { return; }
                --downloadCount;
                if (success) {
                    CheckDownload();
                    downloader.success?.Invoke();
                } else {
                    //下载失败,移到队尾,一会重试
                    downloaders.Add(downloader);
                    CheckDownload();
                }
            });
        }
        public void AddTimer(float seconds, Action callback) {
            timers.Add(new Timer() { end = Time.realtimeSinceStartup + seconds, callback = callback });
        }
        void Update() {
            var now = Time.realtimeSinceStartup;
            for (var i = 0; i < timers.Count;) {
                var timer = timers[i];
                if (now > timer.end) {
                    timers.RemoveAt(i);
                    timer.callback();
                    continue;
                }
                ++i;
            }
        }
    }
}