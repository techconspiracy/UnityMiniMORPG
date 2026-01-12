using UnityEngine;
using Game.Core.Systems;

namespace Game.Core
{
    /// <summary>
    /// Initializes the game world and spawns the player.
    /// Place this on a GameObject in the GameWorld scene.
    /// </summary>
    public class GameWorldInitializer : MonoBehaviour
    {
        [Header("World Settings")]
        [SerializeField] private bool _generateZoneOnStart = true;
        [SerializeField] private string _defaultZoneName = "StartingZone";
        
        [Header("Spawn Settings")]
        [SerializeField] private Vector3 _defaultSpawnPosition = new Vector3(0, 5f, 0);
        
        private async void Start()
        {
            await InitializeGameWorld();
        }
        
        private async Awaitable InitializeGameWorld()
        {
            Debug.Log("[GameWorld] Initializing game world...");
            
            // Wait for CoreSystemManager
            while (CoreSystemManager.Instance == null || !CoreSystemManager.Instance.IsReady())
            {
                await Awaitable.NextFrameAsync();
            }
            
            // Generate starting zone if needed
            if (_generateZoneOnStart)
            {
                await GenerateStartingZone();
            }
            
            // Spawn player
            SpawnPlayer();
            
            Debug.Log("[GameWorld] Initialization complete!");
        }
        
        private async Awaitable GenerateStartingZone()
        {
            ZoneSystemManager zoneManager = CoreSystemManager.ZoneManager;
            if (zoneManager == null)
            {
                Debug.LogError("[GameWorld] ZoneManager not found!");
                return;
            }
            
            ZoneConfig config = new ZoneConfig
            {
                zoneName = _defaultZoneName,
                zoneType = ZoneType.Wilderness,
                biomeType = BiomeType.Grassland,
                levelRange = new Vector2Int(1, 5),
                zoneSize = new Vector3(200, 0, 200),
                seed = Random.Range(1000, 9999)
            };
            
            Debug.Log($"[GameWorld] Generating zone '{_defaultZoneName}'...");
            await zoneManager.GenerateZone(config);
        }
        
        private void SpawnPlayer()
        {
            // Load character data
            string json = PlayerPrefs.GetString("CurrentCharacter", "");
            if (string.IsNullOrEmpty(json))
            {
                Debug.LogError("[GameWorld] No character data found! Creating default character.");
                json = JsonUtility.ToJson(new CharacterCreationData());
            }
            
            CharacterCreationData data = JsonUtility.FromJson<CharacterCreationData>(json);
            
            // Get or create character builder
            ProceduralCharacterBuilder builder = FindFirstObjectByType<ProceduralCharacterBuilder>();
            if (builder == null)
            {
                GameObject builderObj = new GameObject("CharacterBuilder");
                builder = builderObj.AddComponent<ProceduralCharacterBuilder>();
            }
            
            // Generate player character
            GameObject player = builder.GenerateCharacter(data);
            player.name = "Player";
            player.tag = "Player";
            player.layer = LayerMask.NameToLayer("Default");
            
            // Add required components
            if (player.GetComponent<EntityStats>() == null)
            {
                EntityStats stats = player.AddComponent<EntityStats>();
                stats.strength = data.strength;
                stats.dexterity = data.dexterity;
                stats.intelligence = data.intelligence;
                stats.vitality = data.vitality;
                stats.endurance = data.endurance;
                stats.luck = data.luck;
                stats.RecalculateStats();
            }
            
            // Add player controller
            PlayerController controller = player.AddComponent<PlayerController>();
            controller.Initialize(data);
            
            // Position player at spawn point
            SpawnPoint[] spawns = FindObjectsByType<SpawnPoint>(FindObjectsSortMode.None);
            bool foundSpawn = false;
            
            foreach (SpawnPoint spawn in spawns)
            {
                if (spawn.Type == SpawnPointType.Player)
                {
                    player.transform.position = spawn.transform.position + Vector3.up * 2f;
                    foundSpawn = true;
                    Debug.Log($"[GameWorld] Player spawned at spawn point: {spawn.transform.position}");
                    break;
                }
            }
            
            if (!foundSpawn)
            {
                player.transform.position = _defaultSpawnPosition;
                Debug.Log($"[GameWorld] Player spawned at default position: {_defaultSpawnPosition}");
            }
            
            // Setup camera
            Camera mainCam = Camera.main;
            if (mainCam != null)
            {
                mainCam.transform.SetParent(player.transform);
                mainCam.transform.localPosition = new Vector3(0, 1.6f, 0);
                mainCam.transform.localRotation = Quaternion.identity;
            }
            
            Debug.Log($"[GameWorld] Player spawned successfully! ({data.species} {data.gender})");
        }
    }
}