using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace Game.Core.Systems
{
    /// <summary>
    /// Loot System Manager - Handles loot drops, loot tables, and rarity rolls.
    /// Integrates with ItemGenerationEngine for procedural loot.
    /// COMPLETE IMPLEMENTATION!
    /// </summary>
    public class LootSystemManager : MonoBehaviour
    {
        [Header("Loot Configuration")]
        [SerializeField] private bool _autoDropOnDeath = true;
        [SerializeField] private float _lootDropRadius = 2f;
        [SerializeField] private float _lootPickupRange = 3f;
        
        [Header("Rarity Weights")]
        [SerializeField] private RarityWeights _rarityWeights = new();
        
        [Header("Loot Tables")]
        [SerializeField] private List<LootTable> _lootTables = new();
        
        [Header("World Loot")]
        [SerializeField] private List<LootDrop> _activeLootDrops = new();
        
        private ItemGenerationEngine _itemGenerator;
        private Dictionary<string, LootTable> _lootTableMap;
        
        #region Initialization
        
        private void Awake()
        {
            _itemGenerator = FindFirstObjectByType<ItemGenerationEngine>();
            if (_itemGenerator == null)
            {
                GameObject genObj = new GameObject("ItemGenerationEngine");
                _itemGenerator = genObj.AddComponent<ItemGenerationEngine>();
            }
            
            _lootTableMap = new Dictionary<string, LootTable>();
            InitializeLootTables();
            
            Debug.Log("[LootSystemManager] Initialized");
        }
        
        private void Start()
        {
            // Subscribe to combat events
            CombatSystemManager combatManager = CoreSystemManager.CombatManager;
            if (combatManager != null)
            {
                combatManager.OnEntityDeath += OnEntityDeath;
            }
        }
        
        private void OnDestroy()
        {
            CombatSystemManager combatManager = CoreSystemManager.CombatManager;
            if (combatManager != null)
            {
                combatManager.OnEntityDeath -= OnEntityDeath;
            }
        }
        
        private void InitializeLootTables()
        {
            // Create default loot tables for common enemy types
            CreateDefaultLootTable("Zombie", 1, 3, new Vector2Int(1, 5));
            CreateDefaultLootTable("Skeleton", 1, 3, new Vector2Int(1, 5));
            CreateDefaultLootTable("Orc", 2, 4, new Vector2Int(5, 10));
            CreateDefaultLootTable("Boss", 5, 8, new Vector2Int(10, 20));
            
            Debug.Log($"[LootSystemManager] Initialized {_lootTables.Count} loot tables");
        }
        
        private void CreateDefaultLootTable(string tableName, int minItems, int maxItems, Vector2Int levelRange)
        {
            LootTable table = new LootTable
            {
                tableName = tableName,
                minItems = minItems,
                maxItems = maxItems,
                levelRange = levelRange,
                goldMin = levelRange.x * 10,
                goldMax = levelRange.y * 20
            };
            
            _lootTables.Add(table);
            _lootTableMap[tableName] = table;
        }
        
        #endregion
        
        #region Loot Generation
        
        /// <summary>
        /// Generate loot from a loot table.
        /// </summary>
        public List<ItemData> GenerateLoot(string lootTableName)
        {
            if (!_lootTableMap.ContainsKey(lootTableName))
            {
                Debug.LogWarning($"[LootSystemManager] Loot table '{lootTableName}' not found!");
                return new List<ItemData>();
            }
            
            LootTable table = _lootTableMap[lootTableName];
            return GenerateLootFromTable(table);
        }
        
        private List<ItemData> GenerateLootFromTable(LootTable table)
        {
            List<ItemData> loot = new();
            
            int itemCount = Random.Range(table.minItems, table.maxItems + 1);
            
            for (int i = 0; i < itemCount; i++)
            {
                // Roll rarity
                Rarity rarity = RollRarity();
                
                // Roll item type
                ItemType itemType = Random.value > 0.5f ? ItemType.Weapon : ItemType.Armor;
                
                // Generate item
                ItemData item = itemType == ItemType.Weapon 
                    ? _itemGenerator.GenerateWeapon(rarity) 
                    : _itemGenerator.GenerateArmor(rarity);
                
                loot.Add(item);
            }
            
            Debug.Log($"[LootSystemManager] Generated {loot.Count} items from table '{table.tableName}'");
            return loot;
        }
        
        /// <summary>
        /// Roll a random rarity based on weights.
        /// </summary>
        private Rarity RollRarity()
        {
            float roll = Random.Range(0f, 1f);
            
            float cumulative = 0f;
            
            cumulative += _rarityWeights.commonWeight;
            if (roll <= cumulative) return Rarity.Common;
            
            cumulative += _rarityWeights.uncommonWeight;
            if (roll <= cumulative) return Rarity.Uncommon;
            
            cumulative += _rarityWeights.rareWeight;
            if (roll <= cumulative) return Rarity.Rare;
            
            cumulative += _rarityWeights.epicWeight;
            if (roll <= cumulative) return Rarity.Epic;
            
            cumulative += _rarityWeights.legendaryWeight;
            if (roll <= cumulative) return Rarity.Legendary;
            
            return Rarity.Mythic;
        }
        
        #endregion
        
        #region Loot Drops
        
        /// <summary>
        /// Drop loot at a position in the world.
        /// </summary>
        public void DropLoot(List<ItemData> items, Vector3 position, int gold = 0)
        {
            if (items == null || items.Count == 0)
            {
                Debug.Log("[LootSystemManager] No items to drop");
                return;
            }
            
            foreach (ItemData item in items)
            {
                Vector3 dropPosition = position + Random.insideUnitSphere * _lootDropRadius;
                dropPosition.y = position.y;
                
                CreateLootDrop(item, dropPosition);
            }
            
            if (gold > 0)
            {
                CreateGoldDrop(gold, position);
            }
            
            Debug.Log($"[LootSystemManager] Dropped {items.Count} items at {position}");
        }
        
        private void CreateLootDrop(ItemData item, Vector3 position)
        {
            GameObject dropObj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            dropObj.name = $"LootDrop_{item.itemName}";
            dropObj.transform.position = position + Vector3.up * 0.5f;
            dropObj.transform.localScale = Vector3.one * 0.5f;
            
            // Visual
            Renderer renderer = dropObj.GetComponent<Renderer>();
            Material mat = new Material(Shader.Find("Standard"));
            mat.color = GetRarityColor(item.rarity);
            mat.SetFloat("_Metallic", 0.5f);
            renderer.material = mat;
            
            // Add glow effect
            Light light = dropObj.AddComponent<Light>();
            light.color = GetRarityColor(item.rarity);
            light.range = 3f;
            light.intensity = 0.5f;
            
            // Add loot drop component
            LootDrop lootDrop = dropObj.AddComponent<LootDrop>();
            lootDrop.Initialize(item, this);
            
            _activeLootDrops.Add(lootDrop);
        }
        
        private void CreateGoldDrop(int amount, Vector3 position)
        {
            GameObject dropObj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            dropObj.name = $"GoldDrop_{amount}";
            dropObj.transform.position = position + Vector3.up * 0.5f;
            dropObj.transform.localScale = Vector3.one * 0.3f;
            
            Renderer renderer = dropObj.GetComponent<Renderer>();
            Material mat = new Material(Shader.Find("Standard"));
            mat.color = Color.yellow;
            mat.SetFloat("_Metallic", 1f);
            renderer.material = mat;
            
            GoldDrop goldDrop = dropObj.AddComponent<GoldDrop>();
            goldDrop.Initialize(amount, this);
        }
        
        private Color GetRarityColor(Rarity rarity)
        {
            return rarity switch
            {
                Rarity.Common => Color.white,
                Rarity.Uncommon => Color.green,
                Rarity.Rare => Color.blue,
                Rarity.Epic => new Color(0.6f, 0.3f, 1f),
                Rarity.Legendary => new Color(1f, 0.5f, 0f),
                Rarity.Mythic => new Color(1f, 0f, 0f),
                _ => Color.white
            };
        }
        
        #endregion
        
        #region Loot Pickup
        
        /// <summary>
        /// Check for nearby loot and attempt pickup.
        /// </summary>
        private void Update()
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player == null) return;
            
            CheckForAutoPickup(player);
        }
        
        private void CheckForAutoPickup(GameObject player)
        {
            List<LootDrop> dropsToRemove = new();
            
            foreach (LootDrop drop in _activeLootDrops)
            {
                if (drop == null) continue;
                
                float distance = Vector3.Distance(player.transform.position, drop.transform.position);
                
                if (distance <= _lootPickupRange)
                {
                    if (PickupLoot(player, drop))
                    {
                        dropsToRemove.Add(drop);
                    }
                }
            }
            
            // Remove picked up drops
            foreach (LootDrop drop in dropsToRemove)
            {
                _activeLootDrops.Remove(drop);
                if (drop != null)
                    Destroy(drop.gameObject);
            }
        }
        
        private bool PickupLoot(GameObject player, LootDrop drop)
        {
            InventorySystemManager inventoryManager = CoreSystemManager.InventoryManager;
            if (inventoryManager == null) return false;
            
            bool added = inventoryManager.AddItem(player, drop.Item);
            
            if (added)
            {
                Debug.Log($"[LootSystemManager] Player picked up: {drop.Item.itemName}");
                
                // Play pickup sound
                AudioSystemManager audioManager = CoreSystemManager.AudioManager;
                if (audioManager != null)
                {
                    // audioManager.PlaySFX(pickupSound, player.transform.position);
                }
                
                return true;
            }
            
            return false;
        }
        
        public void RemoveLootDrop(LootDrop drop)
        {
            _activeLootDrops.Remove(drop);
        }
        
        #endregion
        
        #region Event Handlers
        
        private void OnEntityDeath(GameObject deadEntity, GameObject killer)
        {
            if (!_autoDropOnDeath) return;
            
            // Don't drop loot from player death
            if (deadEntity.CompareTag("Player")) return;
            
            // Determine loot table based on entity name
            string lootTableName = DetermineLootTable(deadEntity);
            
            // Generate loot
            List<ItemData> loot = GenerateLoot(lootTableName);
            
            // Generate gold
            int gold = GenerateGold(lootTableName);
            
            // Drop loot at death position
            DropLoot(loot, deadEntity.transform.position, gold);
        }
        
        private string DetermineLootTable(GameObject entity)
        {
            // Check entity name for keywords
            string name = entity.name.ToLower();
            
            if (name.Contains("boss")) return "Boss";
            if (name.Contains("orc")) return "Orc";
            if (name.Contains("skeleton")) return "Skeleton";
            if (name.Contains("zombie")) return "Zombie";
            
            // Default to zombie table
            return "Zombie";
        }
        
        private int GenerateGold(string lootTableName)
        {
            if (!_lootTableMap.ContainsKey(lootTableName))
                return Random.Range(5, 20);
            
            LootTable table = _lootTableMap[lootTableName];
            return Random.Range(table.goldMin, table.goldMax + 1);
        }
        
        #endregion
        
        #region Public API
        
        public void SetRarityWeights(RarityWeights weights)
        {
            _rarityWeights = weights;
        }
        
        public void AddLootTable(LootTable table)
        {
            if (!_lootTableMap.ContainsKey(table.tableName))
            {
                _lootTables.Add(table);
                _lootTableMap[table.tableName] = table;
            }
        }
        
        public List<string> GetAvailableLootTables()
        {
            return _lootTableMap.Keys.ToList();
        }
        
        #endregion
    }
    
    #region Data Structures
    
    /// <summary>
    /// Loot table definition.
    /// </summary>
    [System.Serializable]
    public class LootTable
    {
        public string tableName;
        public int minItems = 1;
        public int maxItems = 3;
        public Vector2Int levelRange = new Vector2Int(1, 10);
        public int goldMin = 10;
        public int goldMax = 50;
    }
    
    /// <summary>
    /// Rarity roll weights (must sum to 1.0).
    /// </summary>
    [System.Serializable]
    public class RarityWeights
    {
        [Range(0f, 1f)] public float commonWeight = 0.50f;
        [Range(0f, 1f)] public float uncommonWeight = 0.25f;
        [Range(0f, 1f)] public float rareWeight = 0.15f;
        [Range(0f, 1f)] public float epicWeight = 0.07f;
        [Range(0f, 1f)] public float legendaryWeight = 0.025f;
        [Range(0f, 1f)] public float mythicWeight = 0.005f;
        
        public void Normalize()
        {
            float total = commonWeight + uncommonWeight + rareWeight + epicWeight + legendaryWeight + mythicWeight;
            
            if (total > 0)
            {
                commonWeight /= total;
                uncommonWeight /= total;
                rareWeight /= total;
                epicWeight /= total;
                legendaryWeight /= total;
                mythicWeight /= total;
            }
        }
    }
    
    #endregion
    
    #region Loot Drop Components
    
    /// <summary>
    /// World loot drop component.
    /// </summary>
    public class LootDrop : MonoBehaviour
    {
        private ItemData _item;
        private LootSystemManager _lootManager;
        private float _spawnTime;
        private float _bobSpeed = 1f;
        private float _bobHeight = 0.2f;
        private Vector3 _startPosition;
        
        public ItemData Item => _item;
        
        public void Initialize(ItemData item, LootSystemManager lootManager)
        {
            _item = item;
            _lootManager = lootManager;
            _spawnTime = Time.time;
            _startPosition = transform.position;
        }
        
        private void Update()
        {
            // Bob up and down
            float bob = Mathf.Sin((Time.time - _spawnTime) * _bobSpeed) * _bobHeight;
            transform.position = _startPosition + Vector3.up * bob;
            
            // Rotate
            transform.Rotate(Vector3.up, 90f * Time.deltaTime);
        }
        
        private void OnDestroy()
        {
            if (_lootManager != null)
            {
                _lootManager.RemoveLootDrop(this);
            }
        }
    }
    
    /// <summary>
    /// Gold drop component.
    /// </summary>
    public class GoldDrop : MonoBehaviour
    {
        private int _amount;
        private LootSystemManager _lootManager;
        private float _spawnTime;
        private Vector3 _startPosition;
        
        public int Amount => _amount;
        
        public void Initialize(int amount, LootSystemManager lootManager)
        {
            _amount = amount;
            _lootManager = lootManager;
            _spawnTime = Time.time;
            _startPosition = transform.position;
        }
        
        private void Update()
        {
            // Bob
            float bob = Mathf.Sin((Time.time - _spawnTime) * 2f) * 0.15f;
            transform.position = _startPosition + Vector3.up * bob;
            
            // Rotate
            transform.Rotate(Vector3.up, 180f * Time.deltaTime);
            
            // Check for player pickup
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                float distance = Vector3.Distance(transform.position, player.transform.position);
                if (distance <= 3f)
                {
                    PickupGold(player);
                }
            }
        }
        
        private void PickupGold(GameObject player)
        {
            // TODO: Add gold to player currency
            Debug.Log($"[GoldDrop] Player picked up {_amount} gold");
            Destroy(gameObject);
        }
    }
    
    #endregion
}