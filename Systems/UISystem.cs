using UnityEngine;
using UnityEngine.UI;

namespace Game.Core.Systems
{
    /// <summary>
    /// Central UI management system.
    /// Manages HUD (Doom-style) and menus (D&D-style).
    /// </summary>
    public partial class UISystemManager : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private Canvas _hudCanvas;
        [SerializeField] private Canvas _menuCanvas;
        
        [Header("HUD Elements")]
        [SerializeField] private Slider _healthBar;
        [SerializeField] private Slider _manaBar;
        [SerializeField] private Slider _staminaBar;
        [SerializeField] private Text _healthText;
        [SerializeField] private Text _manaText;
        [SerializeField] private Text _staminaText;
        
        [Header("Menu Panels")]
        [SerializeField] private GameObject _inventoryPanel;
        [SerializeField] private GameObject _characterPanel;
        [SerializeField] private GameObject _spellbookPanel;
        
        private EntityStats _playerStats;
        
        private void Awake()
        {
            InitializeUI();
        }
        
        private void Update()
        {
            UpdateHUD();
            HandleMenuInput();
        }
        
        private void InitializeUI()
        {
            CreateHUDCanvas();
            CreateMenuCanvas();
        }
        
        private void CreateHUDCanvas()
        {
            if (_hudCanvas != null) return;
            
            GameObject hudObj = new("HUDCanvas");
            hudObj.transform.SetParent(transform);
            
            _hudCanvas = hudObj.AddComponent<Canvas>();
            _hudCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            _hudCanvas.sortingOrder = 10;
            
            CanvasScaler scaler = hudObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            
            hudObj.AddComponent<GraphicRaycaster>();
            
            CreateHealthBar();
            CreateManaBar();
            CreateStaminaBar();
        }
        
        private void CreateHealthBar()
        {
            GameObject barObj = new("HealthBar");
            barObj.transform.SetParent(_hudCanvas.transform, false);
            
            RectTransform rect = barObj.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0f);
            rect.anchorMax = new Vector2(0.5f, 0f);
            rect.pivot = new Vector2(0.5f, 0f);
            rect.anchoredPosition = new Vector2(-200, 50);
            rect.sizeDelta = new Vector2(150, 30);
            
            _healthBar = barObj.AddComponent<Slider>();
            _healthBar.minValue = 0;
            _healthBar.maxValue = 1;
            _healthBar.value = 1;
            
            // Background
            GameObject bg = new("Background");
            bg.transform.SetParent(barObj.transform, false);
            Image bgImg = bg.AddComponent<Image>();
            bgImg.color = new Color(0.2f, 0.2f, 0.2f, 0.8f);
            RectTransform bgRect = bg.GetComponent<RectTransform>();
            bgRect.anchorMin = Vector2.zero;
            bgRect.anchorMax = Vector2.one;
            bgRect.sizeDelta = Vector2.zero;
            
            // Fill
            GameObject fill = new("Fill");
            fill.transform.SetParent(barObj.transform, false);
            Image fillImg = fill.AddComponent<Image>();
            fillImg.color = Color.red;
            _healthBar.fillRect = fill.GetComponent<RectTransform>();
            _healthBar.fillRect.anchorMin = Vector2.zero;
            _healthBar.fillRect.anchorMax = Vector2.one;
            _healthBar.fillRect.sizeDelta = Vector2.zero;
            
            // Text
            GameObject textObj = new("Text");
            textObj.transform.SetParent(barObj.transform, false);
            _healthText = textObj.AddComponent<Text>();
            _healthText.text = "1000/1000";
            _healthText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            _healthText.fontSize = 18;
            _healthText.color = Color.white;
            _healthText.alignment = TextAnchor.MiddleCenter;
            RectTransform textRect = textObj.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.sizeDelta = Vector2.zero;
        }
        
        private void CreateManaBar()
        {
            GameObject barObj = new("ManaBar");
            barObj.transform.SetParent(_hudCanvas.transform, false);
            
            RectTransform rect = barObj.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0f);
            rect.anchorMax = new Vector2(0.5f, 0f);
            rect.pivot = new Vector2(0.5f, 0f);
            rect.anchoredPosition = new Vector2(0, 50);
            rect.sizeDelta = new Vector2(150, 30);
            
            _manaBar = barObj.AddComponent<Slider>();
            _manaBar.minValue = 0;
            _manaBar.maxValue = 1;
            _manaBar.value = 1;
            
            GameObject bg = new("Background");
            bg.transform.SetParent(barObj.transform, false);
            Image bgImg = bg.AddComponent<Image>();
            bgImg.color = new Color(0.2f, 0.2f, 0.2f, 0.8f);
            RectTransform bgRect = bg.GetComponent<RectTransform>();
            bgRect.anchorMin = Vector2.zero;
            bgRect.anchorMax = Vector2.one;
            bgRect.sizeDelta = Vector2.zero;
            
            GameObject fill = new("Fill");
            fill.transform.SetParent(barObj.transform, false);
            Image fillImg = fill.AddComponent<Image>();
            fillImg.color = Color.blue;
            _manaBar.fillRect = fill.GetComponent<RectTransform>();
            _manaBar.fillRect.anchorMin = Vector2.zero;
            _manaBar.fillRect.anchorMax = Vector2.one;
            _manaBar.fillRect.sizeDelta = Vector2.zero;
            
            GameObject textObj = new("Text");
            textObj.transform.SetParent(barObj.transform, false);
            _manaText = textObj.AddComponent<Text>();
            _manaText.text = "500/500";
            _manaText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            _manaText.fontSize = 18;
            _manaText.color = Color.white;
            _manaText.alignment = TextAnchor.MiddleCenter;
            RectTransform textRect = textObj.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.sizeDelta = Vector2.zero;
        }
        
        private void CreateStaminaBar()
        {
            GameObject barObj = new("StaminaBar");
            barObj.transform.SetParent(_hudCanvas.transform, false);
            
            RectTransform rect = barObj.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0f);
            rect.anchorMax = new Vector2(0.5f, 0f);
            rect.pivot = new Vector2(0.5f, 0f);
            rect.anchoredPosition = new Vector2(200, 50);
            rect.sizeDelta = new Vector2(150, 30);
            
            _staminaBar = barObj.AddComponent<Slider>();
            _staminaBar.minValue = 0;
            _staminaBar.maxValue = 1;
            _staminaBar.value = 1;
            
            GameObject bg = new("Background");
            bg.transform.SetParent(barObj.transform, false);
            Image bgImg = bg.AddComponent<Image>();
            bgImg.color = new Color(0.2f, 0.2f, 0.2f, 0.8f);
            RectTransform bgRect = bg.GetComponent<RectTransform>();
            bgRect.anchorMin = Vector2.zero;
            bgRect.anchorMax = Vector2.one;
            bgRect.sizeDelta = Vector2.zero;
            
            GameObject fill = new("Fill");
            fill.transform.SetParent(barObj.transform, false);
            Image fillImg = fill.AddComponent<Image>();
            fillImg.color = Color.green;
            _staminaBar.fillRect = fill.GetComponent<RectTransform>();
            _staminaBar.fillRect.anchorMin = Vector2.zero;
            _staminaBar.fillRect.anchorMax = Vector2.one;
            _staminaBar.fillRect.sizeDelta = Vector2.zero;
            
            GameObject textObj = new("Text");
            textObj.transform.SetParent(barObj.transform, false);
            _staminaText = textObj.AddComponent<Text>();
            _staminaText.text = "500/500";
            _staminaText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            _staminaText.fontSize = 18;
            _staminaText.color = Color.white;
            _staminaText.alignment = TextAnchor.MiddleCenter;
            RectTransform textRect = textObj.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.sizeDelta = Vector2.zero;
        }
        
        private void CreateMenuCanvas()
        {
            if (_menuCanvas != null) return;
            
            GameObject menuObj = new("MenuCanvas");
            menuObj.transform.SetParent(transform);
            
            _menuCanvas = menuObj.AddComponent<Canvas>();
            _menuCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            _menuCanvas.sortingOrder = 20;
            
            menuObj.SetActive(false); // Hidden by default
        }
        
        private void UpdateHUD()
        {
            if (_playerStats == null)
            {
                _playerStats = FindFirstObjectByType<EntityStats>();
            }
            
            if (_playerStats != null)
            {
                _healthBar.value = (float)_playerStats.currentHealth / _playerStats.maxHealth;
                _healthText.text = $"{_playerStats.currentHealth}/{_playerStats.maxHealth}";
                
                _manaBar.value = (float)_playerStats.currentMana / _playerStats.maxMana;
                _manaText.text = $"{_playerStats.currentMana}/{_playerStats.maxMana}";
                
                _staminaBar.value = (float)_playerStats.currentStamina / _playerStats.maxStamina;
                _staminaText.text = $"{_playerStats.currentStamina}/{_playerStats.maxStamina}";
            }
        }
        
        private void HandleMenuInput()
        {
            if (Input.GetKeyDown(KeyCode.I))
            {
                ToggleInventory();
            }
            
            if (Input.GetKeyDown(KeyCode.C))
            {
                ToggleCharacterSheet();
            }
            
            if (Input.GetKeyDown(KeyCode.K))
            {
                ToggleSpellbook();
            }
        }
        
        public void ToggleInventory()
        {
            if (_inventoryPanel != null)
            {
                _inventoryPanel.SetActive(!_inventoryPanel.activeSelf);
            }
        }
        
        public void ToggleCharacterSheet()
        {
            if (_characterPanel != null)
            {
                _characterPanel.SetActive(!_characterPanel.activeSelf);
            }
        }
        
        public void ToggleSpellbook()
        {
            if (_spellbookPanel != null)
            {
                _spellbookPanel.SetActive(!_spellbookPanel.activeSelf);
            }
        }
        
        public void Shutdown() { }
    }
}