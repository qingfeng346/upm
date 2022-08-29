using UnityEngine;
using UnityEditor;
using System.Reflection;
using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.IO;

namespace Scorpio.Unity.Command {
    public static class CommandBuild {
        public const BuildAssetBundleOptions ASSETBUNDLE_OPTIONS = BuildAssetBundleOptions.DeterministicAssetBundle | BuildAssetBundleOptions.ChunkBasedCompression;
        public const BuildAssetBundleOptions ASSETBUNDLE_REBUILD_OPTIONS = BuildAssetBundleOptions.DeterministicAssetBundle | BuildAssetBundleOptions.ChunkBasedCompression | BuildAssetBundleOptions.ForceRebuildAssetBundle;
        
        public delegate void CommandAction(CommandLine commandLine);
        private static Dictionary<string, List<Delegate>> commands = new Dictionary<string, List<Delegate>>();
        public static event CommandAction PreAction;
        public static event CommandAction PostAction;
        public static event CommandAction FinallyAction;
        public static uint MaxErrorLength = 2048;
        public static readonly JsonSerializerSettings JsonSerializerSettings = new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore };
        //Build结果
        public class BuildResultData {
            public int code;                    //返回值
            public string error;                //具体错误
            public object result;               //具体返回
            public string BundleID;             //BundleID
            public string ProductName;          //ProductName
            public string Version;              //版本号
            public string AndroidVersionCode;   //android version code
            public string IOSBuildNumber;       //ios build number
            public Dictionary<string, string> files;    //所有下载文件
            public string SetError(string message) {
                this.code = 1;
                if (string.IsNullOrEmpty(message)) {
                    this.error = message;
                } else {
                    this.error = message.Length > MaxErrorLength ? message.Substring(0, (int)MaxErrorLength) : message;
                }
                return message;
            }
            public void AddFile(string filePath) {
                AddFile(Path.GetFileNameWithoutExtension(filePath), filePath);
            }
            public void AddFile(string name, string filePath) {
                if (files == null) {
                    files = new Dictionary<string, string>();
                }
                files[name] = Path.GetFullPath(filePath);
            }
        }
        //[InitializeOnLoadMethod]
        static void CollectCommands() {
            var duplicateAssemblies = new HashSet<string>();
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies()) {
                if (!duplicateAssemblies.Contains(assembly.FullName)) {
                    duplicateAssemblies.Add(assembly.FullName);
                    CollectCommands(assembly);
                }
                foreach (var name in assembly.GetReferencedAssemblies()) {
                    if (!duplicateAssemblies.Contains(name.FullName)) {
                        duplicateAssemblies.Add(name.FullName);
                        CollectCommands(Assembly.Load(name));
                    }
                }
            }
        }
        static void CollectCommands(Assembly assembly) {
            if (assembly.FullName.StartsWith("UnityEngine") || assembly.FullName.StartsWith("System.")) { return; }
            foreach (var type in assembly.GetTypes()) {
                CollectCommands(type);
            }
        }
        static void CollectCommands(Type type) {
            foreach (var method in type.GetMethods(BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public)) {
                var commandAttribute = method.GetCustomAttribute<CommandAttribute>();
                if (commandAttribute != null) {
                    try {
                        AddCommandDelegate(commandAttribute.name, Delegate.CreateDelegate(type, method));
                    } catch (Exception) {
                        Debug.LogError($"Method {method} is not CommandAction");
                    }
                }
                var commandFinallyAttribute = method.GetCustomAttribute<CommandFinallyAttribute>();
                if (commandFinallyAttribute != null) {
                    try {
                        FinallyAction += (CommandAction)method.CreateDelegate(typeof(CommandAction));
                    } catch (Exception) {
                        Debug.LogError($"Method {method} is not CommandAction");
                    }
                }
            }
        }
        static string Now => DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        public static DateTime Time { get; private set; }
        public static string BuildID => Time.ToString("yyyyMMdd-HHmmss");
        public static string BuildTime => Time.ToString("yyyy-MM-dd HH:mm:ss");
        public static string BuildVersion => Time.ToString("MMddHH");
        public static string Record { get; set; }                                //运行记录的文件
        public static CommandLine CommandLine { get; private set; }
        public static string CommandName { get; private set; }
        public static BuildTarget BuildTarget {
            get {
#if UNITY_ANDROID
                return BuildTarget.Android;
#elif UNITY_IOS
                return BuildTarget.iOS;
#elif UNITY_WEBGL
                return BuildTarget.WebGL;
#elif UNITY_UWP || UNITY_WSA
                return BuildTarget.WSAPlayer;
#elif UNITY_STANDALONE_WIN
                return BuildTarget.StandaloneWindows;
#elif UNITY_STANDALONE_OSX
                return BuildTarget.StandaloneOSX;
#elif UNITY_STANDALONE_LINUX
                return BuildTarget.StandaloneLinuxUniversal;
#else
                return BuildTarget.StandaloneWindows;
#endif
            }
        }
        public static BuildTargetGroup BuildTargetGroup {
            get {
#if UNITY_ANDROID
                return BuildTargetGroup.Android;
#elif UNITY_IOS
                return BuildTargetGroup.iOS;
#elif UNITY_WEBGL
                return BuildTargetGroup.WebGL;
#elif UNITY_UWP || UNITY_WSA
                return BuildTargetGroup.WSA;
#else
                return BuildTargetGroup.Standalone;
#endif
            }
        }
        public static BuildResultData BuildResult { get; private set; }
        public static void SetResult(object result) {
            BuildResult.result = result;
        }
        public static void AddResultFile(string name, string file) {
            BuildResult.AddFile(name, file);
        }
        public static void AddCommand(string name, Action commandAction) {
            AddCommandDelegate(name, commandAction);
        }
        public static void AddCommand<T>(string name, Action<T> commandAction) {
            AddCommandDelegate(name, commandAction);
        }
        public static void AddCommand<T1, T2>(string name, Action<T1, T2> commandAction) {
            AddCommandDelegate(name, commandAction);
        }
        public static void AddCommand<T1, T2, T3>(string name, Action<T1, T2, T3> commandAction) {
            AddCommandDelegate(name, commandAction);
        }
        public static void AddCommand<T1, T2, T3, T4>(string name, Action<T1, T2, T3, T4> commandAction) {
            AddCommandDelegate(name, commandAction);
        }
        public static void AddCommand<T1, T2, T3, T4, T5>(string name, Action<T1, T2, T3, T4, T5> commandAction) {
            AddCommandDelegate(name, commandAction);
        }
        public static void AddCommand<T1, T2, T3, T4, T5, T6>(string name, Action<T1, T2, T3, T4, T5, T6> commandAction) {
            AddCommandDelegate(name, commandAction);
        }
        public static void AddCommand<T1, T2, T3, T4, T5, T6, T7>(string name, Action<T1, T2, T3, T4, T5, T6, T7> commandAction) {
            AddCommandDelegate(name, commandAction);
        }
        public static void AddCommand<T1, T2, T3, T4, T5, T6, T7, T8>(string name, Action<T1, T2, T3, T4, T5, T6, T7, T8> commandAction) {
            AddCommandDelegate(name, commandAction);
        }
        public static void AddCommand<T1, T2, T3, T4, T5, T6, T7, T8, T9>(string name, Action<T1, T2, T3, T4, T5, T6, T7, T8, T9> commandAction) {
            AddCommandDelegate(name, commandAction);
        }
        public static void AddCommand<TResult>(string name, Func<TResult> commandAction) {
            AddCommandDelegate(name, commandAction);
        }
        public static void AddCommand<T, TResult>(string name, Func<T, TResult> commandAction) {
            AddCommandDelegate(name, commandAction);
        }
        public static void AddCommand<T1, T2, TResult>(string name, Func<T1, T2, TResult> commandAction) {
            AddCommandDelegate(name, commandAction);
        }
        public static void AddCommand<T1, T2, T3, TResult>(string name, Func<T1, T2, T3, TResult> commandAction) {
            AddCommandDelegate(name, commandAction);
        }
        public static void AddCommand<T1, T2, T3, T4, TResult>(string name, Func<T1, T2, T3, T4, TResult> commandAction) {
            AddCommandDelegate(name, commandAction);
        }
        public static void AddCommand<T1, T2, T3, T4, T5, TResult>(string name, Func<T1, T2, T3, T4, T5, TResult> commandAction) {
            AddCommandDelegate(name, commandAction);
        }
        public static void AddCommand<T1, T2, T3, T4, T5, T6, TResult>(string name, Func<T1, T2, T3, T4, T5, T6, TResult> commandAction) {
            AddCommandDelegate(name, commandAction);
        }
        public static void AddCommand<T1, T2, T3, T4, T5, T6, T7, TResult>(string name, Func<T1, T2, T3, T4, T5, T6, T7, TResult> commandAction) {
            AddCommandDelegate(name, commandAction);
        }
        public static void AddCommand<T1, T2, T3, T4, T5, T6, T7, T8, TResult>(string name, Func<T1, T2, T3, T4, T5, T6, T7, T8, TResult> commandAction) {
            AddCommandDelegate(name, commandAction);
        }
        public static void AddCommand<T1, T2, T3, T4, T5, T6, T7, T8, T9, TResult>(string name, Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TResult> commandAction) {
            AddCommandDelegate(name, commandAction);
        }
        public static void AddCommandDelegate(string name, Delegate commandAction) {
            if (string.IsNullOrEmpty(name) || commandAction == null) { return; }
            if (!commands.TryGetValue(name, out var delegates)) {
                commands[name] = delegates = new List<Delegate>();
            }
            if (!delegates.Contains(commandAction)) {
                delegates.Add(commandAction);
            }
            //Debug.Log($"添加Command命令 {name} - {commandAction.GetMethodInfo()}");
        }
        static CommandLine ParseCommand(string[] lineArgs) {
            var args = new List<string>();
            var first = false;
            Array.ForEach(lineArgs, (value) => {
                if (value == "--args") {
                    first = true;
                } else if (first) {
                    args.Add(value);
                }
            });
            return CommandLine.Parse(args.ToArray());
        }
        static object[] GetParameters(Delegate dele, CommandLine commandLine) {
            var parameters = dele.GetMethodInfo().GetParameters();
            var args = new object[parameters.Length];
            for (var i = 0; i < parameters.Length; ++i) {
                var param = parameters[i];
                var name = $"-{param.Name}";
                var paramType = param.ParameterType;
                if (paramType == typeof(CommandLine)) {
                    args[i] = commandLine;
                } else if (commandLine.HadValue(name)) {
                    args[i] = commandLine.GetValue(name, paramType);
                } else if (param.HasDefaultValue) {
                    args[i] = param.DefaultValue;
                } else {
                    args[i] = paramType.IsValueType ? Activator.CreateInstance(paramType) : null;
                }
            }
            return args;
        }
        public static void Execute() {
            ExecuteWithArgs(Environment.GetCommandLineArgs());
            Application.Quit(BuildResult.code);
        }
        public static void ExecuteWithArgs(string[] args) {
            var resultFile = "";
            var stopWatch = System.Diagnostics.Stopwatch.StartNew();
            try {
                BuildResult = new BuildResultData();
                Debug.Log("命令 : " + string.Join(" ", args));
                CommandLine = ParseCommand(args);
                CommandName = CommandLine.GetValue("-executeType");
                resultFile = CommandLine.GetValue("-result");
                if (double.TryParse(CommandLine.GetValue("-time"), out var time)) {
                    Time = TimeZoneInfo.ConvertTime(new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc).AddMilliseconds(time), TimeZoneInfo.Utc, TimeZoneInfo.Local);
                } else {
                    Time = DateTime.Now;
                }
                Debug.Log($"[{Now}]===================[{BuildID}][开始执行:{CommandName}]===================");
                if (commands.TryGetValue(CommandName, out var delegates)) {
                    PreAction?.Invoke(CommandLine);
                    foreach (var dele in delegates) {
                        dele.DynamicInvoke(GetParameters(dele, CommandLine));
                    }
                } else {
                    throw new Exception($"未知的操作类型 : {CommandName}");
                }
                BuildResult.BundleID = PlayerSettings.GetApplicationIdentifier(BuildTargetGroup);
                BuildResult.ProductName = PlayerSettings.productName;
                BuildResult.Version = PlayerSettings.bundleVersion;
                BuildResult.AndroidVersionCode = PlayerSettings.Android.bundleVersionCode.ToString();
                BuildResult.IOSBuildNumber = PlayerSettings.iOS.buildNumber;
                if (!string.IsNullOrEmpty(Record)) {
                    CommandRecord.GetInstance(Record).UpdateCommandTime(CommandName, Now);
                    AssetDatabase.SaveAssets();
                }
                PostAction?.Invoke(CommandLine);
            } catch (Exception e) {
                var error = BuildResult.SetError(e.ToString());
                Debug.LogError($"执行出错 : {error}");
            } finally {
                FinallyAction?.Invoke(CommandLine);
            }
            Debug.Log($"[{Now}]===================[{BuildID}][执行完成:{CommandName},总耗时:{stopWatch.ElapsedMilliseconds / 1000}s]===================");
            if (!string.IsNullOrEmpty(resultFile)) {
                File.WriteAllText(resultFile, JsonConvert.SerializeObject(BuildResult, JsonSerializerSettings));
            }
            AssetDatabase.Refresh();
        }
    }
}