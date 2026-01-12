using System;
using System.Collections.Generic;
using UnityEngine;
using Game.Core.Pooling;

namespace Game.Core.Systems
{
    /// <summary>
    /// Centralized object pool manager for zero-allocation spawning.
    /// FIXED: Removed DontDestroyOnLoad - parent CoreSystemManager handles persistence
    /// </summary>
    public class ObjectPoolManager : MonoBehaviour
    {
        [Header("Pool Configuration")]
        [SerializeField, Tooltip("Parent transform for all pooled objects")]
        private Transform _poolRoot;
        
        [SerializeField, Tooltip("Initial pool sizes per type")]
        private List<PoolConfiguration> _preWarmPools = new();
        
        [Header("Runtime Pool Statistics (Read-Only)")]
        [SerializeField] private int _totalPooledObjects;
        [SerializeField] private int _totalActiveObjects;
        [SerializeField] private int _poolHits;
        [SerializeField] private int _poolMisses;
        
        private Dictionary<string, Stack<IPoolable>> _pools;
        private Dictionary<string, GameObject> _prefabs;
        private Dictionary<string, HashSet<IPoolable>> _activeObjects;
        private Dictionary<string, int> _poolLimits;
        
        private const int DEFAULT_POOL_LIMIT = 500;
        
        private void Awake()
        {
            // REMOVED: DontDestroyOnLoad - parent handles it
            InitializePools();
        }
        
        private void InitializePools()
        {
            _pools = new Dictionary<string, Stack<IPoolable>>(32);
            _prefabs = new Dictionary<string, GameObject>(32);
            _activeObjects = new Dictionary<string, HashSet<IPoolable>>(32);
            _poolLimits = new Dictionary<string, int>(32);
            
            if (_poolRoot == null)
            {
                GameObject root = new("PoolRoot");
                root.transform.SetParent(transform);
                _poolRoot = root.transform;
            }
            
            foreach (PoolConfiguration config in _preWarmPools)
            {
                if (config.prefab != null)
                {
                    RegisterPrefab(config.poolKey, config.prefab, config.maxPoolSize);
                    WarmPool(config.poolKey, config.initialSize);
                }
            }
            
            Debug.Log($"[ObjectPoolManager] Initialized with {_preWarmPools.Count} pre-warmed pools");
        }
        
        public void RegisterPrefab(string poolKey, GameObject prefab, int maxPoolSize = DEFAULT_POOL_LIMIT)
        {
            if (string.IsNullOrEmpty(poolKey))
            {
                Debug.LogError("[ObjectPoolManager] Cannot register prefab with empty pool key!");
                return;
            }
            
            if (_prefabs.ContainsKey(poolKey))
            {
                Debug.LogWarning($"[ObjectPoolManager] Pool key '{poolKey}' already registered. Overwriting.");
            }
            
            _prefabs[poolKey] = prefab;
            _poolLimits[poolKey] = maxPoolSize;
            
            if (!_pools.ContainsKey(poolKey))
            {
                _pools[poolKey] = new Stack<IPoolable>(maxPoolSize / 4);
            }
            
            if (!_activeObjects.ContainsKey(poolKey))
            {
                _activeObjects[poolKey] = new HashSet<IPoolable>();
            }
        }
        
        public void WarmPool(string poolKey, int count)
        {
            if (!_prefabs.ContainsKey(poolKey))
            {
                Debug.LogError($"[ObjectPoolManager] Cannot warm pool '{poolKey}' - prefab not registered!");
                return;
            }
            
            GameObject prefab = _prefabs[poolKey];
            Stack<IPoolable> pool = _pools[poolKey];
            
            for (int i = 0; i < count; i++)
            {
                GameObject obj = Instantiate(prefab, _poolRoot);
                obj.name = $"{poolKey}_{i}";
                obj.SetActive(false);
                
                IPoolable poolable = obj.GetComponent<IPoolable>();
                if (poolable == null)
                {
                    Debug.LogError($"[ObjectPoolManager] Prefab for '{poolKey}' missing IPoolable component!");
                    Destroy(obj);
                    continue;
                }
                
                poolable.IsActiveInPool = false;
                pool.Push(poolable);
                _totalPooledObjects++;
            }
            
            Debug.Log($"[ObjectPoolManager] Pre-warmed '{poolKey}' pool with {count} instances");
        }
        
        public T Get<T>(string poolKey, Vector3 position, Quaternion rotation) where T : class, IPoolable
        {
            if (!_pools.ContainsKey(poolKey))
            {
                Debug.LogError($"[ObjectPoolManager] Pool '{poolKey}' not found! Did you forget to RegisterPrefab?");
                return null;
            }
            
            Stack<IPoolable> pool = _pools[poolKey];
            IPoolable poolable;
            
            if (pool.Count > 0)
            {
                poolable = pool.Pop();
                _poolHits++;
            }
            else
            {
                _poolMisses++;
                
                if (!_prefabs.ContainsKey(poolKey))
                {
                    Debug.LogError($"[ObjectPoolManager] No prefab registered for '{poolKey}'!");
                    return null;
                }
                
                int activeCount = _activeObjects[poolKey].Count;
                int poolLimit = _poolLimits[poolKey];
                
                if (activeCount >= poolLimit)
                {
                    Debug.LogWarning($"[ObjectPoolManager] Pool '{poolKey}' exceeded limit ({poolLimit}). Reusing oldest object.");
                    HashSet<IPoolable>.Enumerator enumerator = _activeObjects[poolKey].GetEnumerator();
                    enumerator.MoveNext();
                    IPoolable oldest = enumerator.Current;
                    ReturnToPool(oldest);
                    poolable = pool.Pop();
                }
                else
                {
                    GameObject obj = Instantiate(_prefabs[poolKey], _poolRoot);
                    obj.name = $"{poolKey}_Dynamic_{_totalPooledObjects}";
                    poolable = obj.GetComponent<IPoolable>();
                    _totalPooledObjects++;
                }
            }
            
            Transform t = poolable.GameObject.transform;
            t.position = position;
            t.rotation = rotation;
            t.SetParent(null);
            
            poolable.OnSpawnFromPool();
            _activeObjects[poolKey].Add(poolable);
            _totalActiveObjects++;
            
            return poolable as T;
        }
        
        public T Get<T>(string poolKey) where T : class, IPoolable
        {
            return Get<T>(poolKey, Vector3.zero, Quaternion.identity);
        }
        
        public void ReturnToPool(IPoolable poolable)
        {
            if (poolable == null)
            {
                Debug.LogWarning("[ObjectPoolManager] Attempted to return null object to pool!");
                return;
            }
            
            if (!poolable.IsActiveInPool)
            {
                Debug.LogWarning($"[ObjectPoolManager] Object '{poolable.GameObject.name}' already returned to pool!");
                return;
            }
            
            string poolKey = poolable.PoolKey;
            
            if (!_pools.ContainsKey(poolKey))
            {
                Debug.LogError($"[ObjectPoolManager] Pool '{poolKey}' not found for return!");
                return;
            }
            
            poolable.OnReturnToPool();
            poolable.GameObject.transform.SetParent(_poolRoot);
            
            _pools[poolKey].Push(poolable);
            _activeObjects[poolKey].Remove(poolable);
            _totalActiveObjects--;
        }
        
        public void ClearPool(string poolKey)
        {
            if (!_pools.ContainsKey(poolKey))
            {
                return;
            }
            
            Stack<IPoolable> pool = _pools[poolKey];
            while (pool.Count > 0)
            {
                IPoolable poolable = pool.Pop();
                if (poolable != null && poolable.GameObject != null)
                {
                    Destroy(poolable.GameObject);
                    _totalPooledObjects--;
                }
            }
            
            HashSet<IPoolable> active = _activeObjects[poolKey];
            foreach (IPoolable poolable in active)
            {
                if (poolable != null && poolable.GameObject != null)
                {
                    Destroy(poolable.GameObject);
                    _totalPooledObjects--;
                    _totalActiveObjects--;
                }
            }
            active.Clear();
            
            Debug.Log($"[ObjectPoolManager] Cleared pool '{poolKey}'");
        }
        
        public void ClearAllPools()
        {
            foreach (string poolKey in _pools.Keys)
            {
                ClearPool(poolKey);
            }
            
            _poolHits = 0;
            _poolMisses = 0;
            
            Debug.Log("[ObjectPoolManager] All pools cleared");
        }
        
        public PoolStats GetPoolStats(string poolKey)
        {
            if (!_pools.ContainsKey(poolKey))
            {
                return default;
            }
            
            return new PoolStats
            {
                poolKey = poolKey,
                inactiveCount = _pools[poolKey].Count,
                activeCount = _activeObjects[poolKey].Count,
                totalCount = _pools[poolKey].Count + _activeObjects[poolKey].Count,
                maxPoolSize = _poolLimits[poolKey]
            };
        }
        
        [Serializable]
        public class PoolConfiguration
        {
            public string poolKey = "DefaultPool";
            public GameObject prefab;
            public int initialSize = 10;
            public int maxPoolSize = 100;
        }
        
        public struct PoolStats
        {
            public string poolKey;
            public int inactiveCount;
            public int activeCount;
            public int totalCount;
            public int maxPoolSize;
        }
    }
}