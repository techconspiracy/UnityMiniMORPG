using UnityEngine;
using UnityEngine.SceneManagement;
using Game.Core.Systems;

namespace Game.Core
{
    /// <summary>
    /// ENHANCED Game Bootstrap - Streamlined flow with tutorial auto-start.
    /// Skips character creation and goes straight to gameplay.
    /// </summary>
    public class EnhancedGameBootstrap : MonoBehaviour
    {
        [Header("Flow Configuration")]
        [SerializeField] private bool _skipCharacterCreation = true;
        [SerializeField] private bool _goDirectlyToGameWorld = true;
        [SerializeField] private bool _autoStartTutorial = true;
        
        [Header("Performance Settings")]
        [SerializeField] private int _targetFrameRate = 60;
        [SerializeField] private bool _enableVSync = true;
        [SerializeField] private int _defaultQualityLevel = 2;
        
        [Header("Default Character")]
        [SerializeField] private Species _defaultSpecies = Species.Human;
        [SerializeField] private Gender _defaultGender = Gender.Male;
        
        private bool _isBootstrapped;
        private CoreSystemManager _coreSystemManager;
        
        private async void Start()
        {
            if (_isBootstrapped)
            {
                Debug.LogWarning("[EnhancedBootstrap] Already bootstrapped!");
                return;
            }
            
            Debug.Log("[EnhancedBootstrap] ===== STARTING ENHANCED BOOTSTRAP =====");
            
            await BootstrapGame();
        }
        
        private async Awaitable BootstrapGame()
        {
            float startTime = Time.realtimeSinceStartup;
            
            // 1. Apply performance settings
            ApplyPerformanceSettings();
            
            // 2. Ensure CoreSystemManager exists and is ready
            await EnsureCoreSystemManager();
            
            // 3. Create default character data
            CharacterCreationData defaultCharacter = CreateDefaultCharacter();
            PlayerPrefs.SetString("CurrentCharacter", JsonUtility.ToJson(defaultCharacter));
            PlayerPrefs.Save();
            
            // 4. Load appropriate scene
            if (_goDirectlyToGameWorld)
            {
                await LoadGameWorld();
            }
            else
            {
                await LoadMainMenu();
            }
            
            _isBootstrapped = true;
            
            float totalTime = Time.realtimeSinceStartup - startTime;
            Debug.Log($"[EnhancedBootstrap] ===== BOOTSTRAP COMPLETE in {totalTime:F3}s =====");
        }
        
        private void ApplyPerformanceSettings()
        {
            if (_enableVSync)
            {
                QualitySettings.vSyncCount = 1;
                Application.targetFrameRate = -1;
            }
            else
            {
                QualitySettings.vSyncCount = 0;
                Application.targetFrameRate = _targetFrameRate;
            }
            
            QualitySettings.SetQualityLevel(_defaultQualityLevel, true);
            Screen.sleepTimeout = SleepTimeout.NeverSleep;
            
            Debug.Log($"[EnhancedBootstrap] Performance: FPS={Application.targetFrameRate}, VSync={QualitySettings.vSyncCount}");
        }
        
        private async Awaitable EnsureCoreSystemManager()
        {
            _coreSystemManager = FindFirstObjectByType<CoreSystemManager>();
            
            if (_coreSystemManager == null)
            {
                Debug.Log("[EnhancedBootstrap] Creating CoreSystemManager...");
                
                GameObject coreSystemObj = new GameObject("CoreSystemManager");
                coreSystemObj.transform.SetParent(null);
                _coreSystemManager = coreSystemObj.AddComponent<CoreSystemManager>();
                
                // Wait for initialization
                int maxWait = 100;
                int waited = 0;
                while (!_coreSystemManager.IsReady() && waited < maxWait)
                {
                    await Awaitable.NextFrameAsync();
                    waited++;
                }
                
                if (!_coreSystemManager.IsReady())
                {
                    Debug.LogError("[EnhancedBootstrap] CoreSystemManager failed to initialize!");
                }
                else
                {
                    Debug.Log("[EnhancedBootstrap] ✓ CoreSystemManager ready");
                }
            }
            else
            {
                Debug.Log("[EnhancedBootstrap] ✓ CoreSystemManager found");
                
                // Wait for it to be ready
                while (!_coreSystemManager.IsReady())
                {
                    await Awaitable.NextFrameAsync();
                }
            }
        }
        
        private CharacterCreationData CreateDefaultCharacter()
        {
            CharacterCreationData character = new CharacterCreationData
            {
                species = _defaultSpecies,
                gender = _defaultGender,
                bodyType = BodyType.Average,
                skinTone = 0,
                faceShape = 0,
                hairStyle = 0,
                strength = 15,
                dexterity = 15,
                intelligence = 15,
                vitality = 15,
                endurance = 15,
                luck = 10
            };
            
            Debug.Log($"[EnhancedBootstrap] ✓ Created default {character.species} {character.gender} character");
            
            return character;
        }
        
        private async Awaitable LoadGameWorld()
        {
            Debug.Log("[EnhancedBootstrap] Loading GameWorld scene...");
            
            Scene currentScene = SceneManager.GetActiveScene();
            if (currentScene.name == "GameWorld")
            {
                Debug.Log("[EnhancedBootstrap] Already in GameWorld - setting up tutorial");
                SetupTutorial();
                return;
            }
            
            AsyncOperation loadOp = SceneManager.LoadSceneAsync("GameWorld", LoadSceneMode.Single);
            
            if (loadOp == null)
            {
                Debug.LogError("[EnhancedBootstrap] Failed to load GameWorld! Check Build Settings.");
                return;
            }
            
            while (!loadOp.isDone)
            {
                await Awaitable.NextFrameAsync();
            }
            
            Debug.Log("[EnhancedBootstrap] ✓ GameWorld loaded");
            
            // Wait a frame for scene to initialize
            await Awaitable.NextFrameAsync();
            
            SetupTutorial();
        }
        
        private async Awaitable LoadMainMenu()
        {
            Debug.Log("[EnhancedBootstrap] Loading MainMenu scene...");
            
            AsyncOperation loadOp = SceneManager.LoadSceneAsync("MainMenu", LoadSceneMode.Single);
            
            if (loadOp == null)
            {
                Debug.LogError("[EnhancedBootstrap] Failed to load MainMenu!");
                return;
            }
            
            while (!loadOp.isDone)
            {
                await Awaitable.NextFrameAsync();
            }
            
            Debug.Log("[EnhancedBootstrap] ✓ MainMenu loaded");
        }
        
        private void SetupTutorial()
        {
            if (!_autoStartTutorial) return;
            
            TutorialSystemManager tutorial = FindFirstObjectByType<TutorialSystemManager>();
            
            if (tutorial == null)
            {
                Debug.Log("[EnhancedBootstrap] Creating TutorialSystemManager...");
                GameObject tutorialObj = new GameObject("TutorialSystemManager");
                tutorial = tutorialObj.AddComponent<TutorialSystemManager>();
            }
            
            Debug.Log("[EnhancedBootstrap] ✓ Tutorial system ready");
        }
        
        private void OnApplicationQuit()
        {
            if (_coreSystemManager != null)
            {
                _ = _coreSystemManager.ShutdownAllSystems();
            }
        }
    }
}