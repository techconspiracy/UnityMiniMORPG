using UnityEngine;

namespace Game.Core.Pooling
{
    /// <summary>
    /// Interface for all objects that can be pooled.
    /// Ensures consistent lifecycle management across all pooled types.
    /// </summary>
    public interface IPoolable
    {
        /// <summary>
        /// Called when object is retrieved from pool.
        /// Use this instead of Awake/Start for pooled objects.
        /// </summary>
        void OnSpawnFromPool();
        
        /// <summary>
        /// Called when object is returned to pool.
        /// Reset all state here to avoid cross-contamination.
        /// </summary>
        void OnReturnToPool();
        
        /// <summary>
        /// The GameObject this poolable is attached to.
        /// Required for transform operations and SetActive calls.
        /// </summary>
        GameObject GameObject { get; }
        
        /// <summary>
        /// Unique identifier for this poolable type.
        /// Used for pool organization and debugging.
        /// </summary>
        string PoolKey { get; }
        
        /// <summary>
        /// Whether this object is currently active in the scene.
        /// Used to prevent double-returns to pool.
        /// </summary>
        bool IsActiveInPool { get; set; }
    }
    
    /// <summary>
    /// Base MonoBehaviour implementation of IPoolable.
    /// Inherit from this for quick poolable object creation.
    /// </summary>
    public abstract class PoolableObject : MonoBehaviour, IPoolable
    {
        [Header("Pooling")]
        [SerializeField, Tooltip("Unique key for this poolable type")]
        private string _poolKey = "DefaultPool";
        
        private bool _isActiveInPool;
        
        public GameObject GameObject => gameObject;
        public string PoolKey => _poolKey;
        public bool IsActiveInPool 
        { 
            get => _isActiveInPool; 
            set => _isActiveInPool = value; 
        }
        
        /// <summary>
        /// Override this to handle spawn logic.
        /// Called when retrieved from pool.
        /// </summary>
        public virtual void OnSpawnFromPool()
        {
            _isActiveInPool = true;
            gameObject.SetActive(true);
        }
        
        /// <summary>
        /// Override this to handle return logic.
        /// Called when returned to pool.
        /// IMPORTANT: Reset all state here!
        /// </summary>
        public virtual void OnReturnToPool()
        {
            _isActiveInPool = false;
            gameObject.SetActive(false);
        }
        
        /// <summary>
        /// Helper method to return this object to pool.
        /// Call instead of Destroy() for pooled objects.
        /// </summary>
        protected void ReturnToPool()
        {
            Game.Core.Systems.ObjectPoolManager poolManager = Game.Core.Systems.CoreSystemManager.PoolManager;
            if (poolManager != null)
            {
                poolManager.ReturnToPool(this);
            }
            else
            {
                Debug.LogError($"Cannot return {gameObject.name} to pool - ObjectPoolManager not found!");
            }
        }
    }
}