using UnityEngine;
using System;
using System.Collections;

public class YieldUtil : MonoBehaviour {
    private static YieldUtil instance = null;
    public static YieldUtil Instance {
        get {
            if (instance == null) {
                var obj = new GameObject("__YieldUtil");
                GameObject.DontDestroyOnLoad(obj);
                instance = obj.AddComponent<YieldUtil>();
            }
            return instance;
        }
    }
    public static void Start(YieldInstruction obj, Action call) {
        Instance.StartYield(obj, call, false);
	}
    public static void Start(GameObject gameObj, YieldInstruction obj, Action call) {
        gameObj.AddComponent<YieldUtil>().StartYield(obj, call, true);
    }
    void StartYield(YieldInstruction obj, Action back, bool destroy) {
		StartCoroutine (StartYield_impl(obj, back, destroy));
	}
	IEnumerator StartYield_impl(YieldInstruction obj, Action back, bool destroy) {
		yield return obj;
        if (destroy) Destroy(this);
        if (back != null) back ();
	}
}
