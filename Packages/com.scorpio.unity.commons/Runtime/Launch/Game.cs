using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using Scorpio.Timer;
using Scorpio.Pool;
public static class Game
{
    public const string CACHE_VERSION_KEY = "__CacheVersionKey";
    public static object LaunchArgs { get; private set; } //启动参数
    public static GameObject GlobalGameObject { get; private set; } //全局gameObject父级，调用restart会销毁
    
    public static Dictionary<string, object> GlobalValues = new Dictionary<string, object>(); //全局变量,本地启动不清空

    //以下event重启时会清除掉
    public static event UnityAction<Scene, LoadSceneMode> sceneLoaded;
    public static event UnityAction<Scene> sceneUnloaded;
    public static event UnityAction<Scene, Scene> activeSceneChanged;
    public static event Application.LogCallback logMessageReceived;
    public static event Application.LowMemoryCallback lowMemory;
    public static event Func<bool> wantsToQuit;
    public static event Action<Rect, Rect> safeAreaChanged;
    public static event Action escape;    //点击esc键
    public static event Action<bool> applicationPause;
    public static event Action<bool> applicationFocus;
    public static event Action<ScreenOrientation, ScreenOrientation> screenRotated;

    public static Rect lastSafeArea = Rect.zero;
    public static ScreenOrientation lastScreenOrientation = ScreenOrientation.AutoRotation;

    //初始化全局参数
    public static void Launch(object args) {
        EngineUtil.InvariantCulture();
        logger.info("====================================启动游戏====================================");
        SceneManager.sceneLoaded += (scene, mode) => {
            if (mode == LoadSceneMode.Single) {
                PoolManager.Instance.Shutdown();
                PoolManager.Instance.Initialize();
            }
            if (sceneLoaded != null) { sceneLoaded(scene, mode); }
        };
        SceneManager.sceneUnloaded += (scene) => {
            if (sceneUnloaded != null) {
                sceneUnloaded(scene);
            }
        };
        SceneManager.activeSceneChanged += (scene1, scene2) => {
            if (activeSceneChanged != null) {
                activeSceneChanged(scene1, scene2);
            }
        };
        Application.logMessageReceived += (condition, stackTrace, type) => { 
            if (logMessageReceived != null) { 
                logMessageReceived(condition, stackTrace, type); 
            }
        };
        Application.lowMemory += () => { 
            if (lowMemory != null) {
                lowMemory();
            }
        };
        Application.wantsToQuit += () => { 
            return (wantsToQuit != null) ? wantsToQuit() : true; 
        };
        Application.quitting += () => {
            logger.info("====================================游戏退出====================================");
            ScriptManager.Instance.OnQuit();
        };
        LaunchArgs = args;
        lastSafeArea = Screen.safeArea;
        StartGame();
    }
    static void ResetEvents() {
        sceneLoaded = null;
        sceneUnloaded = null;
        activeSceneChanged = null;
        logMessageReceived = null;
        lowMemory = null;
        wantsToQuit = null;
        safeAreaChanged = null;
        escape = null;
        applicationPause = null;
        applicationFocus = null;
        screenRotated = null;
    }
    //新版本首次启动
    static void InitializeNewGame() {
        ResourceManager.Instance.InitializeNewGame();
    }
    //开始游戏，重启游戏
    public static void StartGame() {
        var version = EngineUtil.BuildVersion;
        if (PlayerPrefs.GetString(CACHE_VERSION_KEY, "") != version) {
            logger.info($"新版本 {version} 首次启动");
            InitializeNewGame();
            PlayerPrefs.SetString(CACHE_VERSION_KEY, version);
        }
        ResetEvents();
        InitGlobalGameObject();
        SoundManager.Instance.Shutdown();
        ResourceManager.Instance.Shutdown();
        PoolManager.Instance.Shutdown();
        ScriptManager.Instance.Shutdown();
        LooperManager.Instance.Shutdown();
        TimerManager.Instance.Shutdown();
        SoundManager.Instance.Initialize();
        ResourceManager.Instance.Initialize();
        PoolManager.Instance.Initialize();
        ScriptManager.Instance.Initialize();
    }
    //退出游戏
    public static void QuitGame() {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit ();
#endif
    }
    //进入游戏
    public static void EnterGame() {
        InitGlobalGameObject();
        ResourceManager.Instance.Shutdown();
        PoolManager.Instance.Shutdown();
        ResourceManager.Instance.Initialize();
        PoolManager.Instance.Initialize();
        ScriptManager.Instance.Enter();
    }
    static void InitGlobalGameObject() {
        if (GlobalGameObject != null) { EngineUtil.DestroyImmediate(GlobalGameObject); }
        EngineUtil.DontDestroyOnLoad(GlobalGameObject = new GameObject("__Global"));
    }
    public static GameObject NewGameObject(string name) {
        var gameObject = new GameObject(name);
        gameObject.transform.SetParent(GlobalGameObject.transform);
        return gameObject;
    }

    public static object GetGlobalValue(string name) {
        if (GlobalValues.TryGetValue(name, out var obj)) {
            return obj;
        }
        return null;
    }
    public static object SetGlobalValue(string name, object value) {
        return GlobalValues[name] = value;
    }
    public static object TrySetGlobalValue(string name, object value) {
        if (GlobalValues.TryGetValue(name, out var obj)) {
            return obj;
        }
        return GlobalValues[name] = value;
    }
    public static void DelGlobalValue(string name) {
        if (GlobalValues.ContainsKey(name)) {
            GlobalValues.Remove(name);
        }
    }
    public static void Update() {
#if UNITY_EDITOR
        if (Input.GetKeyDown(KeyCode.Escape) && (!Input.GetMouseButton(0) && !Input.GetMouseButton(1) && !Input.GetMouseButton(2)))
#else
        if (Input.GetKeyDown (KeyCode.Escape) && Input.touchCount == 0)
#endif
        {
            if (escape != null) { escape(); }
        }
        if (lastScreenOrientation != Screen.orientation) {
            var last = lastScreenOrientation;
            lastScreenOrientation = Screen.orientation;
            if (screenRotated != null) { screenRotated(last, lastScreenOrientation); }
        }
        if (lastSafeArea != Screen.safeArea) {
            var last = lastSafeArea;
            lastSafeArea = Screen.safeArea;
            if (safeAreaChanged != null) { safeAreaChanged(last, lastSafeArea); }
        }
        ScriptManager.Instance.Update();
    }
    public static void OnApplicationPause(bool pause) {
        if (applicationPause != null) {
            applicationPause(pause);
        }
    }
    public static void OnApplicationFocus(bool focus) {
        if (applicationFocus != null) {
            applicationFocus(focus);
        }
    }
}