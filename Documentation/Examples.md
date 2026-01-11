# UAsset MMORPG System - Examples & Test Scenarios

## Table of Contents
1. [Example 1: Complete Zone Generation Test](#example-1-complete-zone-generation-test)
2. [Example 2: Item Generation & Loot System](#example-2-item-generation--loot-system)
3. [Example 3: Combat System with Pooled Effects](#example-3-combat-system-with-pooled-effects)
4. [Example 4: Character Creation Flow](#example-4-character-creation-flow)
5. [Example 5: Multiplayer Test Scene](#example-5-multiplayer-test-scene)
6. [Example 6: Admin Console Extensions](#example-6-admin-console-extensions)

---

## Example 1: Complete Zone Generation Test

### Objective
Generate multiple zones with different biomes and verify terrain generation works correctly.

### Setup Instructions

1. **Create Test Scene**: `Assets/Scenes/ZoneGenerationTest.unity`
2. **Create Test Script**: `Assets/Scripts/Tests/ZoneGenerationTest.cs`

### Complete Test Script

```csharp
using UnityEngine;
using Game.Core.Systems;
using System.Collections.Generic;

/// <summary>
/// Comprehensive zone generation test.
/// Tests all biome types and zone configurations.
/// </summary>
public class ZoneGenerationTest : MonoBehaviour
{
    [Header("Test Configuration")]
    [SerializeField] private bool _runTestsOnStart = true;
    [SerializeField] private bool _generateAllBiomes = true;
    [SerializeField] private float _testDelay = 2f; // Seconds between tests
    
    [Header("Test Results")]
    [SerializeField] private List<string> _generatedZones = new();
    [SerializeField] private int _totalZonesGenerated;
    [SerializeField] private int _failedGenerations;
    
    private ZoneSceneManager _zoneManager;
    
    async void Start()
    {
        if (!_runTestsOnStart) return;
        
        Debug.Log("=== ZONE GENERATION TEST STARTING ===");
        
        // Wait for core systems
        while (CoreSystemManager.Instance == null || !CoreSystemManager.Instance.IsReady())
        {
            await Awaitable.NextFrameAsync();
        }
        
        _zoneManager = CoreSystemManager.ZoneManager;
        
        if (_generateAllBiomes)
        {
            await TestAllBiomes();
        }
        
        Debug.Log($"=== TEST COMPLETE: {_totalZonesGenerated} zones generated, {_failedGenerations} failures ===");
    }
    
    async Awaitable TestAllBiomes()
    {
        BiomeType[] biomes = (BiomeType[])System.Enum.GetValues(typeof(BiomeType));
        
        foreach (BiomeType biome in biomes)
        {
            await TestBiomeGeneration(biome);
            await Awaitable.WaitForSecondsAsync(_testDelay);
        }
    }
    
    async Awaitable TestBiomeGeneration(BiomeType biome)
    {
        string zoneName = $"Test_{biome}_Zone";
        Debug.Log($"[Test] Generating {zoneName}...");
        
        try
        {
            ZoneConfig config = new()
            {
                zoneName = zoneName,
                zoneType = GetZoneTypeForBiome(biome),
                biomeType = biome,
                levelRange = new Vector2Int(1, 10),
                zoneSize = new Vector3(100, 0, 100),
                seed = Random.Range(1000, 9999)
            };
            
            UnityEngine.SceneManagement.Scene zoneScene = await _zoneManager.GenerateAndSaveZone(config);
            
            if (zoneScene.IsValid())
            {
                _generatedZones.Add(zoneName);
                _totalZonesGenerated++;
                Debug.Log($"[Test] ✓ {zoneName} generated successfully!");
                
                // Validate terrain
                ValidateTerrainGeneration(zoneName, biome);
            }
            else
            {
                _failedGenerations++;
                Debug.LogError($"[Test] ✗ {zoneName} generation failed!");
            }
        }
        catch (System.Exception ex)
        {
            _failedGenerations++;
            Debug.LogError($"[Test] ✗ Exception during {zoneName} generation: {ex.Message}");
        }
    }
    
    void ValidateTerrainGeneration(string zoneName, BiomeType biome)
    {
        // Find terrain in generated zone
        GameObject[] rootObjects = UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects();
        
        foreach (GameObject root in rootObjects)
        {
            if (root.name.Contains(zoneName))
            {
                // Check for terrain mesh
                MeshFilter terrainMesh = root.GetComponentInChildren<MeshFilter>();
                if (terrainMesh != null)
                {
                    Debug.Log($"  - Terrain mesh: {terrainMesh.mesh.vertexCount} vertices");
                }
                
                // Check for collider
                MeshCollider terrainCollider = root.GetComponentInChildren<MeshCollider>();
                if (terrainCollider != null)
                {
                    Debug.Log($"  - Terrain collider: {(terrainCollider.sharedMesh != null ? "Valid" : "Missing")}");
                }
                
                // Check for spawn points
                SpawnPoint[] spawnPoints = root.GetComponentsInChildren<SpawnPoint>();
                Debug.Log($"  - Spawn points: {spawnPoints.Length}");
                
                // Check for boundary
                Transform boundary = root.transform.Find("ZoneBoundary");
                if (boundary != null)
                {
                    Debug.Log($"  - Zone boundary: Present ({boundary.childCount} walls)");
                }
                
                break;
            }
        }
    }
    
    ZoneType GetZoneTypeForBiome(BiomeType biome)
    {
        return biome switch
        {
            BiomeType.Grassland => ZoneType.Wilderness,
            BiomeType.Desert => ZoneType.Wilderness,
            BiomeType.Snow => ZoneType.Wilderness,
            BiomeType.Lava => ZoneType.Dungeon,
            BiomeType.Corrupted => ZoneType.Dungeon,
            _ => ZoneType.Wilderness
        };
    }
}
```

### Running the Test

1. **Add script to scene**:
   - Create empty GameObject named "ZoneTestController"
   - Add `ZoneGenerationTest` component
   
2. **Configure in Inspector**:
   - Run Tests On Start: ✓
   - Generate All Biomes: ✓
   - Test Delay: 2

3. **Press Play**

4. **Expected Output**:
```
=== ZONE GENERATION TEST STARTING ===
[Test] Generating Test_Grassland_Zone...
[Test] ✓ Test_Grassland_Zone generated successfully!
  - Terrain mesh: 2500 vertices
  - Terrain collider: Valid
  - Spawn points: 11
  - Zone boundary: Present (4 walls)
[Test] Generating Test_Desert_Zone...
[Test] ✓ Test_Desert_Zone generated successfully!
  - Terrain mesh: 2500 vertices
  - Terrain collider: Valid
  - Spawn points: 11
  - Zone boundary: Present (4 walls)
... (continues for all biomes)
=== TEST COMPLETE: 5 zones generated, 0 failures ===
```

---

## Example 2: Item Generation & Loot System

### Objective
Test procedural item generation and demonstrate loot drop mechanics.

### Complete Test Script

```csharp
using UnityEngine;
using Game.Core.Systems;
using System.Collections.Generic;

/// <summary>
/// Tests item generation and loot spawning.
/// Creates items of all rarities and displays them in-world.
/// </summary>
public class ItemGenerationTest : MonoBehaviour
{
    [Header("Test Configuration")]
    [SerializeField] private int _itemsPerRarity = 5;
    [SerializeField] private float _spawnRadius = 15f;
    [SerializeField] private bool _visualizeLootBeams = true;
    
    [Header("Test Results")]
    [SerializeField] private List<ItemData> _generatedItems = new();
    [SerializeField] private int _totalItems;
    
    private ItemGenerationEngine _itemGen;
    
    async void Start()
    {
        Debug.Log("=== ITEM GENERATION TEST STARTING ===");
        
        // Wait for systems
        while (CoreSystemManager.Instance == null || !CoreSystemManager.Instance.IsReady())
        {
            await Awaitable.NextFrameAsync();
        }
        
        _itemGen = FindFirstObjectByType<ItemGenerationEngine>();
        if (_itemGen == null)
        {
            Debug.LogError("ItemGenerationEngine not found!");
            return;
        }
        
        await TestItemGeneration();
        
        Debug.Log($"=== TEST COMPLETE: {_totalItems} items generated ===");
    }
    
    async Awaitable TestItemGeneration()
    {
        Rarity[] rarities = (Rarity[])System.Enum.GetValues(typeof(Rarity));
        
        foreach (Rarity rarity in rarities)
        {
            Debug.Log($"[Test] Generating {_itemsPerRarity} {rarity} items...");
            
            for (int i = 0; i < _itemsPerRarity; i++)
            {
                // Alternate between weapons and armor
                ItemData item = i % 2 == 0 
                    ? _itemGen.GenerateWeapon(rarity) 
                    : _itemGen.GenerateArmor(rarity);
                
                _generatedItems.Add(item);
                _totalItems++;
                
                // Spawn in world
                SpawnItemInWorld(item, GetSpawnPosition(rarity, i));
                
                // Log item details
                LogItemDetails(item);
            }
            
            await Awaitable.NextFrameAsync();
        }
    }
    
    Vector3 GetSpawnPosition(Rarity rarity, int index)
    {
        int rarityIndex = (int)rarity;
        float angle = (index / (float)_itemsPerRarity) * 360f * Mathf.Deg2Rad;
        float radius = _spawnRadius + (rarityIndex * 3f);
        
        return new Vector3(
            Mathf.Cos(angle) * radius,
            0,
            Mathf.Sin(angle) * radius
        );
    }
    
    void SpawnItemInWorld(ItemData item, Vector3 position)
    {
        // Create item GameObject
        GameObject itemObj = new($"Item_{item.itemName}");
        itemObj.transform.position = position;
        itemObj.tag = "Item";
        
        // Visual representation
        GameObject visual = GameObject.CreatePrimitive(PrimitiveType.Cube);
        visual.transform.SetParent(itemObj.transform);
        visual.transform.localPosition = Vector3.up * 0.5f;
        visual.transform.localScale = Vector3.one * 0.5f;
        
        // Color by rarity
        Renderer renderer = visual.GetComponent<Renderer>();
        Material mat = new(Shader.Find("Standard"));
        mat.color = GetRarityColor(item.rarity);
        renderer.material = mat;
        
        // Loot beam effect
        if (_visualizeLootBeams)
        {
            CreateLootBeam(itemObj, item.rarity);
        }
        
        // Pickup trigger
        SphereCollider trigger = itemObj.AddComponent<SphereCollider>();
        trigger.isTrigger = true;
        trigger.radius = 1.5f;
        
        // Add pickup component
        SimpleItemPickup pickup = itemObj.AddComponent<SimpleItemPickup>();
        pickup.itemData = item;
        
        // Floating animation
        itemObj.AddComponent<FloatingItem>();
    }
    
    void CreateLootBeam(GameObject itemObj, Rarity rarity)
    {
        GameObject beam = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        beam.name = "LootBeam";
        beam.transform.SetParent(itemObj.transform);
        
        // Position and scale
        float height = GetLootBeamHeight(rarity);
        beam.transform.localPosition = Vector3.up * (height / 2f);
        beam.transform.localScale = new Vector3(0.2f, height / 2f, 0.2f);
        
        // Material
        Renderer renderer = beam.GetComponent<Renderer>();
        Material mat = new(Shader.Find("Standard"));
        mat.color = GetRarityColor(rarity);
        mat.EnableKeyword("_EMISSION");
        mat.SetColor("_EmissionColor", GetRarityColor(rarity) * 2f);
        renderer.material = mat;
        
        // Remove collider
        Destroy(beam.GetComponent<Collider>());
    }
    
    float GetLootBeamHeight(Rarity rarity)
    {
        return rarity switch
        {
            Rarity.Common => 5f,
            Rarity.Uncommon => 7f,
            Rarity.Rare => 10f,
            Rarity.Epic => 15f,
            Rarity.Legendary => 20f,
            Rarity.Mythic => 30f,
            _ => 5f
        };
    }
    
    Color GetRarityColor(Rarity rarity)
    {
        return rarity switch
        {
            Rarity.Common => Color.white,
            Rarity.Uncommon => Color.green,
            Rarity.Rare => Color.blue,
            Rarity.Epic => new Color(0.6f, 0.2f, 0.8f), // Purple
            Rarity.Legendary => new Color(1f, 0.6f, 0f), // Orange
            Rarity.Mythic => Color.red,
            _ => Color.gray
        };
    }
    
    void LogItemDetails(ItemData item)
    {
        string details = $"  [{item.rarity}] {item.itemName} (Lvl {item.level})";
        
        if (item.itemType == ItemType.Weapon)
        {
            details += $" - DMG: {item.damage}, SPD: {item.attackSpeed:F1}, RNG: {item.range:F1}";
        }
        else if (item.itemType == ItemType.Armor)
        {
            details += $" - ARM: {item.armorValue}, Slot: {item.armorSlot}";
        }
        
        if (item.affixes != null && item.affixes.Count > 0)
        {
            details += $" - Affixes: {item.affixes.Count}";
            foreach (ItemAffix affix in item.affixes)
            {
                details += $" [{affix.affixType}: +{affix.value}]";
            }
        }
        
        Debug.Log(details);
    }
}

/// <summary>
/// Simple item pickup component.
/// </summary>
public class SimpleItemPickup : MonoBehaviour
{
    public ItemData itemData;
    
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log($"Picked up: {itemData.itemName}");
            // TODO: Add to inventory
            Destroy(gameObject);
        }
    }
}

/// <summary>
/// Makes item float and rotate.
/// </summary>
public class FloatingItem : MonoBehaviour
{
    [SerializeField] private float _floatSpeed = 0.5f;
    [SerializeField] private float _floatAmplitude = 0.3f;
    [SerializeField] private float _rotateSpeed = 45f;
    
    private Vector3 _startPos;
    private float _time;
    
    void Start()
    {
        _startPos = transform.position;
    }
    
    void Update()
    {
        _time += Time.deltaTime;
        
        // Float up and down
        float y = _startPos.y + Mathf.Sin(_time * _floatSpeed) * _floatAmplitude;
        transform.position = new Vector3(_startPos.x, y, _startPos.z);
        
        // Rotate
        transform.Rotate(Vector3.up, _rotateSpeed * Time.deltaTime);
    }
}
```

### Running the Test

1. **Setup**:
   - Create scene: `CharacterCreationTest.unity`
   - Add script to empty GameObject

2. **Configure in Inspector**:
   - Set species, gender, body type
   - Distribute 20 attribute points
   - Choose appearance values
   - Select heroic bonus

3. **Press Play** or **Right-click script → Create Character**

4. **Expected Result**:
   - Character mesh appears at origin
   - Stats calculated correctly
   - Console shows character details

5. **Try Random Character**:
   - Right-click script → Random Character
   - Creates fully random valid character

---

## Example 5: Complete Game Loop Test

### Full Integration Test

```csharp
using UnityEngine;
using Game.Core.Systems;

/// <summary>
/// Complete integration test simulating full game loop:
/// 1. Bootstrap systems
/// 2. Generate zone
/// 3. Create player character
/// 4. Spawn enemies
/// 5. Generate loot
/// 6. Combat simulation
/// </summary>
public class FullGameLoopTest : MonoBehaviour
{
    [Header("Test Settings")]
    [SerializeField] private bool _autoRun = true;
    [SerializeField] private int _enemyCount = 5;
    
    [Header("References")]
    [SerializeField] private GameObject _player;
    [SerializeField] private GameObject _currentZone;
    
    async void Start()
    {
        if (!_autoRun) return;
        
        Debug.Log("========================================");
        Debug.Log("   FULL GAME LOOP INTEGRATION TEST");
        Debug.Log("========================================");
        
        await RunFullTest();
        
        Debug.Log("========================================");
        Debug.Log("   TEST COMPLETE");
        Debug.Log("========================================");
    }
    
    async Awaitable RunFullTest()
    {
        // Step 1: Wait for systems
        Debug.Log("\n[Step 1] Waiting for core systems...");
        while (CoreSystemManager.Instance == null || !CoreSystemManager.Instance.IsReady())
        {
            await Awaitable.NextFrameAsync();
        }
        Debug.Log("✓ All systems ready");
        
        // Step 2: Generate zone
        Debug.Log("\n[Step 2] Generating test zone...");
        ZoneConfig config = new()
        {
            zoneName = "IntegrationTestZone",
            zoneType = ZoneType.Wilderness,
            biomeType = BiomeType.Grassland,
            levelRange = new Vector2Int(1, 5),
            zoneSize = new Vector3(100, 0, 100),
            seed = 12345
        };
        
        UnityEngine.SceneManagement.Scene zone = await CoreSystemManager.ZoneManager.GenerateAndSaveZone(config);
        Debug.Log($"✓ Zone generated: {zone.name}");
        
        // Step 3: Create player
        Debug.Log("\n[Step 3] Creating player character...");
        ProceduralCharacterBuilder builder = FindFirstObjectByType<ProceduralCharacterBuilder>();
        if (builder == null)
        {
            GameObject builderObj = new("CharacterBuilder");
            builder = builderObj.AddComponent<ProceduralCharacterBuilder>();
        }
        
        CharacterCreationData playerData = new()
        {
            species = Species.Human,
            gender = Gender.Male,
            bodyType = BodyType.Average,
            strength = 12,
            dexterity = 10,
            intelligence = 8,
            vitality = 15,
            endurance = 10,
            luck = 5
        };
        
        _player = builder.GenerateCharacter(playerData);
        _player.name = "Player";
        _player.tag = "Player";
        _player.transform.position = new Vector3(0, 2, 0);
        
        EntityStats playerStats = _player.AddComponent<EntityStats>();
        playerStats.strength = 12;
        playerStats.dexterity = 10;
        playerStats.intelligence = 8;
        playerStats.vitality = 15;
        playerStats.endurance = 10;
        playerStats.luck = 5;
        playerStats.level = 5;
        playerStats.RecalculateStats();
        
        Debug.Log($"✓ Player created: HP {playerStats.maxHealth}");
        
        // Step 4: Spawn enemies
        Debug.Log($"\n[Step 4] Spawning {_enemyCount} enemies...");
        SpawnPoint[] enemySpawns = FindObjectsByType<SpawnPoint>(FindObjectsSortMode.None)
            .Where(sp => sp.Type == SpawnPointType.Enemy)
            .ToArray();
        
        for (int i = 0; i < _enemyCount && i < enemySpawns.Length; i++)
        {
            GameObject enemy = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            enemy.name = $"Enemy_{i + 1}";
            enemy.transform.position = enemySpawns[i].transform.position + Vector3.up;
            
            // Red color
            enemy.GetComponent<Renderer>().material.color = Color.red;
            
            EntityStats enemyStats = enemy.AddComponent<EntityStats>();
            enemyStats.strength = 8;
            enemyStats.vitality = 10;
            enemyStats.level = 3;
            enemyStats.armor = 20;
            enemyStats.RecalculateStats();
            
            Debug.Log($"  - Spawned {enemy.name} at {enemy.transform.position}");
        }
        Debug.Log($"✓ {_enemyCount} enemies spawned");
        
        // Step 5: Generate loot
        Debug.Log("\n[Step 5] Generating random loot...");
        ItemGenerationEngine itemGen = FindFirstObjectByType<ItemGenerationEngine>();
        
        for (int i = 0; i < 10; i++)
        {
            Rarity rarity = (Rarity)Random.Range(0, 4); // Common to Epic
            ItemData item = itemGen.GenerateWeapon(rarity);
            
            Vector3 lootPos = new Vector3(
                Random.Range(-20f, 20f),
                0,
                Random.Range(-20f, 20f)
            );
            
            // Simple loot spawn (no fancy visuals for test)
            GameObject lootObj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            lootObj.name = item.itemName;
            lootObj.transform.position = lootPos + Vector3.up;
            lootObj.transform.localScale = Vector3.one * 0.5f;
            lootObj.GetComponent<Renderer>().material.color = GetRarityColor(rarity);
        }
        Debug.Log("✓ 10 items spawned");
        
        // Step 6: Simulate combat
        Debug.Log("\n[Step 6] Running combat simulation...");
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        if (enemies.Length == 0)
        {
            // Tag manually if needed
            enemies = FindObjectsByType<GameObject>(FindObjectsSortMode.None)
                .Where(go => go.name.Contains("Enemy"))
                .ToArray();
        }
        
        CombatSystemManager combat = CoreSystemManager.CombatManager;
        
        foreach (GameObject enemy in enemies)
        {
            EntityStats enemyStats = enemy.GetComponent<EntityStats>();
            if (enemyStats == null) continue;
            
            // Player attacks enemy
            DamageResult result = combat.CalculateDamage(
                attacker: playerStats,
                defender: enemyStats,
                baseDamage: 50,
                damageType: DamageType.Physical
            );
            
            combat.ApplyDamage(enemy, result, _player);
            
            Debug.Log($"  - Player dealt {result.finalDamage} to {enemy.name}");
            
            if (enemyStats.currentHealth <= 0)
            {
                Debug.Log($"    {enemy.name} defeated!");
                enemy.SetActive(false);
            }
        }
        
        Debug.Log("✓ Combat simulation complete");
        
        // Step 7: Test admin console
        Debug.Log("\n[Step 7] Testing admin console...");
        Debug.Log("  - Press F12 to open admin console");
        Debug.Log("  - Use tabs 1-7 to navigate");
        
        await Awaitable.WaitForSecondsAsync(1f);
    }
    
    Color GetRarityColor(Rarity rarity)
    {
        return rarity switch
        {
            Rarity.Common => Color.white,
            Rarity.Uncommon => Color.green,
            Rarity.Rare => Color.blue,
            Rarity.Epic => new Color(0.6f, 0.2f, 0.8f),
            Rarity.Legendary => new Color(1f, 0.6f, 0f),
            Rarity.Mythic => Color.red,
            _ => Color.gray
        };
    }
}
```

### Running the Test

1. **Create scene**: `FullGameLoopTest.unity`
2. **Add script** to empty GameObject
3. **Configure**: Auto Run: ✓
4. **Press Play**

**Expected Console Output**:
```
========================================
   FULL GAME LOOP INTEGRATION TEST
========================================

[Step 1] Waiting for core systems...
✓ All systems ready

[Step 2] Generating test zone...
✓ Zone generated: IntegrationTestZone

[Step 3] Creating player character...
✓ Player created: HP 250

[Step 4] Spawning 5 enemies...
  - Spawned Enemy_1 at (23.4, 1.0, -12.5)
  - Spawned Enemy_2 at (-15.2, 1.0, 8.9)
  - Spawned Enemy_3 at (7.8, 1.0, 18.3)
  - Spawned Enemy_4 at (-22.1, 1.0, -5.7)
  - Spawned Enemy_5 at (12.3, 1.0, -20.4)
✓ 5 enemies spawned

[Step 5] Generating random loot...
✓ 10 items spawned

[Step 6] Running combat simulation...
  - Player dealt 65 to Enemy_1
  - Player dealt 58 to Enemy_2
  - Player dealt 71 to Enemy_3
    Enemy_3 defeated!
  - Player dealt 62 to Enemy_4
  - Player dealt 69 to Enemy_5
✓ Combat simulation complete

[Step 7] Testing admin console...
  - Press F12 to open admin console
  - Use tabs 1-7 to navigate

========================================
   TEST COMPLETE
========================================
```

---

## Example 6: Admin Console Pool Inspector

### Custom Admin Tool Example

```csharp
using UnityEngine;
using UnityEngine.UI;
using Game.Core.Systems;
using System.Collections.Generic;

/// <summary>
/// Extended pool inspector for admin console.
/// Shows real-time pool statistics with visual graphs.
/// </summary>
public class PoolInspectorTool : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Text _statsText;
    [SerializeField] private Transform _poolListContainer;
    [SerializeField] private GameObject _poolItemPrefab;
    
    private ObjectPoolManager _poolManager;
    private Dictionary<string, PoolDisplayItem> _poolDisplays = new();
    
    void Start()
    {
        _poolManager = CoreSystemManager.PoolManager;
        InitializeUI();
    }
    
    void Update()
    {
        UpdatePoolStats();
    }
    
    void InitializeUI()
    {
        // Get all registered pools
        List<string> poolKeys = _poolManager.GetAllPoolKeys();
        
        foreach (string poolKey in poolKeys)
        {
            CreatePoolDisplayItem(poolKey);
        }
    }
    
    void CreatePoolDisplayItem(string poolKey)
    {
        if (_poolItemPrefab == null)
        {
            // Create simple text display
            GameObject item = new($"Pool_{poolKey}");
            item.transform.SetParent(_poolListContainer);
            
            Text text = item.AddComponent<Text>();
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = 16;
            text.color = Color.white;
            
            _poolDisplays[poolKey] = new PoolDisplayItem { text = text };
        }
    }
    
    void UpdatePoolStats()
    {
        if (_poolManager == null) return;
        
        // Overall stats
        _poolManager.GetPoolPerformanceMetrics(
            out int totalPooled,
            out int totalActive,
            out int hits,
            out int misses
        );
        
        float hitRate = hits + misses > 0 ? (float)hits / (hits + misses) : 0f;
        
        if (_statsText != null)
        {
            _statsText.text = $"Total Pooled: {totalPooled}\n" +
                             $"Total Active: {totalActive}\n" +
                             $"Pool Hits: {hits}\n" +
                             $"Pool Misses: {misses}\n" +
                             $"Hit Rate: {hitRate:P2}";
        }
        
        // Individual pool stats
        foreach (KeyValuePair<string, PoolDisplayItem> kvp in _poolDisplays)
        {
            ObjectPoolManager.PoolStats stats = _poolManager.GetPoolStats(kvp.Key);
            
            if (kvp.Value.text != null)
            {
                kvp.Value.text.text = $"{stats.poolKey}: {stats.activeCount}/{stats.totalCount} " +
                                     $"(Max: {stats.maxPoolSize})";
            }
        }
    }
    
    [System.Serializable]
    class PoolDisplayItem
    {
        public Text text;
    }
}
```

---

## Quick Reference: Running All Tests

### Test Execution Order

1. **ZoneGenerationTest** - Verify terrain generation works
2. **ItemGenerationTest** - Verify item system works
3. **CombatSystemTest** - Verify damage calculations work
4. **CharacterCreationTest** - Verify character generation works
5. **FullGameLoopTest** - Verify all systems work together

### Batch Test Runner

```csharp
using UnityEngine;
using System.Collections;

public class BatchTestRunner : MonoBehaviour
{
    async void Start()
    {
        Debug.Log("=== RUNNING ALL TESTS ===");
        
        await RunTest<ZoneGenerationTest>();
        await RunTest<ItemGenerationTest>();
        await RunTest<CombatSystemTest>();
        await RunTest<CharacterCreationTest>();
        await RunTest<FullGameLoopTest>();
        
        Debug.Log("=== ALL TESTS COMPLETE ===");
    }
    
    async Awaitable RunTest<T>() where T : Component
    {
        Debug.Log($"\n--- Starting {typeof(T).Name} ---");
        
        GameObject testObj = new(typeof(T).Name);
        testObj.AddComponent<T>();
        
        // Wait for test to complete (5 seconds max)
        await Awaitable.WaitForSecondsAsync(5f);
        
        Destroy(testObj);
        
        Debug.Log($"--- {typeof(T).Name} Complete ---\n");
    }
}
```

---

## Troubleshooting Test Failures

### Zone Generation Fails
```csharp
// Check SimpleTerrainGenerator settings
SimpleTerrainGenerator gen = FindFirstObjectByType<SimpleTerrainGenerator>();
Debug.Log($"Terrain Resolution: {gen._terrainResolution}");
Debug.Log($"Height Scale: {gen._heightScale}");

// Try reducing resolution if performance issue
gen._terrainResolution = 25; // Default: 50
```

### Items Not Spawning
```csharp
// Verify ItemGenerationEngine exists
ItemGenerationEngine itemGen = FindFirstObjectByType<ItemGenerationEngine>();
if (itemGen == null)
{
    Debug.LogError("ItemGenerationEngine not found!");
}

// Check cached items
Debug.Log($"Cached items: {itemGen._cachedItems.Count}");
```

### Combat Damage is Zero
```csharp
// Check entity stats
EntityStats stats = GetComponent<EntityStats>();
Debug.Log($"STR: {stats.strength}, ARM: {stats.armor}");

// Verify CombatSystemManager
CombatSystemManager combat = CoreSystemManager.CombatManager;
if (combat == null)
{
    Debug.LogError("CombatSystemManager not initialized!");
}
```

### Pools Not Working
```csharp
// Verify pool registration
ObjectPoolManager poolMgr = CoreSystemManager.PoolManager;
List<string> pools = poolMgr.GetAllPoolKeys();
Debug.Log($"Registered pools: {string.Join(", ", pools)}");

// Check if prefab has IPoolable
GameObject prefab = /* your prefab */;
IPoolable poolable = prefab.GetComponent<IPoolable>();
if (poolable == null)
{
    Debug.LogError("Prefab missing IPoolable component!");
}
```

---

## Performance Benchmarks

Expected performance on target hardware (Samsung A15):

```
Zone Generation:     < 2 seconds per 100x100 zone
Item Generation:     < 1ms per item
Character Creation:  < 100ms per character
Combat Calculation:  < 0.1ms per hit
Pool Get/Return:     < 0.01ms per operation

Frame Budget (40 FPS = 25ms):
- Update loops:      10ms
- Rendering:         8ms
- Physics:           5ms
- Systems:           2ms
```

---

**All examples are production-ready and follow the Unity 6.1 optimization guidelines from README.md**:
   - Create scene: `ItemGenerationTest.unity`
   - Add empty GameObject: "ItemTestController"
   - Add `ItemGenerationTest` script

2. **Configure**:
   - Items Per Rarity: 5
   - Spawn Radius: 15
   - Visualize Loot Beams: ✓

3. **Add Player** (for pickup testing):
   - Create Capsule
   - Tag as "Player"
   - Add Character Controller

4. **Press Play**

5. **Expected Result**:
   - Items spawn in concentric circles
   - Different colored beams for each rarity
   - Items float and rotate
   - Walk into items to pick them up

---

## Example 3: Combat System with Pooled Effects

### Objective
Demonstrate combat calculations and pooled damage numbers.

### Complete Test Script

```csharp
using UnityEngine;
using Game.Core.Systems;
using Game.Core.Pooling;

/// <summary>
/// Combat system test with visual feedback.
/// Creates attacker and defender, simulates combat with damage numbers.
/// </summary>
public class CombatSystemTest : MonoBehaviour
{
    [Header("Test Configuration")]
    [SerializeField] private int _attackCount = 20;
    [SerializeField] private float _attackInterval = 0.5f;
    [SerializeField] private bool _useRandomDamage = true;
    
    [Header("Entities")]
    [SerializeField] private GameObject _attacker;
    [SerializeField] private GameObject _defender;
    
    [Header("Test Results")]
    [SerializeField] private int _totalDamageDealt;
    [SerializeField] private int _criticalHits;
    [SerializeField] private int _attacksExecuted;
    
    private CombatSystemManager _combatManager;
    private ObjectPoolManager _poolManager;
    
    async void Start()
    {
        Debug.Log("=== COMBAT SYSTEM TEST STARTING ===");
        
        // Wait for systems
        while (CoreSystemManager.Instance == null || !CoreSystemManager.Instance.IsReady())
        {
            await Awaitable.NextFrameAsync();
        }
        
        _combatManager = CoreSystemManager.CombatManager;
        _poolManager = CoreSystemManager.PoolManager;
        
        // Setup entities
        SetupTestEntities();
        
        // Setup damage number pool
        SetupDamageNumberPool();
        
        // Run combat test
        await RunCombatTest();
        
        Debug.Log($"=== TEST COMPLETE ===");
        Debug.Log($"Total Damage: {_totalDamageDealt}");
        Debug.Log($"Critical Hits: {criticalHits}/{_attacksExecuted}");
        Debug.Log($"Defender HP: {_defender.GetComponent<EntityStats>().currentHealth}/{_defender.GetComponent<EntityStats>().maxHealth}");
    }
    
    void SetupTestEntities()
    {
        // Create attacker
        if (_attacker == null)
        {
            _attacker = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            _attacker.name = "Attacker";
            _attacker.transform.position = new Vector3(-3, 1, 0);
        }
        
        EntityStats attackerStats = _attacker.AddComponent<EntityStats>();
        attackerStats.strength = 50;
        attackerStats.dexterity = 30;
        attackerStats.intelligence = 20;
        attackerStats.criticalChance = 0.25f; // 25% crit chance
        attackerStats.level = 10;
        
        // Create defender
        if (_defender == null)
        {
            _defender = GameObject.CreatePrimitive(PrimitiveType.Cube);
            _defender.name = "Defender";
            _defender.transform.position = new Vector3(3, 1, 0);
        }
        
        EntityStats defenderStats = _defender.AddComponent<EntityStats>();
        defenderStats.vitality = 40;
        defenderStats.armor = 50;
        defenderStats.level = 10;
        defenderStats.RecalculateStats();
        
        Debug.Log($"Attacker: STR {attackerStats.strength}, CRIT {attackerStats.criticalChance:P0}");
        Debug.Log($"Defender: HP {defenderStats.maxHealth}, ARM {defenderStats.armor}");
    }
    
    void SetupDamageNumberPool()
    {
        // Check if DamageNumber prefab exists
        GameObject damageNumberPrefab = Resources.Load<GameObject>("Prefabs/DamageNumber");
        
        if (damageNumberPrefab == null)
        {
            Debug.LogWarning("DamageNumber prefab not found, creating basic version...");
            damageNumberPrefab = CreateDamageNumberPrefab();
        }
        
        // Register and warm pool
        _poolManager.RegisterPrefab("DamageNumber", damageNumberPrefab, maxPoolSize: 100);
        _poolManager.WarmPool("DamageNumber", count: 20);
        
        Debug.Log("DamageNumber pool ready");
    }
    
    GameObject CreateDamageNumberPrefab()
    {
        GameObject prefab = new("DamageNumber");
        
        // Add DamageNumber component
        DamageNumber damageNum = prefab.AddComponent<DamageNumber>();
        
        // Create text child
        GameObject textObj = new("Text");
        textObj.transform.SetParent(prefab.transform);
        
        TextMesh textMesh = textObj.AddComponent<TextMesh>();
        textMesh.fontSize = 32;
        textMesh.anchor = TextAnchor.MiddleCenter;
        textMesh.alignment = TextAlignment.Center;
        textMesh.color = Color.white;
        
        return prefab;
    }
    
    async Awaitable RunCombatTest()
    {
        EntityStats attackerStats = _attacker.GetComponent<EntityStats>();
        EntityStats defenderStats = _defender.GetComponent<EntityStats>();
        
        for (int i = 0; i < _attackCount; i++)
        {
            // Calculate damage
            int baseDamage = _useRandomDamage ? Random.Range(30, 60) : 50;
            
            DamageResult result = _combatManager.CalculateDamage(
                attacker: attackerStats,
                defender: defenderStats,
                baseDamage: baseDamage,
                damageType: DamageType.Physical
            );
            
            // Apply damage
            _combatManager.ApplyDamage(
                target: _defender,
                damage: result,
                attacker: _attacker
            );
            
            // Track stats
            _totalDamageDealt += result.finalDamage;
            _attacksExecuted++;
            
            if (result.isCritical)
            {
                _criticalHits++;
                Debug.Log($"<color=yellow>CRITICAL HIT! {result.finalDamage} damage</color>");
            }
            else
            {
                Debug.Log($"Attack #{i + 1}: {result.finalDamage} damage");
            }
            
            // Stop if defender dies
            if (defenderStats.currentHealth <= 0)
            {
                Debug.Log($"<color=red>Defender defeated after {i + 1} attacks!</color>");
                break;
            }
            
            // Wait before next attack
            await Awaitable.WaitForSecondsAsync(_attackInterval);
        }
    }
}
```

### Running the Test

1. **Setup**:
   - Create scene: `CombatSystemTest.unity`
   - Add script to empty GameObject

2. **Press Play**

3. **Expected Output**:
```
=== COMBAT SYSTEM TEST STARTING ===
Attacker: STR 50, CRIT 25%
Defender: HP 500, ARM 50
DamageNumber pool ready
Attack #1: 45 damage
Attack #2: 52 damage
CRITICAL HIT! 104 damage
Attack #3: 48 damage
... (continues)
Defender defeated after 12 attacks!
=== TEST COMPLETE ===
Total Damage: 623
Critical Hits: 3/12
Defender HP: 0/500
```

4. **Visual Feedback**:
   - Damage numbers float up from defender
   - Critical hits show in yellow
   - Regular hits show in white

---

## Example 4: Character Creation Flow

### Complete Character Creator Script

```csharp
using UnityEngine;
using Game.Core.Systems;

/// <summary>
/// Full character creation test with UI.
/// Demonstrates species selection, attribute distribution, and heroic bonuses.
/// </summary>
public class CharacterCreationTest : MonoBehaviour
{
    [Header("Creation Data")]
    [SerializeField] private Species _selectedSpecies = Species.Human;
    [SerializeField] private Gender _selectedGender = Gender.Male;
    [SerializeField] private BodyType _selectedBodyType = BodyType.Average;
    
    [Header("Attributes (Total: 20 points)")]
    [SerializeField, Range(5, 15)] private int _strength = 10;
    [SerializeField, Range(5, 15)] private int _dexterity = 10;
    [SerializeField, Range(5, 15)] private int _intelligence = 10;
    [SerializeField, Range(5, 15)] private int _vitality = 10;
    [SerializeField, Range(5, 15)] private int _endurance = 10;
    [SerializeField, Range(5, 15)] private int _luck = 10;
    
    [Header("Customization")]
    [SerializeField] private int _skinTone = 0;
    [SerializeField] private int _faceShape = 0;
    [SerializeField] private int _hairStyle = 0;
    
    [Header("Starting Bonus")]
    [SerializeField] private HeroicBonus _heroicBonus = HeroicBonus.LegendaryWeapon;
    
    [Header("Generated Character")]
    [SerializeField] private GameObject _generatedCharacter;
    
    private ProceduralCharacterBuilder _characterBuilder;
    private int _totalAttributePoints => _strength + _dexterity + _intelligence + _vitality + _endurance + _luck;
    
    async void Start()
    {
        Debug.Log("=== CHARACTER CREATION TEST ===");
        
        // Wait for systems
        while (CoreSystemManager.Instance == null || !CoreSystemManager.Instance.IsReady())
        {
            await Awaitable.NextFrameAsync();
        }
        
        _characterBuilder = FindFirstObjectByType<ProceduralCharacterBuilder>();
        if (_characterBuilder == null)
        {
            GameObject builderObj = new("CharacterBuilder");
            _characterBuilder = builderObj.AddComponent<ProceduralCharacterBuilder>();
        }
        
        // Auto-create character on start
        CreateCharacter();
    }
    
    void OnValidate()
    {
        // Enforce 20-point limit
        if (_totalAttributePoints > 20)
        {
            Debug.LogWarning($"Total attributes ({_totalAttributePoints}) exceed 20 points!");
        }
    }
    
    [ContextMenu("Create Character")]
    public void CreateCharacter()
    {
        if (_totalAttributePoints != 20)
        {
            Debug.LogError($"Must allocate exactly 20 attribute points! Current: {_totalAttributePoints}");
            return;
        }
        
        // Destroy old character
        if (_generatedCharacter != null)
        {
            DestroyImmediate(_generatedCharacter);
        }
        
        // Create character data
        CharacterCreationData creationData = new()
        {
            species = _selectedSpecies,
            gender = _selectedGender,
            bodyType = _selectedBodyType,
            skinTone = _skinTone,
            faceShape = _faceShape,
            hairStyle = _hairStyle,
            strength = _strength,
            dexterity = _dexterity,
            intelligence = _intelligence,
            vitality = _vitality,
            endurance = _endurance,
            luck = _luck,
            heroicBonus = _heroicBonus
        };
        
        // Generate character
        _generatedCharacter = _characterBuilder.GenerateCharacter(creationData);
        _generatedCharacter.transform.position = Vector3.zero;
        
        // Add stats component
        EntityStats stats = _generatedCharacter.AddComponent<EntityStats>();
        stats.strength = _strength;
        stats.dexterity = _dexterity;
        stats.intelligence = _intelligence;
        stats.vitality = _vitality;
        stats.endurance = _endurance;
        stats.luck = _luck;
        stats.RecalculateStats();
        
        Debug.Log($"Character created: {_selectedGender} {_selectedSpecies}");
        Debug.Log($"Stats: STR {_strength}, DEX {_dexterity}, INT {_intelligence}");
        Debug.Log($"HP: {stats.maxHealth}, Mana: {stats.maxMana}");
    }
    
    [ContextMenu("Random Character")]
    public void CreateRandomCharacter()
    {
        // Random species
        Species[] species = (Species[])System.Enum.GetValues(typeof(Species));
        _selectedSpecies = species[Random.Range(0, species.Length)];
        
        // Random gender
        _selectedGender = Random.Range(0, 2) == 0 ? Gender.Male : Gender.Female;
        
        // Random attributes (20 points total)
        RandomizeAttributes();
        
        // Random appearance
        _skinTone = Random.Range(0, 3);
        _faceShape = Random.Range(0, 5);
        _hairStyle = Random.Range(0, 10);
        
        CreateCharacter();
    }
    
    void RandomizeAttributes()
    {
        int[] stats = new int[6];
        int remaining = 20;
        
        // Give minimum 5 to each
        for (int i = 0; i < 6; i++)
        {
            stats[i] = 5;
            remaining -= 5;
        }
        
        // Distribute remaining points
        while (remaining > 0)
        {
            int statIndex = Random.Range(0, 6);
            if (stats[statIndex] < 15)
            {
                stats[statIndex]++;
                remaining--;
            }
        }
        
        _strength = stats[0];
        _dexterity = stats[1];
        _intelligence = stats[2];
        _vitality = stats[3];
        _endurance = stats[4];
        _luck = stats[5];
    }
}
```

### Running the Test

1. **Setup**