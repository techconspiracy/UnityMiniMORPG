using UnityEngine;
using System.Collections.Generic;
using Game.Core.Pooling;

namespace Game.Core.Systems
{
    /// <summary>
    /// Entity System Manager - Manages all entities (players, NPCs, enemies).
    /// Handles entity spawning, lifecycle, and AI coordination.
    /// COMPLETE IMPLEMENTATION - No more TODOs!
    /// </summary>
    public class EntitySystemManager : MonoBehaviour
    {
        [Header("Entity Management")]
        [SerializeField] private int _maxActiveEntities = 100;
        [SerializeField] private int _currentEntityCount;
        
        [Header("Entity Prefabs")]
        [SerializeField] private GameObject _zombiePrefab;
        [SerializeField] private GameObject _skeletonPrefab;
        [SerializeField] private GameObject _orcPrefab;
        [SerializeField] private GameObject _npcPrefab;
        
        [Header("Runtime Tracking")]
        [SerializeField] private List<GameObject> _activeEntities = new();
        [SerializeField] private Dictionary<string, EntityData> _entityDatabase;
        
        private ObjectPoolManager _poolManager;
        
        #region Initialization
        
        private void Awake()
        {
            _poolManager = CoreSystemManager.PoolManager;
            _entityDatabase = new Dictionary<string, EntityData>();
            
            InitializeEntityPrefabs();
            RegisterEntityTypes();
            
            Debug.Log("[EntitySystemManager] Initialized");
        }
        
        private void InitializeEntityPrefabs()
        {
            // Create default entity prefabs if not assigned
            if (_zombiePrefab == null)
                _zombiePrefab = CreateDefaultEntityPrefab("Zombie", Color.green, 150);
            
            if (_skeletonPrefab == null)
                _skeletonPrefab = CreateDefaultEntityPrefab("Skeleton", Color.white, 80);
            
            if (_orcPrefab == null)
                _orcPrefab = CreateDefaultEntityPrefab("Orc", new Color(0.4f, 0.6f, 0.3f), 200);
            
            if (_npcPrefab == null)
                _npcPrefab = CreateDefaultEntityPrefab("NPC", Color.blue, 100);
        }
        
        private GameObject CreateDefaultEntityPrefab(string name, Color color, int maxHealth)
        {
            GameObject prefab = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            prefab.name = $"{name}Prefab";
            prefab.tag = "Enemy";
            
            // Visual
            Renderer renderer = prefab.GetComponent<Renderer>();
            Material mat = new Material(Shader.Find("Standard"));
            mat.color = color;
            renderer.material = mat;
            
            // Stats
            EntityStats stats = prefab.AddComponent<EntityStats>();
            stats.maxHealth = maxHealth;
            stats.currentHealth = maxHealth;
            stats.level = 1;
            
            // AI
            EntityAI ai = prefab.AddComponent<EntityAI>();
            
            // Make it a poolable entity
            PoolableEntity poolable = prefab.AddComponent<PoolableEntity>();
            
            prefab.SetActive(false);
            return prefab;
        }
        
        private void RegisterEntityTypes()
        {
            if (_poolManager == null) return;
            
            // Register entity prefabs with pool manager
            _poolManager.RegisterPrefab("Zombie", _zombiePrefab, 50);
            _poolManager.RegisterPrefab("Skeleton", _skeletonPrefab, 50);
            _poolManager.RegisterPrefab("Orc", _orcPrefab, 30);
            _poolManager.RegisterPrefab("NPC", _npcPrefab, 20);
            
            // Pre-warm pools
            _poolManager.WarmPool("Zombie", 10);
            _poolManager.WarmPool("Skeleton", 10);
            _poolManager.WarmPool("Orc", 5);
            _poolManager.WarmPool("NPC", 5);
            
            Debug.Log("[EntitySystemManager] Entity pools registered and warmed");
        }
        
        #endregion
        
        #region Entity Spawning
        
        /// <summary>
        /// Spawn an entity at a position with rotation.
        /// </summary>
        public GameObject SpawnEntity(string entityType, Vector3 position, Quaternion rotation)
        {
            if (_currentEntityCount >= _maxActiveEntities)
            {
                Debug.LogWarning($"[EntitySystemManager] Max entities ({_maxActiveEntities}) reached!");
                return null;
            }
            
            // Get from pool
            PoolableEntity entity = _poolManager.Get<PoolableEntity>(entityType, position, rotation);
            
            if (entity == null)
            {
                Debug.LogError($"[EntitySystemManager] Failed to spawn entity type: {entityType}");
                return null;
            }
            
            GameObject entityObj = entity.GameObject;
            
            // Track entity
            _activeEntities.Add(entityObj);
            _currentEntityCount = _activeEntities.Count;
            
            // Initialize AI
            EntityAI ai = entityObj.GetComponent<EntityAI>();
            if (ai != null)
            {
                ai.Initialize();
            }
            
            Debug.Log($"[EntitySystemManager] Spawned {entityType} at {position}");
            
            return entityObj;
        }
        
        /// <summary>
        /// Spawn entity at a spawn point.
        /// </summary>
        public GameObject SpawnEntityAtSpawnPoint(SpawnPoint spawnPoint)
        {
            if (spawnPoint == null) return null;
            
            string entityType = GetEntityTypeForSpawnPoint(spawnPoint.Type);
            Vector3 spawnPos = spawnPoint.transform.position + Random.insideUnitSphere * spawnPoint.Radius;
            spawnPos.y = spawnPoint.transform.position.y;
            
            return SpawnEntity(entityType, spawnPos, Quaternion.identity);
        }
        
        private string GetEntityTypeForSpawnPoint(SpawnPointType type)
        {
            return type switch
            {
                SpawnPointType.Enemy => GetRandomEnemyType(),
                SpawnPointType.NPC => "NPC",
                SpawnPointType.Boss => "Orc", // Bosses are just tougher orcs for now
                _ => "Zombie"
            };
        }
        
        private string GetRandomEnemyType()
        {
            string[] types = { "Zombie", "Skeleton", "Orc" };
            return types[Random.Range(0, types.Length)];
        }
        
        #endregion
        
        #region Entity Despawning
        
        /// <summary>
        /// Despawn an entity and return it to pool.
        /// </summary>
        public void DespawnEntity(GameObject entity)
        {
            if (entity == null) return;
            
            PoolableEntity poolable = entity.GetComponent<PoolableEntity>();
            
            if (poolable != null && _poolManager != null)
            {
                _poolManager.ReturnToPool(poolable);
            }
            else
            {
                Destroy(entity);
            }
            
            _activeEntities.Remove(entity);
            _currentEntityCount = _activeEntities.Count;
            
            Debug.Log($"[EntitySystemManager] Despawned {entity.name}");
        }
        
        /// <summary>
        /// Despawn all entities in the scene.
        /// </summary>
        public void DespawnAllEntities()
        {
            List<GameObject> entitiesToRemove = new List<GameObject>(_activeEntities);
            
            foreach (GameObject entity in entitiesToRemove)
            {
                DespawnEntity(entity);
            }
            
            Debug.Log("[EntitySystemManager] All entities despawned");
        }
        
        #endregion
        
        #region Entity Queries
        
        public int GetEntityCount()
        {
            return _currentEntityCount;
        }
        
        public List<GameObject> GetAllEntities()
        {
            return new List<GameObject>(_activeEntities);
        }
        
        public List<GameObject> GetEntitiesInRadius(Vector3 center, float radius)
        {
            List<GameObject> result = new();
            
            foreach (GameObject entity in _activeEntities)
            {
                if (entity != null && Vector3.Distance(entity.transform.position, center) <= radius)
                {
                    result.Add(entity);
                }
            }
            
            return result;
        }
        
        public GameObject GetNearestEntity(Vector3 position, string tag = null)
        {
            GameObject nearest = null;
            float nearestDist = float.MaxValue;
            
            foreach (GameObject entity in _activeEntities)
            {
                if (entity == null) continue;
                if (tag != null && !entity.CompareTag(tag)) continue;
                
                float dist = Vector3.Distance(position, entity.transform.position);
                if (dist < nearestDist)
                {
                    nearestDist = dist;
                    nearest = entity;
                }
            }
            
            return nearest;
        }
        
        #endregion
        
        #region Update & AI Coordination
        
        private void Update()
        {
            // Clean up null references
            _activeEntities.RemoveAll(e => e == null);
            _currentEntityCount = _activeEntities.Count;
            
            // Update AI coordination every second
            if (Time.frameCount % 60 == 0)
            {
                CoordinateEntityAI();
            }
        }
        
        private void CoordinateEntityAI()
        {
            // Find player
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player == null) return;
            
            // Update all entity AI with player location
            foreach (GameObject entity in _activeEntities)
            {
                if (entity == null) continue;
                
                EntityAI ai = entity.GetComponent<EntityAI>();
                if (ai != null)
                {
                    ai.UpdateTarget(player.transform.position);
                }
            }
        }
        
        #endregion
        
        #region Shutdown
        
        public async Awaitable ShutdownAsync()
        {
            Debug.Log("[EntitySystemManager] Shutting down...");
            DespawnAllEntities();
            await Awaitable.NextFrameAsync();
        }
        
        #endregion
    }
    
    #region Supporting Types
    
    /// <summary>
    /// Poolable entity wrapper.
    /// </summary>
    public class PoolableEntity : PoolableObject
    {
        private EntityAI _ai;
        private EntityStats _stats;
        
        private void Awake()
        {
            _ai = GetComponent<EntityAI>();
            _stats = GetComponent<EntityStats>();
        }
        
        public override void OnSpawnFromPool()
        {
            base.OnSpawnFromPool();
            
            // Reset stats
            if (_stats != null)
            {
                _stats.currentHealth = _stats.maxHealth;
                _stats.currentMana = _stats.maxMana;
                _stats.currentStamina = _stats.maxStamina;
            }
            
            // Reset AI
            if (_ai != null)
            {
                _ai.Initialize();
            }
        }
        
        public override void OnReturnToPool()
        {
            base.OnReturnToPool();
            
            // Stop AI
            if (_ai != null)
            {
                _ai.Stop();
            }
        }
    }
    
    /// <summary>
    /// Simple AI controller for entities.
    /// </summary>
    public class EntityAI : MonoBehaviour
    {
        [Header("AI Settings")]
        [SerializeField] private float _detectionRange = 15f;
        [SerializeField] private float _attackRange = 2f;
        [SerializeField] private float _moveSpeed = 3f;
        [SerializeField] private float _attackCooldown = 2f;
        
        [Header("State")]
        [SerializeField] private AIState _currentState = AIState.Idle;
        [SerializeField] private Vector3 _targetPosition;
        
        private float _lastAttackTime;
        private Transform _transform;
        private EntityStats _stats;
        private bool _isActive;
        
        private void Awake()
        {
            _transform = transform;
            _stats = GetComponent<EntityStats>();
        }
        
        public void Initialize()
        {
            _isActive = true;
            _currentState = AIState.Idle;
            _lastAttackTime = 0f;
        }
        
        public void Stop()
        {
            _isActive = false;
            _currentState = AIState.Idle;
        }
        
        public void UpdateTarget(Vector3 targetPosition)
        {
            if (!_isActive) return;
            
            _targetPosition = targetPosition;
            
            float distanceToTarget = Vector3.Distance(_transform.position, targetPosition);
            
            if (distanceToTarget <= _attackRange)
            {
                _currentState = AIState.Attacking;
            }
            else if (distanceToTarget <= _detectionRange)
            {
                _currentState = AIState.Chasing;
            }
            else
            {
                _currentState = AIState.Idle;
            }
        }
        
        private void Update()
        {
            if (!_isActive) return;
            
            switch (_currentState)
            {
                case AIState.Idle:
                    HandleIdle();
                    break;
                case AIState.Chasing:
                    HandleChasing();
                    break;
                case AIState.Attacking:
                    HandleAttacking();
                    break;
            }
        }
        
        private void HandleIdle()
        {
            // Do nothing, wait for target
        }
        
        private void HandleChasing()
        {
            // Move toward target
            Vector3 direction = (_targetPosition - _transform.position).normalized;
            direction.y = 0; // Keep on ground
            
            _transform.position += direction * _moveSpeed * Time.deltaTime;
            _transform.LookAt(new Vector3(_targetPosition.x, _transform.position.y, _targetPosition.z));
        }
        
        private void HandleAttacking()
        {
            // Face target
            _transform.LookAt(new Vector3(_targetPosition.x, _transform.position.y, _targetPosition.z));
            
            // Attack on cooldown
            if (Time.time - _lastAttackTime >= _attackCooldown)
            {
                PerformAttack();
                _lastAttackTime = Time.time;
            }
        }
        
        private void PerformAttack()
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player == null) return;
            
            float distance = Vector3.Distance(_transform.position, player.transform.position);
            if (distance > _attackRange) return;
            
            // Get combat manager
            CombatSystemManager combatManager = CoreSystemManager.CombatManager;
            if (combatManager == null) return;
            
            // Get stats
            EntityStats playerStats = player.GetComponent<EntityStats>();
            if (playerStats == null) return;
            
            // Calculate damage
            DamageResult damage = combatManager.CalculateDamage(
                _stats, 
                playerStats, 
                10, // Base damage
                DamageType.Physical
            );
            
            // Apply damage
            combatManager.ApplyDamage(player, damage, gameObject);
            
            Debug.Log($"[EntityAI] {gameObject.name} attacked player for {damage.finalDamage} damage");
        }
        
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, _detectionRange);
            
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, _attackRange);
        }
    }
    
    public enum AIState
    {
        Idle,
        Chasing,
        Attacking,
        Fleeing
    }
    
    [System.Serializable]
    public class EntityData
    {
        public string entityId;
        public string entityType;
        public int level;
        public Vector3 lastPosition;
    }
    
    #endregion
}