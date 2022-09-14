using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Scorpio.Config;
using System.Globalization;
using System.Threading;

public static partial class EngineUtil {
    public static int SortByName(Transform a, Transform b) { return string.Compare(a.name, b.name); } //transform 名字排序
    public static int ReverseSortByName(Transform a, Transform b) { return string.Compare(b.name, a.name); } //transform 名字倒序
    public static string BuildVersion { get; } = GameConfig.Get("BuildID");
    //
    public static void InvariantCulture() {
        var culture = CultureInfo.InvariantCulture;
        CultureInfo.DefaultThreadCurrentCulture = culture;
        CultureInfo.DefaultThreadCurrentUICulture = culture;
        Thread.CurrentThread.CurrentCulture = culture;
        Thread.CurrentThread.CurrentUICulture = culture;
    }
    //下面的函数是为IL2CPP准备的  IL2CPP中 引擎类大部分反射不能使用 例如 Application SystemInfo GameObject 等类中的函数和变量
    public static GameObject NewGameObject (string name) { 
        return new GameObject (name); 
    }
    public static GameObject GetGameObject (UnityEngine.Object obj) {
        if (obj == null) {
            return null;
        } else if (obj is Component) {
            return ((Component)obj).gameObject;
        } else {
            return (GameObject)obj;
        }
    }
    public static Transform GetTransform (UnityEngine.Object obj) {
        if (obj == null) {
            return null;
        } else if (obj is Component) {
            return ((Component)obj).transform;
        } else {
            return ((GameObject)obj).transform;
        }
    }
    public static string GetGameObjectFullName (UnityEngine.Object obj) {
        var gameObject = GetGameObject (obj);
        if (gameObject == null) { return null; }
        var name = gameObject.name;
        var parent = gameObject.transform.parent;
        while (parent != null) {
            name = parent.name + "/" + name;
            parent = parent.parent;
        }
        return name;
    }
    public static void SetGameObjectName (UnityEngine.Object obj, string name) { 
        if (obj != null) {
            obj.name = name;
        }
    }
    public static void SetComponentEnable (Behaviour com, bool enable) {
        if (com == null) return;
        com.enabled = enable;
    }
    public static void DontDestroyOnLoad (UnityEngine.Object obj) {
        if (obj == null) { return; }
        UnityEngine.Object.DontDestroyOnLoad (obj);
    }

    public static UnityEngine.Object Instantiate (UnityEngine.Object obj) {
        return UnityEngine.Object.Instantiate (obj);
    }
    public static GameObject GetParent (UnityEngine.Object obj) {
        var transform = GetTransform (obj);
        return transform == null ? null : transform.parent.gameObject;
    }
    public static Component GetParent (UnityEngine.Object obj, Type type) {
        var transform = GetTransform (obj);
        return transform == null ? null : transform.parent.GetComponent (type);
    }
    //上面的函数是为IL2CPP准备的  IL2CPP中 引擎类大部分反射不能使用 例如 Application SystemInfo GameObject 等类中的函数和变量
    public static T GetComponent<T> (UnityEngine.Object obj) where T : Component {
        return GetComponent<T> (obj, true);
    }
    public static T GetComponent<T> (UnityEngine.Object obj, bool autoAdd) where T : Component {
        var gameObject = GetGameObject (obj);
        if (gameObject == null) { return null; }
        var component = gameObject.GetComponent<T> ();
        if (component == null) {
            return autoAdd ? gameObject.AddComponent<T> () : null;
        }
        return component;
    }
    public static T AddComponent<T> (UnityEngine.Object obj) where T : Component {
        return GetComponent<T> (obj);
    }
    public static Component GetComponent (UnityEngine.Object obj, Type type) {
        return GetComponent (obj, type, false);
    }
    public static Component GetComponent (UnityEngine.Object obj, Type type, bool autoAdd) {
        var gameObject = GetGameObject (obj);
        if (gameObject == null) { return null; }
        var component = gameObject.GetComponent (type);
        if (component == null) {
            return autoAdd ? gameObject.AddComponent (type) : null;
        }
        return component;
    }
    public static Component AddComponent (UnityEngine.Object obj, Type type) {
        return GetComponent (obj, type, true);
    }
    public static T GetComponentInChildren<T> (UnityEngine.Object obj) where T : Component {
        return GetComponentInChildren<T> (obj, false);
    }
    public static T GetComponentInChildren<T> (UnityEngine.Object obj, bool includeInactive) where T : Component {
        var gameObject = GetGameObject (obj);
        return gameObject == null ? null : gameObject.GetComponentInChildren<T> (includeInactive);
    }
    public static Component GetComponentInChildren (UnityEngine.Object obj, Type type) {
        return GetComponentInChildren (obj, type, false);
    }
    public static Component GetComponentInChildren (UnityEngine.Object obj, Type type, bool includeInactive) {
        var gameObject = GetGameObject (obj);
        return gameObject == null ? null : gameObject.GetComponentInChildren (type, includeInactive);
    }
    public static T GetComponentInParent<T> (UnityEngine.Object obj) where T : Component {
        var gameObject = GetGameObject (obj);
        return gameObject == null ? null : gameObject.GetComponentInParent<T> ();
    }
    public static Component GetComponentInParent (UnityEngine.Object obj, Type type) {
        var gameObject = GetGameObject (obj);
        return gameObject == null ? null : gameObject.GetComponentInParent (type);
    }

    public static Component[] GetComponentsInChildren(UnityEngine.Object obj, Type type) {
        return GetComponentsInChildren(obj, type, false);
    }
    public static Component[] GetComponentsInChildren (UnityEngine.Object obj, Type type, bool includeInactive) {
        var gameObject = GetGameObject(obj);
        return gameObject == null ? null : gameObject.GetComponentsInChildren(type, includeInactive);
    }
    public static Component[] GetComponentsInParent(UnityEngine.Object obj, Type type) {
        return GetComponentsInParent(obj, type, false);
    }
    public static Component[] GetComponentsInParent (UnityEngine.Object obj, Type type, bool includeInactive) {
        var gameObject = GetGameObject(obj);
        return gameObject == null ? null : gameObject.GetComponentsInParent(type, includeInactive);
    }




    public static GameObject FindChild (UnityEngine.Object obj, string str) {
        var gameObject = GetGameObject (obj);
        if (gameObject == null) { return null; }
        if (string.IsNullOrEmpty (str)) return gameObject;
        var transform = gameObject.transform.Find (str);
        return transform == null ? null : transform.gameObject;
    }
    public static T FindChild<T> (UnityEngine.Object obj, string str) where T : Component {
        var gameObject = FindChild (obj, str);
        return gameObject == null ? null : gameObject.GetComponent<T> ();
    }
    public static object FindChild (UnityEngine.Object obj, string str, Type type) {
        var gameObject = FindChild (obj, str);
        return gameObject == null ? null : gameObject.GetComponent (type);
    }
    public static bool HasChild (UnityEngine.Object obj, string str) {
        var gameObject = GetGameObject (obj);
        if (gameObject == null) { return false; }
        if (string.IsNullOrEmpty (str)) { return true; }
        return gameObject.transform.Find (str) != null;
    }
    public static bool IsChild(UnityEngine.Object parent, UnityEngine.Object child) {
        var childTransform = GetTransform(child);
        var parentTransform = GetTransform(parent);
        if (childTransform == null || parentTransform == null) { return false; }
        while (childTransform != null) {
            if (childTransform == parentTransform) { return true; }
            childTransform = childTransform.parent;
        }
        return false;
    }
    public static GameObject AddChild (UnityEngine.Object parent, UnityEngine.Object child) {
        return AddChild (parent, child, Vector3.one, Vector3.zero, Vector3.zero, true);
    }
    public static GameObject AddChild (UnityEngine.Object parent, UnityEngine.Object child, bool reset) {
        return AddChild (parent, child, Vector3.one, Vector3.zero, Vector3.zero, reset);
    }
    public static GameObject AddChild (UnityEngine.Object parent, UnityEngine.Object child, Vector3 scale, Vector3 position, Vector3 angles) {
        return AddChild (parent, child, scale, position, angles, true);
    }
    public static GameObject AddChild (UnityEngine.Object parent, UnityEngine.Object child, Vector3 scale, Vector3 position, Vector3 angles, bool reset) {
        var parentTransform = GetTransform (parent);
        var childTransform = GetTransform (child);
        if (childTransform == null) { return null; }
        if (parentTransform == null) { return childTransform.gameObject; }
        childTransform.SetParent (parentTransform);
        childTransform.gameObject.layer = parentTransform.gameObject.layer;
        if (reset) {
            childTransform.localScale = scale;
            childTransform.localPosition = position;
            childTransform.localEulerAngles = angles;
        }
        return childTransform.gameObject;
    }
    public static bool Destroy (UnityEngine.Object obj) {
        if (obj == null) return false;
        UnityEngine.Object.Destroy (obj);
        return true;
    }
    public static bool Destroy (UnityEngine.Object obj, float delay) {
        if (obj == null) return false;
        UnityEngine.Object.Destroy (obj, delay);
        return true;
    }
    public static bool DestroyImmediate (UnityEngine.Object obj) {
        return DestroyImmediate(obj, false);
    }
    public static bool DestroyImmediate (UnityEngine.Object obj, bool allowDestroyingAssets) {
        if (obj == null) return false;
        UnityEngine.Object.DestroyImmediate (obj, allowDestroyingAssets);
        return true;
    }

    public static void Activate (UnityEngine.Object obj) { SetActive (obj, true); }
    public static void Deactivate (UnityEngine.Object obj) { SetActive (obj, false); }
    //activeSelf 指 GameObject 自身是否是激活状态
    //activeInHierarchy 指 GameObject 当前是否是激活状态 例如 父级有未激活的Object 那 activeInHierarchy 就是 false
    public static void SetActive (UnityEngine.Object obj, bool active) {
        var gameObject = GetGameObject (obj);
        if (gameObject != null && gameObject.activeSelf != active) {
            gameObject.SetActive (active);
        }
    }
    public static bool IsActive (UnityEngine.Object obj) {
        var gameObject = GetGameObject (obj);
        return gameObject == null ? false : gameObject.activeSelf;
    }
    public static void ClearChildren (UnityEngine.Object obj) {
        var transform = GetTransform (obj);
        if (transform == null) return;
        var count = transform.childCount;
        var trans = new Transform[count];
        for (int i = 0; i < count; ++i)
            trans[i] = transform.GetChild (i);
        for (int i = 0; i < count; ++i)
            UnityEngine.Object.Destroy (trans[i].gameObject);
        transform.DetachChildren ();
    }
    public static void ClearChildrenImmediate (UnityEngine.Object obj) {
        var transform = GetTransform (obj);
        if (transform == null) return;
        var count = transform.childCount;
        var trans = new Transform[count];
        for (int i = 0; i < count; ++i)
            trans[i] = transform.GetChild (i);
        for (int i = 0; i < count; ++i)
            UnityEngine.Object.DestroyImmediate (trans[i].gameObject);
        transform.DetachChildren ();
    }
    //transform 子节点排序
    public static void SortTransform(UnityEngine.Object obj) {
        SortTransform(obj, SortByName);
    }
    //transform 子节点排序
    public static void SortTransform(UnityEngine.Object obj, Comparison<Transform> comparison) {
        var transform = GetTransform(obj);
        if (transform == null) { return; }
        var list = new List<Transform>();
        for (int i = 0; i < transform.childCount; ++i)
            list.Add(transform.GetChild(i));
        list.Sort(comparison);
        for (int i = 0; i < list.Count; ++i)
            list[i].SetAsLastSibling();
    }
    public static void SetObjectValue(UnityEngine.Object obj, object value) {
        GetComponent<ObjectValue>(obj).value = value;
    }
    public static object GetObjectValue(UnityEngine.Object obj) {
        return GetComponent<ObjectValue>(obj).value;
    }
    public static T GetObjectValue<T>(UnityEngine.Object obj) {
        var value = GetComponent<ObjectValue>(obj).value;
        if (value is T) { return (T)value; }
        return (T)Convert.ChangeType(value, typeof(T));
    }
    public static string GetObjectStringValue(UnityEngine.Object obj) {
        return GetObjectValue<string>(obj);
    }
    public static int GetObjectIntValue(UnityEngine.Object obj) {
        return GetObjectValue<int>(obj);
    }


    public static void SetLayer(UnityEngine.Object obj, string layer, bool includeChild) {
        SetLayer(obj, LayerMask.NameToLayer(layer), includeChild);
    }
    public static void SetLayer(UnityEngine.Object obj, int layer, bool includeChild) {
        var transform = GetTransform(obj);
        if (transform == null) return;
        transform.gameObject.layer = layer;
        if (includeChild) {
            var count = transform.childCount;
            for (var i = 0; i < count; ++i) {
                SetLayer(transform.GetChild(i), layer, true);
            }
        }
    }
    public static void SetAsFirstSibling(UnityEngine.Object obj) {
        var transform = GetTransform(obj);
        if (transform == null) return;
        transform.SetAsFirstSibling();
    }
    public static void SetAsLastSibling(UnityEngine.Object obj) {
        var transform = GetTransform(obj);
        if (transform == null) return;
        transform.SetAsLastSibling();
    }
    public static int GetSiblingIndex(UnityEngine.Object obj) {
        var transform = GetTransform(obj);
        return transform == null ? 0 : transform.GetSiblingIndex();
    }
    public static void SetSiblingIndex(UnityEngine.Object obj, int index) {
        var transform = GetTransform(obj);
        if (transform == null) { return; }
        transform.SetSiblingIndex(index);
    }

    public static void SetPosition (UnityEngine.Object obj, float x, float y, float z) {
        SetPosition (obj, new Vector3 (x, y, z));
    }
    public static void SetPosition (UnityEngine.Object obj, Vector3 position) {
        var transform = GetTransform (obj);
        if (transform == null) { return; }
        transform.position = position;
    }
    public static void SetPositionX (UnityEngine.Object obj, float x) {
        var transform = GetTransform (obj);
        if (transform == null) { return; }
        var position = transform.position;
        position.x = x;
        transform.position = position;
    }
    public static void SetPositionY (UnityEngine.Object obj, float y) {
        var transform = GetTransform (obj);
        if (transform == null) { return; }
        var position = transform.position;
        position.y = y;
        transform.position = position;
    }
    public static void SetPositionZ (UnityEngine.Object obj, float z) {
        var transform = GetTransform (obj);
        if (transform == null) { return; }
        var position = transform.position;
        position.z = z;
        transform.position = position;
    }

    
    public static Vector3 GetPosition (UnityEngine.Object obj) {
        var transform = GetTransform (obj);
        return transform == null ? Vector3.zero : transform.position;
    }
    public static float GetPositionX (UnityEngine.Object obj) {
        return GetPosition (obj).x;
    }
    public static float GetPositionY (UnityEngine.Object obj) {
        return GetPosition (obj).y;
    }
    public static float GetPositionZ (UnityEngine.Object obj) {
        return GetPosition (obj).z;
    }


    public static void SetLocalPosition (UnityEngine.Object obj, float x, float y, float z) {
        SetLocalPosition (obj, new Vector3 (x, y, z));
    }
    public static void SetLocalPosition (UnityEngine.Object obj, Vector3 position) {
        var transform = GetTransform (obj);
        if (transform == null) { return; }
        transform.localPosition = position;
    }
    public static void SetLocalPositionX (UnityEngine.Object obj, float x) {
        var transform = GetTransform (obj);
        if (transform == null) { return; }
        var position = transform.localPosition;
        position.x = x;
        transform.localPosition = position;
    }
    public static void SetLocalPositionY (UnityEngine.Object obj, float y) {
        var transform = GetTransform (obj);
        if (transform == null) { return; }
        var position = transform.localPosition;
        position.y = y;
        transform.localPosition = position;
    }
    public static void SetLocalPositionZ (UnityEngine.Object obj, float z) {
        var transform = GetTransform (obj);
        if (transform == null) { return; }
        var position = transform.localPosition;
        position.z = z;
        transform.localPosition = position;
    }

    public static Vector3 GetLocalPosition (UnityEngine.Object obj) {
        var transform = GetTransform (obj);
        return transform == null ? Vector3.zero : transform.localPosition;
    }
    public static float GetLocalPositionX (UnityEngine.Object obj) {
        return GetLocalPosition (obj).x;
    }
    public static float GetLocalPositionY (UnityEngine.Object obj) {
        return GetLocalPosition (obj).y;
    }
    public static float GetLocalPositionZ (UnityEngine.Object obj) {
        return GetLocalPosition (obj).z;
    }

    public static void SetLocalScale (UnityEngine.Object obj, float x, float y, float z) {
        SetLocalScale (obj, new Vector3 (x, y, z));
    }
    public static void SetLocalScale(UnityEngine.Object obj, float scale) {
        SetLocalScale(obj, new Vector3(scale, scale, scale));
    }
    public static void SetLocalScale (UnityEngine.Object obj, Vector3 scale) {
        var transform = GetTransform (obj);
        if (transform == null) { return; }
        transform.localScale = scale;
    }
    public static void SetLocalScaleX (UnityEngine.Object obj, float x) {
        var transform = GetTransform (obj);
        if (transform == null) { return; }
        var scale = transform.localScale;
        scale.x = x;
        transform.localScale = scale;
    }
    public static void SetLocalScaleY (UnityEngine.Object obj, float y) {
        var transform = GetTransform (obj);
        if (transform == null) { return; }
        var scale = transform.localScale;
        scale.y = y;
        transform.localScale = scale;
    }
    public static void SetLocalScaleZ (UnityEngine.Object obj, float z) {
        var transform = GetTransform (obj);
        if (transform == null) { return; }
        var scale = transform.localScale;
        scale.z = z;
        transform.localScale = scale;
    }

    public static Vector3 GetLocalScale (UnityEngine.Object obj) {
        var transform = GetTransform (obj);
        return transform == null ? Vector3.zero : transform.localScale;
    }
    public static float GetLocalScaleX (UnityEngine.Object obj) {
        return GetLocalScale (obj).x;
    }
    public static float GetLocalScaleY (UnityEngine.Object obj) {
        return GetLocalScale (obj).y;
    }
    public static float GetLocalScaleZ (UnityEngine.Object obj) {
        return GetLocalScale (obj).z;
    }

    public static void SetLocalEulerAngles (UnityEngine.Object obj, float x, float y, float z) {
        SetLocalEulerAngles (obj, new Vector3 (x, y, z));
    }
    public static void SetLocalEulerAngles (UnityEngine.Object obj, Vector3 angles) {
        var transform = GetTransform (obj);
        if (transform == null) { return; }
        transform.localEulerAngles = angles;
    }
    public static void SetLocalEulerAnglesX (UnityEngine.Object obj, float x) {
        var transform = GetTransform (obj);
        if (transform == null) { return; }
        var angles = transform.localEulerAngles;
        angles.x = x;
        transform.localEulerAngles = angles;
    }
    public static void SetLocalEulerAnglesY (UnityEngine.Object obj, float y) {
        var transform = GetTransform (obj);
        if (transform == null) { return; }
        var angles = transform.localEulerAngles;
        angles.y = y;
        transform.localEulerAngles = angles;
    }
    public static void SetLocalEulerAnglesZ (UnityEngine.Object obj, float z) {
        var transform = GetTransform (obj);
        if (transform == null) { return; }
        var angles = transform.localEulerAngles;
        angles.z = z;
        transform.localEulerAngles = angles;
    }

    public static Vector3 GetLocalEulerAngles (UnityEngine.Object obj) {
        var transform = GetTransform (obj);
        return transform == null ? Vector3.zero : transform.localEulerAngles;
    }
    public static float GetLocalEulerAnglesX (UnityEngine.Object obj) {
        return GetLocalEulerAngles (obj).x;
    }
    public static float GetLocalEulerAnglesY (UnityEngine.Object obj) {
        return GetLocalEulerAngles (obj).y;
    }
    public static float GetLocalEulerAnglesZ (UnityEngine.Object obj) {
        return GetLocalEulerAngles (obj).z;
    }

    //Unity UI
    public static void SetAnchoredPosition (UnityEngine.Object obj, float x, float y) { 
        SetAnchoredPosition (obj, new Vector2 (x, y)); 
    }
    public static void SetAnchoredPosition (UnityEngine.Object obj, float x, float y, float z) { 
        SetAnchoredPosition (obj, new Vector3 (x, y, z)); 
    }
    public static void SetAnchoredPosition (UnityEngine.Object obj, Vector2 position) {
        var transform = GetComponent<RectTransform> (obj, false);
        if (transform == null) return;
        transform.anchoredPosition = position;
    }
    public static void SetAnchoredPosition (UnityEngine.Object obj, Vector3 position) {
        var transform = GetComponent<RectTransform> (obj, false);
        if (transform == null) return;
        transform.anchoredPosition3D = position;
    }
    public static void SetAnchoredPositionX (UnityEngine.Object obj, float x) {
        var transform = GetComponent<RectTransform> (obj, false);
        if (transform == null) return;
        var position = transform.anchoredPosition3D;
        position.x = x;
        transform.anchoredPosition3D = position;
    }
    public static void SetAnchoredPositionY (UnityEngine.Object obj, float y) {
        var transform = GetComponent<RectTransform> (obj, false);
        if (transform == null) return;
        var position = transform.anchoredPosition3D;
        position.y = y;
        transform.anchoredPosition3D = position;
    }
    public static void SetAnchoredPositionZ (UnityEngine.Object obj, float z) {
        var transform = GetComponent<RectTransform> (obj, false);
        if (transform == null) return;
        var position = transform.anchoredPosition3D;
        position.z = z;
        transform.anchoredPosition3D = position;
    }

    public static Vector3 GetAnchoredPosition (UnityEngine.Object obj) {
        var transform = GetComponent<RectTransform> (obj, false);
        return transform == null ? Vector3.zero : transform.anchoredPosition3D;
    }
    public static float GetAnchoredPositionX (UnityEngine.Object obj) {
        return GetAnchoredPosition (obj).x;
    }
    public static float GetAnchoredPositionY (UnityEngine.Object obj) {
        return GetAnchoredPosition (obj).y;
    }
    public static float GetAnchoredPositionZ (UnityEngine.Object obj) {
        return GetAnchoredPosition (obj).z;
    }

    public static void AddAnchoredPositionX (UnityEngine.Object obj, float deltaX) {
        var transform = GetComponent<RectTransform>(obj, false);
        if (transform == null) return;
        var position = transform.anchoredPosition3D;
        position.x += deltaX;
        transform.anchoredPosition3D = position;
    }
    public static void AddAnchoredPositionY(UnityEngine.Object obj, float deltaY) {
        var transform = GetComponent<RectTransform>(obj, false);
        if (transform == null) return;
        var position = transform.anchoredPosition3D;
        position.y += deltaY;
        transform.anchoredPosition3D = position;
    }
    public static void AddAnchoredPositionZ(UnityEngine.Object obj, float deltaZ) {
        var transform = GetComponent<RectTransform>(obj, false);
        if (transform == null) return;
        var position = transform.anchoredPosition3D;
        position.z += deltaZ;
        transform.anchoredPosition3D = position;
    }

    public static void SetSizeDelta (UnityEngine.Object obj, float x, float y) {
        SetSizeDelta (obj, new Vector2 (x, y));
    }
    public static void SetSizeDelta (UnityEngine.Object obj, Vector2 size) {
        var transform = GetComponent<RectTransform> (obj, false);
        if (transform == null) { return; }
        transform.sizeDelta = size;
    }
    public static void SetSizeDeltaX (UnityEngine.Object obj, float x) {
        var transform = GetComponent<RectTransform> (obj, false);
        if (transform == null) { return; }
        var size = transform.sizeDelta;
        size.x = x;
        transform.sizeDelta = size;
    }
    public static void SetSizeDeltaY (UnityEngine.Object obj, float y) {
        var transform = GetComponent<RectTransform> (obj, false);
        if (transform == null) { return; }
        var size = transform.sizeDelta;
        size.y = y;
        transform.sizeDelta = size;
    }

    public static Vector2 GetSizeDelta (UnityEngine.Object obj) {
        var transform = GetComponent<RectTransform> (obj, false);
        return transform == null ? Vector2.zero : transform.sizeDelta;
    }
    public static float GetSizeDeltaX (UnityEngine.Object obj) {
        return GetSizeDelta (obj).x;
    }
    public static float GetSizeDeltaY (UnityEngine.Object obj) {
        return GetSizeDelta (obj).y;
    }

    public static void SetAnchorMin (UnityEngine.Object obj, Vector2 value) {
        var transform = GetComponent<RectTransform> (obj, false);
        if (transform == null) return;
        transform.anchorMin = value;
    }
    public static void SetAnchorMax (UnityEngine.Object obj, Vector2 value) {
        var transform = GetComponent<RectTransform> (obj, false);
        if (transform == null) return;
        transform.anchorMax = value;
    }
    public static void SetAnchorX(UnityEngine.Object obj, float value) {
        SetAnchorX(obj, value, value);
    }
    public static void SetAnchorX(UnityEngine.Object obj, float minValue, float maxValue) {
        var transform = GetComponent<RectTransform> (obj, false);
        if (transform == null) return;
        var min = transform.anchorMin;
        var max = transform.anchorMax;
        min.x = minValue;
        max.x = maxValue;
        transform.anchorMin = min;
        transform.anchorMax = max;
    }
    public static void SetAnchorY(UnityEngine.Object obj, float value) {
        SetAnchorY(obj, value, value);
    }
    public static void SetAnchorY(UnityEngine.Object obj, float minValue, float maxValue) {
        var transform = GetComponent<RectTransform> (obj, false);
        if (transform == null) return;
        var min = transform.anchorMin;
        var max = transform.anchorMax;
        min.y = minValue;
        max.y = maxValue;
        transform.anchorMin = min;
        transform.anchorMax = max;
    }
    public static void SetOffsetMin (UnityEngine.Object obj, Vector2 value) {
        var transform = GetComponent<RectTransform> (obj, false);
        if (transform == null) return;
        transform.offsetMin = value;
    }
    public static void SetOffsetMax (UnityEngine.Object obj, Vector2 value) {
        var transform = GetComponent<RectTransform> (obj, false);
        if (transform == null) return;
        transform.offsetMax = value;
    }
    public static void SetPivot (UnityEngine.Object obj, Vector2 value) {
        var transform = GetComponent<RectTransform> (obj, false);
        if (transform == null) return;
        transform.pivot = value;
    }

    public static void SetSprite (UnityEngine.Object obj, Sprite sprite) {
        var image = GetComponent<Image> (obj, false);
        if (image != null) {
            image.sprite = sprite;
            return;
        }
        var renderer = GetComponent<SpriteRenderer> (obj, false);
        if (renderer != null) {
            renderer.sprite = sprite;
            return;
        }
        var rawImage = GetComponent<RawImage> (obj, false);
        if (rawImage != null) {
            rawImage.texture = sprite.texture;
        }
    }
    public static void SetColor (UnityEngine.Object obj, Color color) {
        var graphic = GetComponent<MaskableGraphic> (obj, false);
        if (graphic != null) {
            graphic.color = color;
            return;
        }
        var renderer = GetComponent<SpriteRenderer> (obj, false);
        if (renderer != null) {
            renderer.color = color;
        }
    }
    public static void SetAlpha (UnityEngine.Object obj, float alpha) {
        var graphic = GetComponent<MaskableGraphic> (obj, false);
        if (graphic != null) {
            var color = graphic.color;
            color.a = alpha;
            graphic.color = color;
            return;
        }
        var renderer = GetComponent<SpriteRenderer> (obj, false);
        if (renderer != null) {
            var color = renderer.color;
            color.a = alpha;
            renderer.color = color;
            return;
        }
        var group = GetComponent<CanvasGroup> (obj, false);
        if (group != null) {
            group.alpha = alpha;
        }
    }
    public static void SetRaycasts (UnityEngine.Object obj, bool cast) {
        var group = GetComponent<CanvasGroup> (obj, false);
        if (group != null) {
            group.blocksRaycasts = cast;
            return;
        }
        var graphic = GetComponent<MaskableGraphic> (obj, false);
        if (graphic != null) {
            graphic.raycastTarget = cast;
        }
    }
    public static void SetRendererSorting(UnityEngine.Object obj, string sortingLayerName, int sortingOrder) {
        var renderer = GetComponent<Renderer>(obj, false);
        if (renderer == null) { return; }
        renderer.sortingLayerName = sortingLayerName;
        renderer.sortingOrder = sortingOrder;
    }
    public static void SetRendererSorting (UnityEngine.Object obj, string sortingLayerName) {
        var renderer = GetComponent<Renderer> (obj, false);
        if (renderer == null) { return; }
        renderer.sortingLayerName = sortingLayerName;
    }
    public static void SetRendererSorting (UnityEngine.Object obj, int sortingOrder) {
        var renderer = GetComponent<Renderer> (obj, false);
        if (renderer == null) { return; }
        renderer.sortingOrder = sortingOrder;
    }
    public static ActiveAnimation PlayAnimation(UnityEngine.Object obj) {
        return PlayAnimation(obj, null, null);
    }
    public static ActiveAnimation PlayAnimation(UnityEngine.Object obj, AnimationOrTween.OnFinished<ActiveAnimation> onFinished) {
        return PlayAnimation(obj, null, onFinished);
    }
    public static ActiveAnimation PlayAnimation(UnityEngine.Object obj, string clipName) {
        return PlayAnimation(obj, clipName, null);
    }
    public static ActiveAnimation PlayAnimation(UnityEngine.Object obj, string clipName, AnimationOrTween.OnFinished<ActiveAnimation> onFinished) {
        var gameObject = GetGameObject(obj);
        if (gameObject == null) { return null; }
        var animation = GetComponent<Animation>(obj, false);
        if (animation == null) {
            animation = GetComponentInChildren<Animation>(obj, true);
        }
        return ActiveAnimation.PlayAnimation(animation, clipName, onFinished);
    }
    public static ActiveAnimator PlayAnimator (UnityEngine.Object obj, string clipName) {
        return PlayAnimator (obj, clipName, 0, null, null);
    }
    public static ActiveAnimator PlayAnimator (UnityEngine.Object obj, string clipName, Action<object, ActiveAnimator> onFinished) {
        return PlayAnimator (obj, clipName, 0, onFinished, null);
    }
    public static ActiveAnimator PlayAnimator (UnityEngine.Object obj, string clipName, Action<object, ActiveAnimator> onFinished, object args) {
        return PlayAnimator(obj, clipName, 0, onFinished, args);
    }
    public static ActiveAnimator PlayAnimator (UnityEngine.Object obj, string clipName, float normalizedTime) {
        return PlayAnimator (obj, clipName, normalizedTime, null, null);
    }
    public static ActiveAnimator PlayAnimator (UnityEngine.Object obj, string clipName, float normalizedTime, Action<object, ActiveAnimator> onFinished) {
        return PlayAnimator (obj, clipName, normalizedTime, onFinished, null);
    }
    public static ActiveAnimator PlayAnimator (UnityEngine.Object obj, string clipName, float normalizedTime, Action<object, ActiveAnimator> onFinished, object args) {
        var gameObject = GetGameObject (obj);
        if (gameObject == null) { return null; }
        var animator = GetComponent<Animator> (obj, false);
        if (animator == null) {
            animator = GetComponentInChildren<Animator> (obj, true);
        }
        return ActiveAnimator.Play (animator, clipName, normalizedTime, onFinished, args);
    }
    public static float GetAnimationClipLength(UnityEngine.Object obj, string clipName) {
        var gameObject = GetGameObject(obj);
        if (gameObject == null) { return 0; }
        var animator = GetComponent<Animator>(obj, false);
        if (animator == null) {
            animator = GetComponentInChildren<Animator>(obj, true);
        }
        var controller = animator.runtimeAnimatorController;
        foreach (var clip in controller.animationClips) {
            if(clip.name == clipName) {
                return clip.length;
            }
        }
        return 0;
    }
}