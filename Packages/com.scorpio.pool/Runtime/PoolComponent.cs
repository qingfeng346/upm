using UnityEngine;
namespace Scorpio.Pool {
    public class PoolComponent : MonoBehaviour {
        private PoolItem pool;
        internal void SetPool(PoolItem pool) {
            this.pool = pool;
        }
        void OnDestroy() {
            if (pool != null) {
                pool.OnDestroy(gameObject);
            }
        }
    }
}
