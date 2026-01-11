using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

namespace Game.UI
{
    /// <summary>
    /// Quest UI - PURE VIEW LAYER
    /// References QuestSystemManager from Game.Core.Systems
    /// NO business logic - only UI display and user interaction
    /// </summary>
    public class QuestUI : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private Canvas _canvas;
        [SerializeField] private GameObject _questPanel;
        [SerializeField] private Transform _questListContainer;
        [SerializeField] private GameObject _questEntryPrefab;
        
        [Header("Detail View")]
        [SerializeField] private GameObject _questDetailPanel;
        [SerializeField] private Text _questTitleText;
        [SerializeField] private Text _questDescriptionText;
        [SerializeField] private Text _questObjectivesText;
        [SerializeField] private Text _questRewardsText;
        [SerializeField] private Button _acceptButton;
        [SerializeField] private Button _abandonButton;
        
        private Game.Core.Systems.QuestSystemManager _questManager;
        private bool _isOpen;
        
        #region Initialization
        
        private void Awake()
        {
            CreateUI();
        }
        
        private void Start()
        {
            _questManager = Game.Core.Systems.CoreSystemManager.Instance?.GetComponent<Game.Core.Systems.QuestSystemManager>();
            
            if (_questManager != null)
            {
                _questManager.OnQuestStarted += OnQuestStarted;
                _questManager.OnQuestCompleted += OnQuestCompleted;
                _questManager.OnObjectiveCompleted += OnObjectiveCompleted;
            }
            else
            {
                Debug.LogWarning("[QuestUI] QuestSystemManager not found!");
            }
            
            _questPanel?.SetActive(false);
            _questDetailPanel?.SetActive(false);
        }
        
        private void OnDestroy()
        {
            if (_questManager != null)
            {
                _questManager.OnQuestStarted -= OnQuestStarted;
                _questManager.OnQuestCompleted -= OnQuestCompleted;
                _questManager.OnObjectiveCompleted -= OnObjectiveCompleted;
            }
        }
        
        #endregion
        
        #region UI Creation
        
        private void CreateUI()
        {
            if (_canvas == null)
            {
                GameObject canvasObj = new GameObject("QuestCanvas");
                canvasObj.transform.SetParent(transform, false);
                
                _canvas = canvasObj.AddComponent<Canvas>();
                _canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                _canvas.sortingOrder = 15;
                
                CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1920, 1080);
                
                canvasObj.AddComponent<GraphicRaycaster>();
            }
            
            CreateQuestPanel();
            CreateQuestDetailPanel();
        }
        
        private void CreateQuestPanel()
        {
            _questPanel = new GameObject("QuestPanel");
            _questPanel.transform.SetParent(_canvas.transform, false);
            
            RectTransform rect = _questPanel.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.7f, 0.2f);
            rect.anchorMax = new Vector2(0.95f, 0.8f);
            rect.sizeDelta = Vector2.zero;
            
            Image bg = _questPanel.AddComponent<Image>();
            bg.color = new Color(0.1f, 0.1f, 0.15f, 0.95f);
            
            CreateTitle(_questPanel.transform, "QUESTS");
            CreateCloseButton(_questPanel.transform);
            CreateQuestList();
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
            btn.onClick.AddListener(ToggleQuestLog);
            
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
        
        private void CreateQuestList()
        {
            GameObject scrollView = new GameObject("ScrollView");
            scrollView.transform.SetParent(_questPanel.transform, false);
            
            RectTransform scrollRect = scrollView.AddComponent<RectTransform>();
            scrollRect.anchorMin = new Vector2(0, 0);
            scrollRect.anchorMax = new Vector2(1, 1);
            scrollRect.offsetMin = new Vector2(10, 10);
            scrollRect.offsetMax = new Vector2(-10, -70);
            
            ScrollRect scroll = scrollView.AddComponent<ScrollRect>();
            scroll.horizontal = false;
            scroll.vertical = true;
            
            GameObject content = new GameObject("Content");
            content.transform.SetParent(scrollView.transform, false);
            
            RectTransform contentRect = content.AddComponent<RectTransform>();
            contentRect.anchorMin = new Vector2(0, 1);
            contentRect.anchorMax = new Vector2(1, 1);
            contentRect.pivot = new Vector2(0.5f, 1);
            contentRect.sizeDelta = new Vector2(0, 1000);
            
            VerticalLayoutGroup layout = content.AddComponent<VerticalLayoutGroup>();
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;
            layout.spacing = 5;
            layout.padding = new RectOffset(5, 5, 5, 5);
            
            ContentSizeFitter fitter = content.AddComponent<ContentSizeFitter>();
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            
            scroll.content = contentRect;
            _questListContainer = content.transform;
        }
        
        private void CreateQuestDetailPanel()
        {
            _questDetailPanel = new GameObject("QuestDetailPanel");
            _questDetailPanel.transform.SetParent(_canvas.transform, false);
            
            RectTransform rect = _questDetailPanel.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.3f, 0.2f);
            rect.anchorMax = new Vector2(0.65f, 0.8f);
            rect.sizeDelta = Vector2.zero;
            
            Image bg = _questDetailPanel.AddComponent<Image>();
            bg.color = new Color(0.05f, 0.05f, 0.1f, 0.95f);
            
            // Title
            CreateDetailText("TitleText", new Vector2(0.5f, 0.9f), new Vector2(-20, 60), 28, out _questTitleText);
            
            // Description
            CreateDetailText("DescriptionText", new Vector2(0.5f, 0.7f), new Vector2(-20, 150), 16, out _questDescriptionText);
            _questDescriptionText.alignment = TextAnchor.UpperLeft;
            
            // Objectives
            CreateDetailText("ObjectivesText", new Vector2(0.5f, 0.4f), new Vector2(-20, 200), 16, out _questObjectivesText);
            _questObjectivesText.alignment = TextAnchor.UpperLeft;
            
            // Rewards
            CreateDetailText("RewardsText", new Vector2(0.5f, 0.15f), new Vector2(-20, 80), 16, out _questRewardsText);
            _questRewardsText.alignment = TextAnchor.UpperLeft;
            
            // Buttons
            CreateAcceptButton();
            CreateAbandonButton();
        }
        
        private void CreateDetailText(string name, Vector2 anchorPos, Vector2 size, int fontSize, out Text textComponent)
        {
            GameObject textObj = new GameObject(name);
            textObj.transform.SetParent(_questDetailPanel.transform, false);
            
            RectTransform rect = textObj.AddComponent<RectTransform>();
            rect.anchorMin = anchorPos;
            rect.anchorMax = anchorPos;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = size;
            
            textComponent = textObj.AddComponent<Text>();
            textComponent.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            textComponent.fontSize = fontSize;
            textComponent.color = Color.white;
            textComponent.alignment = TextAnchor.MiddleCenter;
        }
        
        private void CreateAcceptButton()
        {
            GameObject btnObj = new GameObject("AcceptButton");
            btnObj.transform.SetParent(_questDetailPanel.transform, false);
            
            RectTransform rect = btnObj.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.25f, 0.05f);
            rect.anchorMax = new Vector2(0.25f, 0.05f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = new Vector2(150, 50);
            
            Image img = btnObj.AddComponent<Image>();
            img.color = new Color(0.2f, 0.6f, 0.2f);
            
            _acceptButton = btnObj.AddComponent<Button>();
            _acceptButton.targetGraphic = img;
            _acceptButton.onClick.AddListener(OnAcceptButtonClicked);
            
            CreateButtonText(btnObj.transform, "ACCEPT");
        }
        
        private void CreateAbandonButton()
        {
            GameObject btnObj = new GameObject("AbandonButton");
            btnObj.transform.SetParent(_questDetailPanel.transform, false);
            
            RectTransform rect = btnObj.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.75f, 0.05f);
            rect.anchorMax = new Vector2(0.75f, 0.05f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = new Vector2(150, 50);
            
            Image img = btnObj.AddComponent<Image>();
            img.color = new Color(0.6f, 0.2f, 0.2f);
            
            _abandonButton = btnObj.AddComponent<Button>();
            _abandonButton.targetGraphic = img;
            _abandonButton.onClick.AddListener(OnAbandonButtonClicked);
            
            CreateButtonText(btnObj.transform, "ABANDON");
        }
        
        private void CreateButtonText(Transform parent, string text)
        {
            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(parent, false);
            
            RectTransform textRect = textObj.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.sizeDelta = Vector2.zero;
            
            Text btnText = textObj.AddComponent<Text>();
            btnText.text = text;
            btnText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            btnText.fontSize = 20;
            btnText.color = Color.white;
            btnText.alignment = TextAnchor.MiddleCenter;
        }
        
        #endregion
        
        #region UI Management
        
        public void ToggleQuestLog()
        {
            _isOpen = !_isOpen;
            _questPanel?.SetActive(_isOpen);
            
            if (_isOpen)
            {
                RefreshQuestList();
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
            else
            {
                _questDetailPanel?.SetActive(false);
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
        }
        
        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Q))
            {
                ToggleQuestLog();
            }
            
            if (Input.GetKeyDown(KeyCode.Escape) && _isOpen)
            {
                ToggleQuestLog();
            }
        }
        
        #endregion
        
        #region Quest Display
        
        private void RefreshQuestList()
        {
            if (_questManager == null || _questListContainer == null) return;
            
            // Clear existing entries
            foreach (Transform child in _questListContainer)
            {
                Destroy(child.gameObject);
            }
            
            // Add active quests
            List<Game.Core.Systems.QuestData> activeQuests = _questManager.GetActiveQuests();
            foreach (var quest in activeQuests)
            {
                CreateQuestEntry(quest, true);
            }
            
            // Add available quests
            List<Game.Core.Systems.QuestData> availableQuests = _questManager.GetAvailableQuests();
            foreach (var quest in availableQuests)
            {
                CreateQuestEntry(quest, false);
            }
        }
        
        private void CreateQuestEntry(Game.Core.Systems.QuestData quest, bool isActive)
        {
            GameObject entry = new GameObject($"Quest_{quest.questId}");
            entry.transform.SetParent(_questListContainer, false);
            
            LayoutElement layout = entry.AddComponent<LayoutElement>();
            layout.preferredHeight = 60;
            
            Image bg = entry.AddComponent<Image>();
            bg.color = isActive ? new Color(0.2f, 0.3f, 0.2f) : new Color(0.15f, 0.15f, 0.2f);
            
            Button btn = entry.AddComponent<Button>();
            btn.targetGraphic = bg;
            btn.onClick.AddListener(() => ShowQuestDetails(quest));
            
            // Quest name
            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(entry.transform, false);
            
            RectTransform textRect = textObj.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = new Vector2(10, 5);
            textRect.offsetMax = new Vector2(-10, -5);
            
            Text text = textObj.AddComponent<Text>();
            text.text = $"{quest.questName}\n<size=12><color=grey>Level {quest.level} - {quest.questType}</color></size>";
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = 16;
            text.color = Color.white;
            text.alignment = TextAnchor.MiddleLeft;
        }
        
        private Game.Core.Systems.QuestData _currentDetailQuest;
        
        private void ShowQuestDetails(Game.Core.Systems.QuestData quest)
        {
            _currentDetailQuest = quest;
            _questDetailPanel?.SetActive(true);
            
            _questTitleText.text = quest.questName;
            _questDescriptionText.text = quest.questDescription;
            
            // Objectives
            string objectivesText = "<b>OBJECTIVES:</b>\n";
            foreach (var objective in quest.objectives)
            {
                string checkmark = objective.isCompleted ? "✓" : "○";
                objectivesText += $"{checkmark} {objective.description} ({objective.currentCount}/{objective.requiredCount})\n";
            }
            _questObjectivesText.text = objectivesText;
            
            // Rewards
            string rewardsText = "<b>REWARDS:</b>\n";
            if (quest.experienceReward > 0)
                rewardsText += $"<color=cyan>+{quest.experienceReward} XP</color>\n";
            if (quest.goldReward > 0)
                rewardsText += $"<color=yellow>+{quest.goldReward} Gold</color>\n";
            _questRewardsText.text = rewardsText;
            
            // Button states
            bool isActive = _questManager.IsQuestActive(quest.questId);
            _acceptButton.gameObject.SetActive(!isActive && quest.status == Game.Core.Systems.QuestStatus.Available);
            _abandonButton.gameObject.SetActive(isActive);
        }
        
        #endregion
        
        #region Button Handlers
        
        private void OnAcceptButtonClicked()
        {
            if (_currentDetailQuest != null && _questManager != null)
            {
                _questManager.StartQuest(_currentDetailQuest.questId);
                _questDetailPanel?.SetActive(false);
                RefreshQuestList();
            }
        }
        
        private void OnAbandonButtonClicked()
        {
            if (_currentDetailQuest != null && _questManager != null)
            {
                _questManager.AbandonQuest(_currentDetailQuest.questId);
                _questDetailPanel?.SetActive(false);
                RefreshQuestList();
            }
        }
        
        #endregion
        
        #region Event Handlers
        
        private void OnQuestStarted(Game.Core.Systems.QuestData quest)
        {
            Debug.Log($"[QuestUI] Quest started: {quest.questName}");
            if (_isOpen) RefreshQuestList();
        }
        
        private void OnQuestCompleted(Game.Core.Systems.QuestData quest)
        {
            Debug.Log($"[QuestUI] Quest completed: {quest.questName}");
            if (_isOpen) RefreshQuestList();
            
            // Show completion notification
            ShowQuestCompletionNotification(quest);
        }
        
        private void OnObjectiveCompleted(Game.Core.Systems.QuestData quest, Game.Core.Systems.QuestObjective objective)
        {
            Debug.Log($"[QuestUI] Objective completed: {objective.description}");
            if (_isOpen && _currentDetailQuest?.questId == quest.questId)
            {
                ShowQuestDetails(quest);
            }
        }
        
        private void ShowQuestCompletionNotification(Game.Core.Systems.QuestData quest)
        {
            // TODO: Implement fancy completion notification
            Debug.Log($"<color=yellow>QUEST COMPLETED: {quest.questName}</color>");
        }
        
        #endregion
    }
}