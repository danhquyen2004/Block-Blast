using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;

namespace BlockBlast.Utils
{
    /// <summary>
    /// Object Pool với UniTask support
    /// </summary>
    /// <typeparam name="T">MonoBehaviour type</typeparam>
    public class ObjectPool<T> where T : MonoBehaviour
    {
        private readonly T prefab;
        private readonly Transform parent;
        private readonly Queue<T> pool = new Queue<T>();
        private readonly HashSet<T> activeObjects = new HashSet<T>();
        private int initialSize;

        public ObjectPool(T prefab, int initialSize = 10, Transform parent = null)
        {
            this.prefab = prefab;
            this.initialSize = initialSize;
            this.parent = parent;
        }

        /// <summary>
        /// Khởi tạo pool với số lượng ban đầu
        /// </summary>
        public async UniTask InitializeAsync()
        {
            for (int i = 0; i < initialSize; i++)
            {
                T obj = GameObject.Instantiate(prefab, parent);
                obj.gameObject.SetActive(false);
                pool.Enqueue(obj);
                
                // Yield để tránh lag frame
                if (i % 5 == 0)
                    await UniTask.Yield();
            }
        }

        /// <summary>
        /// Lấy object từ pool
        /// </summary>
        public T Get()
        {
            T obj;
            
            if (pool.Count > 0)
            {
                obj = pool.Dequeue();
            }
            else
            {
                obj = GameObject.Instantiate(prefab, parent);
            }

            obj.gameObject.SetActive(true);
            activeObjects.Add(obj);
            return obj;
        }

        /// <summary>
        /// Trả object về pool
        /// </summary>
        public void Return(T obj)
        {
            if (obj == null || !activeObjects.Contains(obj))
                return;

            obj.gameObject.SetActive(false);
            activeObjects.Remove(obj);
            pool.Enqueue(obj);
        }

        /// <summary>
        /// Trả tất cả active objects về pool
        /// </summary>
        public void ReturnAll()
        {
            var temp = new List<T>(activeObjects);
            foreach (var obj in temp)
            {
                Return(obj);
            }
        }

        /// <summary>
        /// Clear pool
        /// </summary>
        public void Clear()
        {
            ReturnAll();
            
            while (pool.Count > 0)
            {
                var obj = pool.Dequeue();
                if (obj != null)
                    GameObject.Destroy(obj.gameObject);
            }
        }

        public int ActiveCount => activeObjects.Count;
        public int PooledCount => pool.Count;
        public int TotalCount => ActiveCount + PooledCount;
    }
}
