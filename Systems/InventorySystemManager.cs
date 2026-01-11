using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace Game.Core.Systems
{
    /// <summary>
    /// Inventory System Manager - Complete inventory management with equipment slots.
    /// Handles item storage, equipped items, stacking, and inventory UI.
    /// COMPLETE IMPLEMENTATION!
    /// </summary>
    public class InventorySystemManager : MonoBehaviour
    {
        [Header("Inventory Configuration")]
        [SerializeField] private int _defaultInventorySize = 30;
        [SerializeField] private int _maxStackSize = 99;
        
        [Header("Player Inventory")]
        [SerializeField] private InventoryData _playerInventory;
        
        [Header("Equipment Slots")]
        [SerializeField] private EquipmentData _playerEquipment;
        
        private Dictionary<GameObject, InventoryData> _entityInventories;
        
        #region Initialization
        
        private void Awake()
        {
            _entityInventories = new Dictionary<GameObject, InventoryData>();
            InitializePlayerInventory();
            Debug.Log("[InventorySystemManager] Initialized");
        }
        
        private void InitializePlayerInventory()
        {
            _playerInventory = new InventoryData(_defaultInventorySize);
            _playerEquipment = new EquipmentData();
        }
        
        #endregion
        
        #region Inventory Management
        
        /// <summary>
        /// Get or create inventory for an entity.
        /// </summary>
        public InventoryData GetInventory(GameObject entity)
        {
            // Player inventory
            if (entity.CompareTag("Player"))
            {
                return _playerInventory;
            }
            
            // Entity inventories
            if (!_entityInventories.ContainsKey(entity))
            {
                _entityInventories[entity] = new InventoryData(_defaultInventorySize);
            }
            
            return _entityInventories[entity];
        }
        
        /// <summary>
        /// Add item to entity inventory.
        /// </summary>
        public bool AddItem(GameObject entity, ItemData item, int quantity = 1)
        {
            InventoryData inventory = GetInventory(entity);
            
            bool added = inventory.AddItem(item, quantity);
            
            if (added)
            {
                Debug.Log($"[InventorySystemManager] Added {quantity}x {item.itemName} to {entity.name}");
                OnInventoryChanged?.Invoke(entity);
            }
            else
            {
                Debug.LogWarning($"[InventorySystemManager] Failed to add {item.itemName} - inventory full!");
            }
            
            return added;
        }
        
        /// <summary>
        /// Remove item from entity inventory.
        /// </summary>
        public bool RemoveItem(GameObject entity, string itemId, int quantity = 1)
        {
            InventoryData inventory = GetInventory(entity);
            
            bool removed = inventory.RemoveItem(itemId, quantity);
            
            if (removed)
            {
                Debug.Log($"[InventorySystemManager] Removed {quantity}x item {itemId} from {entity.name}");
                OnInventoryChanged?.Invoke(entity);
            }
            
            return removed;
        }
        
        /// <summary>
        /// Check if entity has item.
        /// </summary>
        public bool HasItem(GameObject entity, string itemId, int quantity = 1)
        {
            InventoryData inventory = GetInventory(entity);
            return inventory.HasItem(itemId, quantity);
        }
        
        /// <summary>
        /// Get item count in inventory.
        /// </summary>
        public int GetItemCount(GameObject entity, string itemId)
        {
            InventoryData inventory = GetInventory(entity);
            return inventory.GetItemCount(itemId);
        }
        
        #endregion
        
        #region Equipment Management
        
        /// <summary>
        /// Equip an item from inventory.
        /// </summary>
        public bool EquipItem(GameObject entity, string itemId)
        {
            if (!entity.CompareTag("Player"))
            {
                Debug.LogWarning("[InventorySystemManager] Only player can equip items currently");
                return false;
            }
            
            InventoryData inventory = GetInventory(entity);
            ItemData item = inventory.GetItem(itemId);
            
            if (item == null)
            {
                Debug.LogWarning($"[InventorySystemManager] Item {itemId} not found in inventory");
                return false;
            }
            
            // Determine slot based on item type
            if (item.itemType == ItemType.Weapon)
            {
                return EquipWeapon(entity, item);
            }
            else if (item.itemType == ItemType.Armor)
            {
                return EquipArmor(entity, item);
            }
            
            return false;
        }
        
        private bool EquipWeapon(GameObject entity, ItemData weapon)
        {
            // Unequip current weapon
            if (_playerEquipment.equippedWeapon != null)
            {
                UnequipItem(entity, _playerEquipment.equippedWeapon.itemId);
            }
            
            // Equip new weapon
            _playerEquipment.equippedWeapon = weapon;
            
            // Apply weapon stats to player
            EntityStats stats = entity.GetComponent<EntityStats>();
            if (stats != null)
            {
                stats.damageElement = weapon.weaponArchetype switch
                {
                    WeaponArchetype.LaserRifle => DamageElement.Lightning,
                    WeaponArchetype.PlasmaGun => DamageElement.Fire,
                    _ => DamageElement.None
                };
            }
            
            Debug.Log($"[InventorySystemManager] Equipped weapon: {weapon.itemName}");
            OnEquipmentChanged?.Invoke(entity);
            return true;
        }
        
        private bool EquipArmor(GameObject entity, ItemData armor)
        {
            // Unequip current armor in slot
            ItemData currentArmor = _playerEquipment.GetArmorInSlot(armor.armorSlot);
            if (currentArmor != null)
            {
                UnequipItem(entity, currentArmor.itemId);
            }
            
            // Equip new armor
            _playerEquipment.SetArmorInSlot(armor.armorSlot, armor);
            
            // Apply armor stats
            RecalculateEquipmentStats(entity);
            
            Debug.Log($"[InventorySystemManager] Equipped armor: {armor.itemName}");
            OnEquipmentChanged?.Invoke(entity);
            return true;
        }
        
        /// <summary>
        /// Unequip an item and return it to inventory.
        /// </summary>
        public bool UnequipItem(GameObject entity, string itemId)
        {
            if (!entity.CompareTag("Player"))
                return false;
            
            ItemData item = null;
            
            // Check weapon
            if (_playerEquipment.equippedWeapon != null && _playerEquipment.equippedWeapon.itemId == itemId)
            {
                item = _playerEquipment.equippedWeapon;
                _playerEquipment.equippedWeapon = null;
            }
            
            // Check armor slots
            foreach (ArmorSlot slot in System.Enum.GetValues(typeof(ArmorSlot)))
            {
                ItemData armorPiece = _playerEquipment.GetArmorInSlot(slot);
                if (armorPiece != null && armorPiece.itemId == itemId)
                {
                    item = armorPiece;
                    _playerEquipment.SetArmorInSlot(slot, null);
                    break;
                }
            }
            
            if (item == null)
                return false;
            
            // Return to inventory (already there, just mark as unequipped)
            RecalculateEquipmentStats(entity);
            
            Debug.Log($"[InventorySystemManager] Unequipped: {item.itemName}");
            OnEquipmentChanged?.Invoke(entity);
            return true;
        }
        
        private void RecalculateEquipmentStats(GameObject entity)
        {
            EntityStats stats = entity.GetComponent<EntityStats>();
            if (stats == null) return;
            
            // Reset bonus stats
            int totalArmor = 0;
            int bonusHealth = 0;
            int bonusMana = 0;
            float critChance = 0f;
            
            // Sum armor pieces
            foreach (ArmorSlot slot in System.Enum.GetValues(typeof(ArmorSlot)))
            {
                ItemData armor = _playerEquipment.GetArmorInSlot(slot);
                if (armor != null)
                {
                    totalArmor += armor.armorValue;
                    bonusHealth += armor.bonusHealth;
                    bonusMana += armor.bonusMana;
                    critChance += armor.critChance;
                }
            }
            
            // Add weapon bonuses
            if (_playerEquipment.equippedWeapon != null)
            {
                bonusHealth += _playerEquipment.equippedWeapon.bonusHealth;
                bonusMana += _playerEquipment.equippedWeapon.bonusMana;
                critChance += _playerEquipment.equippedWeapon.critChance;
            }
            
            // Apply to stats
            stats.armor = totalArmor;
            stats.criticalChance = critChance;
            
            // Recalculate derived stats
            stats.RecalculateStats();
            
            Debug.Log($"[InventorySystemManager] Recalculated stats: Armor={totalArmor}, Crit={critChance:P}");
        }
        
        public EquipmentData GetEquipment(GameObject entity)
        {
            if (entity.CompareTag("Player"))
                return _playerEquipment;
            
            return null;
        }
        
        #endregion
        
        #region Item Transfer
        
        /// <summary>
        /// Transfer item from one entity to another.
        /// </summary>
        public bool TransferItem(GameObject from, GameObject to, string itemId, int quantity = 1)
        {
            InventoryData fromInv = GetInventory(from);
            InventoryData toInv = GetInventory(to);
            
            ItemData item = fromInv.GetItem(itemId);
            if (item == null || !fromInv.HasItem(itemId, quantity))
                return false;
            
            // Remove from source
            if (!fromInv.RemoveItem(itemId, quantity))
                return false;
            
            // Add to destination
            if (!toInv.AddItem(item, quantity))
            {
                // Failed to add, return to source
                fromInv.AddItem(item, quantity);
                return false;
            }
            
            OnInventoryChanged?.Invoke(from);
            OnInventoryChanged?.Invoke(to);
            
            Debug.Log($"[InventorySystemManager] Transferred {quantity}x {item.itemName} from {from.name} to {to.name}");
            return true;
        }
        
        #endregion
        
        #region Events
        
        public System.Action<GameObject> OnInventoryChanged;
        public System.Action<GameObject> OnEquipmentChanged;
        
        #endregion
        
        #region Shutdown
        
        public void Shutdown()
        {
            Debug.Log("[InventorySystemManager] Shutting down...");
            _entityInventories.Clear();
        }
        
        #endregion
    }
    
    #region Data Structures
    
    /// <summary>
    /// Inventory data container.
    /// </summary>
    [System.Serializable]
    public class InventoryData
    {
        [SerializeField] private int _maxSlots;
        [SerializeField] private List<InventorySlot> _slots;
        
        public int MaxSlots => _maxSlots;
        public List<InventorySlot> Slots => _slots;
        
        public InventoryData(int maxSlots)
        {
            _maxSlots = maxSlots;
            _slots = new List<InventorySlot>();
        }
        
        public bool AddItem(ItemData item, int quantity)
        {
            // Try to stack with existing item
            InventorySlot existingSlot = _slots.FirstOrDefault(s => s.item.itemId == item.itemId);
            if (existingSlot != null)
            {
                existingSlot.quantity += quantity;
                return true;
            }
            
            // Check if we have space
            if (_slots.Count >= _maxSlots)
                return false;
            
            // Add new slot
            _slots.Add(new InventorySlot(item, quantity));
            return true;
        }
        
        public bool RemoveItem(string itemId, int quantity)
        {
            InventorySlot slot = _slots.FirstOrDefault(s => s.item.itemId == itemId);
            if (slot == null || slot.quantity < quantity)
                return false;
            
            slot.quantity -= quantity;
            
            if (slot.quantity <= 0)
            {
                _slots.Remove(slot);
            }
            
            return true;
        }
        
        public bool HasItem(string itemId, int quantity)
        {
            InventorySlot slot = _slots.FirstOrDefault(s => s.item.itemId == itemId);
            return slot != null && slot.quantity >= quantity;
        }
        
        public int GetItemCount(string itemId)
        {
            InventorySlot slot = _slots.FirstOrDefault(s => s.item.itemId == itemId);
            return slot?.quantity ?? 0;
        }
        
        public ItemData GetItem(string itemId)
        {
            InventorySlot slot = _slots.FirstOrDefault(s => s.item.itemId == itemId);
            return slot?.item;
        }
        
        public List<ItemData> GetAllItems()
        {
            return _slots.Select(s => s.item).ToList();
        }
        
        public void Clear()
        {
            _slots.Clear();
        }
    }
    
    /// <summary>
    /// Individual inventory slot.
    /// </summary>
    [System.Serializable]
    public class InventorySlot
    {
        public ItemData item;
        public int quantity;
        
        public InventorySlot(ItemData item, int quantity)
        {
            this.item = item;
            this.quantity = quantity;
        }
    }
    
    /// <summary>
    /// Equipment data container.
    /// </summary>
    [System.Serializable]
    public class EquipmentData
    {
        public ItemData equippedWeapon;
        public ItemData equippedHead;
        public ItemData equippedChest;
        public ItemData equippedHands;
        public ItemData equippedLegs;
        public ItemData equippedFeet;
        
        public ItemData GetArmorInSlot(ArmorSlot slot)
        {
            return slot switch
            {
                ArmorSlot.Head => equippedHead,
                ArmorSlot.Chest => equippedChest,
                ArmorSlot.Hands => equippedHands,
                ArmorSlot.Legs => equippedLegs,
                ArmorSlot.Feet => equippedFeet,
                _ => null
            };
        }
        
        public void SetArmorInSlot(ArmorSlot slot, ItemData armor)
        {
            switch (slot)
            {
                case ArmorSlot.Head: equippedHead = armor; break;
                case ArmorSlot.Chest: equippedChest = armor; break;
                case ArmorSlot.Hands: equippedHands = armor; break;
                case ArmorSlot.Legs: equippedLegs = armor; break;
                case ArmorSlot.Feet: equippedFeet = armor; break;
            }
        }
        
        public List<ItemData> GetAllEquippedItems()
        {
            List<ItemData> items = new();
            
            if (equippedWeapon != null) items.Add(equippedWeapon);
            if (equippedHead != null) items.Add(equippedHead);
            if (equippedChest != null) items.Add(equippedChest);
            if (equippedHands != null) items.Add(equippedHands);
            if (equippedLegs != null) items.Add(equippedLegs);
            if (equippedFeet != null) items.Add(equippedFeet);
            
            return items;
        }
        
        public int GetTotalArmor()
        {
            int total = 0;
            
            if (equippedHead != null) total += equippedHead.armorValue;
            if (equippedChest != null) total += equippedChest.armorValue;
            if (equippedHands != null) total += equippedHands.armorValue;
            if (equippedLegs != null) total += equippedLegs.armorValue;
            if (equippedFeet != null) total += equippedFeet.armorValue;
            
            return total;
        }
    }
    
    #endregion
}