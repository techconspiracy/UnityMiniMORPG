using UnityEngine;
using System.Collections.Generic;

namespace Game.Core.Systems
{
    /// <summary>
    /// Admin console manager for in-game editing and debugging.
    /// F12 toggles console (server host/admin only).
    /// Provides UI for editing weapons, armor, spells, pools, entities, zones.
    /// </summary>
    public class AdminConsoleManager : MonoBehaviour
    {
        [Header("Console Configuration")]
        [SerializeField, Tooltip("Key to toggle admin console")]
        private KeyCode _toggleKey = KeyCode.F12;
        
        [SerializeField, Tooltip("Only allow admin access for server host")]
        private bool _hostOnly = true;
        
        [Header("Console State")]
        [SerializeField] private bool _isConsoleOpen;
        [SerializeField] private bool _isAdminAuthorized;
        [SerializeField] private AdminTab _currentTab = AdminTab.PoolDatabase;
        
        [Header("Runtime References (Auto-Assigned)")]
        [SerializeField] private Canvas _consoleCanvas;
        [SerializeField] private GameObject _consolePanel;
        
        // Tab controllers (to be implemented)
        private PoolDatabaseController _poolDatabaseController;
        private WeaponEditorController _weaponEditorController;
        private ArmorEditorController _armorEditorController;
        private SpellEditorController _spellEditorController;
        private EntityInspectorController _entityInspectorController;
        private ZoneEditorController _zoneEditorController;
        private PlayerManagementController _playerManagementController;
        
        private Dictionary<AdminTab, GameObject> _tabPanels;
        
        #region Unity Lifecycle
        
        private void Awake()
        {
            InitializeConsole();
            CheckAdminAuthorization();
        }
        
        private void Update()
        {
            // Toggle console with F12
            if (Input.GetKeyDown(_toggleKey))
            {
                if (_isAdminAuthorized)
                {
                    ToggleConsole();
                }
                else
                {
                    Debug.LogWarning("[AdminConsole] Admin access denied. Host only.");
                }
            }
            
            // Tab switching (1-7 keys when console open)
            if (_isConsoleOpen)
            {
                HandleTabSwitching();
            }
        }
        
        #endregion
        
        #region Initialization
        
        private void InitializeConsole()
        {
            CreateConsoleUI();
            _tabPanels = new Dictionary<AdminTab, GameObject>();
            _isConsoleOpen = false;
        }
        
        private void CheckAdminAuthorization()
        {
            // Check if player is server host
            if (_hostOnly)
            {
                WebSocketNetworkManager netManager = CoreSystemManager.NetworkManager;
                _isAdminAuthorized = netManager != null && netManager.IsHost();
            }
            else
            {
                // Allow admin access in single-player
                _isAdminAuthorized = true;
            }
            
            Debug.Log($"[AdminConsole] Admin authorized: {_isAdminAuthorized}");
        }
        
        private void CreateConsoleUI()
        {
            // Create canvas for admin console
            GameObject canvasObj = new("AdminConsoleCanvas");
            canvasObj.transform.SetParent(transform);
            
            _consoleCanvas = canvasObj.AddComponent<Canvas>();
            _consoleCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            _consoleCanvas.sortingOrder = 1000; // Above game UI
            
            UnityEngine.UI.CanvasScaler scaler = canvasObj.AddComponent<UnityEngine.UI.CanvasScaler>();
            scaler.uiScaleMode = UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            
            canvasObj.AddComponent<UnityEngine.UI.GraphicRaycaster>();
            
            // Create main console panel
            CreateConsolePanel();
            
            // Hide by default
            _consolePanel.SetActive(false);
        }
        
        private void CreateConsolePanel()
        {
            _consolePanel = new GameObject("ConsolePanel");
            _consolePanel.transform.SetParent(_consoleCanvas.transform, false);
            
            // Background
            UnityEngine.UI.Image bg = _consolePanel.AddComponent<UnityEngine.UI.Image>();
            bg.color = new Color(0.1f, 0.1f, 0.1f, 0.95f); // Dark semi-transparent
            
            RectTransform rect = _consolePanel.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.1f, 0.1f);
            rect.anchorMax = new Vector2(0.9f, 0.9f);
            rect.sizeDelta = Vector2.zero;
            
            // Title bar
            CreateTitleBar();
            
            // Tab buttons
            CreateTabButtons();
            
            // Content area (where tab-specific content goes)
            CreateContentArea();
        }
        
        private void CreateTitleBar()
        {
            GameObject titleBar = new("TitleBar");
            titleBar.transform.SetParent(_consolePanel.transform, false);
            
            UnityEngine.UI.Image bg = titleBar.AddComponent<UnityEngine.UI.Image>();
            bg.color = new Color(0.2f, 0.2f, 0.2f, 1f);
            
            RectTransform rect = titleBar.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0, 1);
            rect.anchorMax = new Vector2(1, 1);
            rect.pivot = new Vector2(0.5f, 1);
            rect.sizeDelta = new Vector2(0, 50);
            
            // Title text
            GameObject textObj = new("TitleText");
            textObj.transform.SetParent(titleBar.transform, false);
            
            UnityEngine.UI.Text text = textObj.AddComponent<UnityEngine.UI.Text>();
            text.text = "ADMIN CONSOLE - F12 to Close";
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = 24;
            text.color = Color.cyan;
            text.alignment = TextAnchor.MiddleCenter;
            
            RectTransform textRect = textObj.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.sizeDelta = Vector2.zero;
        }
        
        private void CreateTabButtons()
        {
            GameObject tabBar = new("TabBar");
            tabBar.transform.SetParent(_consolePanel.transform, false);
            
            UnityEngine.UI.Image bg = tabBar.AddComponent<UnityEngine.UI.Image>();
            bg.color = new Color(0.15f, 0.15f, 0.15f, 1f);
            
            RectTransform rect = tabBar.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0, 1);
            rect.anchorMax = new Vector2(1, 1);
            rect.pivot = new Vector2(0.5f, 1);
            rect.anchoredPosition = new Vector2(0, -50);
            rect.sizeDelta = new Vector2(0, 40);
            
            // Add horizontal layout
            UnityEngine.UI.HorizontalLayoutGroup layout = tabBar.AddComponent<UnityEngine.UI.HorizontalLayoutGroup>();
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = true;
            layout.spacing = 5;
            layout.padding = new RectOffset(5, 5, 5, 5);
            
            // Create tab buttons
            string[] tabNames = System.Enum.GetNames(typeof(AdminTab));
            for (int i = 0; i < tabNames.Length; i++)
            {
                CreateTabButton(tabBar.transform, tabNames[i], (AdminTab)i);
            }
        }
        
        private void CreateTabButton(Transform parent, string label, AdminTab tab)
        {
            GameObject button = new($"Tab_{label}");
            button.transform.SetParent(parent, false);
            
            UnityEngine.UI.Button btn = button.AddComponent<UnityEngine.UI.Button>();
            UnityEngine.UI.Image btnImg = button.AddComponent<UnityEngine.UI.Image>();
            btnImg.color = new Color(0.3f, 0.3f, 0.3f, 1f);
            
            // Button text
            GameObject textObj = new("Text");
            textObj.transform.SetParent(button.transform, false);
            
            UnityEngine.UI.Text text = textObj.AddComponent<UnityEngine.UI.Text>();
            text.text = $"{label}\n(Key {(int)tab + 1})";
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = 16;
            text.color = Color.white;
            text.alignment = TextAnchor.MiddleCenter;
            
            RectTransform textRect = textObj.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.sizeDelta = Vector2.zero;
            
            // Button click handler
            btn.onClick.AddListener(() => SwitchToTab(tab));
        }
        
        private void CreateContentArea()
        {
            GameObject contentArea = new("ContentArea");
            contentArea.transform.SetParent(_consolePanel.transform, false);
            
            RectTransform rect = contentArea.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0, 0);
            rect.anchorMax = new Vector2(1, 1);
            rect.offsetMin = new Vector2(10, 10); // Left, Bottom
            rect.offsetMax = new Vector2(-10, -100); // Right, Top (account for title+tabs)
            
            // Create placeholder tab panels
            CreateTabPanels(contentArea.transform);
        }
        
        private void CreateTabPanels(Transform parent)
        {
            // Create a panel for each tab (to be populated by specific controllers)
            foreach (AdminTab tab in System.Enum.GetValues(typeof(AdminTab)))
            {
                GameObject panel = new($"Panel_{tab}");
                panel.transform.SetParent(parent, false);
                
                RectTransform rect = panel.GetComponent<RectTransform>();
                rect.anchorMin = Vector2.zero;
                rect.anchorMax = Vector2.one;
                rect.sizeDelta = Vector2.zero;
                
                // Add scroll view for content
                UnityEngine.UI.ScrollRect scroll = panel.AddComponent<UnityEngine.UI.ScrollRect>();
                UnityEngine.UI.Image scrollBg = panel.AddComponent<UnityEngine.UI.Image>();
                scrollBg.color = new Color(0.05f, 0.05f, 0.05f, 1f);
                
                // Content container
                GameObject content = new("Content");
                content.transform.SetParent(panel.transform, false);
                
                RectTransform contentRect = content.GetComponent<RectTransform>();
                contentRect.anchorMin = new Vector2(0, 1);
                contentRect.anchorMax = new Vector2(1, 1);
                contentRect.pivot = new Vector2(0.5f, 1);
                contentRect.sizeDelta = new Vector2(0, 1000); // Will expand with content
                
                scroll.content = contentRect;
                scroll.horizontal = false;
                scroll.vertical = true;
                
                // Add placeholder text
                CreatePlaceholderText(content.transform, tab);
                
                _tabPanels[tab] = panel;
                panel.SetActive(tab == _currentTab); // Only show current tab
            }
        }
        
        private void CreatePlaceholderText(Transform parent, AdminTab tab)
        {
            GameObject textObj = new("PlaceholderText");
            textObj.transform.SetParent(parent, false);
            
            UnityEngine.UI.Text text = textObj.AddComponent<UnityEngine.UI.Text>();
            text.text = $"{tab} panel\n\n(Content will be populated by specific controller)";
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = 20;
            text.color = Color.gray;
            text.alignment = TextAnchor.UpperLeft;
            
            RectTransform rect = textObj.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0, 1);
            rect.anchorMax = new Vector2(1, 1);
            rect.pivot = new Vector2(0, 1);
            rect.sizeDelta = new Vector2(-20, 100);
            rect.anchoredPosition = new Vector2(10, -10);
        }
        
        #endregion
        
        #region Console Control
        
        public void ToggleConsole()
        {
            _isConsoleOpen = !_isConsoleOpen;
            _consolePanel.SetActive(_isConsoleOpen);
            
            // Pause game when console open (optional)
            Time.timeScale = _isConsoleOpen ? 0f : 1f;
            
            // Lock/unlock cursor
            Cursor.visible = _isConsoleOpen;
            Cursor.lockState = _isConsoleOpen ? CursorLockMode.None : CursorLockMode.Locked;
            
            Debug.Log($"[AdminConsole] Console {(_isConsoleOpen ? "opened" : "closed")}");
        }
        
        private void SwitchToTab(AdminTab tab)
        {
            _currentTab = tab;
            
            // Hide all tab panels
            foreach (KeyValuePair<AdminTab, GameObject> kvp in _tabPanels)
            {
                kvp.Value.SetActive(kvp.Key == tab);
            }
            
            Debug.Log($"[AdminConsole] Switched to tab: {tab}");
        }
        
        private void HandleTabSwitching()
        {
            // Keys 1-7 switch tabs
            if (Input.GetKeyDown(KeyCode.Alpha1)) SwitchToTab(AdminTab.PoolDatabase);
            if (Input.GetKeyDown(KeyCode.Alpha2)) SwitchToTab(AdminTab.WeaponEditor);
            if (Input.GetKeyDown(KeyCode.Alpha3)) SwitchToTab(AdminTab.ArmorEditor);
            if (Input.GetKeyDown(KeyCode.Alpha4)) SwitchToTab(AdminTab.SpellEditor);
            if (Input.GetKeyDown(KeyCode.Alpha5)) SwitchToTab(AdminTab.EntityInspector);
            if (Input.GetKeyDown(KeyCode.Alpha6)) SwitchToTab(AdminTab.ZoneEditor);
            if (Input.GetKeyDown(KeyCode.Alpha7)) SwitchToTab(AdminTab.PlayerManagement);
        }
        
        #endregion
        
        #region Public API
        
        public bool IsConsoleOpen() => _isConsoleOpen;
        public bool IsAdminAuthorized() => _isAdminAuthorized;
        
        public void Shutdown()
        {
            if (_isConsoleOpen)
            {
                ToggleConsole(); // Close and restore time scale
            }
        }
        
        #endregion
        
        #region Nested Types
        
        public enum AdminTab
        {
            PoolDatabase = 0,
            WeaponEditor = 1,
            ArmorEditor = 2,
            SpellEditor = 3,
            EntityInspector = 4,
            ZoneEditor = 5,
            PlayerManagement = 6
        }
        
        #endregion
    }
    
    #region Placeholder Controllers (To be implemented)
    
    public class EntityInspectorController { }
    public class ZoneEditorController { }
    public class PlayerManagementController { }
    
    #endregion
}