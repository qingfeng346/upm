using System.Collections.Generic;
using UnityEngine;

namespace Scorpio.Timer {
    /// <summary> 计时器回调 </summary>
    public delegate void LooperDelegate(object args);
    public partial class LooperManager {
        public static LooperManager Instance { get; } = new LooperManager ();
        private class LooperData {
            public LooperDelegate Call;
            public object Args;
            public int number;
            public string key;
        }
        private List<LooperData> m_LooperDatas = new List<LooperData> (); //主线程回调
        private List<LooperData> m_RemoveDatas = new List<LooperData> (); //要删除的结构
        private object sync = new object (); //线程锁
        private LooperManager() {
            TimerBehaviour.Initialize();
        }
        public void Run (LooperDelegate call) {
            Run (call, 0, null);
        }
        public void Run (LooperDelegate call, object args) {
            Run (call, 0, args);
        }
        public void Run (LooperDelegate call, int number, object args) {
            if (call == null) { return; }
            lock (sync) {
                m_LooperDatas.Add (new LooperData () { Call = call, number = number, Args = args });
            }
        }
        public void RunWithKey(LooperDelegate call, string key) {
            RunWithKey(call, key, 0, null);
        }
        public void RunWithKey(LooperDelegate call, string key, object args) {
            RunWithKey(call, key, 0, args);
        }
        public void RunWithKey(LooperDelegate call, string key, int number, object args) {
            if (call == null) { return; }
            lock (sync) {
                if (m_LooperDatas.FindIndex(_ => _.key == key) >= 0) { return; }
                m_LooperDatas.Add (new LooperData () { Call = call, key = key, number = number, Args = args });
            }
        }
        public void OnUpdate () {
            if (m_LooperDatas.Count <= 0) { return; }
            LooperData[] datas = null;
            lock (sync) {
                foreach (var data in m_RemoveDatas) {
                    m_LooperDatas.Remove (data);
                }
                m_RemoveDatas.Clear ();
                datas = m_LooperDatas.ToArray ();
            }
            foreach (var data in datas) {
                if ((--data.number) > 0) { continue; }
                m_RemoveDatas.Add (data);
                try {
                    data.Call (data.Args);
                } catch (System.Exception e) {
#if UNITY_EDITOR
                    string target = data.Call.Target != null ? data.Call.Target.GetType ().Name : "";
                    string method = data.Call.Method != null ? data.Call.Method.ToString () : "";
                    Debug.LogError (string.Format ("LooperData is error {0} - {1} stack : {2}", target, method, e.ToString ()));
#else
                    Debug.LogError (string.Format ("LooperData is error stack : {0}", e.ToString ()));
#endif
                }
            }
        }
        public void Shutdown() {
            lock (sync) {
                m_RemoveDatas.Clear();
                m_LooperDatas.Clear();
            }
        }
    }
}