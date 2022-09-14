using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using Scorpio.Timer;
using Scorpio.Unity.Util;
namespace Commons.Util {
    /// <summary> 下载文件类 </summary>
    public class DownloadFile {
        // 每次读取的字节大小
        private const int READ_LENGTH = 4096;

        /// <summary> 下载文件回调 只有不保存文件的时候 buffer才有值</summary>
        public delegate void DownloadCallback(int errorCode, string errorMessage, string fileName, byte[] buffer);

        private string m_url;                       //下载链接
        private string m_fileName;                  //保存的文件名称,如果没值则不保存文件
        private string m_fileVersion;               //断点续传文件服务器版本号,如果没值则不支持断点续传
        private DownloadCallback m_callBack = null;     //下载完成的回调


        private long lastSpeedTime = 0;             //上次请求下载速度的时间点
        private long lastLength = 0;                //上次请求下载速度的文件大小
        private long downSpeed = 0;                 //下载速度（如果当前请求距离上次请求距离少于一毫秒则返回此值）
        private Thread m_thread;                    //下载线程
        private Stream m_fileStream = null;         //本地文件流
        private Stream m_httpStream = null;         //网络文件流

        /// <summary> 下载一个文件 (不保存文件 只会返回byte[])</summary>
        public DownloadFile(string url, DownloadCallback callBack) : this(url, null, null, callBack) { }
        /// <summary> 下载一个文件 </summary>
        public DownloadFile(string url, string fileName, DownloadCallback callBack) : this(url, fileName, null, callBack) { }
        /// <summary>
        /// 下载一个文件
        /// </summary>
        /// <param name="url">下载链接</param>
        /// <param name="fileFullName">文件保存路径</param>
        /// <param name="fileVersion">文件版本号</param>
        /// <param name="callBack">下载完成回调</param>
        public DownloadFile(string url, string fileName, string fileVersion, DownloadCallback callBack) {
            m_url = url;
            m_fileName = fileName;
            m_fileVersion = fileVersion;
            m_callBack = callBack;
            m_thread = new Thread(Download);
            m_thread.Start();
        }
        /// <summary> 销毁 </summary>
        public void Destroy() {
            lock (this) {
                if (m_thread != null && m_thread.IsAlive) {
                    m_thread.Abort();
                }
                m_thread = null;
                DestroyStream();
            }
        }
        /// <summary> 下载结束 </summary>
		void Finshed(int errorCode, string errorMessage, byte[] buffer) {
            if (errorCode == 0) {
                logger.info ($"下载文件成功 url:{m_url}");
            } else {
                logger.error($"下载文件失败 url:{m_url} code:{errorCode} message:{errorMessage}");
            }
            IsFinished = true;
            try {
                //回调到主线程
                LooperManager.Instance.Run(_ => {
                    m_callBack?.Invoke(errorCode, errorMessage, m_fileName, buffer);
                });
            } catch (Exception e) {
                logger.error($"下载文件 {m_url} CallBack is error : " + e.ToString());
            }
            DestroyStream();
        }
        void DestroyStream() {
            if (m_fileStream != null) {
                m_fileStream.Dispose();
                m_fileStream = null;
            }
            if (m_httpStream != null) {
                m_httpStream.Dispose();
                m_httpStream = null;
            }
        }
        /// <summary> 获得下载进度(百分比) </summary>
        public float Percent {
            get {
                if (IsFinished)
                    return 1;
                else if (FileLength <= 0)
                    return 0;
                return Convert.ToSingle(Convert.ToDouble(DownLength) / Convert.ToDouble(FileLength));
            }
        }
        /// <summary> 获得当前下载速度 </summary>
        public long Speed {
            get {
                if (IsFinished) return 0;
                long length = DownLength - lastLength;
                long nowTime = Environment.TickCount;
                long time = nowTime - lastSpeedTime;
                if (time <= 100) return downSpeed;
                lastSpeedTime = nowTime;
                lastLength = DownLength;
                downSpeed = length * 1000 / time;
                return downSpeed;
            }
        }
        /// <summary> 已经下载的大小 </summary>
        public long DownLength { get; private set; } = 0;
        /// <summary> 要下载文件的大小 </summary>
        public long FileLength { get; private set; } = 0;
        /// <summary> 是否已经下载完成 </summary>
        public bool IsFinished { get; private set; } = false;
        /// <summary> 下载文件 </summary>
        void Download() {
            //打开网络连接 
            try {
                var startPosition = 0L;
                var bytes = new byte[READ_LENGTH];                      //每次读取的数据长度
                var saveFile = !string.IsNullOrEmpty(m_fileName);       //是否保存文件
                var breakPoint = !string.IsNullOrEmpty(m_fileVersion);  //是否使用断点续传
                string versionFile = null;                              //断点续传的版本保存文件
                string tempFile = null;                                 //断点续传的临时保存文件
                if (saveFile) {
                    versionFile = $"{m_fileName}.version";
                    tempFile = $"{m_fileName}.temp";
                    if (breakPoint) {
                        string version = FileUtil.GetFileString(versionFile, Encoding.UTF8);
                        if (string.IsNullOrEmpty(version) || version != m_fileVersion) {
                            FileUtil.DeleteFile(versionFile);
                            FileUtil.DeleteFile(tempFile);
                        }
                        FileUtil.CreateFile(versionFile, m_fileVersion, Encoding.UTF8);
                    } else {
                        FileUtil.DeleteFile(versionFile);
                        FileUtil.DeleteFile(tempFile);
                    }
                    if (FileUtil.FileExist(tempFile)) {
                        m_fileStream = File.OpenWrite(tempFile);
                        startPosition = m_fileStream.Length;
                        m_fileStream.Seek(startPosition, SeekOrigin.Current); //移动文件流中的当前指针
                    } else {
                        startPosition = 0;
                        FileUtil.CreateDirectoryByFile(tempFile);
                        m_fileStream = new FileStream(tempFile, FileMode.Create);
                    }
                } else {
                    startPosition = 0;
                    m_fileStream = new MemoryStream();
                }
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(m_url);
                if (startPosition > 0) request.AddRange(startPosition);
                request.BeginGetResponse((result) => {
                    try {
                        HttpWebResponse response = (HttpWebResponse)request.EndGetResponse(result);
                        FileLength = response.ContentLength + startPosition;
                        DownLength = startPosition;
                        logger.info($"开始下载文件:{m_url}  续传进度:{DownLength}/{FileLength}");
                        m_httpStream = response.GetResponseStream();
                        int readSize = 0;
                        while (m_httpStream != null) {
                            readSize = m_httpStream.Read(bytes, 0, READ_LENGTH);
                            if (readSize <= 0) break;
                            DownLength += readSize;
                            lock (this) {
                                if (m_fileStream != null) {
                                    m_fileStream.Write(bytes, 0, readSize);
                                }
                            }
                        }
                        if (DownLength < FileLength) {
                            throw new Exception($"文件未下载完全 已下载大小:{DownLength}/{FileLength}");
                        }
                        if (saveFile) {
                            DestroyStream();
                            FileUtil.MoveFile(tempFile, m_fileName, true);
                            FileUtil.DeleteFile(versionFile);
                            FileUtil.DeleteFile(tempFile);
                            Finshed(0, null, null);
                        } else {
                            Finshed(0, null, ((MemoryStream)m_fileStream).ToArray());
                        }
                    } catch (WebException e) {
                        Finshed((int)e.Status, e.ToString(), null);
                    } catch (Exception e) {
                        Finshed(-1, e.ToString(), null);
                    }
                }, null);
            } catch (Exception e) {
                Finshed(-1, e.ToString(), null);
            }
        }
    }
}