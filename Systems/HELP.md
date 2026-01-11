###########
using UnityEngine;

namespace Game.Core.Pooling
{
    /// <summary>
    /// Interface for all objects that can be pooled.
    /// Ensures consistent lifecycle management across all pooled types.
    /// </summary>
    public interface IPoolable
    {
        /// <summary>
        /// Called when object is retrieved from pool.
        /// Use this instead of Awake/Start for pooled objects.
        /// </summary>
        void OnSpawnFromPool();
        
        /// <summary>
        /// Called when object is returned to pool.
        /// Reset all state here to avoid cross-contamination.
        /// </summary>
        void OnReturnToPool();
        
        /// <summary>
        /// The GameObject this poolable is attached to.
        /// Required for transform operations and SetActive calls.
        /// </summary>
        GameObject GameObject { get; }
        
        /// <summary>
        /// Unique identifier for this poolable type.
        /// Used for pool organization and debugging.
        /// </summary>
        string PoolKey { get; }
        
        /// <summary>
        /// Whether this object is currently active in the scene.
        /// Used to prevent double-returns to pool.
        /// </summary>
        bool IsActiveInPool { get; set; }
    }
    
    /// <summary>
    /// Base MonoBehaviour implementation of IPoolable.
    /// Inherit from this for quick poolable object creation.
    /// </summary>
    public abstract class PoolableObject : MonoBehaviour, IPoolable
    {
        [Header("Pooling")]
        [SerializeField, Tooltip("Unique key for this poolable type")]
        private string _poolKey = "DefaultPool";
        
        private bool _isActiveInPool;
        
        public GameObject GameObject => gameObject;
        public string PoolKey => _poolKey;
        public bool IsActiveInPool 
        { 
            get => _isActiveInPool; 
            set => _isActiveInPool = value; 
        }
        
        /// <summary>
        /// Override this to handle spawn logic.
        /// Called when retrieved from pool.
        /// </summary>
        public virtual void OnSpawnFromPool()
        {
            _isActiveInPool = true;
            gameObject.SetActive(true);
        }
        
        /// <summary>
        /// Override this to handle return logic.
        /// Called when returned to pool.
        /// IMPORTANT: Reset all state here!
        /// </summary>
        public virtual void OnReturnToPool()
        {
            _isActiveInPool = false;
            gameObject.SetActive(false);
        }
        
        /// <summary>
        /// Helper method to return this object to pool.
        /// Call instead of Destroy() for pooled objects.
        /// </summary>
        protected void ReturnToPool()
        {
            Game.Core.Systems.ObjectPoolManager poolManager = Game.Core.Systems.CoreSystemManager.PoolManager;
            if (poolManager != null)
            {
                poolManager.ReturnToPool(this);
            }
            else
            {
                Debug.LogError($"Cannot return {gameObject.name} to pool - ObjectPoolManager not found!");
            }
        }
    }
}
###########
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
    
    public class PoolDatabaseController { }
    public class WeaponEditorController { }
    public class ArmorEditorController { }
    public class SpellEditorController { }
    public class EntityInspectorController { }
    public class ZoneEditorController { }
    public class PlayerManagementController { }
    
    #endregion
}
###########
using UnityEngine;
using System.Collections.Generic;

namespace Game.Core.Systems
{
    /// <summary>
    /// Complete combat system with damage calculation, abilities, and magic.
    /// RPG-tanky combat with fun early game pacing.
    /// </summary>
    public partial class CombatSystemManager : MonoBehaviour
    {
        [Header("Combat Settings")]
        [SerializeField] private float _globalDamageMultiplier = 1.0f;
        [SerializeField] private float _criticalHitMultiplier = 2.0f;
        
        public void Shutdown() { }
        
        /// <summary>
        /// Calculate final damage with all modifiers.
        /// </summary>
        public DamageResult CalculateDamage(EntityStats attacker, EntityStats defender, int baseDamage, DamageType damageType)
        {
            DamageResult result = new();
            
            // Get relevant stat for damage type
            float attackerStat = damageType switch
            {
                DamageType.Physical => attacker.strength,
                DamageType.Ranged => attacker.dexterity,
                DamageType.Magic => attacker.intelligence,
                _ => 0
            };
            
            // Base damage calculation
            float damage = baseDamage * (1 + attackerStat / 100f);
            
            // Apply global multiplier
            damage *= _globalDamageMultiplier;
            
            // Check for critical hit
            float critRoll = Random.Range(0f, 1f);
            if (critRoll <= attacker.criticalChance)
            {
                damage *= _criticalHitMultiplier;
                result.isCritical = true;
            }
            
            // Apply elemental multiplier
            damage *= GetElementalMultiplier(attacker.damageElement, defender.resistances);
            
            // Apply armor reduction (diminishing returns formula)
            float armorReduction = defender.armor / (defender.armor + 100f);
            damage *= (1f - armorReduction);
            
            // Ensure minimum damage
            damage = Mathf.Max(1, damage);
            
            result.finalDamage = Mathf.RoundToInt(damage);
            result.damageType = damageType;
            result.wasBlocked = false; // TODO: Implement blocking
            
            return result;
        }
        
        private float GetElementalMultiplier(DamageElement element, EntityResistances resistances)
        {
            return element switch
            {
                DamageElement.Fire => 1f - resistances.fireResistance,
                DamageElement.Ice => 1f - resistances.iceResistance,
                DamageElement.Lightning => 1f - resistances.lightningResistance,
                DamageElement.Poison => 1f - resistances.poisonResistance,
                DamageElement.Holy => 1f - resistances.holyResistance,
                DamageElement.Dark => 1f - resistances.darkResistance,
                _ => 1f
            };
        }
        
        /// <summary>
        /// Apply damage to an entity and trigger effects.
        /// </summary>
        public void ApplyDamage(GameObject target, DamageResult damage, GameObject attacker)
        {
            EntityStats targetStats = target.GetComponent<EntityStats>();
            if (targetStats == null) return;
            
            // Apply damage
            targetStats.currentHealth -= damage.finalDamage;
            targetStats.currentHealth = Mathf.Max(0, targetStats.currentHealth);
            
            // Trigger damage events
            OnDamageDealt?.Invoke(attacker, target, damage);
            
            // Check for death
            if (targetStats.currentHealth <= 0)
            {
                OnEntityDeath?.Invoke(target, attacker);
            }
            
            // Spawn damage number (pooled)
            SpawnDamageNumber(target.transform.position, damage);
        }
        
        private void SpawnDamageNumber(Vector3 position, DamageResult damage)
        {
            ObjectPoolManager poolManager = CoreSystemManager.PoolManager;
            if (poolManager == null) return;
            
            DamageNumber damageNum = poolManager.Get<DamageNumber>("DamageNumber", position, Quaternion.identity);
            if (damageNum != null)
            {
                damageNum.Initialize(damage.finalDamage, damage.isCritical, damage.damageType);
            }
        }
        
        // Events
        public System.Action<GameObject, GameObject, DamageResult> OnDamageDealt;
        public System.Action<GameObject, GameObject> OnEntityDeath;
    }
    
    /// <summary>
    /// Entity stats component for all combat entities.
    /// </summary>
    [System.Serializable]
    public class EntityStats : MonoBehaviour
    {
        [Header("Primary Stats")]
        public int strength = 10;
        public int dexterity = 10;
        public int intelligence = 10;
        public int vitality = 10;
        public int endurance = 10;
        public int luck = 10;
        
        [Header("Combat Stats")]
        public int currentHealth = 100;
        public int maxHealth = 100;
        public int currentMana = 50;
        public int maxMana = 50;
        public int currentStamina = 100;
        public int maxStamina = 100;
        
        [Header("Defense")]
        public int armor = 0;
        public EntityResistances resistances;
        
        [Header("Offense")]
        public DamageElement damageElement = DamageElement.None;
        public float criticalChance = 0.05f;
        
        [Header("Level")]
        public int level = 1;
        public int experience = 0;
        public int experienceToNextLevel = 100;
        
        private void Start()
        {
            RecalculateStats();
        }
        
        public void RecalculateStats()
        {
            maxHealth = 100 + (vitality * 10);
            maxMana = 100 + (intelligence * 10);
            maxStamina = 100 + (endurance * 10);
            
            currentHealth = Mathf.Min(currentHealth, maxHealth);
            currentMana = Mathf.Min(currentMana, maxMana);
            currentStamina = Mathf.Min(currentStamina, maxStamina);
        }
        
        public void AddExperience(int amount)
        {
            experience += amount;
            
            while (experience >= experienceToNextLevel)
            {
                LevelUp();
            }
        }
        
        private void LevelUp()
        {
            level++;
            experience -= experienceToNextLevel;
            experienceToNextLevel = Mathf.RoundToInt(experienceToNextLevel * 1.2f);
            
            // Grant attribute points
            strength += 2;
            dexterity += 2;
            intelligence += 2;
            vitality += 2;
            endurance += 2;
            luck += 1;
            
            RecalculateStats();
            
            Debug.Log($"[EntityStats] Level up! Now level {level}");
        }
    }
    
    [System.Serializable]
    public struct EntityResistances
    {
        public float fireResistance;
        public float iceResistance;
        public float lightningResistance;
        public float poisonResistance;
        public float holyResistance;
        public float darkResistance;
    }
    
    public struct DamageResult
    {
        public int finalDamage;
        public DamageType damageType;
        public bool isCritical;
        public bool wasBlocked;
    }
    
    public enum DamageType { Physical, Ranged, Magic }
    public enum DamageElement { None, Fire, Ice, Lightning, Poison, Holy, Dark }
    
    /// <summary>
    /// Poolable damage number display.
    /// </summary>
    public class DamageNumber : Game.Core.Pooling.PoolableObject
    {
        [SerializeField] private TextMesh _textMesh;
        [SerializeField] private float _floatSpeed = 2f;
        [SerializeField] private float _lifetime = 1.5f;
        
        private float _spawnTime;
        
        private void Awake()
        {
            if (_textMesh == null)
            {
                _textMesh = GetComponentInChildren<TextMesh>();
                if (_textMesh == null)
                {
                    GameObject textObj = new("Text");
                    textObj.transform.SetParent(transform);
                    _textMesh = textObj.AddComponent<TextMesh>();
                    _textMesh.fontSize = 32;
                    _textMesh.anchor = TextAnchor.MiddleCenter;
                }
            }
        }
        
        public void Initialize(int damage, bool isCrit, DamageType type)
        {
            _textMesh.text = isCrit ? $"{damage}!" : damage.ToString();
            _textMesh.color = isCrit ? Color.yellow : Color.white;
            _spawnTime = Time.time;
        }
        
        private void Update()
        {
            if (!IsActiveInPool) return;
            
            // Float upward
            transform.position += Vector3.up * _floatSpeed * Time.deltaTime;
            
            // Fade out
            float alpha = 1f - ((Time.time - _spawnTime) / _lifetime);
            Color color = _textMesh.color;
            color.a = alpha;
            _textMesh.color = color;
            
            // Return to pool when lifetime expires
            if (Time.time - _spawnTime >= _lifetime)
            {
                ReturnToPool();
            }
        }
        
        public override void OnReturnToPool()
        {
            base.OnReturnToPool();
            _textMesh.color = Color.white;
        }
    }
    
    /// <summary>
    /// Ability system controller.
    /// </summary>
    public class AbilitySystemController : MonoBehaviour
    {
        [SerializeField] private List<Ability> _equippedAbilities = new();
        [SerializeField] private int _maxAbilitySlots = 10;
        
        public bool UseAbility(int slotIndex, GameObject caster, Vector3 targetPosition)
        {
            if (slotIndex < 0 || slotIndex >= _equippedAbilities.Count)
                return false;
            
            Ability ability = _equippedAbilities[slotIndex];
            if (ability == null) return false;
            
            // Check cooldown
            if (ability.IsOnCooldown()) return false;
            
            // Check resource cost
            EntityStats stats = caster.GetComponent<EntityStats>();
            if (stats == null) return false;
            
            if (stats.currentMana < ability.manaCost || stats.currentStamina < ability.staminaCost)
                return false;
            
            // Execute ability
            ability.Execute(caster, targetPosition);
            
            // Consume resources
            stats.currentMana -= ability.manaCost;
            stats.currentStamina -= ability.staminaCost;
            
            // Start cooldown
            ability.StartCooldown();
            
            return true;
        }
    }
    
    [System.Serializable]
    public class Ability
    {
        public string abilityName;
        public AbilityType abilityType;
        public int manaCost;
        public int staminaCost;
        public float cooldown;
        public float baseDamage;
        public float aoeRadius;
        
        private float _cooldownEndTime;
        
        public bool IsOnCooldown() => Time.time < _cooldownEndTime;
        
        public void StartCooldown() => _cooldownEndTime = Time.time + cooldown;
        
        public virtual void Execute(GameObject caster, Vector3 targetPosition)
        {
            Debug.Log($"[Ability] {caster.name} used {abilityName}");
        }
    }
    
    public enum AbilityType { Damage, Heal, Buff, Debuff, Summon, Movement, Utility }
}
###########
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Game.Core.Systems
{
    /// <summary>
    /// Central hub for all core game systems.
    /// Manages initialization order and system dependencies.
    /// DontDestroyOnLoad singleton - persists across scenes.
    /// </summary>
    public class CoreSystemManager : MonoBehaviour
    {
        public static CoreSystemManager Instance { get; private set; }
        
        [Header("System References (Auto-Assigned at Runtime)")]
        [SerializeField] private ObjectPoolManager _poolManager;
        [SerializeField] private ZoneSystemManager _zoneManager;
        [SerializeField] private EntitySystemManager _entityManager;
        [SerializeField] private CombatSystemManager _combatManager;
        [SerializeField] private InventorySystemManager _inventoryManager;
        [SerializeField] private UISystemManager _uiManager;
        [SerializeField] private AudioSystemManager _audioManager;
        [SerializeField] private WebSocketNetworkManager _networkManager;
        [SerializeField] private AdminConsoleManager _adminConsoleManager;
        
        [Header("Initialization Status")]
        [SerializeField] private bool _isInitialized;
        [SerializeField] private float _initializationTime;
        
        // Public accessors for systems
        public static ObjectPoolManager PoolManager => Instance?._poolManager;
        public static ZoneSystemManager ZoneManager => Instance?._zoneManager;
        public static EntitySystemManager EntityManager => Instance?._entityManager;
        public static CombatSystemManager CombatManager => Instance?._combatManager;
        public static InventorySystemManager InventoryManager => Instance?._inventoryManager;
        public static UISystemManager UIManager => Instance?._uiManager;
        public static AudioSystemManager AudioManager => Instance?._audioManager;
        public static WebSocketNetworkManager NetworkManager => Instance?._networkManager;
        public static AdminConsoleManager AdminConsole => Instance?._adminConsoleManager;
        
        #region Unity Lifecycle
        
        private void Awake()
        {
            // Singleton enforcement
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            
            Instance = this;
            DontDestroyOnLoad(gameObject);
            
            InitializeSystems();
        }
        
        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }
        
        #endregion
        
        #region System Initialization
        
        private void InitializeSystems()
        {
            float startTime = Time.realtimeSinceStartup;
            
            Debug.Log("[CoreSystemManager] Initializing core systems...");
            
            // Initialize systems in dependency order
            InitializePoolManager();
            InitializeNetworkManager();
            InitializeZoneManager();
            InitializeEntityManager();
            InitializeCombatManager();
            InitializeInventoryManager();
            InitializeAudioManager();
            InitializeUIManager();
            InitializeAdminConsole();
            
            _initializationTime = Time.realtimeSinceStartup - startTime;
            _isInitialized = true;
            
            Debug.Log($"[CoreSystemManager] All systems initialized in {_initializationTime:F3}s");
        }
        
        private void InitializePoolManager()
        {
            _poolManager = GetOrCreateChildSystem<ObjectPoolManager>("ObjectPoolManager");
        }
        
        private void InitializeNetworkManager()
        {
            _networkManager = GetOrCreateChildSystem<WebSocketNetworkManager>("WebSocketNetworkManager");
        }
        
        private void InitializeZoneManager()
        {
            _zoneManager = GetOrCreateChildSystem<ZoneSystemManager>("ZoneSystemManager");
        }
        
        private void InitializeEntityManager()
        {
            _entityManager = GetOrCreateChildSystem<EntitySystemManager>("EntitySystemManager");
        }
        
        private void InitializeCombatManager()
        {
            _combatManager = GetOrCreateChildSystem<CombatSystemManager>("CombatSystemManager");
        }
        
        private void InitializeInventoryManager()
        {
            _inventoryManager = GetOrCreateChildSystem<InventorySystemManager>("InventorySystemManager");
        }
        
        private void InitializeAudioManager()
        {
            _audioManager = GetOrCreateChildSystem<AudioSystemManager>("AudioSystemManager");
        }
        
        private void InitializeUIManager()
        {
            _uiManager = GetOrCreateChildSystem<UISystemManager>("UISystemManager");
        }
        
        private void InitializeAdminConsole()
        {
            _adminConsoleManager = GetOrCreateChildSystem<AdminConsoleManager>("AdminConsoleManager");
        }
        
        /// <summary>
        /// Helper method to get or create a child system component.
        /// </summary>
        private T GetOrCreateChildSystem<T>(string childName) where T : Component
        {
            // Check if already exists as child
            Transform child = transform.Find(childName);
            if (child != null)
            {
                T component = child.GetComponent<T>();
                if (component != null)
                {
                    Debug.Log($"[CoreSystemManager] Found existing {typeof(T).Name}");
                    return component;
                }
            }
            
            // Create new child object with component
            GameObject systemObj = new GameObject(childName);
            systemObj.transform.SetParent(transform);
            T newComponent = systemObj.AddComponent<T>();
            
            Debug.Log($"[CoreSystemManager] Created {typeof(T).Name}");
            return newComponent;
        }
        
        #endregion
        
        #region Public API
        
        /// <summary>
        /// Check if all core systems are initialized and ready.
        /// </summary>
        public bool IsReady()
        {
            return _isInitialized 
                && _poolManager != null
                && _zoneManager != null
                && _entityManager != null
                && _combatManager != null
                && _inventoryManager != null
                && _uiManager != null
                && _audioManager != null
                && _networkManager != null
                && _adminConsoleManager != null;
        }
        
        /// <summary>
        /// Shutdown all systems gracefully.
        /// Call before application quit or when switching to main menu.
        /// </summary>
        public async Awaitable ShutdownAllSystems()
        {
            Debug.Log("[CoreSystemManager] Shutting down all systems...");
            
            // Shutdown in reverse order
            if (_adminConsoleManager != null) 
                _adminConsoleManager.Shutdown();
            
            if (_uiManager != null) 
                _uiManager.Shutdown();
            
            if (_audioManager != null) 
                _audioManager.Shutdown();
            
            if (_inventoryManager != null) 
                _inventoryManager.Shutdown();
            
            if (_combatManager != null) 
                _combatManager.Shutdown();
            
            if (_entityManager != null) 
                await _entityManager.ShutdownAsync();
            
            if (_zoneManager != null) 
                await _zoneManager.ShutdownAsync();
            
            if (_networkManager != null) 
                await _networkManager.DisconnectAsync();
            
            if (_poolManager != null)
                _poolManager.ClearAllPools();
            
            _isInitialized = false;
            
            Debug.Log("[CoreSystemManager] All systems shut down");
        }
        
        #endregion
    }
}
###########
using UnityEngine;
using UnityEngine.SceneManagement;
using Game.Core.Systems;

namespace Game.Core
{
    /// <summary>
    /// Application entry point and bootstrap system.
    /// Ensures CoreSystemManager exists and handles initial scene loading.
    /// Place this on a GameObject in your first scene (typically "Bootstrap" scene).
    /// </summary>
    public class GameBootstrap : MonoBehaviour
    {
        [Header("Bootstrap Configuration")]
        [SerializeField, Tooltip("Scene to load after initialization")]
        private string _initialSceneName = "MainMenu";
        
        [SerializeField, Tooltip("Show loading screen during initialization")]
        private bool _showLoadingScreen = true;
        
        [SerializeField, Tooltip("Minimum time to show loading screen (prevents flicker)")]
        private float _minLoadingTime = 1f;
        
        [Header("Performance Settings")]
        [SerializeField, Tooltip("Target frame rate (0 = unlimited)")]
        private int _targetFrameRate = 60;
        
        [SerializeField, Tooltip("Enable VSync (overrides target frame rate)")]
        private bool _enableVSync = true;
        
        [Header("Quality Settings")]
        [SerializeField, Tooltip("Default quality level on startup (0-5)")]
        [Range(0, 5)]
        private int _defaultQualityLevel = 2; // Medium
        
        [Header("Runtime References (Auto-Assigned)")]
        [SerializeField] private CoreSystemManager _coreSystemManager;
        [SerializeField] private Canvas _loadingCanvas;
        
        private bool _isBootstrapped;
        
        #region Unity Lifecycle
        
        private async void Start()
        {
            if (_isBootstrapped)
            {
                Debug.LogWarning("[GameBootstrap] Already bootstrapped!");
                return;
            }
            
            await BootstrapGame();
        }
        
        private void OnApplicationQuit()
        {
            if (_coreSystemManager != null)
            {
                // Note: Cannot await in OnApplicationQuit, but we can start the shutdown
                _ = _coreSystemManager.ShutdownAllSystems();
            }
        }
        
        #endregion
        
        #region Bootstrap Process
        
        private async Awaitable BootstrapGame()
        {
            float startTime = Time.realtimeSinceStartup;
            
            Debug.Log("[GameBootstrap] Starting game bootstrap...");
            
            // Step 1: Apply performance settings
            ApplyPerformanceSettings();
            
            // Step 2: Show loading screen
            if (_showLoadingScreen)
            {
                ShowLoadingScreen();
            }
            
            // Step 3: Ensure CoreSystemManager exists
            await EnsureCoreSystemManager();
            
            // Step 4: Wait for minimum loading time (prevents flicker)
            float elapsed = Time.realtimeSinceStartup - startTime;
            if (elapsed < _minLoadingTime)
            {
                float waitTime = _minLoadingTime - elapsed;
                await Awaitable.WaitForSecondsAsync(waitTime);
            }
            
            // Step 5: Load initial scene
            await LoadInitialScene();
            
            // Step 6: Hide loading screen
            if (_showLoadingScreen)
            {
                HideLoadingScreen();
            }
            
            _isBootstrapped = true;
            
            float totalTime = Time.realtimeSinceStartup - startTime;
            Debug.Log($"[GameBootstrap] Bootstrap complete in {totalTime:F3}s");
        }
        
        private void ApplyPerformanceSettings()
        {
            // Set target frame rate
            if (_enableVSync)
            {
                QualitySettings.vSyncCount = 1;
                Application.targetFrameRate = -1; // VSync controls framerate
            }
            else
            {
                QualitySettings.vSyncCount = 0;
                Application.targetFrameRate = _targetFrameRate;
            }
            
            // Set quality level (with bounds checking)
            int maxQualityLevel = QualitySettings.names.Length - 1;
            int safeQualityLevel = Mathf.Clamp(_defaultQualityLevel, 0, maxQualityLevel);
            
            if (safeQualityLevel != _defaultQualityLevel)
            {
                Debug.LogWarning($"[GameBootstrap] Quality level {_defaultQualityLevel} out of range (0-{maxQualityLevel}). Using {safeQualityLevel} instead.");
            }
            
            QualitySettings.SetQualityLevel(safeQualityLevel, true);
            
            // Prevent screen dimming on mobile
            Screen.sleepTimeout = SleepTimeout.NeverSleep;
            
            // Get quality name safely
            string qualityName = "Unknown";
            if (safeQualityLevel >= 0 && safeQualityLevel < QualitySettings.names.Length)
            {
                qualityName = QualitySettings.names[safeQualityLevel];
            }
            
            Debug.Log($"[GameBootstrap] Performance settings applied: " +
                     $"TargetFPS={Application.targetFrameRate}, " +
                     $"VSync={QualitySettings.vSyncCount}, " +
                     $"Quality={qualityName}");
        }
        
        private async Awaitable EnsureCoreSystemManager()
        {
            // Check if CoreSystemManager already exists
            _coreSystemManager = FindFirstObjectByType<CoreSystemManager>();
            
            if (_coreSystemManager == null)
            {
                Debug.Log("[GameBootstrap] Creating CoreSystemManager...");
                
                GameObject coreSystemObj = new("CoreSystemManager");
                _coreSystemManager = coreSystemObj.AddComponent<CoreSystemManager>();
                
                // Wait for initialization
                while (!_coreSystemManager.IsReady())
                {
                    await Awaitable.NextFrameAsync();
                }
            }
            else
            {
                Debug.Log("[GameBootstrap] CoreSystemManager already exists");
            }
        }
        
        private async Awaitable LoadInitialScene()
        {
            if (string.IsNullOrEmpty(_initialSceneName))
            {
                Debug.LogWarning("[GameBootstrap] No initial scene specified!");
                return;
            }
            
            // Check if we're already in the target scene
            Scene currentScene = SceneManager.GetActiveScene();
            if (currentScene.name == _initialSceneName)
            {
                Debug.Log($"[GameBootstrap] Already in scene '{_initialSceneName}'");
                return;
            }
            
            Debug.Log($"[GameBootstrap] Loading scene '{_initialSceneName}'...");
            
            AsyncOperation loadOp = SceneManager.LoadSceneAsync(_initialSceneName, LoadSceneMode.Single);
            
            if (loadOp == null)
            {
                Debug.LogError($"[GameBootstrap] Failed to load scene '{_initialSceneName}'!");
                return;
            }
            
            // Wait for scene to load
            while (!loadOp.isDone)
            {
                await Awaitable.NextFrameAsync();
            }
            
            Debug.Log($"[GameBootstrap] Scene '{_initialSceneName}' loaded");
        }
        
        #endregion
        
        #region Loading Screen
        
        private void ShowLoadingScreen()
        {
            if (_loadingCanvas != null)
            {
                _loadingCanvas.gameObject.SetActive(true);
                return;
            }
            
            // Create simple loading canvas if not assigned
            GameObject canvasObj = new("LoadingCanvas");
            _loadingCanvas = canvasObj.AddComponent<Canvas>();
            _loadingCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            _loadingCanvas.sortingOrder = 9999; // Top-most
            
            // Add CanvasScaler for resolution independence
            UnityEngine.UI.CanvasScaler scaler = canvasObj.AddComponent<UnityEngine.UI.CanvasScaler>();
            scaler.uiScaleMode = UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            
            // Add GraphicRaycaster (required for UI)
            canvasObj.AddComponent<UnityEngine.UI.GraphicRaycaster>();
            
            // Create loading panel (simple black background)
            GameObject panelObj = new("LoadingPanel");
            panelObj.transform.SetParent(canvasObj.transform, false);
            
            UnityEngine.UI.Image panel = panelObj.AddComponent<UnityEngine.UI.Image>();
            panel.color = Color.black;
            
            RectTransform panelRect = panelObj.GetComponent<RectTransform>();
            panelRect.anchorMin = Vector2.zero;
            panelRect.anchorMax = Vector2.one;
            panelRect.sizeDelta = Vector2.zero;
            
            // Create loading text
            GameObject textObj = new("LoadingText");
            textObj.transform.SetParent(panelObj.transform, false);
            
            UnityEngine.UI.Text text = textObj.AddComponent<UnityEngine.UI.Text>();
            text.text = "Loading...";
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = 48;
            text.color = Color.white;
            text.alignment = TextAnchor.MiddleCenter;
            
            RectTransform textRect = textObj.GetComponent<RectTransform>();
            textRect.anchorMin = new Vector2(0.5f, 0.5f);
            textRect.anchorMax = new Vector2(0.5f, 0.5f);
            textRect.sizeDelta = new Vector2(400, 100);
            
            DontDestroyOnLoad(canvasObj);
        }
        
        private void HideLoadingScreen()
        {
            if (_loadingCanvas != null)
            {
                Destroy(_loadingCanvas.gameObject);
                _loadingCanvas = null;
            }
        }
        
        #endregion
    }
}
###########
using UnityEngine;
using System.Collections.Generic;

namespace Game.Core.Systems
{
    /// <summary>
    /// Registry for all interactable objects in zones.
    /// Manages doors, chests, ladders, stairs, windows, etc.
    /// </summary>
    public class InteractableRegistry : MonoBehaviour
    {
        [Header("Interactable Prefabs")]
        [SerializeField] private GameObject _doorPrefab;
        [SerializeField] private GameObject _chestPrefab;
        [SerializeField] private GameObject _ladderPrefab;
        [SerializeField] private GameObject _stairsPrefab;
        
        [Header("Runtime Registry")]
        [SerializeField] private List<InteractableObject> _registeredInteractables = new();
        
        private Dictionary<string, InteractableObject> _interactableMap;
        
        private void Awake()
        {
            _interactableMap = new Dictionary<string, InteractableObject>();
        }
        
        public void RegisterInteractable(InteractableObject interactable)
        {
            if (!_registeredInteractables.Contains(interactable))
            {
                _registeredInteractables.Add(interactable);
                _interactableMap[interactable.GetInstanceID().ToString()] = interactable;
            }
        }
        
        public void UnregisterInteractable(InteractableObject interactable)
        {
            _registeredInteractables.Remove(interactable);
            _interactableMap.Remove(interactable.GetInstanceID().ToString());
        }
        
        public List<InteractableObject> GetInteractablesInRange(Vector3 position, float range)
        {
            List<InteractableObject> nearby = new();
            
            foreach (InteractableObject interactable in _registeredInteractables)
            {
                if (interactable != null && Vector3.Distance(position, interactable.transform.position) <= range)
                {
                    nearby.Add(interactable);
                }
            }
            
            return nearby;
        }
    }
    
    /// <summary>
    /// Base class for all interactable objects.
    /// </summary>
    public abstract class InteractableObject : MonoBehaviour
    {
        [Header("Interactable Settings")]
        [SerializeField] protected string _interactionPrompt = "Press E to interact";
        [SerializeField] protected float _interactionRange = 3f;
        [SerializeField] protected bool _isInteractable = true;
        
        public string InteractionPrompt => _interactionPrompt;
        public float InteractionRange => _interactionRange;
        public bool IsInteractable => _isInteractable;
        
        protected virtual void Start()
        {
            InteractableRegistry registry = FindFirstObjectByType<InteractableRegistry>();
            registry?.RegisterInteractable(this);
        }
        
        protected virtual void OnDestroy()
        {
            InteractableRegistry registry = FindFirstObjectByType<InteractableRegistry>();
            registry?.UnregisterInteractable(this);
        }
        
        public abstract void Interact(GameObject interactor);
        
        protected virtual void OnDrawGizmos()
        {
            Gizmos.color = _isInteractable ? Color.cyan : Color.gray;
            Gizmos.DrawWireSphere(transform.position, _interactionRange);
        }
    }
    
    /// <summary>
    /// Door interactable - can be opened/closed, locked/unlocked.
    /// </summary>
    public class Door : InteractableObject
    {
        [Header("Door Settings")]
        [SerializeField] private bool _isLocked;
        [SerializeField] private bool _isOpen;
        [SerializeField] private float _openAngle = 90f;
        [SerializeField] private float _openSpeed = 2f;
        
        private Quaternion _closedRotation;
        private Quaternion _openRotation;
        private bool _isAnimating;
        
        protected override void Start()
        {
            base.Start();
            _closedRotation = transform.rotation;
            _openRotation = _closedRotation * Quaternion.Euler(0, _openAngle, 0);
        }
        
        public override void Interact(GameObject interactor)
        {
            if (_isLocked)
            {
                Debug.Log("[Door] Door is locked!");
                return;
            }
            
            if (!_isAnimating)
            {
                _isOpen = !_isOpen;
                StartCoroutine(AnimateDoor());
            }
        }
        
        private System.Collections.IEnumerator AnimateDoor()
        {
            _isAnimating = true;
            Quaternion targetRotation = _isOpen ? _openRotation : _closedRotation;
            
            while (Quaternion.Angle(transform.rotation, targetRotation) > 0.1f)
            {
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * _openSpeed);
                yield return null;
            }
            
            transform.rotation = targetRotation;
            _isAnimating = false;
        }
    }
    
    /// <summary>
    /// Chest interactable - contains loot.
    /// </summary>
    public class Chest : InteractableObject
    {
        [Header("Chest Settings")]
        [SerializeField] private bool _isOpened;
        [SerializeField] private int _lootCount = 3;
        
        public override void Interact(GameObject interactor)
        {
            if (_isOpened)
            {
                Debug.Log("[Chest] Already looted!");
                return;
            }
            
            _isOpened = true;
            SpawnLoot();
        }
        
        private void SpawnLoot()
        {
            // TODO: Integrate with LootManager
            Debug.Log($"[Chest] Spawning {_lootCount} items of loot!");
        }
    }
    
    /// <summary>
    /// Ladder interactable - allows vertical traversal.
    /// </summary>
    public class Ladder : InteractableObject
    {
        [Header("Ladder Settings")]
        [SerializeField] private float _climbSpeed = 3f;
        [SerializeField] private Transform _topPosition;
        [SerializeField] private Transform _bottomPosition;
        
        public override void Interact(GameObject interactor)
        {
            // TODO: Implement climbing mechanics
            Debug.Log("[Ladder] Climbing!");
        }
    }
}
###########
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
###########
   using UnityEngine;
   using UnityEngine.SceneManagement;

   public class MainMenuController : MonoBehaviour
   {
       void Update()
       {
           if (Input.GetKeyDown(KeyCode.Space))
           {
               // Will be replaced with proper character creation
               Debug.Log("Starting game...");
           }
       }
   }
###########
using System;
using System.Collections.Generic;
using UnityEngine;
using Game.Core.Pooling;

namespace Game.Core.Systems
{
    /// <summary>
    /// Centralized object pool manager for zero-allocation spawning.
    /// Handles all poolable objects in the game.
    /// CRITICAL: This system MUST have zero allocations in Get/Return operations.
    /// </summary>
    public class ObjectPoolManager : MonoBehaviour
    {
        public static ObjectPoolManager Instance { get; private set; }
        
        [Header("Pool Configuration")]
        [SerializeField, Tooltip("Parent transform for all pooled objects")]
        private Transform _poolRoot;
        
        [SerializeField, Tooltip("Initial pool sizes per type")]
        private List<PoolConfiguration> _preWarmPools = new();
        
        [Header("Runtime Pool Statistics (Read-Only)")]
        [SerializeField] private int _totalPooledObjects;
        [SerializeField] private int _totalActiveObjects;
        [SerializeField] private int _poolHits;
        [SerializeField] private int _poolMisses;
        
        // Pool storage: Dictionary<PoolKey, Stack<IPoolable>>
        private Dictionary<string, Stack<IPoolable>> _pools;
        
        // Prefab registry: Dictionary<PoolKey, GameObject>
        private Dictionary<string, GameObject> _prefabs;
        
        // Active tracking for admin inspection
        private Dictionary<string, HashSet<IPoolable>> _activeObjects;
        
        // Pool limits to prevent memory bloat
        private Dictionary<string, int> _poolLimits;
        
        private const int DEFAULT_POOL_LIMIT = 500;
        
        #region Unity Lifecycle
        
        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            
            Instance = this;
            DontDestroyOnLoad(gameObject);
            
            InitializePools();
        }
        
        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }
        
        #endregion
        
        #region Initialization
        
        private void InitializePools()
        {
            _pools = new Dictionary<string, Stack<IPoolable>>(32);
            _prefabs = new Dictionary<string, GameObject>(32);
            _activeObjects = new Dictionary<string, HashSet<IPoolable>>(32);
            _poolLimits = new Dictionary<string, int>(32);
            
            // Create pool root if not assigned
            if (_poolRoot == null)
            {
                GameObject root = new("PoolRoot");
                root.transform.SetParent(transform);
                _poolRoot = root.transform;
            }
            
            // Pre-warm configured pools
            foreach (PoolConfiguration config in _preWarmPools)
            {
                if (config.prefab != null)
                {
                    RegisterPrefab(config.poolKey, config.prefab, config.maxPoolSize);
                    WarmPool(config.poolKey, config.initialSize);
                }
            }
            
            Debug.Log($"[ObjectPoolManager] Initialized with {_preWarmPools.Count} pre-warmed pools");
        }
        
        #endregion
        
        #region Public API
        
        /// <summary>
        /// Register a prefab for pooling.
        /// Must be called before Get() for that pool type.
        /// </summary>
        public void RegisterPrefab(string poolKey, GameObject prefab, int maxPoolSize = DEFAULT_POOL_LIMIT)
        {
            if (string.IsNullOrEmpty(poolKey))
            {
                Debug.LogError("[ObjectPoolManager] Cannot register prefab with empty pool key!");
                return;
            }
            
            if (_prefabs.ContainsKey(poolKey))
            {
                Debug.LogWarning($"[ObjectPoolManager] Pool key '{poolKey}' already registered. Overwriting.");
            }
            
            _prefabs[poolKey] = prefab;
            _poolLimits[poolKey] = maxPoolSize;
            
            // Initialize pool structures
            if (!_pools.ContainsKey(poolKey))
            {
                _pools[poolKey] = new Stack<IPoolable>(maxPoolSize / 4);
            }
            
            if (!_activeObjects.ContainsKey(poolKey))
            {
                _activeObjects[poolKey] = new HashSet<IPoolable>();
            }
        }
        
        /// <summary>
        /// Pre-warm a pool with specified number of instances.
        /// Call during scene load or async to avoid runtime hitches.
        /// </summary>
        public void WarmPool(string poolKey, int count)
        {
            if (!_prefabs.ContainsKey(poolKey))
            {
                Debug.LogError($"[ObjectPoolManager] Cannot warm pool '{poolKey}' - prefab not registered!");
                return;
            }
            
            GameObject prefab = _prefabs[poolKey];
            Stack<IPoolable> pool = _pools[poolKey];
            
            for (int i = 0; i < count; i++)
            {
                GameObject obj = Instantiate(prefab, _poolRoot);
                obj.name = $"{poolKey}_{i}";
                obj.SetActive(false);
                
                IPoolable poolable = obj.GetComponent<IPoolable>();
                if (poolable == null)
                {
                    Debug.LogError($"[ObjectPoolManager] Prefab for '{poolKey}' missing IPoolable component!");
                    Destroy(obj);
                    continue;
                }
                
                poolable.IsActiveInPool = false;
                pool.Push(poolable);
                _totalPooledObjects++;
            }
            
            Debug.Log($"[ObjectPoolManager] Pre-warmed '{poolKey}' pool with {count} instances");
        }
        
        /// <summary>
        /// Get an object from the pool.
        /// Zero allocation if pool has available instances.
        /// </summary>
        public T Get<T>(string poolKey, Vector3 position, Quaternion rotation) where T : class, IPoolable
        {
            if (!_pools.ContainsKey(poolKey))
            {
                Debug.LogError($"[ObjectPoolManager] Pool '{poolKey}' not found! Did you forget to RegisterPrefab?");
                return null;
            }
            
            Stack<IPoolable> pool = _pools[poolKey];
            IPoolable poolable;
            
            // Try to get from pool (zero allocation)
            if (pool.Count > 0)
            {
                poolable = pool.Pop();
                _poolHits++;
            }
            else
            {
                // Pool exhausted - create new instance (1 allocation)
                _poolMisses++;
                
                if (!_prefabs.ContainsKey(poolKey))
                {
                    Debug.LogError($"[ObjectPoolManager] No prefab registered for '{poolKey}'!");
                    return null;
                }
                
                // Check pool limit
                int activeCount = _activeObjects[poolKey].Count;
                int poolLimit = _poolLimits[poolKey];
                
                if (activeCount >= poolLimit)
                {
                    Debug.LogWarning($"[ObjectPoolManager] Pool '{poolKey}' exceeded limit ({poolLimit}). Reusing oldest object.");
                    // Force return oldest active object
                    HashSet<IPoolable>.Enumerator enumerator = _activeObjects[poolKey].GetEnumerator();
                    enumerator.MoveNext();
                    IPoolable oldest = enumerator.Current;
                    ReturnToPool(oldest);
                    poolable = pool.Pop();
                }
                else
                {
                    GameObject obj = Instantiate(_prefabs[poolKey], _poolRoot);
                    obj.name = $"{poolKey}_Dynamic_{_totalPooledObjects}";
                    poolable = obj.GetComponent<IPoolable>();
                    _totalPooledObjects++;
                }
            }
            
            // Setup pooled object
            Transform t = poolable.GameObject.transform;
            t.position = position;
            t.rotation = rotation;
            t.SetParent(null); // Remove from pool root
            
            poolable.OnSpawnFromPool();
            _activeObjects[poolKey].Add(poolable);
            _totalActiveObjects++;
            
            return poolable as T;
        }
        
        /// <summary>
        /// Convenience overload - spawn at origin with identity rotation.
        /// </summary>
        public T Get<T>(string poolKey) where T : class, IPoolable
        {
            return Get<T>(poolKey, Vector3.zero, Quaternion.identity);
        }
        
        /// <summary>
        /// Return an object to the pool.
        /// Zero allocation operation.
        /// </summary>
        public void ReturnToPool(IPoolable poolable)
        {
            if (poolable == null)
            {
                Debug.LogWarning("[ObjectPoolManager] Attempted to return null object to pool!");
                return;
            }
            
            if (!poolable.IsActiveInPool)
            {
                Debug.LogWarning($"[ObjectPoolManager] Object '{poolable.GameObject.name}' already returned to pool!");
                return;
            }
            
            string poolKey = poolable.PoolKey;
            
            if (!_pools.ContainsKey(poolKey))
            {
                Debug.LogError($"[ObjectPoolManager] Pool '{poolKey}' not found for return!");
                return;
            }
            
            // Reset object state
            poolable.OnReturnToPool();
            
            // Re-parent to pool root
            poolable.GameObject.transform.SetParent(_poolRoot);
            
            // Return to pool
            _pools[poolKey].Push(poolable);
            _activeObjects[poolKey].Remove(poolable);
            _totalActiveObjects--;
        }
        
        /// <summary>
        /// Clear a specific pool, destroying all instances.
        /// Use sparingly - typically only during scene transitions.
        /// </summary>
        public void ClearPool(string poolKey)
        {
            if (!_pools.ContainsKey(poolKey))
            {
                return;
            }
            
            // Destroy all inactive instances
            Stack<IPoolable> pool = _pools[poolKey];
            while (pool.Count > 0)
            {
                IPoolable poolable = pool.Pop();
                if (poolable != null && poolable.GameObject != null)
                {
                    Destroy(poolable.GameObject);
                    _totalPooledObjects--;
                }
            }
            
            // Destroy all active instances
            HashSet<IPoolable> active = _activeObjects[poolKey];
            foreach (IPoolable poolable in active)
            {
                if (poolable != null && poolable.GameObject != null)
                {
                    Destroy(poolable.GameObject);
                    _totalPooledObjects--;
                    _totalActiveObjects--;
                }
            }
            active.Clear();
            
            Debug.Log($"[ObjectPoolManager] Cleared pool '{poolKey}'");
        }
        
        /// <summary>
        /// Get pool statistics for admin UI or debugging.
        /// Zero allocation.
        /// </summary>
        public PoolStats GetPoolStats(string poolKey)
        {
            if (!_pools.ContainsKey(poolKey))
            {
                return default;
            }
            
            return new PoolStats
            {
                poolKey = poolKey,
                inactiveCount = _pools[poolKey].Count,
                activeCount = _activeObjects[poolKey].Count,
                totalCount = _pools[poolKey].Count + _activeObjects[poolKey].Count,
                maxPoolSize = _poolLimits[poolKey]
            };
        }
        
        /// <summary>
        /// Get all active objects of a specific pool type.
        /// Used by admin UI for inspection.
        /// Returns new HashSet to avoid modification issues.
        /// </summary>
        public HashSet<IPoolable> GetActiveObjects(string poolKey)
        {
            if (!_activeObjects.ContainsKey(poolKey))
            {
                return new HashSet<IPoolable>();
            }
            
            return new HashSet<IPoolable>(_activeObjects[poolKey]);
        }
        
        /// <summary>
        /// Get all registered pool keys.
        /// Used by admin UI for pool navigation.
        /// </summary>
        public List<string> GetAllPoolKeys()
        {
            return new List<string>(_pools.Keys);
        }
        
        #endregion
        
        #region Admin/Debug Methods
        
        /// <summary>
        /// Force clear all pools and reset statistics.
        /// Admin-only operation.
        /// </summary>
        public void ClearAllPools()
        {
            foreach (string poolKey in _pools.Keys)
            {
                ClearPool(poolKey);
            }
            
            _poolHits = 0;
            _poolMisses = 0;
            
            Debug.Log("[ObjectPoolManager] All pools cleared");
        }
        
        /// <summary>
        /// Get overall pool performance metrics.
        /// </summary>
        public void GetPoolPerformanceMetrics(out int totalPooled, out int totalActive, out int hits, out int misses)
        {
            totalPooled = _totalPooledObjects;
            totalActive = _totalActiveObjects;
            hits = _poolHits;
            misses = _poolMisses;
        }
        
        #endregion
        
        #region Nested Types
        
        [Serializable]
        public class PoolConfiguration
        {
            [Tooltip("Unique identifier for this pool")]
            public string poolKey = "DefaultPool";
            
            [Tooltip("Prefab to pool")]
            public GameObject prefab;
            
            [Tooltip("Number of instances to pre-create")]
            public int initialSize = 10;
            
            [Tooltip("Maximum instances allowed in pool")]
            public int maxPoolSize = 100;
        }
        
        public struct PoolStats
        {
            public string poolKey;
            public int inactiveCount;
            public int activeCount;
            public int totalCount;
            public int maxPoolSize;
        }
        
        #endregion
    }
}
###########
using UnityEngine;
using System.Collections.Generic;

namespace Game.Core.Systems
{
    /// <summary>
    /// Procedurally generates character models based on creation choices.
    /// Supports multiple species (human, elf, dwarf, orc, etc.) with male/female variants.
    /// </summary>
    public class ProceduralCharacterBuilder : MonoBehaviour
    {
        [Header("Character Generation Settings")]
        [SerializeField] private float _baseHeight = 1.8f;
        [SerializeField] private float _baseWidth = 0.5f;
        
        /// <summary>
        /// Generate a complete character based on creation data.
        /// </summary>
        public GameObject GenerateCharacter(CharacterCreationData data)
        {
            Debug.Log($"[ProceduralCharacterBuilder] Generating {data.gender} {data.species}...");
            
            GameObject character = new($"Character_{data.species}_{data.gender}");
            
            // Generate body parts
            Mesh bodyMesh = GenerateBodyMesh(data);
            Mesh headMesh = GenerateHeadMesh(data);
            
            // Combine meshes
            Mesh finalMesh = CombineMeshes(bodyMesh, headMesh);
            
            // Setup mesh components
            MeshFilter meshFilter = character.AddComponent<MeshFilter>();
            meshFilter.mesh = finalMesh;
            
            MeshRenderer meshRenderer = character.AddComponent<MeshRenderer>();
            meshRenderer.material = GenerateSkinMaterial(data);
            
            // Add character controller
            CharacterController controller = character.AddComponent<CharacterController>();
            controller.height = GetSpeciesHeight(data.species, data.bodyType);
            controller.radius = 0.3f;
            
            // Add animator
            Animator animator = character.AddComponent<Animator>();
            // TODO: Assign runtime controller
            
            return character;
        }
        
        private Mesh GenerateBodyMesh(CharacterCreationData data)
        {
            Mesh mesh = new();
            mesh.name = "BodyMesh";
            
            float height = GetSpeciesHeight(data.species, data.bodyType);
            float width = _baseWidth * GetBodyTypeScale(data.bodyType);
            
            // Simple capsule-like body
            Vector3[] vertices = new Vector3[]
            {
                // Torso vertices (simplified box)
                new(-width, 0, -width), new(width, 0, -width), new(width, 0, width), new(-width, 0, width), // Bottom
                new(-width, height * 0.6f, -width), new(width, height * 0.6f, -width), new(width, height * 0.6f, width), new(-width, height * 0.6f, width), // Top
            };
            
            int[] triangles = new int[]
            {
                // Bottom face
                0, 2, 1, 0, 3, 2,
                // Top face
                4, 5, 6, 4, 6, 7,
                // Front face
                0, 1, 5, 0, 5, 4,
                // Back face
                3, 7, 6, 3, 6, 2,
                // Left face
                0, 4, 7, 0, 7, 3,
                // Right face
                1, 2, 6, 1, 6, 5
            };
            
            mesh.vertices = vertices;
            mesh.triangles = triangles;
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            
            return mesh;
        }
        
        private Mesh GenerateHeadMesh(CharacterCreationData data)
        {
            Mesh mesh = new();
            mesh.name = "HeadMesh";
            
            float height = GetSpeciesHeight(data.species, data.bodyType);
            float headSize = 0.15f;
            float neckHeight = height * 0.6f;
            
            // Simple sphere-like head
            Vector3 headCenter = new(0, neckHeight + headSize, 0);
            
            // Create sphere vertices (simplified)
            List<Vector3> vertices = new();
            List<int> triangles = new();
            
            int segments = 8;
            for (int lat = 0; lat <= segments; lat++)
            {
                float theta = lat * Mathf.PI / segments;
                float sinTheta = Mathf.Sin(theta);
                float cosTheta = Mathf.Cos(theta);
                
                for (int lon = 0; lon <= segments; lon++)
                {
                    float phi = lon * 2 * Mathf.PI / segments;
                    float sinPhi = Mathf.Sin(phi);
                    float cosPhi = Mathf.Cos(phi);
                    
                    Vector3 vertex = new(
                        headSize * sinTheta * cosPhi,
                        headSize * cosTheta,
                        headSize * sinTheta * sinPhi
                    );
                    
                    vertices.Add(headCenter + vertex);
                }
            }
            
            // Generate triangles
            for (int lat = 0; lat < segments; lat++)
            {
                for (int lon = 0; lon < segments; lon++)
                {
                    int first = lat * (segments + 1) + lon;
                    int second = first + segments + 1;
                    
                    triangles.Add(first);
                    triangles.Add(second);
                    triangles.Add(first + 1);
                    
                    triangles.Add(second);
                    triangles.Add(second + 1);
                    triangles.Add(first + 1);
                }
            }
            
            mesh.vertices = vertices.ToArray();
            mesh.triangles = triangles.ToArray();
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            
            return mesh;
        }
        
        private Mesh CombineMeshes(params Mesh[] meshes)
        {
            CombineInstance[] combine = new CombineInstance[meshes.Length];
            
            for (int i = 0; i < meshes.Length; i++)
            {
                combine[i].mesh = meshes[i];
                combine[i].transform = Matrix4x4.identity;
            }
            
            Mesh finalMesh = new();
            finalMesh.CombineMeshes(combine, true, false);
            finalMesh.RecalculateNormals();
            finalMesh.RecalculateBounds();
            
            return finalMesh;
        }
        
        private Material GenerateSkinMaterial(CharacterCreationData data)
        {
            Material material = new(Shader.Find("Standard"));
            material.name = $"CharacterMaterial_{data.species}";
            material.color = GetSkinColor(data.species, data.skinTone);
            
            return material;
        }
        
        private float GetSpeciesHeight(Species species, BodyType bodyType)
        {
            float baseHeight = species switch
            {
                Species.Human => 1.8f,
                Species.Elf => 1.9f,
                Species.Dwarf => 1.3f,
                Species.Orc => 2.1f,
                Species.Lizardfolk => 1.85f,
                Species.Android => 1.8f,
                Species.Cyborg => 1.85f,
                Species.Alien => 1.7f,
                Species.Demon => 2.0f,
                Species.Angel => 1.95f,
                _ => 1.8f
            };
            
            return baseHeight;
        }
        
        private float GetBodyTypeScale(BodyType bodyType)
        {
            return bodyType switch
            {
                BodyType.Slim => 0.8f,
                BodyType.Average => 1.0f,
                BodyType.Muscular => 1.2f,
                _ => 1.0f
            };
        }
        
        private Color GetSkinColor(Species species, int skinTone)
        {
            Color[] palette = species switch
            {
                Species.Human => new[] { new Color(1f, 0.8f, 0.6f), new Color(0.8f, 0.6f, 0.4f), new Color(0.4f, 0.3f, 0.2f) },
                Species.Elf => new[] { new Color(1f, 0.95f, 0.9f), new Color(0.9f, 0.85f, 0.8f) },
                Species.Dwarf => new[] { new Color(0.9f, 0.7f, 0.5f), new Color(0.8f, 0.6f, 0.4f) },
                Species.Orc => new[] { new Color(0.4f, 0.6f, 0.3f), new Color(0.3f, 0.5f, 0.2f) },
                Species.Lizardfolk => new[] { new Color(0.3f, 0.7f, 0.3f), new Color(0.4f, 0.6f, 0.5f) },
                Species.Android => new[] { new Color(0.7f, 0.7f, 0.8f), new Color(0.6f, 0.6f, 0.7f) },
                Species.Demon => new[] { new Color(0.8f, 0.2f, 0.2f), new Color(0.6f, 0.1f, 0.1f) },
                Species.Angel => new[] { new Color(1f, 1f, 0.95f), new Color(0.95f, 0.95f, 0.9f) },
                _ => new[] { Color.gray }
            };
            
            return palette[Mathf.Clamp(skinTone, 0, palette.Length - 1)];
        }
    }
    
    #region Character Creation Data
    
    [System.Serializable]
    public class CharacterCreationData
    {
        public Species species = Species.Human;
        public Gender gender = Gender.Male;
        public BodyType bodyType = BodyType.Average;
        public int skinTone = 0;
        public int faceShape = 0;
        public int hairStyle = 0;
        
        // Attributes
        public int strength = 10;
        public int dexterity = 10;
        public int intelligence = 10;
        public int vitality = 10;
        public int endurance = 10;
        public int luck = 10;
        
        // Species ability
        public SpeciesAbility selectedAbility;
        
        // Heroic start bonus
        public HeroicBonus heroicBonus;
    }
    
    public enum Species
    {
        Human, Elf, Dwarf, Orc, Lizardfolk,
        Android, Cyborg, Alien, Demon, Angel
    }
    
    public enum Gender { Male, Female }
    
    public enum BodyType { Slim, Average, Muscular }
    
    public enum SpeciesAbility
    {
        // Human
        Adaptable, SecondWind,
        // Elf
        KeenSenses, NaturesStep,
        // Add more as needed
    }
    
    public enum HeroicBonus
    {
        LegendaryWeapon,
        AncientArmor,
        Spellbook,
        Companion,
        GoldHoard
    }
    
    #endregion
}
###########
using UnityEngine;
using System.Collections.Generic;

namespace Game.Core.Systems
{
    /// <summary>
    /// Simple procedural terrain generator using Perlin noise.
    /// Generates rolling hills with biome-specific features.
    /// Optimized for low-end hardware (40fps target).
    /// </summary>
    public class SimpleTerrainGenerator : MonoBehaviour
    {
        [Header("Terrain Settings")]
        [SerializeField, Tooltip("Resolution of terrain mesh (vertices per side)")]
        private int _terrainResolution = 50;
        
        [SerializeField, Tooltip("Height multiplier for terrain")]
        private float _heightScale = 10f;
        
        [SerializeField, Tooltip("Frequency of Perlin noise")]
        private float _noiseScale = 0.1f;
        
        [Header("Biome Colors")]
        [SerializeField] private Color _grasslandColor = new(0.3f, 0.6f, 0.2f);
        [SerializeField] private Color _desertColor = new(0.8f, 0.7f, 0.4f);
        [SerializeField] private Color _snowColor = new(0.9f, 0.9f, 1f);
        [SerializeField] private Color _lavaColor = new(0.8f, 0.2f, 0.1f);
        [SerializeField] private Color _corruptedColor = new(0.4f, 0.1f, 0.5f);
        
        // Cached material for terrain
        private Material _terrainMaterial;
        
        #region Terrain Generation
        
        /// <summary>
        /// Generate terrain for a zone with specified configuration.
        /// </summary>
        public async Awaitable GenerateTerrainForZone(Transform zoneRoot, ZoneConfig config)
        {
            Debug.Log($"[SimpleTerrainGenerator] Generating terrain for zone '{config.zoneName}'...");
            
            float startTime = Time.realtimeSinceStartup;
            
            // Create terrain container
            GameObject terrainObj = new("Terrain");
            terrainObj.transform.SetParent(zoneRoot);
            
            // Generate mesh
            Mesh terrainMesh = GenerateTerrainMesh(config);
            
            // Setup mesh components
            MeshFilter meshFilter = terrainObj.AddComponent<MeshFilter>();
            meshFilter.mesh = terrainMesh;
            
            MeshRenderer meshRenderer = terrainObj.AddComponent<MeshRenderer>();
            meshRenderer.material = GetBiomeMaterial(config.biomeType);
            
            // Add collider for walkability
            MeshCollider meshCollider = terrainObj.AddComponent<MeshCollider>();
            meshCollider.sharedMesh = terrainMesh;
            
            float elapsed = Time.realtimeSinceStartup - startTime;
            Debug.Log($"[SimpleTerrainGenerator] Terrain generated in {elapsed:F3}s");
            
            await Awaitable.NextFrameAsync();
        }
        
        private Mesh GenerateTerrainMesh(ZoneConfig config)
        {
            Mesh mesh = new();
            mesh.name = $"TerrainMesh_{config.zoneName}";
            
            int resolution = _terrainResolution;
            Vector3 size = config.zoneSize;
            
            // Calculate vertex count
            int vertexCount = resolution * resolution;
            Vector3[] vertices = new Vector3[vertexCount];
            Vector2[] uvs = new Vector2[vertexCount];
            Color[] colors = new Color[vertexCount];
            
            // Generate vertices with Perlin noise
            float stepX = size.x / (resolution - 1);
            float stepZ = size.z / (resolution - 1);
            
            for (int z = 0; z < resolution; z++)
            {
                for (int x = 0; x < resolution; x++)
                {
                    int index = z * resolution + x;
                    
                    // Position
                    float xPos = x * stepX - size.x / 2f;
                    float zPos = z * stepZ - size.z / 2f;
                    
                    // Height from Perlin noise
                    float noiseX = (xPos + config.seed) * _noiseScale;
                    float noiseZ = (zPos + config.seed) * _noiseScale;
                    float height = Mathf.PerlinNoise(noiseX, noiseZ) * _heightScale;
                    
                    // Apply biome-specific height modifications
                    height = ApplyBiomeModifier(height, config.biomeType);
                    
                    vertices[index] = new Vector3(xPos, height, zPos);
                    
                    // UVs
                    uvs[index] = new Vector2((float)x / (resolution - 1), (float)z / (resolution - 1));
                    
                    // Vertex colors (for visual variety)
                    colors[index] = GetBiomeColor(config.biomeType, height);
                }
            }
            
            // Generate triangles
            int triangleCount = (resolution - 1) * (resolution - 1) * 6;
            int[] triangles = new int[triangleCount];
            int triIndex = 0;
            
            for (int z = 0; z < resolution - 1; z++)
            {
                for (int x = 0; x < resolution - 1; x++)
                {
                    int topLeft = z * resolution + x;
                    int topRight = topLeft + 1;
                    int bottomLeft = (z + 1) * resolution + x;
                    int bottomRight = bottomLeft + 1;
                    
                    // First triangle
                    triangles[triIndex++] = topLeft;
                    triangles[triIndex++] = bottomLeft;
                    triangles[triIndex++] = topRight;
                    
                    // Second triangle
                    triangles[triIndex++] = topRight;
                    triangles[triIndex++] = bottomLeft;
                    triangles[triIndex++] = bottomRight;
                }
            }
            
            // Assign to mesh
            mesh.vertices = vertices;
            mesh.triangles = triangles;
            mesh.uv = uvs;
            mesh.colors = colors;
            
            // Recalculate normals and bounds
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            
            return mesh;
        }
        
        #endregion
        
        #region Biome Modifiers
        
        private float ApplyBiomeModifier(float height, BiomeType biome)
        {
            return biome switch
            {
                BiomeType.Grassland => height, // Gentle rolling hills
                BiomeType.Desert => height * 0.5f, // Flatter terrain
                BiomeType.Snow => height * 1.5f, // More dramatic peaks
                BiomeType.Lava => height * 0.3f + Mathf.Abs(Mathf.Sin(height * 5f)) * 2f, // Rocky plateaus
                BiomeType.Corrupted => height + Mathf.PerlinNoise(height * 10f, height * 10f) * 3f, // Chaotic terrain
                _ => height
            };
        }
        
        private Color GetBiomeColor(BiomeType biome, float height)
        {
            Color baseColor = biome switch
            {
                BiomeType.Grassland => _grasslandColor,
                BiomeType.Desert => _desertColor,
                BiomeType.Snow => _snowColor,
                BiomeType.Lava => _lavaColor,
                BiomeType.Corrupted => _corruptedColor,
                _ => Color.gray
            };
            
            // Add height-based variation
            float variation = height / _heightScale;
            return Color.Lerp(baseColor * 0.7f, baseColor, variation);
        }
        
        private Material GetBiomeMaterial(BiomeType biome)
        {
            if (_terrainMaterial == null)
            {
                _terrainMaterial = new Material(Shader.Find("Standard"));
                _terrainMaterial.name = "TerrainMaterial";
            }
            
            _terrainMaterial.color = biome switch
            {
                BiomeType.Grassland => _grasslandColor,
                BiomeType.Desert => _desertColor,
                BiomeType.Snow => _snowColor,
                BiomeType.Lava => _lavaColor,
                BiomeType.Corrupted => _corruptedColor,
                _ => Color.gray
            };
            
            return _terrainMaterial;
        }
        
        #endregion
    }
}
###########
using UnityEngine;

namespace Game.Core.Systems
{
    /// <summary>
    /// Zone System Manager - Manages zone loading, generation, and transitions.
    /// Integrates with ZoneSceneManager for scene management.
    /// </summary>
    public class ZoneSystemManager : MonoBehaviour
    {
        [Header("Zone Management")]
        [SerializeField] private ZoneSceneManager _sceneManager;
        
        private void Awake()
        {
            _sceneManager = GetComponent<ZoneSceneManager>();
            if (_sceneManager == null)
            {
                _sceneManager = gameObject.AddComponent<ZoneSceneManager>();
            }
        }
        
        public async Awaitable<UnityEngine.SceneManagement.Scene> LoadZone(string zoneName, ZoneConfig config = null)
        {
            return await _sceneManager.LoadZone(zoneName, config);
        }
        
        public async Awaitable UnloadZone(string zoneName)
        {
            await _sceneManager.UnloadZone(zoneName);
        }
        
        public async Awaitable<UnityEngine.SceneManagement.Scene> GenerateZone(ZoneConfig config)
        {
            return await _sceneManager.GenerateAndSaveZone(config);
        }
        
        public string GetCurrentZone()
        {
            return _sceneManager.GetCurrentZone();
        }
        
        public async Awaitable ShutdownAsync()
        {
            if (_sceneManager != null)
            {
                await _sceneManager.ShutdownAsync();
            }
        }
    }
    
    /// <summary>
    /// Entity System Manager - Manages all entities (players, NPCs, enemies).
    /// Handles entity spawning, lifecycle, and AI coordination.
    /// </summary>
    public class EntitySystemManager : MonoBehaviour
    {
        [Header("Entity Management")]
        [SerializeField] private int _maxActiveEntities = 100;
        [SerializeField] private int _currentEntityCount;
        
        private void Awake()
        {
            Debug.Log("[EntitySystemManager] Initialized");
        }
        
        public GameObject SpawnEntity(string entityType, Vector3 position, Quaternion rotation)
        {
            // TODO: Implement entity spawning from pool
            Debug.Log($"[EntitySystemManager] Spawning {entityType} at {position}");
            return null;
        }
        
        public void DespawnEntity(GameObject entity)
        {
            // TODO: Return entity to pool
            Debug.Log($"[EntitySystemManager] Despawning {entity.name}");
        }
        
        public int GetEntityCount()
        {
            return _currentEntityCount;
        }
        
        public async Awaitable ShutdownAsync()
        {
            Debug.Log("[EntitySystemManager] Shutting down...");
            await Awaitable.NextFrameAsync();
        }
    }
    
    /// <summary>
    /// Inventory System Manager - Manages player and entity inventories.
    /// Handles item storage, equipped items, and inventory UI.
    /// </summary>
    public class InventorySystemManager : MonoBehaviour
    {
        [Header("Inventory Configuration")]
        [SerializeField] private int _defaultInventorySize = 30;
        
        private void Awake()
        {
            Debug.Log("[InventorySystemManager] Initialized");
        }
        
        public void AddItem(GameObject entity, ItemData item)
        {
            // TODO: Implement inventory system
            Debug.Log($"[InventorySystemManager] Adding item {item.itemName} to {entity.name}");
        }
        
        public void RemoveItem(GameObject entity, string itemId)
        {
            // TODO: Implement item removal
            Debug.Log($"[InventorySystemManager] Removing item {itemId} from {entity.name}");
        }
        
        public void Shutdown()
        {
            Debug.Log("[InventorySystemManager] Shutting down...");
        }
    }
    
    /// <summary>
    /// Audio System Manager - Manages all audio playback.
    /// Handles music, sound effects, and spatial audio.
    /// </summary>
    public class AudioSystemManager : MonoBehaviour
    {
        [Header("Audio Configuration")]
        [SerializeField] private float _masterVolume = 1.0f;
        [SerializeField] private float _musicVolume = 0.7f;
        [SerializeField] private float _sfxVolume = 1.0f;
        
        private AudioSource _musicSource;
        
        private void Awake()
        {
            _musicSource = gameObject.AddComponent<AudioSource>();
            _musicSource.loop = true;
            _musicSource.playOnAwake = false;
            _musicSource.volume = _musicVolume;
            
            Debug.Log("[AudioSystemManager] Initialized");
        }
        
        public void PlayMusic(AudioClip clip)
        {
            if (_musicSource != null && clip != null)
            {
                _musicSource.clip = clip;
                _musicSource.Play();
            }
        }
        
        public void PlaySFX(AudioClip clip, Vector3 position)
        {
            if (clip != null)
            {
                AudioSource.PlayClipAtPoint(clip, position, _sfxVolume * _masterVolume);
            }
        }
        
        public void StopMusic()
        {
            if (_musicSource != null)
            {
                _musicSource.Stop();
            }
        }
        
        public void SetMasterVolume(float volume)
        {
            _masterVolume = Mathf.Clamp01(volume);
        }
        
        public void SetMusicVolume(float volume)
        {
            _musicVolume = Mathf.Clamp01(volume);
            if (_musicSource != null)
            {
                _musicSource.volume = _musicVolume * _masterVolume;
            }
        }
        
        public void SetSFXVolume(float volume)
        {
            _sfxVolume = Mathf.Clamp01(volume);
        }
        
        public void Shutdown()
        {
            StopMusic();
            Debug.Log("[AudioSystemManager] Shutting down...");
        }
    }
}
###########
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
###########
using UnityEngine;

namespace Game.Core.Systems
{
    /// <summary>
    /// WebSocket-based LAN multiplayer manager (max 6 players).
    /// Stub implementation - will be fully implemented in Phase 5.
    /// </summary>
    public partial class WebSocketNetworkManager : MonoBehaviour
    {
        [Header("Network Configuration")]
        [SerializeField] private int _maxPlayers = 6;
        [SerializeField] private int _port = 7777;
        
        [Header("Network State")]
        [SerializeField] private bool _isHost;
        [SerializeField] private bool _isConnected;
        [SerializeField] private int _connectedPlayerCount;
        
        public bool IsHost() => _isHost;
        public bool IsConnected() => _isConnected;
        public int GetPlayerCount() => _connectedPlayerCount;
        
        public async Awaitable DisconnectAsync()
        {
            _isConnected = false;
            _isHost = false;
            _connectedPlayerCount = 0;
            await Awaitable.NextFrameAsync();
        }
    }
}
###########
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
###########
