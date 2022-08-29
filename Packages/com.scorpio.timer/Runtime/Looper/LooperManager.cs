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
        private List<LooperData> m_LooperDatas = new List<LooperData> ();   //主线程回调
        private List<LooperData> m_AddDatas = new List<LooperData>();       //要添加的数据 下一帧统一添加
        private List<LooperData> m_DelDatas = new List<LooperData> ();      //要删除的数据 下一帧统一删除
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
                m_AddDatas.Add (new LooperData () { Call = call, number = number, Args = args });
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
                if (m_LooperDatas.FindIndex(_ => _.key == key && _.number > 0) >= 0 || m_AddDatas.FindIndex(_ => _.key == key) >= 0) { return; }
                m_AddDatas.Add (new LooperData () { Call = call, key = key, number = number, Args = args });
            }
        }
        public void OnUpdate () {
            lock (sync) {
                if (m_DelDatas.Count > 0) {
                    for (var i = 0; i < m_DelDatas.Count; ++i) {
                        m_LooperDatas.Remove(m_DelDatas[i]);
                    }
                    m_DelDatas.Clear();
                }
                if (m_AddDatas.Count > 0) {
                    for (var i = 0; i < m_AddDatas.Count; ++i) {
                        m_LooperDatas.Add(m_AddDatas[i]);
                    }
                    m_AddDatas.Clear();
                }
            }
            if (m_LooperDatas.Count <= 0) { return; }
            for (var i = 0; i < m_LooperDatas.Count; ++i) {
                var data = m_LooperDatas[i];
                if ((--data.number) > 0) { continue; }
                m_DelDatas.Add (data);
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
                m_AddDatas.Clear();
                m_DelDatas.Clear();
                m_LooperDatas.Clear();
            }
        }
    }
}