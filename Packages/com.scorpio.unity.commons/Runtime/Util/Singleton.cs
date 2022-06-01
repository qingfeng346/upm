using System;
using System.Collections.Generic;
using System.Text;

/// <summary> 单例类 </summary>
public abstract class Singleton<T> where T : Singleton<T>, new() {
    public static T Instance { get; } = new T();
    // private static T instance = null;
    // /// <summary> 返回单例 </summary>
    // public static T Instance { get { return instance ?? (instance = new T()); } }
    /// <summary> 重置单例 </summary>
    // public static void ResetInstance() {
    //     if (instance != null) {
    //         instance = null;
    //     }
    // }
    // /// <summary> 初始化单例时调用函数 </summary>
    // protected virtual void InitializeInstance() { }
    // /// <summary> 重置单例时调用函数 </summary>
    // protected virtual void ShutdownInstance() { }
}

