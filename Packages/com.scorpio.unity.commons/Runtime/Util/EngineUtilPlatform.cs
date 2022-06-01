using UnityEngine;

public enum PlatformType {
    IOS = 1,                        //IOS设备 iPhone iPad
    Android = 2,                    //Android设备
    UWP = 3,                        //UWP应用
    WebGL = 4,                      //WebGL
    StandaloneWindows = 5,          //Windows
    StandaloneOSXUniversal = 7,     //Mac 通用
    StandaloneLinuxUniversal = 8,	//Linux 通用
}
public static partial class EngineUtil {
    public static PlatformType PlatformType {
        get {
#if UNITY_ANDROID
            return PlatformType.Android;
#elif UNITY_IOS
			return PlatformType.IOS;
#elif UNITY_UWP || UNITY_WSA
            return PlatformType.UWP;
#elif UNITY_WEBGL
			return PlatformType.WebGL;
#elif UNITY_STANDALONE_WIN
			return PlatformType.StandaloneWindows;
#elif UNITY_STANDALONE_OSX
			return PlatformType.StandaloneOSXUniversal;
#elif UNITY_STANDALONE_LINUX
			return PlatformType.StandaloneLinuxUniversal;
#else
			return PlatformType.StandaloneWindows;
#endif
        }
    }
}
