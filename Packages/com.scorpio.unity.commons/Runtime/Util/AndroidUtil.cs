#if UNITY_ANDROID
using UnityEngine;
using System.Runtime.InteropServices;

public static class AndroidUtil {
    public static AndroidJavaClass NewJavaClass(string className) {
        return new AndroidJavaClass(className);
    }
    public static AndroidJavaObject NewJavaObject(string className, params object[] args) {
        return new AndroidJavaObject(className, args);
    }
    private static T GetInternal<T>(AndroidJavaObject javaObject, string fieldName) {
        return javaObject.Get<T>(fieldName);
    }
    private static T GetStaticInternal<T>(AndroidJavaObject javaObject, string fieldName) {
        return javaObject.GetStatic<T>(fieldName);
    }
    private static void SetInternal<T>(AndroidJavaObject javaObject, string fieldName, T value) {
        javaObject.Set(fieldName, value);
    }
    private static void SetStaticInternal<T>(AndroidJavaObject javaObject, string fieldName, T value) {
        javaObject.SetStatic(fieldName, value);
    }
    public static void Call(AndroidJavaObject javaObject, string methodName, params object[] args) {
        javaObject.Call(methodName, args);
    }
    public static void CallStatic(AndroidJavaObject javaObject, string methodName, params object[] args) {
        javaObject.CallStatic(methodName, args);
    }
    private static T CallInternal<T>(AndroidJavaObject javaObject, string methodName, params object[] args) {
        return javaObject.Call<T>(methodName, args);
    }
    private static T CallStaticInternal<T>(AndroidJavaObject javaObject, string methodName, params object[] args) {
        return javaObject.CallStatic<T>(methodName, args);
    }

    public static bool GetBool(AndroidJavaObject javaObject, string fieldName) {
        return GetInternal<bool>(javaObject, fieldName);
    }
    public static int CallInt(AndroidJavaObject javaObject, string fieldName) {
        return GetInternal<int>(javaObject, fieldName);
    }
    public static float CallFloat(AndroidJavaObject javaObject, string fieldName) {
        return GetInternal<float>(javaObject, fieldName);
    }
    public static string CallString(AndroidJavaObject javaObject, string fieldName) {
        return GetInternal<string>(javaObject, fieldName);
    }
    public static AndroidJavaObject CallObject(AndroidJavaObject javaObject, string fieldName) {
        return GetInternal<AndroidJavaObject>(javaObject, fieldName);
    }


    public static bool GetStaticBool(AndroidJavaObject javaObject, string fieldName) {
        return GetStaticInternal<bool>(javaObject, fieldName);
    }
    public static int CallStaticInt(AndroidJavaObject javaObject, string fieldName) {
        return GetStaticInternal<int>(javaObject, fieldName);
    }
    public static float CallStaticFloat(AndroidJavaObject javaObject, string fieldName) {
        return GetStaticInternal<float>(javaObject, fieldName);
    }
    public static string CallStaticString(AndroidJavaObject javaObject, string fieldName) {
        return GetStaticInternal<string>(javaObject, fieldName);
    }
    public static AndroidJavaObject CallStaticObject(AndroidJavaObject javaObject, string fieldName) {
        return GetStaticInternal<AndroidJavaObject>(javaObject, fieldName);
    }

    public static void SetBool(AndroidJavaObject javaObject, string fieldName, bool value) {
        SetInternal(javaObject, fieldName, value);
    }
    public static void SetInt(AndroidJavaObject javaObject, string fieldName, int value) {
        SetInternal(javaObject, fieldName, value);
    }
    public static void SetFloat(AndroidJavaObject javaObject, string fieldName, float value) {
        SetInternal(javaObject, fieldName, value);
    }
    public static void SetString(AndroidJavaObject javaObject, string fieldName, string value) {
        SetInternal(javaObject, fieldName, value);
    }
    public static void SetObject(AndroidJavaObject javaObject, string fieldName, AndroidJavaObject value) {
        SetInternal(javaObject, fieldName, value);
    }


    public static void SetStaticBool(AndroidJavaObject javaObject, string fieldName, bool value) {
        SetStaticInternal(javaObject, fieldName, value);
    }
    public static void SetStaticInt(AndroidJavaObject javaObject, string fieldName, int value) {
        SetStaticInternal(javaObject, fieldName, value);
    }
    public static void SetStaticFloat(AndroidJavaObject javaObject, string fieldName, float value) {
        SetStaticInternal(javaObject, fieldName, value);
    }
    public static void SetStaticString(AndroidJavaObject javaObject, string fieldName, string value) {
        SetStaticInternal(javaObject, fieldName, value);
    }
    public static void SetStaticObject(AndroidJavaObject javaObject, string fieldName, AndroidJavaObject value) {
        SetStaticInternal(javaObject, fieldName, value);
    }

    public static bool CallBool(AndroidJavaObject javaObject, string methodName, params object[] args) {
        return CallInternal<bool>(javaObject, methodName, args);
    }
    public static int CallInt(AndroidJavaObject javaObject, string methodName, params object[] args) {
        return CallInternal<int>(javaObject, methodName, args);
    }
    public static float CallFloat(AndroidJavaObject javaObject, string methodName, params object[] args) {
        return CallInternal<float>(javaObject, methodName, args);
    }
    public static string CallString(AndroidJavaObject javaObject, string methodName, params object[] args) {
        return CallInternal<string>(javaObject, methodName, args);
    }
    public static AndroidJavaObject CallObject(AndroidJavaObject javaObject, string methodName, params object[] args) {
        return CallInternal<AndroidJavaObject>(javaObject, methodName, args);
    }

    public static bool CallStaticBool(AndroidJavaObject javaObject, string methodName, params object[] args) {
        return CallStaticInternal<bool>(javaObject, methodName, args);
    }
    public static int CallStaticInt(AndroidJavaObject javaObject, string methodName, params object[] args) {
        return CallStaticInternal<int>(javaObject, methodName, args);
    }
    public static float CallStaticFloat(AndroidJavaObject javaObject, string methodName, params object[] args) {
        return CallStaticInternal<float>(javaObject, methodName, args);
    }
    public static string CallStaticString(AndroidJavaObject javaObject, string methodName, params object[] args) {
        return CallStaticInternal<string>(javaObject, methodName, args);
    }
    public static AndroidJavaObject CallStaticObject(AndroidJavaObject javaObject, string methodName, params object[] args) {
        return CallStaticInternal<AndroidJavaObject>(javaObject, methodName, args);
    }

    public static void Dispose(AndroidJavaObject javaObject) {
        javaObject.Dispose();
    }
}

#endif