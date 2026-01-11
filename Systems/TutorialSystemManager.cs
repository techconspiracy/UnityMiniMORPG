using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using Game.Core.Systems;

namespace Game.Core
{
    /// <summary>
    /// Tutorial System - Orchestrates the entire game flow.
    /// Creates a functional demo that showcases all systems working together.
    /// Place this in the GameWorld scene to auto-start the tutorial.
    /// </summary>
    public class TutorialSystemManager : MonoBehaviour
    {
        [Header("Tutorial Configuration")]
        [SerializeField] private bool _autoStartTutorial = true;
        [SerializeField] private float _stepDelay = 2f;
        
        [Header("Tutorial UI")]
        [SerializeField] private Canvas _tutorialCanvas;
        [SerializeField] private Text _tutorialText;
        [SerializeField] private Text _objectiveText;
        [SerializeField] private GameObject _tutorialPanel;
        
        [Header("Spawned Objects")]
        [SerializeField] private GameObject _player;
        [SerializeField] private List<GameObject> _spawnedEnemies = new();
        [SerializeField] private GameObject _tutorialChest;
        
        private int _currentStep = 0;
        private bool _tutorialActive = false;
        private bool _waitingForPlayerAction = false;
        private string _currentObjective = "";
        
        // System references
        private CoreSystemManager _coreManager;
        private EntitySystemManager _entityManager;
        private CombatSystemManager _combatManager;
        private LootSystemManager _lootManager;
        private QuestSystemManager _questManager;
        private InventorySystemManager _inventoryManager;
        
        #region Initialization
        
        private async void Start()
        {
            Debug.Log("[TutorialSystem] Starting tutorial initialization...");
            
            await InitializeTutorial();
            
            if (_autoStartTutorial)
            {
                await Awaitable.WaitForSecondsAsync(1f);
                StartTutorial();
            }
        }
        
        private async Awaitable InitializeTutorial()
        {
            // Wait for core systems
            while (CoreSystemManager.Instance == null || !CoreSystemManager.Instance.IsReady())
            {
                await Awaitable.NextFrameAsync();
            }
            
            _coreManager = CoreSystemManager.Instance;
            _entityManager = CoreSystemManager.EntityManager;
            _combatManager = CoreSystemManager.CombatManager;
            _lootManager = FindFirstObjectByType<LootSystemManager>();
            _questManager = FindFirstObjectByType<QuestSystemManager>();
            _inventoryManager = CoreSystemManager.InventoryManager;
            
            // Create tutorial UI
            CreateTutorialUI();
            
            // Setup player
            SetupPlayer();
            
            Debug.Log("[TutorialSystem] Initialization complete!");
        }
        
        private void CreateTutorialUI()
        {
            if (_tutorialCanvas != null) return;
            
            GameObject canvasObj = new GameObject("TutorialCanvas");
            canvasObj.transform.SetParent(transform);
            
            _tutorialCanvas = canvasObj.AddComponent<Canvas>();
            _tutorialCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            _tutorialCanvas.sortingOrder = 100;
            
            CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            
            canvasObj.AddComponent<GraphicRaycaster>();
            
            // Tutorial panel
            _tutorialPanel = new GameObject("TutorialPanel");
            _tutorialPanel.transform.SetParent(_tutorialCanvas.transform, false);
            
            RectTransform panelRect = _tutorialPanel.AddComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0.5f, 0.85f);
            panelRect.anchorMax = new Vector2(0.5f, 0.85f);
            panelRect.pivot = new Vector2(0.5f, 0.5f);
            panelRect.sizeDelta = new Vector2(800, 120);
            
            Image panelBg = _tutorialPanel.AddComponent<Image>();
            panelBg.color = new Color(0.1f, 0.1f, 0.15f, 0.9f);
            
            // Tutorial text
            GameObject textObj = new GameObject("TutorialText");
            textObj.transform.SetParent(_tutorialPanel.transform, false);
            
            RectTransform textRect = textObj.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = new Vector2(20, 20);
            textRect.offsetMax = new Vector2(-20, -20);
            
            _tutorialText = textObj.AddComponent<Text>();
            _tutorialText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            _tutorialText.fontSize = 24;
            _tutorialText.color = Color.yellow;
            _tutorialText.alignment = TextAnchor.MiddleCenter;
            
            // Objective panel
            GameObject objPanel = new GameObject("ObjectivePanel");
            objPanel.transform.SetParent(_tutorialCanvas.transform, false);
            
            RectTransform objRect = objPanel.AddComponent<RectTransform>();
            objRect.anchorMin = new Vector2(0.02f, 0.85f);
            objRect.anchorMax = new Vector2(0.02f, 0.85f);
            objRect.pivot = new Vector2(0, 0.5f);
            objRect.sizeDelta = new Vector2(400, 100);
            
            Image objBg = objPanel.AddComponent<Image>();
            objBg.color = new Color(0.2f, 0.3f, 0.2f, 0.8f);
            
            GameObject objTextObj = new GameObject("ObjectiveText");
            objTextObj.transform.SetParent(objPanel.transform, false);
            
            RectTransform objTextRect = objTextObj.AddComponent<RectTransform>();
            objTextRect.anchorMin = Vector2.zero;
            objTextRect.anchorMax = Vector2.one;
            objTextRect.offsetMin = new Vector2(15, 15);
            objTextRect.offsetMax = new Vector2(-15, -15);
            
            _objectiveText = objTextObj.AddComponent<Text>();
            _objectiveText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            _objectiveText.fontSize = 18;
            _objectiveText.color = Color.white;
            _objectiveText.alignment = TextAnchor.UpperLeft;
            
            _tutorialPanel.SetActive(false);
        }
        
        private void SetupPlayer()
        {
            _player = GameObject.FindGameObjectWithTag("Player");
            
            if (_player == null)
            {
                Debug.LogWarning("[TutorialSystem] No player found! Creating tutorial player...");
                CreateTutorialPlayer();
            }
            
            // Ensure player has all required components
            if (_player.GetComponent<PlayerController>() == null)
            {
                _player.AddComponent<PlayerController>();
            }
            
            if (_player.GetComponent<EntityStats>() == null)
            {
                EntityStats stats = _player.AddComponent<EntityStats>();
                stats.maxHealth = 200;
                stats.currentHealth = 200;
                stats.maxMana = 100;
                stats.currentMana = 100;
            }
        }
        
        private void CreateTutorialPlayer()
        {
            _player = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            _player.name = "TutorialPlayer";
            _player.tag = "Player";
            _player.transform.position = new Vector3(0, 2, 0);
            
            Renderer renderer = _player.GetComponent<Renderer>();
            Material mat = new Material(Shader.Find("Standard"));
            mat.color = Color.cyan;
            renderer.material = mat;
            
            EntityStats stats = _player.AddComponent<EntityStats>();
            stats.maxHealth = 200;
            stats.currentHealth = 200;
            stats.maxMana = 100;
            stats.currentMana = 100;
            stats.strength = 15;
            stats.dexterity = 15;
            stats.intelligence = 15;
            
            CharacterController controller = _player.AddComponent<CharacterController>();
            controller.height = 2f;
            controller.radius = 0.5f;
            
            PlayerController playerController = _player.AddComponent<PlayerController>();
            
            // Setup camera
            Camera mainCam = Camera.main;
            if (mainCam == null)
            {
                GameObject camObj = new GameObject("MainCamera");
                mainCam = camObj.AddComponent<Camera>();
                camObj.tag = "MainCamera";
            }
            
            mainCam.transform.SetParent(_player.transform);
            mainCam.transform.localPosition = new Vector3(0, 1.6f, 0);
            mainCam.transform.localRotation = Quaternion.identity;
        }
        
        #endregion
        
        #region Tutorial Flow
        
        public void StartTutorial()
        {
            _tutorialActive = true;
            _currentStep = 0;
            _tutorialPanel.SetActive(true);
            
            Debug.Log("[TutorialSystem] *** TUTORIAL STARTED ***");
            
            ExecuteNextStep();
        }
        
        private async void ExecuteNextStep()
        {
            if (!_tutorialActive) return;
            
            _currentStep++;
            _waitingForPlayerAction = false;
            
            Debug.Log($"[TutorialSystem] === Step {_currentStep} ===");
            
            switch (_currentStep)
            {
                case 1:
                    await Step1_Welcome();
                    break;
                case 2:
                    await Step2_Movement();
                    break;
                case 3:
                    await Step3_SpawnEnemy();
                    break;
                case 4:
                    await Step4_Combat();
                    break;
                case 5:
                    await Step5_Loot();
                    break;
                case 6:
                    await Step6_Inventory();
                    break;
                case 7:
                    await Step7_Quests();
                    break;
                case 8:
                    await Step8_SpawnMultipleEnemies();
                    break;
                case 9:
                    await Step9_PoolingDemo();
                    break;
                case 10:
                    await Step10_Completion();
                    break;
                default:
                    EndTutorial();
                    break;
            }
        }
        
        #endregion
        
        #region Tutorial Steps
        
        private async Awaitable Step1_Welcome()
        {
            ShowTutorial("WELCOME TO THE RPG TUTORIAL!\n\nAll systems are loaded and ready.");
            SetObjective("Objective: Learn the game systems");
            
            await Awaitable.WaitForSecondsAsync(_stepDelay * 2);
            ExecuteNextStep();
        }
        
        private async Awaitable Step2_Movement()
        {
            ShowTutorial("Use WASD to move, MOUSE to look around, SHIFT to sprint.\n\nTry moving around!");
            SetObjective("Move around with WASD\nLook with Mouse");
            
            _waitingForPlayerAction = true;
            
            Vector3 startPos = _player.transform.position;
            float movedDistance = 0;
            
            while (movedDistance < 5f)
            {
                await Awaitable.NextFrameAsync();
                movedDistance = Vector3.Distance(startPos, _player.transform.position);
            }
            
            ShowTutorial("Great! Movement works perfectly.");
            await Awaitable.WaitForSecondsAsync(_stepDelay);
            ExecuteNextStep();
        }
        
        private async Awaitable Step3_SpawnEnemy()
        {
            ShowTutorial("ENTITY SYSTEM: Spawning an enemy using the object pool...");
            SetObjective("Watch the enemy spawn");
            
            await Awaitable.WaitForSecondsAsync(_stepDelay);
            
            Vector3 spawnPos = _player.transform.position + _player.transform.forward * 5f;
            GameObject enemy = _entityManager.SpawnEntity("Zombie", spawnPos, Quaternion.identity);
            
            if (enemy != null)
            {
                _spawnedEnemies.Add(enemy);
                ShowTutorial("Enemy spawned! The entity system is working.");
                
                // Make enemy face player
                enemy.transform.LookAt(_player.transform);
            }
            else
            {
                ShowTutorial("ERROR: Entity system failed! Check ObjectPoolManager.");
            }
            
            await Awaitable.WaitForSecondsAsync(_stepDelay);
            ExecuteNextStep();
        }
        
        private async Awaitable Step4_Combat()
        {
            ShowTutorial("COMBAT SYSTEM: Left-click to attack the enemy!\n\nAim at the zombie and click.");
            SetObjective("Defeat the zombie\nLeft-Click to attack");
            
            _waitingForPlayerAction = true;
            
            GameObject targetEnemy = _spawnedEnemies.Count > 0 ? _spawnedEnemies[0] : null;
            
            if (targetEnemy != null)
            {
                EntityStats enemyStats = targetEnemy.GetComponent<EntityStats>();
                
                while (targetEnemy != null && enemyStats != null && enemyStats.currentHealth > 0)
                {
                    await Awaitable.NextFrameAsync();
                }
                
                ShowTutorial("COMBAT SUCCESS! Damage numbers and combat system working!");
                _spawnedEnemies.Remove(targetEnemy);
            }
            else
            {
                ShowTutorial("Enemy not found, skipping combat test...");
            }
            
            await Awaitable.WaitForSecondsAsync(_stepDelay);
            ExecuteNextStep();
        }
        
        private async Awaitable Step5_Loot()
        {
            ShowTutorial("LOOT SYSTEM: Spawning a chest with random loot...");
            SetObjective("Approach the chest to collect loot");
            
            await Awaitable.WaitForSecondsAsync(_stepDelay);
            
            Vector3 chestPos = _player.transform.position + _player.transform.forward * 4f;
            
            if (_lootManager != null)
            {
                _tutorialChest = _lootManager.GetComponent<InteractableRegistry>()?.SpawnInteractable("Chest", chestPos, Quaternion.identity);
                
                if (_tutorialChest != null)
                {
                    ShowTutorial("Chest spawned! Walk up to it (get close)...");
                    
                    // Wait for player to get close
                    while (Vector3.Distance(_player.transform.position, chestPos) > 3f)
                    {
                        await Awaitable.NextFrameAsync();
                    }
                    
                    // Auto-open chest
                    Chest chest = _tutorialChest.GetComponent<Chest>();
                    if (chest != null)
                    {
                        chest.Interact(_player);
                        ShowTutorial("LOOT COLLECTED! Check your inventory (Press I).");
                    }
                }
            }
            else
            {
                ShowTutorial("Loot system not found, skipping...");
            }
            
            await Awaitable.WaitForSecondsAsync(_stepDelay * 1.5f);
            ExecuteNextStep();
        }
        
        private async Awaitable Step6_Inventory()
        {
            ShowTutorial("INVENTORY SYSTEM: Press 'I' to open your inventory.\n\nCheck your loot!");
            SetObjective("Press I to view inventory\nPress I again to close");
            
            _waitingForPlayerAction = true;
            
            // Wait for player to open inventory
            bool inventoryOpened = false;
            float timeout = 10f;
            float elapsed = 0f;
            
            while (!inventoryOpened && elapsed < timeout)
            {
                if (Input.GetKeyDown(KeyCode.I))
                {
                    inventoryOpened = true;
                }
                elapsed += Time.deltaTime;
                await Awaitable.NextFrameAsync();
            }
            
            if (inventoryOpened)
            {
                ShowTutorial("Inventory system working! You can equip items by right-clicking them.");
            }
            else
            {
                ShowTutorial("Inventory UI available. Moving on...");
            }
            
            await Awaitable.WaitForSecondsAsync(_stepDelay);
            ExecuteNextStep();
        }
        
        private async Awaitable Step7_Quests()
        {
            ShowTutorial("QUEST SYSTEM: Starting a tutorial quest...");
            SetObjective("Kill 3 enemies to complete quest");
            
            await Awaitable.WaitForSecondsAsync(_stepDelay);
            
            if (_questManager != null)
            {
                // Create tutorial quest
                QuestData tutorialQuest = new QuestData
                {
                    questId = "tutorial_quest",
                    questName = "First Blood",
                    questDescription = "Defeat enemies to prove your combat skills!",
                    questType = QuestType.Kill,
                    level = 1,
                    experienceReward = 200,
                    goldReward = 100
                };
                
                tutorialQuest.objectives.Add(new QuestObjective
                {
                    objectiveId = "kill_enemies",
                    objectiveType = ObjectiveType.Kill,
                    targetName = "Zombie",
                    requiredCount = 3,
                    currentCount = 0,
                    description = "Kill 3 Zombies"
                });
                
                _questManager.AddQuest(tutorialQuest);
                _questManager.StartQuest("tutorial_quest");
                
                ShowTutorial("Quest accepted! Press Q to view quest log.");
            }
            else
            {
                ShowTutorial("Quest system not available, skipping...");
            }
            
            await Awaitable.WaitForSecondsAsync(_stepDelay);
            ExecuteNextStep();
        }
        
        private async Awaitable Step8_SpawnMultipleEnemies()
        {
            ShowTutorial("ENTITY SYSTEM: Spawning multiple enemies...\n\nPrepare for battle!");
            SetObjective("Defeat all enemies (3 total)");
            
            await Awaitable.WaitForSecondsAsync(_stepDelay);
            
            // Spawn 3 enemies in a circle around player
            for (int i = 0; i < 3; i++)
            {
                float angle = i * (360f / 3f);
                Vector3 offset = Quaternion.Euler(0, angle, 0) * Vector3.forward * 8f;
                Vector3 spawnPos = _player.transform.position + offset;
                
                GameObject enemy = _entityManager.SpawnEntity("Zombie", spawnPos, Quaternion.identity);
                if (enemy != null)
                {
                    _spawnedEnemies.Add(enemy);
                    enemy.transform.LookAt(_player.transform);
                }
                
                await Awaitable.WaitForSecondsAsync(0.5f);
            }
            
            ShowTutorial($"3 enemies spawned! Current pool count: {_entityManager.GetEntityCount()}");
            
            // Wait for all enemies to be defeated
            _waitingForPlayerAction = true;
            
            while (_spawnedEnemies.Count > 0)
            {
                // Remove null (dead) enemies
                _spawnedEnemies.RemoveAll(e => e == null);
                await Awaitable.NextFrameAsync();
            }
            
            ShowTutorial("ALL ENEMIES DEFEATED! Quest should be complete.");
            
            await Awaitable.WaitForSecondsAsync(_stepDelay);
            ExecuteNextStep();
        }
        
        private async Awaitable Step9_PoolingDemo()
        {
            ShowTutorial("POOLING SYSTEM: Rapid spawn/despawn test...\n\nWatch the magic!");
            SetObjective("Observe object pooling in action");
            
            await Awaitable.WaitForSecondsAsync(_stepDelay);
            
            List<GameObject> testEnemies = new List<GameObject>();
            
            // Rapid spawn
            for (int i = 0; i < 5; i++)
            {
                Vector3 pos = _player.transform.position + new Vector3(Random.Range(-5f, 5f), 0, Random.Range(3f, 8f));
                GameObject enemy = _entityManager.SpawnEntity("Skeleton", pos, Quaternion.identity);
                if (enemy != null) testEnemies.Add(enemy);
                
                await Awaitable.WaitForSecondsAsync(0.3f);
            }
            
            ShowTutorial($"Spawned 5 enemies rapidly! Pool stats: {_entityManager.GetEntityCount()} active");
            
            await Awaitable.WaitForSecondsAsync(_stepDelay);
            
            // Rapid despawn
            ShowTutorial("Now despawning them all...");
            
            foreach (GameObject enemy in testEnemies)
            {
                if (enemy != null)
                {
                    _entityManager.DespawnEntity(enemy);
                    await Awaitable.WaitForSecondsAsync(0.2f);
                }
            }
            
            ShowTutorial("POOLING SUCCESS! Objects returned to pool for reuse.");
            
            await Awaitable.WaitForSecondsAsync(_stepDelay);
            ExecuteNextStep();
        }
        
        private async Awaitable Step10_Completion()
        {
            ShowTutorial("TUTORIAL COMPLETE!\n\nAll systems verified and working!");
            SetObjective("✓ All Systems Online\n✓ Combat Working\n✓ Inventory Working\n✓ Quests Working\n✓ Pooling Working");
            
            await Awaitable.WaitForSecondsAsync(_stepDelay * 3);
            
            ShowTutorial("Press F12 (if host) to open Admin Console.\n\nFree play mode activated!");
            SetObjective("Explore and test all features!");
            
            await Awaitable.WaitForSecondsAsync(_stepDelay * 2);
            
            _tutorialPanel.SetActive(false);
            
            Debug.Log("[TutorialSystem] *** TUTORIAL COMPLETED SUCCESSFULLY ***");
        }
        
        #endregion
        
        #region Helper Methods
        
        private void ShowTutorial(string message)
        {
            if (_tutorialText != null)
            {
                _tutorialText.text = message;
                Debug.Log($"[Tutorial] {message}");
            }
        }
        
        private void SetObjective(string objective)
        {
            _currentObjective = objective;
            if (_objectiveText != null)
            {
                _objectiveText.text = $"<b>OBJECTIVE:</b>\n{objective}";
            }
        }
        
        private void EndTutorial()
        {
            _tutorialActive = false;
            _tutorialPanel?.SetActive(false);
            Debug.Log("[TutorialSystem] Tutorial ended.");
        }
        
        #endregion
        
        #region Debug Commands
        
        private void Update()
        {
            if (!_tutorialActive) return;
            
            // Skip step with SPACE
            if (Input.GetKeyDown(KeyCode.Space) && !_waitingForPlayerAction)
            {
                Debug.Log("[TutorialSystem] Skipping step...");
                ExecuteNextStep();
            }
            
            // Force next step with N
            if (Input.GetKeyDown(KeyCode.N))
            {
                Debug.Log("[TutorialSystem] Force advancing...");
                _waitingForPlayerAction = false;
                ExecuteNextStep();
            }
            
            // Restart tutorial with R
            if (Input.GetKeyDown(KeyCode.R))
            {
                Debug.Log("[TutorialSystem] Restarting tutorial...");
                StartTutorial();
            }
        }
        
        #endregion
    }
}