using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

namespace Game.UI
{
    /// <summary>
    /// Complete inventory UI with equipment panel, item tooltips, and drag-drop.
    /// </summary>
    public class InventoryUI : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private Canvas _canvas;
        [SerializeField] private GameObject _inventoryPanel;
        [SerializeField] private GameObject _equipmentPanel;
        [SerializeField] private GameObject _tooltipPanel;
        
        [Header("Inventory Grid")]
        [SerializeField] private Transform _inventoryGridParent;
        [SerializeField] private List<InventorySlotUI> _inventorySlots = new();
        
        [Header("Equipment Slots")]
        [SerializeField] private EquipmentSlotUI _weaponSlot;
        [SerializeField] private EquipmentSlotUI _headSlot;
        [SerializeField] private EquipmentSlotUI _chestSlot;
        [SerializeField] private EquipmentSlotUI _handsSlot;
        [SerializeField] private EquipmentSlotUI _legsSlot;
        [SerializeField] private EquipmentSlotUI _feetSlot;
        
        [Header("Tooltip")]
        [SerializeField] private Text _tooltipText;
        
        private Game.Core.Systems.InventorySystemManager _inventoryManager;
        private GameObject _player;
        private bool _isOpen;
        
        #region Initialization
        
        private void Awake()
        {
            CreateUI();
        }
        
        private void Start()
        {
            _inventoryManager = Game.Core.Systems.CoreSystemManager.InventoryManager;
            _player = GameObject.FindGameObjectWithTag("Player");
            
            if (_inventoryManager != null)
            {
                _inventoryManager.OnInventoryChanged += OnInventoryChanged;
                _inventoryManager.OnEquipmentChanged += OnEquipmentChanged;
            }
            
            RefreshInventory();
            _inventoryPanel.SetActive(false);
        }
        
        private void OnDestroy()
        {
            if (_inventoryManager != null)
            {
                _inventoryManager.OnInventoryChanged -= OnInventoryChanged;
                _inventoryManager.OnEquipmentChanged -= OnEquipmentChanged;
            }
        }
        
        #endregion
        
        #region UI Creation
        
        private void CreateUI()
        {
            // Create canvas
            if (_canvas == null)
            {
                GameObject canvasObj = new GameObject("InventoryCanvas");
                canvasObj.transform.SetParent(transform, false);
                
                _canvas = canvasObj.AddComponent<Canvas>();
                _canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                _canvas.sortingOrder = 20;
                
                CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1920, 1080);
                
                canvasObj.AddComponent<GraphicRaycaster>();
            }
            
            CreateInventoryPanel();
            CreateEquipmentPanel();
            CreateTooltipPanel();
        }
        
        private void CreateInventoryPanel()
        {
            _inventoryPanel = new GameObject("InventoryPanel");
            _inventoryPanel.transform.SetParent(_canvas.transform, false);
            
            RectTransform rect = _inventoryPanel.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.25f, 0.2f);
            rect.anchorMax = new Vector2(0.75f, 0.8f);
            rect.sizeDelta = Vector2.zero;
            
            Image bg = _inventoryPanel.AddComponent<Image>();
            bg.color = new Color(0.1f, 0.1f, 0.15f, 0.95f);
            
            // Title
            CreateTitle(_inventoryPanel.transform, "INVENTORY");
            
            // Close button
            CreateCloseButton(_inventoryPanel.transform);
            
            // Inventory grid
            CreateInventoryGrid();
        }
        
        private void CreateTitle(Transform parent, string text)
        {
            GameObject titleObj = new GameObject("Title");
            titleObj.transform.SetParent(parent, false);
            
            RectTransform rect = titleObj.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0, 1);
            rect.anchorMax = new Vector2(1, 1);
            rect.pivot = new Vector2(0.5f, 1);
            rect.anchoredPosition = Vector2.zero;
            rect.sizeDelta = new Vector2(0, 60);
            
            Text titleText = titleObj.AddComponent<Text>();
            titleText.text = text;
            titleText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            titleText.fontSize = 32;
            titleText.color = Color.yellow;
            titleText.alignment = TextAnchor.MiddleCenter;
        }
        
        private void CreateCloseButton(Transform parent)
        {
            GameObject btnObj = new GameObject("CloseButton");
            btnObj.transform.SetParent(parent, false);
            
            RectTransform rect = btnObj.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(1, 1);
            rect.anchorMax = new Vector2(1, 1);
            rect.pivot = new Vector2(1, 1);
            rect.anchoredPosition = new Vector2(-10, -10);
            rect.sizeDelta = new Vector2(50, 50);
            
            Image img = btnObj.AddComponent<Image>();
            img.color = new Color(0.6f, 0.2f, 0.2f);
            
            Button btn = btnObj.AddComponent<Button>();
            btn.targetGraphic = img;
            btn.onClick.AddListener(ToggleInventory);
            
            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(btnObj.transform, false);
            
            RectTransform textRect = textObj.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.sizeDelta = Vector2.zero;
            
            Text text = textObj.AddComponent<Text>();
            text.text = "X";
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = 24;
            text.color = Color.white;
            text.alignment = TextAnchor.MiddleCenter;
        }
        
        private void CreateInventoryGrid()
        {
            GameObject gridObj = new GameObject("InventoryGrid");
            gridObj.transform.SetParent(_inventoryPanel.transform, false);
            
            RectTransform rect = gridObj.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0, 0);
            rect.anchorMax = new Vector2(0.65f, 1);
            rect.offsetMin = new Vector2(10, 10);
            rect.offsetMax = new Vector2(-10, -70);
            
            ScrollRect scroll = gridObj.AddComponent<ScrollRect>();
            scroll.horizontal = false;
            scroll.vertical = true;
            
            // Content
            GameObject content = new GameObject("Content");
            content.transform.SetParent(gridObj.transform, false);
            
            RectTransform contentRect = content.AddComponent<RectTransform>();
            contentRect.anchorMin = new Vector2(0, 1);
            contentRect.anchorMax = new Vector2(1, 1);
            contentRect.pivot = new Vector2(0.5f, 1);
            contentRect.sizeDelta = new Vector2(0, 1000);
            
            GridLayoutGroup grid = content.AddComponent<GridLayoutGroup>();
            grid.cellSize = new Vector2(80, 80);
            grid.spacing = new Vector2(5, 5);
            grid.padding = new RectOffset(10, 10, 10, 10);
            grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            grid.constraintCount = 5;
            
            ContentSizeFitter fitter = content.AddComponent<ContentSizeFitter>();
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            
            scroll.content = contentRect;
            _inventoryGridParent = content.transform;
            
            // Create 30 slots
            for (int i = 0; i < 30; i++)
            {
                CreateInventorySlot(i);
            }
        }
        
        private void CreateInventorySlot(int index)
        {
            GameObject slotObj = new GameObject($"InventorySlot_{index}");
            slotObj.transform.SetParent(_inventoryGridParent, false);
            
            Image bg = slotObj.AddComponent<Image>();
            bg.color = new Color(0.2f, 0.2f, 0.25f);
            
            InventorySlotUI slot = slotObj.AddComponent<InventorySlotUI>();
            slot.Initialize(index, this);
            
            _inventorySlots.Add(slot);
        }
        
        private void CreateEquipmentPanel()
        {
            _equipmentPanel = new GameObject("EquipmentPanel");
            _equipmentPanel.transform.SetParent(_canvas.transform, false);
            
            RectTransform rect = _equipmentPanel.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0, 0);
            rect.anchorMax = new Vector2(0, 0);
            rect.pivot = new Vector2(0, 0);
            rect.anchoredPosition = new Vector2(10, 10);
            rect.sizeDelta = new Vector2(300, 500);
            
            Image bg = _equipmentPanel.AddComponent<Image>();
            bg.color = new Color(0.1f, 0.1f, 0.15f, 0.95f);
            
            // Title
            GameObject titleObj = new GameObject("Title");
            titleObj.transform.SetParent(_equipmentPanel.transform, false);
            
            RectTransform titleRect = titleObj.AddComponent<RectTransform>();
            titleRect.anchorMin = new Vector2(0, 1);
            titleRect.anchorMax = new Vector2(1, 1);
            titleRect.pivot = new Vector2(0.5f, 1);
            titleRect.sizeDelta = new Vector2(0, 50);
            
            Text titleText = titleObj.AddComponent<Text>();
            titleText.text = "EQUIPMENT";
            titleText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            titleText.fontSize = 24;
            titleText.color = Color.cyan;
            titleText.alignment = TextAnchor.MiddleCenter;
            
            // Equipment slots
            _weaponSlot = CreateEquipmentSlot("Weapon", new Vector2(0.5f, 0.8f));
            _headSlot = CreateEquipmentSlot("Head", new Vector2(0.5f, 0.65f));
            _chestSlot = CreateEquipmentSlot("Chest", new Vector2(0.5f, 0.5f));
            _handsSlot = CreateEquipmentSlot("Hands", new Vector2(0.5f, 0.35f));
            _legsSlot = CreateEquipmentSlot("Legs", new Vector2(0.5f, 0.2f));
            _feetSlot = CreateEquipmentSlot("Feet", new Vector2(0.5f, 0.05f));
            
            _equipmentPanel.SetActive(false);
        }
        
        private EquipmentSlotUI CreateEquipmentSlot(string slotName, Vector2 anchorPos)
        {
            GameObject slotObj = new GameObject($"EquipmentSlot_{slotName}");
            slotObj.transform.SetParent(_equipmentPanel.transform, false);
            
            RectTransform rect = slotObj.AddComponent<RectTransform>();
            rect.anchorMin = anchorPos;
            rect.anchorMax = anchorPos;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = new Vector2(100, 100);
            
            Image bg = slotObj.AddComponent<Image>();
            bg.color = new Color(0.3f, 0.3f, 0.35f);
            
            // Label
            GameObject labelObj = new GameObject("Label");
            labelObj.transform.SetParent(slotObj.transform, false);
            
            RectTransform labelRect = labelObj.AddComponent<RectTransform>();
            labelRect.anchorMin = new Vector2(0, 0);
            labelRect.anchorMax = new Vector2(1, 0);
            labelRect.pivot = new Vector2(0.5f, 0);
            labelRect.sizeDelta = new Vector2(0, 20);
            
            Text labelText = labelObj.AddComponent<Text>();
            labelText.text = slotName;
            labelText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            labelText.fontSize = 12;
            labelText.color = Color.gray;
            labelText.alignment = TextAnchor.MiddleCenter;
            
            EquipmentSlotUI slot = slotObj.AddComponent<EquipmentSlotUI>();
            slot.Initialize(slotName, this);
            
            return slot;
        }
        
        private void CreateTooltipPanel()
        {
            _tooltipPanel = new GameObject("TooltipPanel");
            _tooltipPanel.transform.SetParent(_canvas.transform, false);
            
            RectTransform rect = _tooltipPanel.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0, 0);
            rect.anchorMax = new Vector2(0, 0);
            rect.pivot = new Vector2(0, 0);
            rect.sizeDelta = new Vector2(300, 200);
            
            Image bg = _tooltipPanel.AddComponent<Image>();
            bg.color = new Color(0.05f, 0.05f, 0.1f, 0.95f);
            
            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(_tooltipPanel.transform, false);
            
            RectTransform textRect = textObj.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = new Vector2(10, 10);
            textRect.offsetMax = new Vector2(-10, -10);
            
            _tooltipText = textObj.AddComponent<Text>();
            _tooltipText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            _tooltipText.fontSize = 14;
            _tooltipText.color = Color.white;
            _tooltipText.alignment = TextAnchor.UpperLeft;
            
            _tooltipPanel.SetActive(false);
        }
        
        #endregion
        
        #region UI Management
        
        public void ToggleInventory()
        {
            _isOpen = !_isOpen;
            _inventoryPanel.SetActive(_isOpen);
            _equipmentPanel.SetActive(_isOpen);
            
            if (_isOpen)
            {
                RefreshInventory();
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
            else
            {
                HideTooltip();
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
        }
        
        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.I))
            {
                ToggleInventory();
            }
            
            if (Input.GetKeyDown(KeyCode.Escape) && _isOpen)
            {
                ToggleInventory();
            }
        }
        
        #endregion
        
        #region Refresh
        
        private void OnInventoryChanged(GameObject entity)
        {
            if (entity == _player)
                RefreshInventory();
        }
        
        private void OnEquipmentChanged(GameObject entity)
        {
            if (entity == _player)
                RefreshEquipment();
        }
        
        private void RefreshInventory()
        {
            if (_inventoryManager == null || _player == null) return;
            
            Game.Core.Systems.InventoryData inventory = _inventoryManager.GetInventory(_player);
            
            // Clear all slots
            foreach (InventorySlotUI slot in _inventorySlots)
            {
                slot.ClearSlot();
            }
            
            // Fill slots with items
            for (int i = 0; i < inventory.Slots.Count && i < _inventorySlots.Count; i++)
            {
                _inventorySlots[i].SetItem(inventory.Slots[i].item, inventory.Slots[i].quantity);
            }
            
            RefreshEquipment();
        }
        
        private void RefreshEquipment()
        {
            if (_inventoryManager == null || _player == null) return;
            
            Game.Core.Systems.EquipmentData equipment = _inventoryManager.GetEquipment(_player);
            if (equipment == null) return;
            
            _weaponSlot.SetItem(equipment.equippedWeapon);
            _headSlot.SetItem(equipment.equippedHead);
            _chestSlot.SetItem(equipment.equippedChest);
            _handsSlot.SetItem(equipment.equippedHands);
            _legsSlot.SetItem(equipment.equippedLegs);
            _feetSlot.SetItem(equipment.equippedFeet);
        }
        
        #endregion
        
        #region Tooltip
        
        public void ShowTooltip(Game.Core.Systems.ItemData item, Vector2 position)
        {
            if (item == null)
            {
                HideTooltip();
                return;
            }
            
            _tooltipPanel.SetActive(true);
            
            Color rarityColor = GetRarityColor(item.rarity);
            string rarityHex = ColorUtility.ToHtmlStringRGB(rarityColor);
            
            string tooltip = $"<color=#{rarityHex}><b>{item.itemName}</b></color>\n";
            tooltip += $"<color=grey>{item.itemType} - Level {item.level}</color>\n\n";
            
            if (item.itemType == Game.Core.Systems.ItemType.Weapon)
            {
                tooltip += $"<color=orange>Damage:</color> {item.damage}\n";
                tooltip += $"<color=cyan>Attack Speed:</color> {item.attackSpeed:F2}\n";
                tooltip += $"<color=yellow>Range:</color> {item.range:F1}\n";
            }
            else if (item.itemType == Game.Core.Systems.ItemType.Armor)
            {
                tooltip += $"<color=orange>Armor:</color> {item.armorValue}\n";
            }
            
            if (item.affixes != null && item.affixes.Count > 0)
            {
                tooltip += "\n<b>Affixes:</b>\n";
                foreach (var affix in item.affixes)
                {
                    tooltip += $"<color=green>+ {affix.affixType}: {affix.value}</color>\n";
                }
            }
            
            _tooltipText.text = tooltip;
            
            RectTransform tooltipRect = _tooltipPanel.GetComponent<RectTransform>();
            tooltipRect.position = position + new Vector2(10, -10);
        }
        
        public void HideTooltip()
        {
            _tooltipPanel.SetActive(false);
        }
        
        private Color GetRarityColor(Game.Core.Systems.Rarity rarity)
        {
            return rarity switch
            {
                Game.Core.Systems.Rarity.Common => Color.white,
                Game.Core.Systems.Rarity.Uncommon => Color.green,
                Game.Core.Systems.Rarity.Rare => Color.blue,
                Game.Core.Systems.Rarity.Epic => new Color(0.6f, 0.3f, 1f),
                Game.Core.Systems.Rarity.Legendary => new Color(1f, 0.5f, 0f),
                Game.Core.Systems.Rarity.Mythic => new Color(1f, 0f, 0f),
                _ => Color.white
            };
        }
        
        #endregion
        
        #region Item Actions
        
        public void OnItemClicked(Game.Core.Systems.ItemData item)
        {
            if (item == null || _inventoryManager == null || _player == null) return;
            
            // Right click to equip
            if (Input.GetMouseButtonDown(1))
            {
                _inventoryManager.EquipItem(_player, item.itemId);
            }
        }
        
        public void OnEquipmentSlotClicked(Game.Core.Systems.ItemData item)
        {
            if (item == null || _inventoryManager == null || _player == null) return;
            
            // Right click to unequip
            if (Input.GetMouseButtonDown(1))
            {
                _inventoryManager.UnequipItem(_player, item.itemId);
            }
        }
        
        #endregion
    }
    
    #region Slot UI Components
    
    /// <summary>
    /// Individual inventory slot UI.
    /// </summary>
    public class InventorySlotUI : MonoBehaviour
    {
        private int _slotIndex;
        private Game.Core.Systems.ItemData _item;
        private int _quantity;
        private InventoryUI _inventoryUI;
        
        private Image _icon;
        private Text _quantityText;
        
        public void Initialize(int slotIndex, InventoryUI inventoryUI)
        {
            _slotIndex = slotIndex;
            _inventoryUI = inventoryUI;
            
            CreateSlotUI();
        }
        
        private void CreateSlotUI()
        {
            // Icon
            GameObject iconObj = new GameObject("Icon");
            iconObj.transform.SetParent(transform, false);
            
            RectTransform iconRect = iconObj.AddComponent<RectTransform>();
            iconRect.anchorMin = Vector2.zero;
            iconRect.anchorMax = Vector2.one;
            iconRect.offsetMin = new Vector2(5, 5);
            iconRect.offsetMax = new Vector2(-5, -5);
            
            _icon = iconObj.AddComponent<Image>();
            _icon.color = Color.clear;
            
            // Quantity text
            GameObject quantityObj = new GameObject("Quantity");
            quantityObj.transform.SetParent(transform, false);
            
            RectTransform quantityRect = quantityObj.AddComponent<RectTransform>();
            quantityRect.anchorMin = new Vector2(1, 0);
            quantityRect.anchorMax = new Vector2(1, 0);
            quantityRect.pivot = new Vector2(1, 0);
            quantityRect.sizeDelta = new Vector2(30, 20);
            
            _quantityText = quantityObj.AddComponent<Text>();
            _quantityText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            _quantityText.fontSize = 14;
            _quantityText.color = Color.white;
            _quantityText.alignment = TextAnchor.LowerRight;
            
            // Button for click handling
            Button btn = gameObject.AddComponent<Button>();
            btn.targetGraphic = GetComponent<Image>();
            btn.onClick.AddListener(OnClicked);
        }
        
        public void SetItem(Game.Core.Systems.ItemData item, int quantity)
        {
            _item = item;
            _quantity = quantity;
            
            if (item != null)
            {
                _icon.color = GetRarityColor(item.rarity);
                _quantityText.text = quantity > 1 ? quantity.ToString() : "";
            }
            else
            {
                ClearSlot();
            }
        }
        
        public void ClearSlot()
        {
            _item = null;
            _quantity = 0;
            _icon.color = Color.clear;
            _quantityText.text = "";
        }
        
        private void OnClicked()
        {
            if (_item != null)
            {
                _inventoryUI.OnItemClicked(_item);
            }
        }
        
        private void OnPointerEnter()
        {
            if (_item != null)
            {
                _inventoryUI.ShowTooltip(_item, Input.mousePosition);
            }
        }
        
        private void OnPointerExit()
        {
            _inventoryUI.HideTooltip();
        }
        
        private Color GetRarityColor(Game.Core.Systems.Rarity rarity)
        {
            return rarity switch
            {
                Game.Core.Systems.Rarity.Common => new Color(0.6f, 0.6f, 0.6f),
                Game.Core.Systems.Rarity.Uncommon => new Color(0.2f, 0.8f, 0.2f),
                Game.Core.Systems.Rarity.Rare => new Color(0.2f, 0.4f, 1f),
                Game.Core.Systems.Rarity.Epic => new Color(0.6f, 0.3f, 1f),
                Game.Core.Systems.Rarity.Legendary => new Color(1f, 0.6f, 0f),
                Game.Core.Systems.Rarity.Mythic => new Color(1f, 0.2f, 0.2f),
                _ => Color.gray
            };
        }
    }
    
    /// <summary>
    /// Equipment slot UI.
    /// </summary>
    public class EquipmentSlotUI : MonoBehaviour
    {
        private string _slotName;
        private Game.Core.Systems.ItemData _item;
        private InventoryUI _inventoryUI;
        
        private Image _icon;
        
        public void Initialize(string slotName, InventoryUI inventoryUI)
        {
            _slotName = slotName;
            _inventoryUI = inventoryUI;
            
            CreateSlotUI();
        }
        
        private void CreateSlotUI()
        {
            // Icon
            GameObject iconObj = new GameObject("Icon");
            iconObj.transform.SetParent(transform, false);
            
            RectTransform iconRect = iconObj.AddComponent<RectTransform>();
            iconRect.anchorMin = Vector2.zero;
            iconRect.anchorMax = Vector2.one;
            iconRect.offsetMin = new Vector2(10, 20);
            iconRect.offsetMax = new Vector2(-10, -10);
            
            _icon = iconObj.AddComponent<Image>();
            _icon.color = Color.clear;
            
            // Button
            Button btn = gameObject.AddComponent<Button>();
            btn.targetGraphic = GetComponent<Image>();
            btn.onClick.AddListener(OnClicked);
        }
        
        public void SetItem(Game.Core.Systems.ItemData item)
        {
            _item = item;
            
            if (item != null)
            {
                _icon.color = GetRarityColor(item.rarity);
            }
            else
            {
                _icon.color = Color.clear;
            }
        }
        
        private void OnClicked()
        {
            if (_item != null)
            {
                _inventoryUI.OnEquipmentSlotClicked(_item);
            }
        }
        
        private Color GetRarityColor(Game.Core.Systems.Rarity rarity)
        {
            return rarity switch
            {
                Game.Core.Systems.Rarity.Common => new Color(0.6f, 0.6f, 0.6f),
                Game.Core.Systems.Rarity.Uncommon => new Color(0.2f, 0.8f, 0.2f),
                Game.Core.Systems.Rarity.Rare => new Color(0.2f, 0.4f, 1f),
                Game.Core.Systems.Rarity.Epic => new Color(0.6f, 0.3f, 1f),
                Game.Core.Systems.Rarity.Legendary => new Color(1f, 0.6f, 0f),
                Game.Core.Systems.Rarity.Mythic => new Color(1f, 0.2f, 0.2f),
                _ => Color.gray
            };
        }
    }
    
    #endregion
}