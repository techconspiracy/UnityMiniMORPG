using UnityEngine;
using UnityEngine.SceneManagement;

namespace Game.Core.Systems
{
    /// <summary>
    /// Central hub for all core game systems.
    /// FIXED: Proper root GameObject for DontDestroyOnLoad
    /// FIXED: Child systems no longer try to persist independently
    /// </summary>
    public class CoreSystemManager : MonoBehaviour
    {
        public static CoreSystemManager Instance { get; private set; }
        
        [Header("System References (Auto-Assigned at Runtime)")]
        [SerializeField] private ObjectPoolManager _poolManager;
        [SerializeField] private ZoneSystemManager _zoneManager;
        [SerializeField] private EntitySystemManager _entityManager;
        [SerializeField] private CombatSystemManager _combatManager;
        [SerializeField] private InventorySystemManager _inventoryManager;
        [SerializeField] private UISystemManager _uiManager;
        [SerializeField] private AudioSystemManager _audioManager;
        [SerializeField] private WebSocketNetworkManager _networkManager;
        [SerializeField] private AdminConsoleManager _adminConsoleManager;
        
        [Header("Initialization Status")]
        [SerializeField] private bool _isInitialized;
        [SerializeField] private float _initializationTime;
        
        public static ObjectPoolManager PoolManager => Instance?._poolManager;
        public static ZoneSystemManager ZoneManager => Instance?._zoneManager;
        public static EntitySystemManager EntityManager => Instance?._entityManager;
        public static CombatSystemManager CombatManager => Instance?._combatManager;
        public static InventorySystemManager InventoryManager => Instance?._inventoryManager;
        public static UISystemManager UIManager => Instance?._uiManager;
        public static AudioSystemManager AudioManager => Instance?._audioManager;
        public static WebSocketNetworkManager NetworkManager => Instance?._networkManager;
        public static AdminConsoleManager AdminConsole => Instance?._adminConsoleManager;
        
        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            
            Instance = this;
            
            // CRITICAL FIX: Must be root GameObject for DontDestroyOnLoad
            if (transform.parent != null)
            {
                transform.SetParent(null);
            }
            
            DontDestroyOnLoad(gameObject);
            
            InitializeSystems();
        }
        
        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }
        
        private void InitializeSystems()
        {
            float startTime = Time.realtimeSinceStartup;
            
            Debug.Log("[CoreSystemManager] Initializing core systems...");
            
            // Initialize in dependency order
            InitializePoolManager();
            InitializeNetworkManager();
            InitializeZoneManager();
            InitializeEntityManager();
            InitializeCombatManager();
            InitializeInventoryManager();
            InitializeAudioManager();
            InitializeUIManager();
            InitializeAdminConsole();
            
            _initializationTime = Time.realtimeSinceStartup - startTime;
            _isInitialized = true;
            
            Debug.Log($"[CoreSystemManager] All systems initialized in {_initializationTime:F3}s");
        }
        
        private void InitializePoolManager()
        {
            _poolManager = GetOrCreateChildSystem<ObjectPoolManager>("ObjectPoolManager");
        }
        
        private void InitializeNetworkManager()
        {
            _networkManager = GetOrCreateChildSystem<WebSocketNetworkManager>("WebSocketNetworkManager");
        }
        
        private void InitializeZoneManager()
        {
            _zoneManager = GetOrCreateChildSystem<ZoneSystemManager>("ZoneSystemManager");
        }
        
        private void InitializeEntityManager()
        {
            _entityManager = GetOrCreateChildSystem<EntitySystemManager>("EntitySystemManager");
        }
        
        private void InitializeCombatManager()
        {
            _combatManager = GetOrCreateChildSystem<CombatSystemManager>("CombatSystemManager");
        }
        
        private void InitializeInventoryManager()
        {
            _inventoryManager = GetOrCreateChildSystem<InventorySystemManager>("InventorySystemManager");
        }
        
        private void InitializeAudioManager()
        {
            _audioManager = GetOrCreateChildSystem<AudioSystemManager>("AudioSystemManager");
        }
        
        private void InitializeUIManager()
        {
            _uiManager = GetOrCreateChildSystem<UISystemManager>("UISystemManager");
        }
        
        private void InitializeAdminConsole()
        {
            // DISABLED: Admin console has critical bugs, skip for now
            // _adminConsoleManager = GetOrCreateChildSystem<AdminConsoleManager>("AdminConsoleManager");
            Debug.Log("[CoreSystemManager] Admin console disabled (has UI bugs)");
        }
        
        private T GetOrCreateChildSystem<T>(string childName) where T : Component
        {
            Transform child = transform.Find(childName);
            if (child != null)
            {
                T component = child.GetComponent<T>();
                if (component != null)
                {
                    Debug.Log($"[CoreSystemManager] Found existing {typeof(T).Name}");
                    return component;
                }
            }
            
            GameObject systemObj = new GameObject(childName);
            systemObj.transform.SetParent(transform);
            T newComponent = systemObj.AddComponent<T>();
            
            Debug.Log($"[CoreSystemManager] Created {typeof(T).Name}");
            return newComponent;
        }
        
        public bool IsReady()
        {
            return _isInitialized 
                && _poolManager != null
                && _zoneManager != null
                && _entityManager != null
                && _combatManager != null
                && _inventoryManager != null
                && _uiManager != null
                && _audioManager != null
                && _networkManager != null;
        }
        
        public async Awaitable ShutdownAllSystems()
        {
            Debug.Log("[CoreSystemManager] Shutting down all systems...");
            
            if (_uiManager != null) 
                _uiManager.Shutdown();
            
            if (_audioManager != null) 
                _audioManager.Shutdown();
            
            if (_inventoryManager != null) 
                _inventoryManager.Shutdown();
            
            if (_combatManager != null) 
                _combatManager.Shutdown();
            
            if (_entityManager != null) 
                await _entityManager.ShutdownAsync();
            
            if (_zoneManager != null) 
                await _zoneManager.ShutdownAsync();
            
            if (_networkManager != null) 
                await _networkManager.DisconnectAsync();
            
            if (_poolManager != null)
                _poolManager.ClearAllPools();
            
            _isInitialized = false;
            
            Debug.Log("[CoreSystemManager] All systems shut down");
        }
    }
}