using UnityEngine;
using UnityEngine.SceneManagement;
using Game.Core.Systems;

namespace Game.Core
{
    /// <summary>
    /// FIXED: Validates build settings, ensures scenes exist
    /// </summary>
    public class GameBootstrap : MonoBehaviour
    {
        [Header("Bootstrap Configuration")]
        [SerializeField, Tooltip("Scene to load after initialization")]
        private string _initialSceneName = "MainMenu";
        
        [SerializeField, Tooltip("Show loading screen during initialization")]
        private bool _showLoadingScreen = true;
        
        [SerializeField, Tooltip("Minimum time to show loading screen")]
        private float _minLoadingTime = 1f;
        
        [Header("Performance Settings")]
        [SerializeField, Tooltip("Target frame rate (0 = unlimited)")]
        private int _targetFrameRate = 60;
        
        [SerializeField, Tooltip("Enable VSync")]
        private bool _enableVSync = true;
        
        [Header("Quality Settings")]
        [SerializeField, Tooltip("Default quality level on startup (0-5)")]
        [Range(0, 5)]
        private int _defaultQualityLevel = 2;
        
        [Header("Runtime References")]
        [SerializeField] private CoreSystemManager _coreSystemManager;
        [SerializeField] private Canvas _loadingCanvas;
        
        private bool _isBootstrapped;
        
        private async void Start()
        {
            if (_isBootstrapped)
            {
                Debug.LogWarning("[GameBootstrap] Already bootstrapped!");
                return;
            }
            
            // CRITICAL: Validate scene setup FIRST
            if (!ValidateSceneSetup())
            {
                Debug.LogError("[GameBootstrap] Scene validation failed! Check Build Settings.");
                return;
            }
            
            await BootstrapGame();
        }
        
        private void OnApplicationQuit()
        {
            if (_coreSystemManager != null)
            {
                _ = _coreSystemManager.ShutdownAllSystems();
            }
        }
        
        private bool ValidateSceneSetup()
        {
            // Check if this is Bootstrap scene
            Scene currentScene = SceneManager.GetActiveScene();
            if (currentScene.buildIndex != 0)
            {
                Debug.LogWarning($"[GameBootstrap] Bootstrap should be build index 0, currently {currentScene.buildIndex}");
            }
            
            // Check if target scene exists in build settings
            int sceneCount = SceneManager.sceneCountInBuildSettings;
            bool foundTarget = false;
            
            for (int i = 0; i < sceneCount; i++)
            {
                string scenePath = SceneUtility.GetScenePathByBuildIndex(i);
                string sceneName = System.IO.Path.GetFileNameWithoutExtension(scenePath);
                
                if (sceneName == _initialSceneName)
                {
                    foundTarget = true;
                    Debug.Log($"[GameBootstrap] Found target scene '{_initialSceneName}' at index {i}");
                    break;
                }
            }
            
            if (!foundTarget)
            {
                Debug.LogError($"[GameBootstrap] Scene '{_initialSceneName}' not found in Build Settings!");
                Debug.LogError("[GameBootstrap] Go to File > Build Settings and add your scenes!");
                return false;
            }
            
            // Check for GameWorld scene
            bool hasGameWorld = false;
            for (int i = 0; i < sceneCount; i++)
            {
                string scenePath = SceneUtility.GetScenePathByBuildIndex(i);
                string sceneName = System.IO.Path.GetFileNameWithoutExtension(scenePath);
                
                if (sceneName == "GameWorld")
                {
                    hasGameWorld = true;
                    break;
                }
            }
            
            if (!hasGameWorld)
            {
                Debug.LogWarning("[GameBootstrap] 'GameWorld' scene not in Build Settings - character creation will fail!");
            }
            
            return true;
        }
        
        private async Awaitable BootstrapGame()
        {
            float startTime = Time.realtimeSinceStartup;
            
            Debug.Log("[GameBootstrap] Starting game bootstrap...");
            
            ApplyPerformanceSettings();
            
            if (_showLoadingScreen)
            {
                ShowLoadingScreen();
            }
            
            await EnsureCoreSystemManager();
            
            float elapsed = Time.realtimeSinceStartup - startTime;
            if (elapsed < _minLoadingTime)
            {
                float waitTime = _minLoadingTime - elapsed;
                await Awaitable.WaitForSecondsAsync(waitTime);
            }
            
            await LoadInitialScene();
            
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
            
            int maxQualityLevel = QualitySettings.names.Length - 1;
            int safeQualityLevel = Mathf.Clamp(_defaultQualityLevel, 0, maxQualityLevel);
            
            if (safeQualityLevel != _defaultQualityLevel)
            {
                Debug.LogWarning($"[GameBootstrap] Quality level {_defaultQualityLevel} out of range (0-{maxQualityLevel}). Using {safeQualityLevel}.");
            }
            
            QualitySettings.SetQualityLevel(safeQualityLevel, true);
            Screen.sleepTimeout = SleepTimeout.NeverSleep;
            
            string qualityName = "Unknown";
            if (safeQualityLevel >= 0 && safeQualityLevel < QualitySettings.names.Length)
            {
                qualityName = QualitySettings.names[safeQualityLevel];
            }
            
            Debug.Log($"[GameBootstrap] Performance: TargetFPS={Application.targetFrameRate}, VSync={QualitySettings.vSyncCount}, Quality={qualityName}");
        }
        
        private async Awaitable EnsureCoreSystemManager()
        {
            _coreSystemManager = FindFirstObjectByType<CoreSystemManager>();
            
            if (_coreSystemManager == null)
            {
                Debug.Log("[GameBootstrap] Creating CoreSystemManager...");
                
                GameObject coreSystemObj = new("CoreSystemManager");
                // CRITICAL: Ensure it's a root GameObject
                coreSystemObj.transform.SetParent(null);
                _coreSystemManager = coreSystemObj.AddComponent<CoreSystemManager>();
                
                int maxWait = 100;
                int waited = 0;
                while (!_coreSystemManager.IsReady() && waited < maxWait)
                {
                    await Awaitable.NextFrameAsync();
                    waited++;
                }
                
                if (!_coreSystemManager.IsReady())
                {
                    Debug.LogError("[GameBootstrap] CoreSystemManager failed to initialize!");
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
                Debug.LogError($"[GameBootstrap] Failed to load scene '{_initialSceneName}'! Check Build Settings.");
                return;
            }
            
            while (!loadOp.isDone)
            {
                await Awaitable.NextFrameAsync();
            }
            
            Debug.Log($"[GameBootstrap] Scene '{_initialSceneName}' loaded successfully");
        }
        
        private void ShowLoadingScreen()
        {
            if (_loadingCanvas != null)
            {
                _loadingCanvas.gameObject.SetActive(true);
                return;
            }
            
            GameObject canvasObj = new("LoadingCanvas");
            _loadingCanvas = canvasObj.AddComponent<Canvas>();
            _loadingCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            _loadingCanvas.sortingOrder = 9999;
            
            UnityEngine.UI.CanvasScaler scaler = canvasObj.AddComponent<UnityEngine.UI.CanvasScaler>();
            scaler.uiScaleMode = UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            
            canvasObj.AddComponent<UnityEngine.UI.GraphicRaycaster>();
            
            GameObject panelObj = new("LoadingPanel");
            panelObj.transform.SetParent(canvasObj.transform, false);
            
            UnityEngine.UI.Image panel = panelObj.AddComponent<UnityEngine.UI.Image>();
            panel.color = Color.black;
            
            RectTransform panelRect = panelObj.GetComponent<RectTransform>();
            panelRect.anchorMin = Vector2.zero;
            panelRect.anchorMax = Vector2.one;
            panelRect.sizeDelta = Vector2.zero;
            
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
    }
}