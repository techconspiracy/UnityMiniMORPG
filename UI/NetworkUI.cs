using UnityEngine;
using UnityEngine.UI;

namespace Game.UI
{
    /// <summary>
    /// Network UI for hosting/joining games.
    /// </summary>
    public class NetworkUI : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private Canvas _canvas;
        [SerializeField] private GameObject _menuPanel;
        
        [Header("Connection")]
        [SerializeField] private InputField _serverAddressInput;
        [SerializeField] private Button _hostButton;
        [SerializeField] private Button _joinButton;
        [SerializeField] private Text _statusText;
        
        private Game.Core.Systems.WebSocketNetworkManager _networkManager;
        
        private void Awake()
        {
            CreateUI();
        }
        
        private void Start()
        {
            _networkManager = Game.Core.Systems.CoreSystemManager.NetworkManager;
            
            if (_networkManager != null)
            {
                _networkManager.OnPlayerConnected += OnPlayerConnected;
                _networkManager.OnPlayerDisconnected += OnPlayerDisconnected;
            }
        }
        
        private void OnDestroy()
        {
            if (_networkManager != null)
            {
                _networkManager.OnPlayerConnected -= OnPlayerConnected;
                _networkManager.OnPlayerDisconnected -= OnPlayerDisconnected;
            }
        }
        
        private void CreateUI()
        {
            if (_canvas == null)
            {
                GameObject canvasObj = new GameObject("NetworkCanvas");
                canvasObj.transform.SetParent(transform, false);
                
                _canvas = canvasObj.AddComponent<Canvas>();
                _canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                _canvas.sortingOrder = 25;
                
                CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1920, 1080);
                
                canvasObj.AddComponent<GraphicRaycaster>();
            }
            
            CreateMenuPanel();
        }
        
        private void CreateMenuPanel()
        {
            _menuPanel = new GameObject("NetworkMenuPanel");
            _menuPanel.transform.SetParent(_canvas.transform, false);
            
            RectTransform rect = _menuPanel.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = new Vector2(400, 300);
            
            Image bg = _menuPanel.AddComponent<Image>();
            bg.color = new Color(0.1f, 0.1f, 0.15f, 0.95f);
            
            // Title
            CreateTitle();
            
            // Server address input
            CreateServerAddressInput();
            
            // Buttons
            CreateHostButton();
            CreateJoinButton();
            
            // Status text
            CreateStatusText();
            
            _menuPanel.SetActive(false);
        }
        
        private void CreateTitle()
        {
            GameObject titleObj = new GameObject("Title");
            titleObj.transform.SetParent(_menuPanel.transform, false);
            
            RectTransform rect = titleObj.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0, 1);
            rect.anchorMax = new Vector2(1, 1);
            rect.pivot = new Vector2(0.5f, 1);
            rect.anchoredPosition = new Vector2(0, -10);
            rect.sizeDelta = new Vector2(0, 50);
            
            Text title = titleObj.AddComponent<Text>();
            title.text = "MULTIPLAYER";
            title.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            title.fontSize = 28;
            title.color = Color.cyan;
            title.alignment = TextAnchor.MiddleCenter;
        }
        
        private void CreateServerAddressInput()
        {
            GameObject inputObj = new GameObject("ServerAddressInput");
            inputObj.transform.SetParent(_menuPanel.transform, false);
            
            RectTransform rect = inputObj.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.6f);
            rect.anchorMax = new Vector2(0.5f, 0.6f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = new Vector2(350, 40);
            
            Image img = inputObj.AddComponent<Image>();
            img.color = new Color(0.2f, 0.2f, 0.3f);
            
            _serverAddressInput = inputObj.AddComponent<InputField>();
            _serverAddressInput.text = "127.0.0.1";
            
            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(inputObj.transform, false);
            
            RectTransform textRect = textObj.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = new Vector2(10, 5);
            textRect.offsetMax = new Vector2(-10, -5);
            
            Text text = textObj.AddComponent<Text>();
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = 18;
            text.color = Color.white;
            text.alignment = TextAnchor.MiddleLeft;
            
            _serverAddressInput.textComponent = text;
            
            // Placeholder
            GameObject placeholderObj = new GameObject("Placeholder");
            placeholderObj.transform.SetParent(inputObj.transform, false);
            
            RectTransform placeholderRect = placeholderObj.AddComponent<RectTransform>();
            placeholderRect.anchorMin = Vector2.zero;
            placeholderRect.anchorMax = Vector2.one;
            placeholderRect.offsetMin = new Vector2(10, 5);
            placeholderRect.offsetMax = new Vector2(-10, -5);
            
            Text placeholder = placeholderObj.AddComponent<Text>();
            placeholder.text = "Server IP Address";
            placeholder.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            placeholder.fontSize = 18;
            placeholder.color = Color.gray;
            placeholder.alignment = TextAnchor.MiddleLeft;
            
            _serverAddressInput.placeholder = placeholder;
        }
        
        private void CreateHostButton()
        {
            GameObject btnObj = new GameObject("HostButton");
            btnObj.transform.SetParent(_menuPanel.transform, false);
            
            RectTransform rect = btnObj.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.4f);
            rect.anchorMax = new Vector2(0.5f, 0.4f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = new Vector2(350, 50);
            
            Image img = btnObj.AddComponent<Image>();
            img.color = new Color(0.2f, 0.6f, 0.2f);
            
            _hostButton = btnObj.AddComponent<Button>();
            _hostButton.targetGraphic = img;
            _hostButton.onClick.AddListener(OnHostClicked);
            
            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(btnObj.transform, false);
            
            RectTransform textRect = textObj.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.sizeDelta = Vector2.zero;
            
            Text text = textObj.AddComponent<Text>();
            text.text = "HOST GAME";
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = 24;
            text.color = Color.white;
            text.alignment = TextAnchor.MiddleCenter;
        }
        
        private void CreateJoinButton()
        {
            GameObject btnObj = new GameObject("JoinButton");
            btnObj.transform.SetParent(_menuPanel.transform, false);
            
            RectTransform rect = btnObj.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.25f);
            rect.anchorMax = new Vector2(0.5f, 0.25f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = new Vector2(350, 50);
            
            Image img = btnObj.AddComponent<Image>();
            img.color = new Color(0.2f, 0.4f, 0.6f);
            
            _joinButton = btnObj.AddComponent<Button>();
            _joinButton.targetGraphic = img;
            _joinButton.onClick.AddListener(OnJoinClicked);
            
            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(btnObj.transform, false);
            
            RectTransform textRect = textObj.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.sizeDelta = Vector2.zero;
            
            Text text = textObj.AddComponent<Text>();
            text.text = "JOIN GAME";
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = 24;
            text.color = Color.white;
            text.alignment = TextAnchor.MiddleCenter;
        }
        
        private void CreateStatusText()
        {
            GameObject textObj = new GameObject("StatusText");
            textObj.transform.SetParent(_menuPanel.transform, false);
            
            RectTransform rect = textObj.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0, 0);
            rect.anchorMax = new Vector2(1, 0);
            rect.pivot = new Vector2(0.5f, 0);
            rect.anchoredPosition = new Vector2(0, 10);
            rect.sizeDelta = new Vector2(-20, 40);
            
            _statusText = textObj.AddComponent<Text>();
            _statusText.text = "";
            _statusText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            _statusText.fontSize = 14;
            _statusText.color = Color.yellow;
            _statusText.alignment = TextAnchor.LowerCenter;
        }
        
        private async void OnHostClicked()
        {
            if (_networkManager == null) return;
            
            _statusText.text = "Starting server...";
            bool success = await _networkManager.StartHostAsync();
            
            if (success)
            {
                _statusText.text = "Server started! Waiting for players...";
                _statusText.color = Color.green;
                _menuPanel.SetActive(false);
            }
            else
            {
                _statusText.text = "Failed to start server";
                _statusText.color = Color.red;
            }
        }
        
        private async void OnJoinClicked()
        {
            if (_networkManager == null) return;
            
            string serverAddress = _serverAddressInput.text;
            if (string.IsNullOrEmpty(serverAddress))
            {
                _statusText.text = "Please enter server address";
                _statusText.color = Color.red;
                return;
            }
            
            _statusText.text = $"Connecting to {serverAddress}...";
            bool success = await _networkManager.ConnectToServerAsync(serverAddress);
            
            if (success)
            {
                _statusText.text = "Connected!";
                _statusText.color = Color.green;
                _menuPanel.SetActive(false);
            }
            else
            {
                _statusText.text = "Failed to connect";
                _statusText.color = Color.red;
            }
        }
        
        public void ShowMenu()
        {
            _menuPanel.SetActive(true);
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        
        public void HideMenu()
        {
            _menuPanel.SetActive(false);
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        
        private void OnPlayerConnected(string playerId)
        {
            Debug.Log($"[NetworkUI] Player connected: {playerId}");
        }
        
        private void OnPlayerDisconnected(string playerId)
        {
            Debug.Log($"[NetworkUI] Player disconnected: {playerId}");
        }
        
        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.M))
            {
                if (_menuPanel.activeSelf)
                    HideMenu();
                else
                    ShowMenu();
            }
        }
    }
}