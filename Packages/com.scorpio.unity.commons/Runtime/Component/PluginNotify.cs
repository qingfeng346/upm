using UnityEngine;
using Scorpio.Unity.Util;
public class PluginNotify : MonoBehaviour
{
	void Log(string msg) {
        logger.info ($"deviceLog : {msg}");
	}
    void Call(string info) {
        int index = info.IndexOf(":");
        if (index < 0) {
            ScriptManager.Instance.Call(info);
            return;
        }
        string name = info.Substring(0, index);
        string args = info.Substring(index + 1);
        ScriptManager.Instance.Call(name, args);
    }
}
