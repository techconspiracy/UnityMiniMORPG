# UAsset MMORPG System - API Documentation

## Table of Contents
1. [Core Systems API](#core-systems-api)
2. [Object Pooling API](#object-pooling-api)
3. [Zone Management API](#zone-management-api)
4. [Item Generation API](#item-generation-api)
5. [Combat System API](#combat-system-api)
6. [Character System API](#character-system-api)
7. [UI System API](#ui-system-api)
8. [Extension Patterns](#extension-patterns)

---

## Core Systems API

### CoreSystemManager

Central hub for all game systems.

```csharp
using Game.Core.Systems;

// Access singleton instance
CoreSystemManager core = CoreSystemManager.Instance;

// Check if systems are ready
if (core.IsReady())
{
    // Systems initialized and ready to use
}

// Access individual managers
ObjectPoolManager poolMgr = CoreSystemManager.PoolManager;
ZoneSceneManager zoneMgr = CoreSystemManager.ZoneManager;
ItemGenerationEngine itemGen = CoreSystemManager.InventoryManager;
CombatSystemManager combatMgr = CoreSystemManager.CombatManager;

// Shutdown all systems
await core.ShutdownAllSystems();
```

**Best Practices**:
- Always check `IsReady()` before accessing systems
- Use static accessors (`CoreSystemManager.PoolManager`) for cleaner code
- Await `ShutdownAllSystems()` before changing scenes

---

## Object Pooling API

### Creating Poolable Objects

#### Inherit from PoolableObject

```csharp
using Game.Core.Pooling;

public class MyProjectile : PoolableObject
{
    private Vector3 _velocity;
    private float _damage;
    
    // Called when retrieved from pool
    public override void OnSpawnFromPool()
    {
        base.OnSpawnFromPool(); // Sets IsActiveInPool = true, activates GameObject
        
        // Initialize your object
        _velocity = Vector3.forward * 10f;
        _damage = 50f;
    }
    
    // Called when returned to pool
    public override void OnReturnToPool()
    {
        base.OnReturnToPool(); // Sets IsActiveInPool = false, deactivates GameObject
        
        // Reset state to avoid cross-contamination
        _velocity = Vector3.zero;
        _damage = 0f;
    }
    
    private void Update()
    {
        if (!IsActiveInPool) return; // Important: Don't run logic when pooled
        
        transform.position += _velocity * Time.deltaTime;
        
        // Example: Return to pool after 5 seconds
        if (Vector3.Distance(Vector3.zero, transform.position) > 50f)
        {
            ReturnToPool(); // Helper method from PoolableObject
        }
    }
}
```

#### Manual IPoolable Implementation

```csharp
using UnityEngine;
using Game.Core.Pooling;

public class CustomPoolable : MonoBehaviour, IPoolable
{
    public GameObject GameObject => gameObject;
    public string PoolKey => "CustomPool";
    public bool IsActiveInPool { get; set; }
    
    public void OnSpawnFromPool()
    {
        IsActiveInPool = true;
        gameObject.SetActive(true);
        // Your initialization
    }
    
    public void OnReturnToPool()
    {
        IsActiveInPool = false;
        gameObject.SetActive(false);
        // Your cleanup
    }
}
```

### Using ObjectPoolManager

#### Register and Pre-Warm Pools

```csharp
using Game.Core.Systems;

// Get manager instance
ObjectPoolManager poolMgr = CoreSystemManager.PoolManager;

// Register a prefab for pooling
poolMgr.RegisterPrefab(
    poolKey: "Bullet",
    prefab: bulletPrefab,
    maxPoolSize: 200
);

// Pre-warm pool (creates instances ahead of time)
poolMgr.WarmPool("Bullet", count: 50);
```

#### Get Objects from Pool

```csharp
// Get object with position and rotation
Bullet bullet = poolMgr.Get<Bullet>(
    poolKey: "Bullet",
    position: spawnPoint.position,
    rotation: spawnPoint.rotation
);

// Configure object
bullet.SetDamage(damage);
bullet.SetVelocity(direction * speed);

// Get object at origin (convenience method)
Explosion explosion = poolMgr.Get<Explosion>("Explosion");
explosion.transform.position = hitPosition;
```

#### Return Objects to Pool

```csharp
// From within poolable object
ReturnToPool();

// From external code
poolMgr.ReturnToPool(poolableObject);
```

#### Query Pool Statistics

```csharp
// Get stats for specific pool
ObjectPoolManager.PoolStats stats = poolMgr.GetPoolStats("Bullet");
Debug.Log($"Pool: {stats.poolKey}");
Debug.Log($"Active: {stats.activeCount} / Inactive: {stats.inactiveCount}");
Debug.Log($"Total: {stats.totalCount} / Max: {stats.maxPoolSize}");

// Get performance metrics
poolMgr.GetPoolPerformanceMetrics(
    out int totalPooled,
    out int totalActive,
    out int hits,
    out int misses
);

float hitRate = (float)hits / (hits + misses);
Debug.Log($"Pool hit rate: {hitRate:P2}"); // e.g., "98.50%"
```

#### Admin Operations

```csharp
// Get all pool keys
List<string> allPools = poolMgr.GetAllPoolKeys();

// Get active objects from specific pool
HashSet<IPoolable> activeObjects = poolMgr.GetActiveObjects("Enemy");

// Clear specific pool (destroys all instances)
poolMgr.ClearPool("OldEffects");

// Nuclear option: Clear everything
poolMgr.ClearAllPools();
```

---

## Zone Management API

### ZoneSceneManager

Manages zone loading, generation, and caching.

#### Generate New Zone

```csharp
using Game.Core.Systems;

ZoneSceneManager zoneMgr = CoreSystemManager.ZoneManager;

// Create zone configuration
ZoneConfig config = new()
{
    zoneName = "ForestArea_01",
    zoneType = ZoneType.Wilderness,
    biomeType = BiomeType.Grassland,
    levelRange = new Vector2Int(10, 15),
    zoneSize = new Vector3(200, 0, 200),
    seed = 54321 // For reproducible generation
};

// Generate and save zone
Scene zoneScene = await zoneMgr.GenerateAndSaveZone(config);

// Zone is now loaded additively and saved for future loads
```

#### Load Existing Zone

```csharp
// Load from cache (or generate if not cached)
Scene zone = await zoneMgr.LoadZone("ForestArea_01");

// Load with custom config (if not cached)
ZoneConfig customConfig = ZoneConfig.CreateDefault("CustomZone");
customConfig.biomeType = BiomeType.Desert;
Scene customZone = await zoneMgr.LoadZone("CustomZone", customConfig);
```

#### Unload Zone

```csharp
// Unload specific zone
await zoneMgr.UnloadZone("ForestArea_01");

// System automatically unloads oldest zone when limit reached
```

#### Query Zone Info

```csharp
// Check if zone exists in cache
bool exists = zoneMgr.IsZoneCached("ForestArea_01");

// Get list of all cached zones
List<string> cachedZones = zoneMgr.GetCachedZones();

// Get current active zone
string currentZone = zoneMgr.GetCurrentZone();

// Get zone metadata
ZoneMetadata metadata = zoneMgr.GetZoneMetadata("ForestArea_01");
if (metadata != null)
{
    Debug.Log($"Zone Type: {metadata.zoneType}");
    Debug.Log($"Biome: {metadata.biomeType}");
    Debug.Log($"Level Range: {metadata.levelRange.x}-{metadata.levelRange.y}");
}
```

### SimpleTerrainGenerator

Procedural terrain generation.

```csharp
SimpleTerrainGenerator terrainGen = GetComponent<SimpleTerrainGenerator>();

// Generate terrain for a zone
await terrainGen.GenerateTerrainForZone(zoneRoot.transform, zoneConfig);

// Terrain will be:
// - Procedurally generated mesh based on Perlin noise
// - Colored according to biome type
// - Fully walkable with MeshCollider
```

**Customization**:
```csharp
[SerializeField] private int _terrainResolution = 50; // Higher = more detailed, slower
[SerializeField] private float _heightScale = 10f; // Terrain elevation multiplier
[SerializeField] private float _noiseScale = 0.1f; // Perlin noise frequency
```

### Spawn Points

#### Create Spawn Point in Code

```csharp
GameObject spawnObj = new("PlayerSpawn");
spawnObj.transform.position = new Vector3(0, 5, 0);

SpawnPoint spawn = spawnObj.AddComponent<SpawnPoint>();
spawn.Initialize(
    type: SpawnPointType.Player,
    radius: 5f,
    maxEntities: 1
);
```

#### Place Spawn Points in Editor

1. Create Empty GameObject
2. Add `SpawnPoint` component
3. Configure in Inspector:
   - Type: Player / Enemy / NPC / Boss / Resource
   - Radius: Detection/spawn radius
   - Max Entities: Limit for this spawn point
4. Spawn point auto-registers with system

#### Query Spawn Points

```csharp
// Find all spawn points of specific type
SpawnPoint[] playerSpawns = FindObjectsByType<SpawnPoint>()
    .Where(sp => sp.Type == SpawnPointType.Player)
    .ToArray();

// Spawn entity at random spawn point
SpawnPoint randomSpawn = playerSpawns[Random.Range(0, playerSpawns.Length)];
Vector3 spawnPos = randomSpawn.transform.position;
```

---

## Item Generation API

### ItemGenerationEngine

Hybrid procedural item generation.

#### Generate Items

```csharp
ItemGenerationEngine itemGen = FindFirstObjectByType<ItemGenerationEngine>();

// Generate random weapon
ItemData weapon = itemGen.GenerateWeapon(Rarity.Rare);

// Generate random armor
ItemData armor = itemGen.GenerateArmor(Rarity.Epic);

// Get or generate item (uses cache if available)
ItemData item = itemGen.GetOrGenerateItem(Rarity.Legendary, ItemType.Weapon);
```

#### Item Data Structure

```csharp
[System.Serializable]
public class ItemData
{
    // Basic info
    public string itemId;           // Unique ID
    public string itemName;         // Display name
    public ItemType itemType;       // Weapon / Armor / Consumable / Material
    public Rarity rarity;           // Common → Mythic
    public int level;               // Required level
    
    // Weapon stats
    public WeaponArchetype weaponArchetype;
    public int damage;
    public float attackSpeed;
    public float range;
    
    // Armor stats
    public ArmorSlot armorSlot;
    public ArmorArchetype armorArchetype;
    public int armorValue;
    
    // Bonus stats
    public int bonusHealth;
    public int bonusMana;
    public float critChance;
    
    // Affixes
    public List<ItemAffix> affixes;
}
```

#### Create Custom Item

```csharp
ItemData customSword = new()
{
    itemId = System.Guid.NewGuid().ToString(),
    itemName = "Excalibur",
    itemType = ItemType.Weapon,
    rarity = Rarity.Legendary,
    level = 50,
    weaponArchetype = WeaponArchetype.Sword,
    damage = 500,
    attackSpeed = 1.8f,
    range = 2.5f,
    affixes = new List<ItemAffix>
    {
        new() { affixType = AffixType.BonusDamage, value = 100 },
        new() { affixType = AffixType.CriticalChance, value = 15 },
        new() { affixType = AffixType.LifeSteal, value = 10 }
    }
};
```

#### Spawn Item in World

```csharp
// Create item GameObject
GameObject itemObj = new($"Item_{item.itemName}");
itemObj.transform.position = dropPosition;

// Add visual (simple cube for now)
GameObject visual = GameObject.CreatePrimitive(PrimitiveType.Cube);
visual.transform.SetParent(itemObj.transform);
visual.transform.localScale = Vector3.one * 0.5f;

// Color by rarity
Renderer renderer = visual.GetComponent<Renderer>();
renderer.material.color = GetRarityColor(item.rarity);

// Add pickup trigger
SphereCollider trigger = itemObj.AddComponent<SphereCollider>();
trigger.isTrigger = true;
trigger.radius = 1.5f;

// Add item data component
ItemPickup pickup = itemObj.AddComponent<ItemPickup>();
pickup.itemData = item;
```

### Item Pickup Component

```csharp
public class ItemPickup : MonoBehaviour
{
    public ItemData itemData;
    
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // Add to player inventory
            InventorySystemManager inventory = CoreSystemManager.InventoryManager;
            if (inventory.AddItem(itemData))
            {
                Debug.Log($"Picked up: {itemData.itemName}");
                Destroy(gameObject);
            }
            else
            {
                Debug.Log("Inventory full!");
            }
        }
    }
}
```

---

## Combat System API

### CombatSystemManager

#### Calculate Damage

```csharp
CombatSystemManager combatMgr = CoreSystemManager.CombatManager;

EntityStats attacker = player.GetComponent<EntityStats>();
EntityStats defender = enemy.GetComponent<EntityStats>();

// Calculate damage
DamageResult result = combatMgr.CalculateDamage(
    attacker: attacker,
    defender: defender,
    baseDamage: 100,
    damageType: DamageType.Physical
);

Debug.Log($"Damage: {result.finalDamage}");
Debug.Log($"Critical: {result.isCritical}");
Debug.Log($"Type: {result.damageType}");
```

#### Apply Damage

```csharp
// Apply damage to target
combatMgr.ApplyDamage(
    target: enemyGameObject,
    damage: damageResult,
    attacker: playerGameObject
);

// This will:
// - Reduce target HP
// - Trigger damage events
// - Spawn damage number (pooled)
// - Check for death
```

#### Subscribe to Combat Events

```csharp
void Start()
{
    CombatSystemManager combat = CoreSystemManager.CombatManager;
    
    // Subscribe to damage event
    combat.OnDamageDealt += HandleDamageDealt;
    
    // Subscribe to death event
    combat.OnEntityDeath += HandleEntityDeath;
}

void HandleDamageDealt(GameObject attacker, GameObject target, DamageResult damage)
{
    Debug.Log($"{attacker.name} dealt {damage.finalDamage} to {target.name}");
    
    if (damage.isCritical)
    {
        // Play special effect for crits
    }
}

void HandleEntityDeath(GameObject victim, GameObject killer)
{
    Debug.Log($"{victim.name} was killed by {killer.name}");
    
    // Drop loot
    // Award XP
    // Trigger quest objectives
}

void OnDestroy()
{
    CombatSystemManager combat = CoreSystemManager.CombatManager;
    if (combat != null)
    {
        combat.OnDamageDealt -= HandleDamageDealt;
        combat.OnEntityDeath -= HandleEntityDeath;
    }
}
```

### EntityStats Component

```csharp
// Get entity stats
EntityStats stats = GetComponent<EntityStats>();

// Access primary stats
int strength = stats.strength;
int dexterity = stats.dexterity;
int intelligence = stats.intelligence;

// Access combat stats
int currentHP = stats.currentHealth;
int maxHP = stats.maxHealth;
float critChance = stats.criticalChance;

// Modify stats
stats.strength += 10;
stats.criticalChance += 0.05f;

// Recalculate derived stats
stats.RecalculateStats(); // Call after stat changes

// Add experience
stats.AddExperience(500);

// Check level
int currentLevel = stats.level;
```

### Abilities

#### Create Custom Ability

```csharp
[System.Serializable]
public class FireballAbility : Ability
{
    public override void Execute(GameObject caster, Vector3 targetPosition)
    {
        base.Execute(caster, targetPosition);
        
        // Spawn projectile
        ObjectPoolManager poolMgr = CoreSystemManager.PoolManager;
        Projectile fireball = poolMgr.Get<Projectile>("Fireball", caster.transform.position, Quaternion.identity);
        
        // Configure projectile
        Vector3 direction = (targetPosition - caster.transform.position).normalized;
        fireball.Initialize(direction, baseDamage, DamageType.Magic, DamageElement.Fire);
    }
}
```

#### Use Abilities

```csharp
AbilitySystemController abilityController = GetComponent<AbilitySystemController>();

// Use ability from hotbar slot
bool success = abilityController.UseAbility(
    slotIndex: 0, // Hotbar slot (0-9)
    caster: gameObject,
    targetPosition: targetPos
);

if (!success)
{
    Debug.Log("Ability failed: on cooldown or insufficient resources");
}
```

---

## Character System API

### ProceduralCharacterBuilder

```csharp
ProceduralCharacterBuilder builder = FindFirstObjectByType<ProceduralCharacterBuilder>();

// Create character creation data
CharacterCreationData creationData = new()
{
    species = Species.Elf,
    gender = Gender.Female,
    bodyType = BodyType.Slim,
    skinTone = 0,
    faceShape = 2,
    hairStyle = 5,
    
    // Attributes (must sum to 20 at creation)
    strength = 8,
    dexterity = 15,
    intelligence = 12,
    vitality = 10,
    endurance = 8,
    luck = 7,
    
    // Species ability
    selectedAbility = SpeciesAbility.KeenSenses,
    
    // Heroic bonus
    heroicBonus = HeroicBonus.Legendary Weapon
};

// Generate character
GameObject character = builder.GenerateCharacter(creationData);

// Character now has:
// - Procedural mesh (body + head combined)
// - Species-appropriate material
// - CharacterController component
// - Animator component (needs controller assigned)
```

### Character Customization

#### Change Appearance at Runtime

```csharp
// Regenerate character with new data
CharacterCreationData newData = existingData;
newData.hairStyle = 8; // Change hair
newData.bodyType = BodyType.Muscular; // Change build

GameObject newCharacter = builder.GenerateCharacter(newData);

// Destroy old character
Destroy(oldCharacter);

// Replace reference
player = newCharacter.AddComponent<PlayerController>();
```

---

## UI System API

### UISystemManager

```csharp
UISystemManager uiMgr = CoreSystemManager.UIManager;

// Toggle UI panels
uiMgr.ToggleInventory();
uiMgr.ToggleCharacterSheet();
uiMgr.ToggleSpellbook();
```

### HUD Updates

HUD automatically updates when player EntityStats changes. No manual calls needed.

To force HUD reference update:
```csharp
// If player GameObject changes
UISystemManager uiMgr = CoreSystemManager.UIManager;
// Next Update() cycle will find new player automatically
```

### Custom UI Windows

```csharp
using UnityEngine;
using UnityEngine.UI;

public class CustomWindow : MonoBehaviour
{
    [SerializeField] private Canvas _canvas;
    private bool _isOpen;
    
    public void Toggle()
    {
        _isOpen = !_isOpen;
        _canvas.gameObject.SetActive(_isOpen);
        
        // Pause game when window open (optional)
        Time.timeScale = _isOpen ? 0f : 1f;
    }
    
    // Call from button
    public void OnCloseButton()
    {
        Toggle();
    }
}
```

---

## Extension Patterns

### Adding New Item Types

1. **Add to ItemType enum**:
```csharp
public enum ItemType { Weapon, Armor, Consumable, Material, QuestItem } // Added QuestItem
```

2. **Extend ItemData**:
```csharp
public class ItemData
{
    // ... existing fields ...
    
    // Quest item fields
    public int questId;
    public bool isQuestItem;
}
```

3. **Add generation method**:
```csharp
public class ItemGenerationEngine : MonoBehaviour
{
    public ItemData GenerateQuestItem(int questId)
    {
        return new ItemData
        {
            itemId = System.Guid.NewGuid().ToString(),
            itemName = $"Quest Item {questId}",
            itemType = ItemType.QuestItem,
            rarity = Rarity.Uncommon,
            isQuestItem = true,
            questId = questId
        };
    }
}
```

### Adding New Ability Types

1. **Create ability script**:
```csharp
[System.Serializable]
public class SummonAbility : Ability
{
    public GameObject summonPrefab;
    public float duration = 30f;
    
    public override void Execute(GameObject caster, Vector3 targetPosition)
    {
        base.Execute(caster, targetPosition);
        
        // Spawn summon
        GameObject summon = Instantiate(summonPrefab, targetPosition, Quaternion.identity);
        
        // Set summon to follow/fight for caster
        SummonAI ai = summon.GetComponent<SummonAI>();
        ai.SetMaster(caster);
        
        // Destroy after duration
        Destroy(summon, duration);
    }
}
```

2. **Add to hotbar**:
```csharp
SummonAbility summonWolf = new()
{
    abilityName = "Summon Wolf",
    abilityType = AbilityType.Summon,
    manaCost = 50,
    cooldown = 60f,
    summonPrefab = wolfPrefab,
    duration = 30f
};

abilityController.EquipAbility(summonWolf, slotIndex: 5);
```

### Creating New Biomes

1. **Add to BiomeType enum**:
```csharp
public enum BiomeType { Grassland, Desert, Snow, Lava, Corrupted, Swamp } // Added Swamp
```

2. **Extend SimpleTerrainGenerator**:
```csharp
private float ApplyBiomeModifier(float height, BiomeType biome)
{
    return biome switch
    {
        // ... existing cases ...
        BiomeType.Swamp => height * 0.4f + Mathf.PerlinNoise(height * 20f, height * 20f) * 1f, // Flat with puddles
        _ => height
    };
}

private Color GetBiomeColor(BiomeType biome, float height)
{
    Color baseColor = biome switch
    {
        // ... existing cases ...
        BiomeType.Swamp => new Color(0.2f, 0.4f, 0.2f), // Dark green
        _ => Color.gray
    };
    
    // ... rest of method ...
}
```

3. **Generate swamp zone**:
```csharp
ZoneConfig swampConfig = new()
{
    zoneName = "SwampZone",
    biomeType = BiomeType.Swamp,
    zoneType = ZoneType.Wilderness,
    zoneSize = new Vector3(150, 0, 150),
    seed = 99999
};

await zoneMgr.GenerateAndSaveZone(swampConfig);
```

---

## Performance Optimization Tips

### 1. Object Pooling
```csharp
// BAD: Creates garbage
for (int i = 0; i < 100; i++)
{
    GameObject bullet = Instantiate(bulletPrefab);
    // ... use bullet ...
    Destroy(bullet, 5f);
}

// GOOD: Zero allocations
for (int i = 0; i < 100; i++)
{
    Bullet bullet = poolMgr.Get<Bullet>("Bullet");
    // ... use bullet ...
    // Bullet returns itself to pool after lifetime
}
```

### 2. Caching Components
```csharp
public class EnemyAI : MonoBehaviour
{
    // BAD: GetComponent every frame
    void Update()
    {
        EntityStats stats = GetComponent<EntityStats>(); // Allocation!
        // ...
    }
    
    // GOOD: Cache on Awake
    private EntityStats _stats;
    
    void Awake()
    {
        _stats = GetComponent<EntityStats>();
    }
    
    void Update()
    {
        // Use cached reference
        if (_stats.currentHealth < _stats.maxHealth * 0.3f)
        {
            // Flee
        }
    }
}
```

### 3. Avoid LINQ in Update
```csharp
// BAD: LINQ allocates
void Update()
{
    var nearbyEnemies = FindObjectsByType<Enemy>()
        .Where(e => Vector3.Distance(transform.position, e.transform.position) < 10f)
        .ToList(); // Allocation every frame!
}

// GOOD: Manual loop, no allocations
private List<Enemy> _cachedEnemies = new();

void Update()
{
    _cachedEnemies.Clear(); // Reuse list
    
    foreach (Enemy enemy in _allEnemies) // Cached array
    {
        if (Vector3.Distance(transform.position, enemy.transform.position) < 10f)
        {
            _cachedEnemies.Add(enemy);
        }
    }
}
```

### 4. Use Burst-Compatible Jobs
```csharp
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;

[BurstCompile]
struct CalculateDamageJob : IJobParallelFor
{
    [ReadOnly] public NativeArray<int> baseDamages;
    [ReadOnly] public NativeArray<float> multipliers;
    [WriteOnly] public NativeArray<int> finalDamages;
    
    public void Execute(int index)
    {
        finalDamages[index] = (int)(baseDamages[index] * multipliers[index]);
    }
}

// Schedule job for 1000 damage calculations
NativeArray<int> damages = new(1000, Allocator.TempJob);
// ... fill arrays ...
CalculateDamageJob job = new() { /* ... */ };
JobHandle handle = job.Schedule(1000, 64);
handle.Complete();
// ... use results ...
damages.Dispose();
```

---

## Debugging Tools

### Enable Verbose Logging

```csharp
#define VERBOSE_DEBUG

public class MySystem : MonoBehaviour
{
    void SomeMethod()
    {
        #if VERBOSE_DEBUG
        Debug.Log($"[{GetType().Name}] Method called with params: {param1}, {param2}");
        #endif
    }
}
```

### Inspector Debugging

All serialized fields are visible in Inspector:
- Runtime references auto-populate
- Can manually link before first run
- Monitor state changes in Play mode

### Profiler Integration

```csharp
using UnityEngine.Profiling;

void ExpensiveMethod()
{
    Profiler.BeginSample("MyExpensiveMethod");
    
    // ... expensive code ...
    
    Profiler.EndSample();
}
```

---

For complete examples, see EXAMPLES.md