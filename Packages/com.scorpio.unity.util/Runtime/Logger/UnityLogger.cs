using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using System.Threading;

namespace Scorpio.Unity.Util {
    public class UnityLogger : ILogger {
        public static int BackupReserveDays = 3;            //玩家日志最多保留几天
        public static string PersistentLogPath;             //玩家日志保存目录
        public static string PersistentBackupLogPath;       //玩家日志备份目录
        public static string PersistentLogArchivePath;      //玩家日志上传文件路径
        private class UnityLoggerInfo {
            private object sync = new object();
            private FileStream m_FileStream; //文件stream
            public string FileName { get; private set; }            //文件名字
            public string FullFileName { get; private set; }        //完整文件路径
            public string TimeKey { get; private set; }             //隔一天文件要重新创建,只有重新启动检测
            public UnityLoggerInfo(string fileName) {
                FileName = $"{fileName}.log";
                FullFileName = $"{PersistentLogPath}{FileName}";
                TimeKey = $"__logTime_{fileName}";
            }
            internal void Initialize() {
                lock (sync) {
                    long now = TimeUtil.Unix;
                    if (PlayerPrefs.HasKey(TimeKey)) {
                        if (long.TryParse(PlayerPrefs.GetString(TimeKey), out var time) && !TimeUtil.IsSameDay(now, time)) {
                            FileUtil.MoveFile(FullFileName, PersistentBackupLogPath + TimeUtil.GetUtcDateString(time, "yyyy-MM-dd") + $"/{FileName}", true);
                            FileUtil.DeleteEmptyFolder(PersistentBackupLogPath, true);
                            CheckBackup();
                            FileUtil.CreateFile(FullFileName, "");
                        }
                    }
                    PlayerPrefs.SetString(TimeKey, now.ToString());
                    m_FileStream = new FileStream(FullFileName, FileMode.Append);
                }
            }
            void CheckBackup() {
                if (Directory.Exists(PersistentBackupLogPath)) {
                    var dirs = new List<string>(Directory.GetDirectories(PersistentBackupLogPath));
                    if (dirs.Count < BackupReserveDays) { return; }
                    dirs.Sort();
                    for (int i = 0; i < dirs.Count - BackupReserveDays; ++i) {
                        FileUtil.DeleteFolder(dirs[i], null, true);
                    }
                }
            }
            internal void Write(byte[] bytes) {
                if (bytes == null || bytes.Length == 0) return;
                lock (sync) {
                    if (m_FileStream != null) {
                        m_FileStream.Write(bytes, 0, bytes.Length);
                        m_FileStream.Flush();
                    }
                }
            }
            internal void Destroy() {
                lock (sync) {
                    if (m_FileStream != null) {
                        m_FileStream.Dispose();
                        m_FileStream = null;
                    }
                }
            }
            internal void BackupFile() {
                lock (sync) {
                    if (m_FileStream != null) {
                        m_FileStream.Dispose();
                        m_FileStream = null;
                    }
                    try {
                        FileUtil.CopyFile(FullFileName, Path.Combine(PersistentBackupLogPath, FileName), true);
                    } finally {
                        m_FileStream = new FileStream(FullFileName, FileMode.Append);
                    }
                }
            }
        }
        private static object sync = new object();                      //异步锁
        private UnityLoggerInfo console;                                //写入日志
        private StringBuilder builder;                                  //日志缓存
        private Thread writeThread;                                     //写入日志文件线程
        public event Action<string, string, LogType> loggerReceived;    //日志回调
        private bool isCompressing = false;                             //是否正在压缩日志文件
        public event Action<string, string> compressFinished;           //压缩日志文件结束回调
        public string Prefix { get; set; } = "";                        //日志前缀
        public bool PrintConsole { get; set; } = true;                  //是否打印到控制台
        public UnityLogger() {
            PersistentLogPath = PathUtil.InternalDataPath + "Log/";
            PersistentBackupLogPath = PersistentLogPath + "Backup/";
            PersistentLogArchivePath = PathUtil.InternalDataPath + "LogArchive/";
            FileUtil.CreateDirectory(PersistentLogPath);
            FileUtil.DeleteFolder(PersistentLogArchivePath, null, true);
            FileUtil.CreateDirectory(PersistentLogArchivePath);
            builder = new StringBuilder();
            builder.AppendLine($@"============================={TimeUtil.GetUtcNowDateString()}=============================
Now Time = {TimeUtil.GetNowDateString()}
TimeZone = {TimeZoneInfo.Local}
platform = {Application.platform}
bundleIdentifier = {Application.identifier}
version = {Application.version}
systemLanguage = {Application.systemLanguage}
operatingSystem = {SystemInfo.operatingSystem}
processorType = {SystemInfo.processorType}
systemMemorySize = {SystemInfo.systemMemorySize} mb
graphicsDeviceVersion = {SystemInfo.graphicsDeviceVersion}
graphicsDeviceName = {SystemInfo.graphicsDeviceName}
graphicsMemorySize = {SystemInfo.graphicsMemorySize} mb
deviceModel = {SystemInfo.deviceModel}
deviceName = {SystemInfo.deviceName}
batteryLevel = {SystemInfo.batteryLevel}
persistentDataPath = {Application.persistentDataPath}");
            console = new UnityLoggerInfo("console");
            console.Initialize();
            Application.logMessageReceivedThreaded += OnLoggerReceived;
            //每1000毫秒写入一次文件
            writeThread = new Thread(WriteToFileThread);
        }
        public void Shutdown() {
            Application.logMessageReceivedThreaded -= OnLoggerReceived;
            WriteToFileThread();
            console.Destroy();
            if (writeThread != null && writeThread.IsAlive) {
                writeThread.Abort();
            }
            writeThread = null;
        }
        public void debug(string msg) {
            if (PrintConsole) {
                Debug.Log(msg);
            } else {
                OnLoggerReceived(msg, "", LogType.Log);
            }
        }
        public void info(string msg) {
            if (PrintConsole) {
                Debug.Log(msg);
            } else {
                OnLoggerReceived(msg, "", LogType.Log);
            }
        }
        public void warn(string msg) {
            if (PrintConsole) {
                Debug.LogWarning(msg);
            } else {
                OnLoggerReceived(msg, "", LogType.Warning);
            }
        }
        public void error(string msg) {
            if (PrintConsole) {
                Debug.LogError(msg);
            } else {
                OnLoggerReceived(msg, "", LogType.Error);
            }
        }

        void OnLoggerReceived(string condition, string stackTrace, LogType type) {
            lock (sync) {
                builder.AppendFormat("[{0}]{1}[{2}] {3}\n", DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss:fff"), Prefix, type, condition);
                //如果是warning 或 error 立即写入文件，否则等待时间间隔写入
                if (type == LogType.Warning || type == LogType.Error || type == LogType.Exception) {
                    WriteToFileThread();
                }
                if (loggerReceived != null) {
                    loggerReceived(condition, stackTrace, type);
                }
            }
        }
        void WriteToFileThread() {
            byte[] data = null;
            lock (sync) {
                if (builder.Length > 0) {
                    data = Encoding.UTF8.GetBytes(builder.ToString());
                    builder.Length = 0;
                }
            }
            console.Write(data);
        }
        public void Compress() {
            lock (sync) {
                if (isCompressing) { return; }
                isCompressing = true;
                WriteToFileThread();
                console.BackupFile();
                var fileName = $"{PersistentLogArchivePath}{Guid.NewGuid()}.zip";
                ThreadPool.QueueUserWorkItem((state) => {
                    try {
                        FileUtil.DeleteEmptyFolder(PersistentBackupLogPath, true);
                        System.IO.Compression.ZipFile.CreateFromDirectory(PersistentBackupLogPath, fileName);
                        compressFinished?.Invoke(null, fileName);
                    } catch (Exception e) {
                        compressFinished?.Invoke(e.ToString(), fileName);
                    } finally {
                        isCompressing = false;
                    }
                });
            }
        }
    }
}