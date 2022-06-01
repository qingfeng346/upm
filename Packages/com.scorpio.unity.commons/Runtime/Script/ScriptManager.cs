using System.Reflection;
using System.Text;
using System.Collections;
using System.Collections.Generic;

using Scorpio;
using Scorpio.Coroutine;
using UnityEngine;
using Scorpio.Serialize;
using Scorpio.Compile.Compiler;
using Scorpio.Unity.Commons;
using System.IO;

public class ScriptManager : Singleton<ScriptManager> {
    public const string ScriptPath = "Assets/Sco";
    private abstract class output {
        private Script script;
        public output (Script script) {
            this.script = script;
        }
        protected string Builder (ScriptValue[] args, int length, bool stack) {
            var builder = new StringBuilder ();
            var stackInfo = script.GetStackInfo ();
            builder.Append ($"{stackInfo.Breviary}:{stackInfo.Line} ");
            for (int i = 0; i < length; ++i) {
                if (i != 0) { builder.Append ("    "); }
                builder.Append (args[i].ToString ());
            }
            if (stack) {
                var stackInfos = script.GetStackInfos ();
                for (var i = stackInfos.Length - 2; i >= 0; --i) {
                    builder.Append ($@"
    {stackInfos[i].Breviary}:{stackInfos[i].Line}");
                }
            }
            return builder.ToString ();
        }
    }
    private class debug : output, ScorpioHandle {
        public debug (Script script) : base (script) { }
        public ScriptValue Call (ScriptValue thisObject, ScriptValue[] args, int length) {
            logger.debug (Builder(args, length, false));
            return ScriptValue.Null;
        }
    }
    private class debugln : output, ScorpioHandle {
        public debugln (Script script) : base (script) { }
        public ScriptValue Call (ScriptValue thisObject, ScriptValue[] args, int length) {
            logger.debug (Builder(args, length, true));
            return ScriptValue.Null;
        }
    }
    private class print : output, ScorpioHandle {
        public print (Script script) : base (script) { }
        public ScriptValue Call (ScriptValue thisObject, ScriptValue[] args, int length) {
            logger.info (Builder(args, length, false));
            return ScriptValue.Null;
        }
    }
    private class println : output, ScorpioHandle {
        public println (Script script) : base (script) { }
        public ScriptValue Call (ScriptValue thisObject, ScriptValue[] args, int length) {
            logger.info (Builder(args, length, true));
            return ScriptValue.Null;
        }
    }
    private class logWarn : output, ScorpioHandle {
        public logWarn (Script script) : base (script) { }
        public ScriptValue Call (ScriptValue thisObject, ScriptValue[] args, int length) {
            logger.warn (Builder(args, length, true));
            return ScriptValue.Null;
        }
    }
    private class logError : output, ScorpioHandle {
        public logError (Script script) : base (script) { }
        public ScriptValue Call (ScriptValue thisObject, ScriptValue[] args, int length) {
            logger.error (Builder(args, length, true));
            return ScriptValue.Null;
        }
    }
    private class require : ScorpioHandle {
        public ScriptValue Call (ScriptValue thisObject, ScriptValue[] args, int length) {
            return Instance.LoadFile (args[0].ToString ());
        }
    }
    private class CoroutineProcessor : ICoroutineProcessor {
        object current;
        ICoroutine coroutine;
        AsyncOperation asyncOperation;
        public void SetCurrent(object current) {
            if (this.current == current) { return; }
            this.current = current;
            if (current is ICoroutine) {
                coroutine = current as ICoroutine;
            } else if (current is AsyncOperation) {
                asyncOperation = current as AsyncOperation;
            }
        }
        public bool MoveNext(IEnumerator enumerator) {
            if (coroutine != null) {
                if (!coroutine.IsDone) {
                    return true;
                }
            } else if (asyncOperation != null) {
                if (!asyncOperation.isDone) {
                    return true;
                }
            }
            return enumerator.MoveNext();
        }
    }
    #if UNITY_EDITOR
    public static CompileOption LoadCompileOption(bool editor) {
        var defines = new List<string>(new string[] {
#if UNITY_ANDROID
            "UNITY_ANDROID",
#elif UNITY_IOS
            "UNITY_IOS",
#endif
        });
        if (editor) {
            defines.Add("UNITY_EDITOR");
        }
        var script = new Script();
        script.LoadLibraryV1();
        script.PushSearchPath("Assets/ScriptScorpio/Constant");
        var path = CommonScorpioPath;
        var searchPaths = string.IsNullOrEmpty(path) ? null : new [] { path };
        return new CompileOption() { 
            defines = defines, 
            searchPaths = searchPaths,
            scriptConst = script.LoadConst("Constant.sco"),
            preprocessImportFile = (parser, fileName) => {
                if (!fileName.EndsWith(".im.sco")) {
                    throw new Scorpio.Compile.Exception.ParserException(parser, $"import file 后缀名必须是 .im.sco: {fileName}", parser.PeekToken());
                }
            }
        };
    }
#endif
    public static string CommonScorpioPath {
        get {
            if (FileUtil.PathExist("Packages/com.scorpio.common"))
                return "Packages/com.scorpio.common/ScriptScorpio";
            foreach (var dir in Directory.GetDirectories("Library/PackageCache")) {
                var name = Path.GetFileName(dir);
                if (name.StartsWith("com.scorpio.common")) {
                    return dir + "/ScriptScorpio";
                }
            }
            return null;
        }
    }

    public Script Script { get; private set; } //脚本引擎
    public bool Started { get; private set; } //是否已经启动脚本
    public bool Ended { get; private set; } //是否已经释放脚本
    private HashSet<string> LoadedFiles = new HashSet<string> (); //已经加载的文件列表
    public ScriptManager() {
        ScorpioTypeManager.PushAssembly (GetType ().Assembly);
        ScorpioTypeManager.PushAssembly (typeof (UnityEngine.AI.NavMesh).Assembly); //UnityEngine.AIModule.dll
        ScorpioTypeManager.PushAssembly (typeof (UnityEngine.Animation).Assembly); //UnityEngine.AnimationModule.dll
        ScorpioTypeManager.PushAssembly (typeof (UnityEngine.AssetBundle).Assembly); //UnityEngine.AssetBundleModule.dll
        ScorpioTypeManager.PushAssembly (typeof (UnityEngine.AudioClip).Assembly); //UnityEngine.AudioModule.dll

        ScorpioTypeManager.PushAssembly (typeof (UnityEngine.GameObject).Assembly); //UnityEngine.CoreModule.dll (已设置非裁剪)
        ScorpioTypeManager.PushAssembly (typeof (UnityEngine.CrashReportHandler.CrashReportHandler).Assembly); //UnityEngine.CrashReportingModule.dll

        ScorpioTypeManager.PushAssembly (typeof (UnityEngine.Playables.PlayableDirector).Assembly); //UnityEngine.DirectorModule.dll

        ScorpioTypeManager.PushAssembly (typeof (UnityEngine.Social).Assembly); //UnityEngine.GameCenterModule.dll
        ScorpioTypeManager.PushAssembly (typeof (UnityEngine.Grid).Assembly); //UnityEngine.GridModule.dll

        ScorpioTypeManager.PushAssembly (typeof (UnityEngine.ImageConversion).Assembly); //UnityEngine.ImageConversionModule.dll
        ScorpioTypeManager.PushAssembly (typeof (UnityEngine.GUI).Assembly); //UnityEngine.IMGUIModule.dll

        ScorpioTypeManager.PushAssembly (typeof (UnityEngine.Input).Assembly); //UnityEngine.InputLegacyModule.dll

        ScorpioTypeManager.PushAssembly (typeof (UnityEngine.JsonUtility).Assembly); //UnityEngine.JSONSerializeModule.dll

        ScorpioTypeManager.PushAssembly (typeof (UnityEngine.LocalizationAsset).Assembly); //UnityEngine.LocalizationModule.dll

        ScorpioTypeManager.PushAssembly (typeof (UnityEngine.ParticleSystem).Assembly); //UnityEngine.ParticleSystemModule.dll(已设置非裁剪)
        ScorpioTypeManager.PushAssembly (typeof (UnityEngine.BoxCollider2D).Assembly); //UnityEngine.Physics2DModule.dll(已设置非裁剪)
        ScorpioTypeManager.PushAssembly (typeof (UnityEngine.BoxCollider).Assembly); //UnityEngine.PhysicsModule.dll(已设置非裁剪)
        ScorpioTypeManager.PushAssembly (typeof (UnityEngine.ScreenCapture).Assembly); //UnityEngine.ScreenCaptureModule.dll
        ScorpioTypeManager.PushAssembly (typeof (UnityEngine.SpriteMask).Assembly); //UnityEngine.SpriteMaskModule.dll
        ScorpioTypeManager.PushAssembly (typeof (UnityEngine.U2D.SpriteShapeUtility).Assembly); //UnityEngine.SpriteShapeModule.dll

        ScorpioTypeManager.PushAssembly (typeof (UnityEngine.Terrain).Assembly); //UnityEngine.TerrainModule.dll(已设置非裁剪)
        ScorpioTypeManager.PushAssembly (typeof (UnityEngine.TerrainCollider).Assembly); //UnityEngine.TerrainPhysicsModule.dll(已设置非裁剪)

        ScorpioTypeManager.PushAssembly (typeof (UnityEngine.TextCore.FaceInfo).Assembly); //UnityEngine.TextCoreModule.dll
        ScorpioTypeManager.PushAssembly (typeof (UnityEngine.Font).Assembly); //UnityEngine.TextRenderingModule.dll
        ScorpioTypeManager.PushAssembly (typeof (UnityEngine.Tilemaps.Tilemap).Assembly); //UnityEngine.TilemapModule.dll(已设置非裁剪)

        ScorpioTypeManager.PushAssembly (typeof (UnityEngine.UIElements.TextElement).Assembly); //UnityEngine.UIElementsModule.dll
        ScorpioTypeManager.PushAssembly (typeof (UnityEngine.Canvas).Assembly); //UnityEngine.UIModule.dll(已设置非裁剪)
        ScorpioTypeManager.PushAssembly (typeof (UnityEngine.Networking.NetworkError).Assembly); //UnityEngine.UNETModule.dll

        ScorpioTypeManager.PushAssembly (typeof (UnityEngine.Networking.UnityWebRequestAssetBundle).Assembly); //UnityEngine.UnityWebRequestAssetBundleModule.dll(已设置非裁剪)
        ScorpioTypeManager.PushAssembly (typeof (UnityEngine.Networking.DownloadHandlerAudioClip).Assembly); //UnityEngine.UnityWebRequestAudioModule.dll(已设置非裁剪)
        ScorpioTypeManager.PushAssembly (typeof (UnityEngine.WWWForm).Assembly); //UnityEngine.UnityWebRequestModule.dll(已设置非裁剪)
        ScorpioTypeManager.PushAssembly (typeof (UnityEngine.Networking.DownloadHandlerTexture).Assembly); //UnityEngine.UnityWebRequestTextureModule.dll(已设置非裁剪)

        ScorpioTypeManager.PushAssembly (typeof (UnityEngine.Video.VideoClip).Assembly); //UnityEngine.VideoModule.dll

        //Package Manager
        ScorpioTypeManager.PushAssembly (typeof (UnityEngine.UI.CanvasScaler).Assembly); //UnityEngine.UI.dll(已设置非裁剪)
        ScorpioTypeManager.PushAssembly (typeof (System.IO.Compression.ZipFile).Assembly); //System.IO.Compression.FileSystem.dll

        foreach (var assemblyName in GetType ().Assembly.GetReferencedAssemblies ()) {
            try {
                ScorpioTypeManager.PushAssembly (Assembly.Load (assemblyName));
                // logger.info($"加载程序集成功 : {assemblyName.FullName}");
            } catch (System.Exception) {
                logger.error ($"加载程序集失败 : {assemblyName.FullName}");
            }
        }
    }
    public void Initialize () {
        Script = new Scorpio.Script ();
        Script.LoadLibraryV1 ();
        Script.CoroutineProcessor = new CoroutineProcessor();
        Script.SetGlobal ("debug", Script.CreateFunction (new debug (Script)));
        Script.SetGlobal ("debugln", Script.CreateFunction (new debugln (Script)));
        Script.SetGlobal ("print", Script.CreateFunction (new print (Script)));
        Script.SetGlobal ("println", Script.CreateFunction (new println (Script)));
        Script.SetGlobal ("logWarn", Script.CreateFunction (new logWarn (Script)));
        Script.SetGlobal ("logError", Script.CreateFunction (new logError (Script)));
        Script.SetGlobal ("require", Script.CreateFunction (new require ()));
        LoadedFiles.Clear ();
        Started = true;
        LoadFileInternal("Start.sco");
        CallInternal("Start");
    }
    public void Shutdown () {
        if (!Started) { return; }
        Started = false;
        Call ("Shutdown");
        Script.Shutdown();
        Script = null;
    }
    public void Enter () {
        LoadedFiles.Clear ();
        LoadFile ("Enter.sco");
        Call ("Enter");
    }
    public ScriptValue Call (string func, params object[] args) {
        if (!Started || Ended) return ScriptValue.Null;
        try {
            return Script.call (func, args);
        } catch (System.Exception e) {
            logger.error ($"CallFunction [{func}] is error stack : {e}");
        }
        return ScriptValue.Null;
    }
    public ScriptValue Call (ScriptFunction func, params object[] args) {
        if (!Started || Ended) return ScriptValue.Null;
        try {
            return func.call (ScriptValue.Null, args);
        } catch (System.Exception e) {
            logger.error ($"CallFunction [{func}] is error stack : {e.ToString()}");
        }
        return ScriptValue.Null;
    }
    private ScriptValue CallInternal(string func, params object[] args) {
        return Script.call(func, args);
    }
    public ScriptValue LoadFile (string file) {
        if (Script == null) { 
            return ScriptValue.Null;
        }
        try {
            if (LoadedFiles.Contains (file))
                return ScriptValue.Null;
            LoadedFiles.Add (file);
            return LoadFileInternal(file);
        } catch (System.Exception e) {
            logger.error ($"LoadFile [{file}] is error : {e}");
        }
        return ScriptValue.Null;
    }
    private ScriptValue LoadFileInternal(string file) {
#if UNITY_EDITOR && !UNITY_WEB_ASSETS
        var filePath = $"{ScriptPath}/{file}";
        if (file.EndsWith(".im.sco")) {
            throw new System.Exception($"{file} is import file, 只能使用 #import 引入");
        }
        if (!FileUtil.FileExist(filePath)) {
            filePath = $"{CommonScorpioPath}/{file}";
            if (!FileUtil.FileExist(filePath)) {
                throw new System.Exception($"{file} not found");
            }
        }
        return Script.Execute(Serializer.Serialize(filePath, 
            FileUtil.GetFileString(filePath, Encoding.UTF8), 
            new string[] { ScriptPath }, 
            LoadCompileOption(true)));
#else
        return Script.LoadBuffer (file, ResourceManager.Instance.LoadBlueprints ($"scripts/{file}.bytes"));
#endif
    }
    public byte[] GetTableBuffer (string file) {

        try {
#if UNITY_EDITOR && !UNITY_WEB_ASSETS
            return ResourceManager.Instance.LoadResource<TextAsset> ("blueprints", $"tables/{file}").bytes;
#else
            return ResourceManager.Instance.LoadBlueprints ($"tables/{file}.bytes");
#endif
        } catch (System.Exception e) {
            logger.error ($"LoadTableFile [{file}] is error : {e}");
        }
        return null;
    }
    //程序退出
    public void OnQuit () {
        Ended = true;
    }
    public string GetStackInfo() {
        var builder = new StringBuilder ();
        var stackInfos = Script.GetStackInfos ();
        for (var i = stackInfos.Length - 1; i >= 0; --i) {
            builder.Append ($@"
{stackInfos[i].Breviary}:{stackInfos[i].Line}");
        }
        return builder.ToString ();
    }
    public void Update() {
        if (Script != null) {
            Script.UpdateCoroutine();
        }
    }
}