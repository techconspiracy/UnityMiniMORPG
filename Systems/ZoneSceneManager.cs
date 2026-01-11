using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System;

namespace Game.Core.Systems
{
    /// <summary>
    /// Manages zone loading, unloading, and scene caching.
    /// Zones are Unity scenes that can be generated procedurally and saved.
    /// Supports additive scene loading for seamless zone transitions.
    /// </summary>
    public class ZoneSceneManager : MonoBehaviour
    {
        [Header("Zone Configuration")]
        [SerializeField, Tooltip("Path where generated zones are saved")]
        private string _zoneSavePath = "Assets/Scenes/Zones/";
        
        [SerializeField, Tooltip("Maximum zones loaded simultaneously")]
        private int _maxLoadedZones = 3;
        
        [Header("Runtime State")]
        [SerializeField] private string _currentZoneName;
        [SerializeField] private List<string> _loadedZones = new();
        [SerializeField] private List<string> _cachedZoneNames = new();
        
        // Zone metadata cache
        private Dictionary<string, ZoneMetadata> _zoneMetadataCache;
        
        // Active zone scenes
        private Dictionary<string, Scene> _activeZoneScenes;
        
        // Zone generation callback
        private SimpleTerrainGenerator _terrainGenerator;
        
        #region Unity Lifecycle
        
        private void Awake()
        {
            InitializeZoneManager();
        }
        
        #endregion
        
        #region Initialization
        
        private void InitializeZoneManager()
        {
            _zoneMetadataCache = new Dictionary<string, ZoneMetadata>();
            _activeZoneScenes = new Dictionary<string, Scene>();
            _loadedZones = new List<string>();
            _cachedZoneNames = new List<string>();
            
            // Get terrain generator reference
            _terrainGenerator = GetComponent<SimpleTerrainGenerator>();
            if (_terrainGenerator == null)
            {
                _terrainGenerator = gameObject.AddComponent<SimpleTerrainGenerator>();
            }
            
            ScanForCachedZones();
            
            Debug.Log($"[ZoneSceneManager] Initialized. Found {_cachedZoneNames.Count} cached zones.");
        }
        
        private void ScanForCachedZones()
        {
            // In runtime, check PlayerPrefs for cached zone list
            // In editor, scan directory for .unity files
            
            #if UNITY_EDITOR
            if (System.IO.Directory.Exists(_zoneSavePath))
            {
                string[] sceneFiles = System.IO.Directory.GetFiles(_zoneSavePath, "*.unity");
                foreach (string file in sceneFiles)
                {
                    string zoneName = System.IO.Path.GetFileNameWithoutExtension(file);
                    _cachedZoneNames.Add(zoneName);
                }
            }
            #else
            // Runtime: Load from PlayerPrefs
            string cachedList = PlayerPrefs.GetString("CachedZones", "");
            if (!string.IsNullOrEmpty(cachedList))
            {
                _cachedZoneNames.AddRange(cachedList.Split(','));
            }
            #endif
        }
        
        #endregion
        
        #region Zone Generation
        
        /// <summary>
        /// Generate a new zone procedurally and optionally save it as a scene.
        /// </summary>
        public async Awaitable<Scene> GenerateAndSaveZone(ZoneConfig config)
        {
            Debug.Log($"[ZoneSceneManager] Generating zone '{config.zoneName}'...");
            
            // Create new scene additively
            Scene newScene = SceneManager.CreateScene(config.zoneName);
            SceneManager.SetActiveScene(newScene);
            
            // Create zone root object
            GameObject zoneRoot = new($"Zone_{config.zoneName}");
            SceneManager.MoveGameObjectToScene(zoneRoot, newScene);
            
            // Generate terrain
            await _terrainGenerator.GenerateTerrainForZone(zoneRoot.transform, config);
            
            // Place interactables
            await PlaceInteractables(zoneRoot.transform, config);
            
            // Setup spawn points
            CreateSpawnPoints(zoneRoot.transform, config);
            
            // Create zone boundary
            CreateZoneBoundary(zoneRoot.transform, config);
            
            // Save metadata
            ZoneMetadata metadata = new()
            {
                zoneName = config.zoneName,
                zoneType = config.zoneType,
                levelRange = config.levelRange,
                biomeType = config.biomeType,
                generatedTimestamp = DateTime.Now.ToString()
            };
            _zoneMetadataCache[config.zoneName] = metadata;
            
            // Save scene (Editor only)
            #if UNITY_EDITOR
            SaveZoneAsScene(config.zoneName, newScene);
            #else
            // Runtime: Save to ScriptableObject
            SaveZoneMetadata(metadata);
            #endif
            
            // Add to loaded zones
            _activeZoneScenes[config.zoneName] = newScene;
            _loadedZones.Add(config.zoneName);
            _currentZoneName = config.zoneName;
            
            Debug.Log($"[ZoneSceneManager] Zone '{config.zoneName}' generated and saved");
            
            return newScene;
        }
        
        #endregion
        
        #region Zone Loading
        
        /// <summary>
        /// Load a zone scene additively.
        /// Checks cache first, generates if not found.
        /// </summary>
        public async Awaitable<Scene> LoadZone(string zoneName, ZoneConfig config = null)
        {
            // Check if already loaded
            if (_activeZoneScenes.ContainsKey(zoneName))
            {
                Debug.Log($"[ZoneSceneManager] Zone '{zoneName}' already loaded");
                return _activeZoneScenes[zoneName];
            }
            
            // Check max loaded zones limit
            if (_loadedZones.Count >= _maxLoadedZones)
            {
                await UnloadOldestZone();
            }
            
            // Check if zone is cached
            if (IsZoneCached(zoneName))
            {
                return await LoadCachedZone(zoneName);
            }
            else
            {
                // Generate new zone
                if (config == null)
                {
                    Debug.LogWarning($"[ZoneSceneManager] Zone '{zoneName}' not cached and no config provided. Using default.");
                    config = ZoneConfig.CreateDefault(zoneName);
                }
                
                return await GenerateAndSaveZone(config);
            }
        }
        
        private async Awaitable<Scene> LoadCachedZone(string zoneName)
        {
            Debug.Log($"[ZoneSceneManager] Loading cached zone '{zoneName}'...");
            
            AsyncOperation loadOp = SceneManager.LoadSceneAsync(zoneName, LoadSceneMode.Additive);
            
            if (loadOp == null)
            {
                Debug.LogError($"[ZoneSceneManager] Failed to load zone '{zoneName}'!");
                return default;
            }
            
            while (!loadOp.isDone)
            {
                await Awaitable.NextFrameAsync();
            }
            
            Scene loadedScene = SceneManager.GetSceneByName(zoneName);
            _activeZoneScenes[zoneName] = loadedScene;
            _loadedZones.Add(zoneName);
            _currentZoneName = zoneName;
            
            Debug.Log($"[ZoneSceneManager] Zone '{zoneName}' loaded from cache");
            
            return loadedScene;
        }
        
        #endregion
        
        #region Zone Unloading
        
        /// <summary>
        /// Unload a zone scene and cache its state.
        /// </summary>
        public async Awaitable UnloadZone(string zoneName)
        {
            if (!_activeZoneScenes.ContainsKey(zoneName))
            {
                Debug.LogWarning($"[ZoneSceneManager] Zone '{zoneName}' not loaded, cannot unload");
                return;
            }
            
            Debug.Log($"[ZoneSceneManager] Unloading zone '{zoneName}'...");
            
            Scene scene = _activeZoneScenes[zoneName];
            
            // Save zone state before unloading (if needed)
            // TODO: Serialize entity states, player progress, etc.
            
            AsyncOperation unloadOp = SceneManager.UnloadSceneAsync(scene);
            
            while (!unloadOp.isDone)
            {
                await Awaitable.NextFrameAsync();
            }
            
            _activeZoneScenes.Remove(zoneName);
            _loadedZones.Remove(zoneName);
            
            if (_currentZoneName == zoneName)
            {
                _currentZoneName = _loadedZones.Count > 0 ? _loadedZones[0] : null;
            }
            
            Debug.Log($"[ZoneSceneManager] Zone '{zoneName}' unloaded");
        }
        
        private async Awaitable UnloadOldestZone()
        {
            if (_loadedZones.Count == 0) return;
            
            string oldestZone = _loadedZones[0];
            Debug.Log($"[ZoneSceneManager] Unloading oldest zone '{oldestZone}' due to max limit");
            await UnloadZone(oldestZone);
        }
        
        #endregion
        
        #region Zone Helpers
        
        public bool IsZoneCached(string zoneName)
        {
            return _cachedZoneNames.Contains(zoneName);
        }
        
        public List<string> GetCachedZones()
        {
            return new List<string>(_cachedZoneNames);
        }
        
        public string GetCurrentZone()
        {
            return _currentZoneName;
        }
        
        public ZoneMetadata GetZoneMetadata(string zoneName)
        {
            return _zoneMetadataCache.ContainsKey(zoneName) 
                ? _zoneMetadataCache[zoneName] 
                : null;
        }
        
        #endregion
        
        #region Interactable Placement
        
        private async Awaitable PlaceInteractables(Transform zoneRoot, ZoneConfig config)
        {
            // Create interactables container
            GameObject interactablesContainer = new("Interactables");
            interactablesContainer.transform.SetParent(zoneRoot);
            
            // Place based on zone type
            switch (config.zoneType)
            {
                case ZoneType.Town:
                    await PlaceTownInteractables(interactablesContainer.transform, config);
                    break;
                case ZoneType.Dungeon:
                    await PlaceDungeonInteractables(interactablesContainer.transform, config);
                    break;
                case ZoneType.Wilderness:
                    await PlaceWildernessInteractables(interactablesContainer.transform, config);
                    break;
            }
            
            await Awaitable.NextFrameAsync();
        }
        
        private async Awaitable PlaceTownInteractables(Transform parent, ZoneConfig config)
        {
            // TODO: Place NPCs, shops, quest givers, doors, etc.
            await Awaitable.NextFrameAsync();
        }
        
        private async Awaitable PlaceDungeonInteractables(Transform parent, ZoneConfig config)
        {
            // TODO: Place chests, doors, traps, levers, etc.
            await Awaitable.NextFrameAsync();
        }
        
        private async Awaitable PlaceWildernessInteractables(Transform parent, ZoneConfig config)
        {
            // TODO: Place resource nodes, campfires, etc.
            await Awaitable.NextFrameAsync();
        }
        
        #endregion
        
        #region Spawn Points
        
        private void CreateSpawnPoints(Transform zoneRoot, ZoneConfig config)
        {
            GameObject spawnContainer = new("SpawnPoints");
            spawnContainer.transform.SetParent(zoneRoot);
            
            // Create player spawn
            CreateSpawnPoint(spawnContainer.transform, "PlayerSpawn", SpawnPointType.Player, Vector3.zero);
            
            // Create enemy spawns based on zone type
            int enemySpawnCount = config.zoneType == ZoneType.Town ? 0 : 10;
            for (int i = 0; i < enemySpawnCount; i++)
            {
                Vector3 randomPos = new(
                    UnityEngine.Random.Range(-config.zoneSize.x / 2, config.zoneSize.x / 2),
                    0,
                    UnityEngine.Random.Range(-config.zoneSize.z / 2, config.zoneSize.z / 2)
                );
                
                CreateSpawnPoint(spawnContainer.transform, $"EnemySpawn_{i}", SpawnPointType.Enemy, randomPos);
            }
        }
        
        private void CreateSpawnPoint(Transform parent, string spawnName, SpawnPointType type, Vector3 position)
        {
            GameObject spawnObj = new(spawnName);
            spawnObj.transform.SetParent(parent);
            spawnObj.transform.position = position;
            
            SpawnPoint spawn = spawnObj.AddComponent<SpawnPoint>();
            spawn.Initialize(type, 5f, 10);
        }
        
        #endregion
        
        #region Zone Boundary
        
        private void CreateZoneBoundary(Transform zoneRoot, ZoneConfig config)
        {
            GameObject boundary = new("ZoneBoundary");
            boundary.transform.SetParent(zoneRoot);
            boundary.layer = LayerMask.NameToLayer("Default");
            
            // Create invisible box collider walls around zone
            float wallHeight = 50f;
            Vector3 size = config.zoneSize;
            
            // North wall
            CreateBoundaryWall(boundary.transform, new Vector3(0, wallHeight / 2, size.z / 2), 
                new Vector3(size.x, wallHeight, 1));
            
            // South wall
            CreateBoundaryWall(boundary.transform, new Vector3(0, wallHeight / 2, -size.z / 2), 
                new Vector3(size.x, wallHeight, 1));
            
            // East wall
            CreateBoundaryWall(boundary.transform, new Vector3(size.x / 2, wallHeight / 2, 0), 
                new Vector3(1, wallHeight, size.z));
            
            // West wall
            CreateBoundaryWall(boundary.transform, new Vector3(-size.x / 2, wallHeight / 2, 0), 
                new Vector3(1, wallHeight, size.z));
        }
        
        private void CreateBoundaryWall(Transform parent, Vector3 position, Vector3 size)
        {
            GameObject wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
            wall.name = "BoundaryWall";
            wall.transform.SetParent(parent);
            wall.transform.localPosition = position;
            wall.transform.localScale = size;
            
            // Make invisible but keep collider
            Renderer renderer = wall.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.enabled = false;
            }
        }
        
        #endregion
        
        #region Serialization
        
        #if UNITY_EDITOR
        private void SaveZoneAsScene(string zoneName, Scene scene)
        {
            if (!System.IO.Directory.Exists(_zoneSavePath))
            {
                System.IO.Directory.CreateDirectory(_zoneSavePath);
            }
            
            string scenePath = $"{_zoneSavePath}{zoneName}.unity";
            bool saved = UnityEditor.SceneManagement.EditorSceneManager.SaveScene(scene, scenePath);
            
            if (saved)
            {
                _cachedZoneNames.Add(zoneName);
                Debug.Log($"[ZoneSceneManager] Zone saved to: {scenePath}");
            }
        }
        #endif
        
        private void SaveZoneMetadata(ZoneMetadata metadata)
        {
            // Runtime: Save metadata to PlayerPrefs or file
            string json = JsonUtility.ToJson(metadata);
            PlayerPrefs.SetString($"ZoneMeta_{metadata.zoneName}", json);
            
            // Update cached zones list
            if (!_cachedZoneNames.Contains(metadata.zoneName))
            {
                _cachedZoneNames.Add(metadata.zoneName);
                PlayerPrefs.SetString("CachedZones", string.Join(",", _cachedZoneNames));
            }
            
            PlayerPrefs.Save();
        }
        
        #endregion
        
        #region Shutdown
        
        public async Awaitable ShutdownAsync()
        {
            // Unload all zones
            List<string> zonesToUnload = new(_loadedZones);
            foreach (string zone in zonesToUnload)
            {
                await UnloadZone(zone);
            }
        }
        
        #endregion
    }
    
    #region Supporting Types
    
    [Serializable]
    public class ZoneConfig
    {
        public string zoneName = "NewZone";
        public ZoneType zoneType = ZoneType.Wilderness;
        public BiomeType biomeType = BiomeType.Grassland;
        public Vector2Int levelRange = new(1, 10);
        public Vector3 zoneSize = new(100, 0, 100);
        public int seed = 12345;
        
        public static ZoneConfig CreateDefault(string name)
        {
            return new ZoneConfig { zoneName = name };
        }
    }
    
    [Serializable]
    public class ZoneMetadata
    {
        public string zoneName;
        public ZoneType zoneType;
        public BiomeType biomeType;
        public Vector2Int levelRange;
        public string generatedTimestamp;
    }
    
    public enum ZoneType
    {
        Town,
        Dungeon,
        Wilderness,
        Arena
    }
    
    public enum BiomeType
    {
        Grassland,
        Desert,
        Snow,
        Lava,
        Corrupted
    }
    
    public enum SpawnPointType
    {
        Player,
        Enemy,
        NPC,
        Boss,
        Resource
    }
    
    /// <summary>
    /// Spawn point component for zone entity placement.
    /// </summary>
    public class SpawnPoint : MonoBehaviour
    {
        [SerializeField] private SpawnPointType _type;
        [SerializeField] private float _radius = 5f;
        [SerializeField] private int _maxEntities = 10;
        
        public SpawnPointType Type => _type;
        public float Radius => _radius;
        public int MaxEntities => _maxEntities;
        
        public void Initialize(SpawnPointType type, float radius, int maxEntities)
        {
            _type = type;
            _radius = radius;
            _maxEntities = maxEntities;
        }
        
        private void OnDrawGizmos()
        {
            Gizmos.color = _type switch
            {
                SpawnPointType.Player => Color.green,
                SpawnPointType.Enemy => Color.red,
                SpawnPointType.NPC => Color.blue,
                SpawnPointType.Boss => Color.magenta,
                SpawnPointType.Resource => Color.yellow,
                _ => Color.white
            };
            
            Gizmos.DrawWireSphere(transform.position, _radius);
        }
    }
    
    #endregion
}