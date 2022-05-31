using System;
using System.Collections.Generic;
using UnityEngine;

namespace Scorpio.Pool {
    public delegate GameObject ItemCreate(object value);
    public class PoolManager {
        public static PoolManager Instance { get; } = new PoolManager();
        private GameObject m_PoolObject = null;
        private Dictionary<string, PoolItem> m_Pool = new Dictionary<string, PoolItem>();
        public void Initialize() { }
        public bool Contains(string name) {
            return m_Pool.ContainsKey(name);
        }
        public void AddPoolObject(GameObject gameObject) {
            gameObject.transform.SetParent(poolObject.transform);
        }
        public GameObject poolObject {
            get {
                if (m_PoolObject == null) {
                    m_PoolObject = new GameObject("__Pool");
                    m_PoolObject.SetActive(false);
                }
                return m_PoolObject;
            }
        }
        GameObject Instantiate(object value) {
            return UnityEngine.Object.Instantiate((GameObject)value);
        }
        public PoolItem CreatePrefabPool(string name, GameObject gameObject) {
            return CreatePrefabPool(name, Instantiate, gameObject);
        }
        public PoolItem CreatePrefabPool(string name, ItemCreate action) {
            return CreatePrefabPool(name, action, null);
        }
        public PoolItem CreatePrefabPool(string name, ItemCreate action, object value) {
            if (m_Pool.TryGetValue(name, out var pool))
                return pool;
            return m_Pool[name] = new PoolItem(name).Init(action, value);
        }
        public PoolItem GetPrefabPool(string name) {
            if (m_Pool.TryGetValue(name, out var pool))
                return pool;
            return null;
        }
        public GameObject Spawn(string name) {
            return Spawn(name, null);
        }
        public GameObject Spawn(string name, GameObject parent) {
            if (m_Pool.TryGetValue(name, out var pool))
                return pool.Spawn(parent);
            return null;
        }
        public void Despawn(GameObject gameObject) {
            if (gameObject != null) {
                foreach (var pair in m_Pool) {
                    if (pair.Value.Despawn(gameObject))
                        return;
                }
                UnityEngine.Object.DestroyImmediate(gameObject);
            }
        }
        public void Shutdown(string name) {
            if (m_Pool.TryGetValue(name, out var pool))
                pool.Shutdown();
        }
        public void Shutdown() {
            foreach (var pair in m_Pool) {
                pair.Value.Shutdown();
            }
            UnityEngine.Object.DestroyImmediate(m_PoolObject);
            m_PoolObject = null;
        }
        public void ClearChildren(Component component) {
            if (component == null) return;
            ClearChildren(component.gameObject);
        }
        public void ClearChildren(GameObject gameObject) {
            if (gameObject == null) return;
            int count = gameObject.transform.childCount;
            var trans = new Transform[count];
            for (int i = 0; i < count; ++i)
                trans[i] = gameObject.transform.GetChild(i);
            for (int i = 0; i < count; ++i)
                Despawn(trans[i].gameObject);
            gameObject.transform.DetachChildren();
        }
    }
}