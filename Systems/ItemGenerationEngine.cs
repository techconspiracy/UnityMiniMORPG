using UnityEngine;
using System.Collections.Generic;

namespace Game.Core.Systems
{
    /// <summary>
    /// Hybrid item generation system.
    /// Pre-generates common items at startup, generates rare items on-demand.
    /// Supports weapons, armor, consumables with procedural stats and affixes.
    /// </summary>
    public class ItemGenerationEngine : MonoBehaviour
    {
        [Header("Generation Settings")]
        [SerializeField] private int _commonItemPregenCount = 50;
        [SerializeField] private int _maxCachedItems = 500;
        
        [Header("Item Pools")]
        [SerializeField] private List<ItemData> _cachedItems = new();
        
        private Dictionary<string, ItemData> _itemCache;
        private System.Random _rng;
        
        #region Initialization
        
        private void Awake()
        {
            _itemCache = new Dictionary<string, ItemData>();
            _rng = new System.Random();
            
            PreGenerateCommonItems();
        }
        
        private void PreGenerateCommonItems()
        {
            Debug.Log("[ItemGenerationEngine] Pre-generating common items...");
            
            for (int i = 0; i < _commonItemPregenCount; i++)
            {
                // Generate mix of weapons and armor
                ItemData item = i % 2 == 0 
                    ? GenerateWeapon(Rarity.Common) 
                    : GenerateArmor(Rarity.Common);
                
                CacheItem(item);
            }
            
            Debug.Log($"[ItemGenerationEngine] Pre-generated {_cachedItems.Count} items");
        }
        
        #endregion
        
        #region Weapon Generation
        
        public ItemData GenerateWeapon(Rarity rarity)
        {
            WeaponArchetype archetype = GetRandomWeaponArchetype();
            
            ItemData weapon = new()
            {
                itemId = System.Guid.NewGuid().ToString(),
                itemName = GenerateWeaponName(archetype, rarity),
                itemType = ItemType.Weapon,
                rarity = rarity,
                weaponArchetype = archetype,
                level = GetLevelForRarity(rarity),
            };
            
            // Generate base stats
            weapon.damage = GetWeaponBaseDamage(archetype, rarity);
            weapon.attackSpeed = GetWeaponAttackSpeed(archetype);
            weapon.range = GetWeaponRange(archetype);
            
            // Generate affixes based on rarity
            weapon.affixes = GenerateAffixes(rarity);
            
            // Apply affix bonuses
            ApplyAffixBonuses(weapon);
            
            return weapon;
        }
        
        private WeaponArchetype GetRandomWeaponArchetype()
        {
            WeaponArchetype[] archetypes = (WeaponArchetype[])System.Enum.GetValues(typeof(WeaponArchetype));
            return archetypes[_rng.Next(archetypes.Length)];
        }
        
        private string GenerateWeaponName(WeaponArchetype archetype, Rarity rarity)
        {
            string prefix = rarity switch
            {
                Rarity.Common => "",
                Rarity.Uncommon => "Fine ",
                Rarity.Rare => "Superior ",
                Rarity.Epic => "Masterwork ",
                Rarity.Legendary => "Legendary ",
                Rarity.Mythic => "Divine ",
                _ => ""
            };
            
            return $"{prefix}{archetype}";
        }
        
        private int GetWeaponBaseDamage(WeaponArchetype archetype, Rarity rarity)
        {
            int baseDamage = archetype switch
            {
                WeaponArchetype.Stick => 5,
                WeaponArchetype.Sword => 25,
                WeaponArchetype.Axe => 30,
                WeaponArchetype.Hammer => 35,
                WeaponArchetype.Bow => 20,
                WeaponArchetype.Pistol => 40,
                WeaponArchetype.Rifle => 50,
                WeaponArchetype.LaserRifle => 60,
                WeaponArchetype.PlasmaGun => 70,
                WeaponArchetype.QuantumBlade => 100,
                _ => 10
            };
            
            // Scale with rarity
            float multiplier = rarity switch
            {
                Rarity.Common => 1.0f,
                Rarity.Uncommon => 1.3f,
                Rarity.Rare => 1.7f,
                Rarity.Epic => 2.2f,
                Rarity.Legendary => 3.0f,
                Rarity.Mythic => 4.5f,
                _ => 1.0f
            };
            
            return Mathf.RoundToInt(baseDamage * multiplier);
        }
        
        private float GetWeaponAttackSpeed(WeaponArchetype archetype)
        {
            return archetype switch
            {
                WeaponArchetype.Stick => 2.0f,
                WeaponArchetype.Sword => 1.5f,
                WeaponArchetype.Axe => 1.2f,
                WeaponArchetype.Hammer => 1.0f,
                WeaponArchetype.Bow => 1.3f,
                WeaponArchetype.Pistol => 2.5f,
                WeaponArchetype.Rifle => 1.8f,
                _ => 1.5f
            };
        }
        
        private float GetWeaponRange(WeaponArchetype archetype)
        {
            return archetype switch
            {
                WeaponArchetype.Stick => 2f,
                WeaponArchetype.Sword => 2.5f,
                WeaponArchetype.Axe => 2.5f,
                WeaponArchetype.Hammer => 2f,
                WeaponArchetype.Bow => 30f,
                WeaponArchetype.Pistol => 25f,
                WeaponArchetype.Rifle => 50f,
                WeaponArchetype.LaserRifle => 60f,
                _ => 3f
            };
        }
        
        #endregion
        
        #region Armor Generation
        
        public ItemData GenerateArmor(Rarity rarity)
        {
            ArmorSlot slot = GetRandomArmorSlot();
            ArmorArchetype archetype = GetRandomArmorArchetype();
            
            ItemData armor = new()
            {
                itemId = System.Guid.NewGuid().ToString(),
                itemName = GenerateArmorName(archetype, slot, rarity),
                itemType = ItemType.Armor,
                rarity = rarity,
                armorSlot = slot,
                armorArchetype = archetype,
                level = GetLevelForRarity(rarity),
            };
            
            // Generate base stats
            armor.armorValue = GetArmorValue(archetype, slot, rarity);
            
            // Generate affixes
            armor.affixes = GenerateAffixes(rarity);
            
            // Apply affix bonuses
            ApplyAffixBonuses(armor);
            
            return armor;
        }
        
        private ArmorSlot GetRandomArmorSlot()
        {
            ArmorSlot[] slots = (ArmorSlot[])System.Enum.GetValues(typeof(ArmorSlot));
            return slots[_rng.Next(slots.Length)];
        }
        
        private ArmorArchetype GetRandomArmorArchetype()
        {
            ArmorArchetype[] archetypes = (ArmorArchetype[])System.Enum.GetValues(typeof(ArmorArchetype));
            return archetypes[_rng.Next(archetypes.Length)];
        }
        
        private string GenerateArmorName(ArmorArchetype archetype, ArmorSlot slot, Rarity rarity)
        {
            string prefix = rarity switch
            {
                Rarity.Uncommon => "Fine ",
                Rarity.Rare => "Superior ",
                Rarity.Epic => "Masterwork ",
                Rarity.Legendary => "Legendary ",
                Rarity.Mythic => "Divine ",
                _ => ""
            };
            
            return $"{prefix}{archetype} {slot}";
        }
        
        private int GetArmorValue(ArmorArchetype archetype, ArmorSlot slot, Rarity rarity)
        {
            int baseArmor = archetype switch
            {
                ArmorArchetype.Cloth => 5,
                ArmorArchetype.Leather => 15,
                ArmorArchetype.Chain => 30,
                ArmorArchetype.Plate => 50,
                ArmorArchetype.PowerArmor => 80,
                ArmorArchetype.EnergyShield => 100,
                _ => 10
            };
            
            // Scale with slot
            float slotMultiplier = slot switch
            {
                ArmorSlot.Head => 0.8f,
                ArmorSlot.Chest => 1.5f,
                ArmorSlot.Hands => 0.6f,
                ArmorSlot.Legs => 1.0f,
                ArmorSlot.Feet => 0.6f,
                _ => 1.0f
            };
            
            // Scale with rarity
            float rarityMultiplier = rarity switch
            {
                Rarity.Common => 1.0f,
                Rarity.Uncommon => 1.3f,
                Rarity.Rare => 1.7f,
                Rarity.Epic => 2.2f,
                Rarity.Legendary => 3.0f,
                Rarity.Mythic => 4.5f,
                _ => 1.0f
            };
            
            return Mathf.RoundToInt(baseArmor * slotMultiplier * rarityMultiplier);
        }
        
        #endregion
        
        #region Affix Generation
        
        private List<ItemAffix> GenerateAffixes(Rarity rarity)
        {
            List<ItemAffix> affixes = new();
            
            int affixCount = rarity switch
            {
                Rarity.Common => 0,
                Rarity.Uncommon => 1,
                Rarity.Rare => 2,
                Rarity.Epic => 3,
                Rarity.Legendary => 4,
                Rarity.Mythic => 6,
                _ => 0
            };
            
            for (int i = 0; i < affixCount; i++)
            {
                affixes.Add(GenerateRandomAffix(rarity));
            }
            
            return affixes;
        }
        
        private ItemAffix GenerateRandomAffix(Rarity rarity)
        {
            AffixType[] types = (AffixType[])System.Enum.GetValues(typeof(AffixType));
            AffixType type = types[_rng.Next(types.Length)];
            
            float valueMultiplier = rarity switch
            {
                Rarity.Uncommon => 1.0f,
                Rarity.Rare => 1.5f,
                Rarity.Epic => 2.5f,
                Rarity.Legendary => 4.0f,
                Rarity.Mythic => 6.0f,
                _ => 1.0f
            };
            
            return new ItemAffix
            {
                affixType = type,
                value = Mathf.RoundToInt(_rng.Next(5, 20) * valueMultiplier)
            };
        }
        
        private void ApplyAffixBonuses(ItemData item)
        {
            foreach (ItemAffix affix in item.affixes)
            {
                switch (affix.affixType)
                {
                    case AffixType.BonusDamage:
                        item.damage += affix.value;
                        break;
                    case AffixType.BonusArmor:
                        item.armorValue += affix.value;
                        break;
                    case AffixType.BonusHealth:
                        item.bonusHealth = affix.value;
                        break;
                    case AffixType.BonusMana:
                        item.bonusMana = affix.value;
                        break;
                    case AffixType.CriticalChance:
                        item.critChance = affix.value / 100f;
                        break;
                }
            }
        }
        
        #endregion
        
        #region Cache Management
        
        private void CacheItem(ItemData item)
        {
            if (_cachedItems.Count >= _maxCachedItems)
            {
                // Remove oldest common item
                for (int i = 0; i < _cachedItems.Count; i++)
                {
                    if (_cachedItems[i].rarity == Rarity.Common)
                    {
                        _itemCache.Remove(_cachedItems[i].itemId);
                        _cachedItems.RemoveAt(i);
                        break;
                    }
                }
            }
            
            _cachedItems.Add(item);
            _itemCache[item.itemId] = item;
        }
        
        public ItemData GetOrGenerateItem(Rarity rarity, ItemType type)
        {
            // Try to find cached item of matching rarity/type
            foreach (ItemData item in _cachedItems)
            {
                if (item.rarity == rarity && item.itemType == type)
                {
                    return item;
                }
            }
            
            // Generate new item
            ItemData newItem = type == ItemType.Weapon 
                ? GenerateWeapon(rarity) 
                : GenerateArmor(rarity);
            
            CacheItem(newItem);
            return newItem;
        }
        
        #endregion
        
        #region Helpers
        
        private int GetLevelForRarity(Rarity rarity)
        {
            return rarity switch
            {
                Rarity.Common => _rng.Next(1, 10),
                Rarity.Uncommon => _rng.Next(10, 20),
                Rarity.Rare => _rng.Next(20, 35),
                Rarity.Epic => _rng.Next(35, 50),
                Rarity.Legendary => _rng.Next(50, 75),
                Rarity.Mythic => _rng.Next(75, 100),
                _ => 1
            };
        }
        
        #endregion
    }
    
    #region Item Data Structures
    
    [System.Serializable]
    public class ItemData
    {
        public string itemId;
        public string itemName;
        public ItemType itemType;
        public Rarity rarity;
        public int level;
        
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
        public List<ItemAffix> affixes = new();
    }
    
    [System.Serializable]
    public class ItemAffix
    {
        public AffixType affixType;
        public int value;
    }
    
    public enum ItemType { Weapon, Armor, Consumable, Material }
    
    public enum Rarity { Common, Uncommon, Rare, Epic, Legendary, Mythic }
    
    public enum WeaponArchetype
    {
        Stick, Sword, Axe, Hammer, Spear, Bow, Crossbow,
        Pistol, Rifle, Shotgun, LaserRifle, PlasmaGun, Railgun,
        Staff, Wand, QuantumBlade
    }
    
    public enum ArmorSlot { Head, Chest, Hands, Legs, Feet }
    
    public enum ArmorArchetype
    {
        Cloth, Leather, Chain, Plate, PowerArmor, EnergyShield
    }
    
    public enum AffixType
    {
        BonusDamage, BonusArmor, BonusHealth, BonusMana,
        CriticalChance, AttackSpeed, MovementSpeed, LifeSteal
    }
    
    #endregion
}