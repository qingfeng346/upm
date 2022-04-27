using System;
using System.Collections.Generic;
using UnityEngine;
namespace Scorpio.Pool {
    public class PoolItem {
        public int Limit { get; set; } = 10;
        public string Name { get; private set; }
        public Action<GameObject> SpawnAction;
        public Action<GameObject> DespawnAction;
        private GameObject m_GameObject = null;
        private ItemCreate m_Action = null;
        private object m_Value = null;
        private Stack<GameObject> m_Cache = new Stack<GameObject>();        //缓存的GameObject
        private HashSet<GameObject> m_Array = new HashSet<GameObject>();    //派生的GameObject
        public PoolItem(string name) {
            Name = name;
        }
        public PoolItem Init(ItemCreate action, object value) {
            m_Action = action;
            m_Value = value;
            return this;
        }
        public bool Contains(GameObject gameObject) {
            return m_Array.Contains(gameObject);
        }
        public GameObject Spawn() {
            return Spawn(null);
        }
        public GameObject Spawn(GameObject parent) {
            if (m_GameObject == null) {
                m_GameObject = m_Action?.Invoke(m_Value);
                if (m_GameObject == null) {
                    throw new Exception($"PoolItem Name : {Name} is not return a gameObject");
                }
                PoolManager.Instance.AddPoolObject(m_GameObject);
            }
            GameObject gameObject;
            if (m_Cache.Count > 0) {
                gameObject = m_Cache.Pop();
            } else {
                gameObject = UnityEngine.Object.Instantiate(m_GameObject);
            }
            if (parent == null) {
                gameObject.transform.SetParent(null);
            } else {
                gameObject.transform.SetParent(gameObject.transform);
            }
            gameObject.SetActive(true);
            SpawnAction?.Invoke(gameObject);
            gameObject.SendMessage("OnSpawn", SendMessageOptions.DontRequireReceiver);
            m_Array.Add(gameObject);
            return gameObject;
        }
        public bool Despawn(GameObject gameObject) {
            if (m_Cache.Contains(gameObject)) return true;
            if (!m_Array.Contains(gameObject)) return false;
            DespawnAction?.Invoke(gameObject);
            gameObject.SendMessage("OnDespawn", SendMessageOptions.DontRequireReceiver);
            var poolComponent = gameObject.GetComponent<PoolComponent>();
            if (poolComponent == null) {
                poolComponent = gameObject.AddComponent<PoolComponent>();
            }
            poolComponent.SetPool(this);
            PoolManager.Instance.AddPoolObject(m_GameObject);
            m_Array.Remove(gameObject);
            if (m_Cache.Count > Limit) {
                UnityEngine.Object.DestroyImmediate(gameObject);
            } else {
                m_Cache.Push(gameObject);
            }
            return true;
        }
        internal void OnDestroy(GameObject gameObject) {
            m_Array.Remove(gameObject);
        }
        //清空所有缓存的GameObject,不清除已派生的GameObject
        public void Shutdown() {
            foreach (var gameObject in m_Cache) {
                UnityEngine.Object.DestroyImmediate(gameObject);
            }
            m_Cache.Clear();
            m_Array.Clear();
            UnityEngine.Object.DestroyImmediate(m_GameObject);
            m_GameObject = null;
        }
    }
}