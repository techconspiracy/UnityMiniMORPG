using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Game.Core.Systems;
using System.Collections.Generic;

namespace Game.UI
{
    public class CharacterCreationUI : MonoBehaviour
    {
        [Header("Navigation")]
        [SerializeField] private Button _nextButton;
        [SerializeField] private Button _backButton;
        [SerializeField] private Button _startGameButton;
        
        private CharacterCreationData _characterData;
        private int _currentStep;
        private int _attributePoints = 20;
        private GameObject _previewCharacter;
        private ProceduralCharacterBuilder _characterBuilder;
        private Canvas _mainCanvas;
        
        private List<GameObject> _allPanels = new List<GameObject>();
        
        private void Awake()
        {
            _characterData = new CharacterCreationData();
            _currentStep = 0;
            
            _characterBuilder = FindFirstObjectByType<ProceduralCharacterBuilder>();
            if (_characterBuilder == null)
            {
                GameObject builderObj = new GameObject("CharacterBuilder");
                _characterBuilder = builderObj.AddComponent<ProceduralCharacterBuilder>();
            }
            
            InitializeUI();
        }
        
        private void Start()
        {
            ShowStep(_currentStep);
        }
        
        private void InitializeUI()
        {
            _mainCanvas = GetComponent<Canvas>();
            if (_mainCanvas == null)
            {
                _mainCanvas = gameObject.AddComponent<Canvas>();
                _mainCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
                
                CanvasScaler scaler = gameObject.AddComponent<CanvasScaler>();
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1920, 1080);
                
                gameObject.AddComponent<GraphicRaycaster>();
            }
            
            CreateAllPanels();
            CreateNavigationButtons();
        }
        
        private void CreateAllPanels()
        {
            _allPanels.Add(CreateWelcomePanel());
            _allPanels.Add(CreateSpeciesPanel());
            _allPanels.Add(CreateCustomizationPanel());
            _allPanels.Add(CreateAttributesPanel());
            _allPanels.Add(CreateConfirmPanel());
        }
        
        private GameObject CreateWelcomePanel()
        {
            GameObject panel = CreatePanel("WelcomePanel");
            
            CreateText(panel.transform, "CREATE YOUR HERO", 
                new Vector2(0.5f, 0.6f), new Vector2(800, 100), 60, Color.white);
            
            CreateText(panel.transform, "Begin your epic adventure", 
                new Vector2(0.5f, 0.5f), new Vector2(600, 50), 24, Color.gray);
            
            Button startBtn = CreateButton(panel.transform, "START", 
                new Vector2(0.5f, 0.35f), new Vector2(300, 60), () => NextStep());
            
            return panel;
        }
        
        private GameObject CreateSpeciesPanel()
        {
            GameObject panel = CreatePanel("SpeciesPanel");
            
            CreateText(panel.transform, "SELECT YOUR SPECIES", 
                new Vector2(0.5f, 0.9f), new Vector2(800, 60), 40, Color.white);
            
            Species[] species = (Species[])System.Enum.GetValues(typeof(Species));
            int columns = 2;
            float startY = 0.75f;
            float spacing = 0.08f;
            
            for (int i = 0; i < species.Length; i++)
            {
                int row = i / columns;
                int col = i % columns;
                
                float x = 0.3f + (col * 0.4f);
                float y = startY - (row * spacing);
                
                Species currentSpecies = species[i];
                CreateButton(panel.transform, species[i].ToString(), 
                    new Vector2(x, y), new Vector2(350, 50), 
                    () => SelectSpecies(currentSpecies));
            }
            
            CreateText(panel.transform, "Choose wisely - each species has unique abilities!", 
                new Vector2(0.5f, 0.1f), new Vector2(700, 40), 18, Color.yellow);
            
            return panel;
        }
        
        private GameObject CreateCustomizationPanel()
        {
            GameObject panel = CreatePanel("CustomizationPanel");
            
            CreateText(panel.transform, "CUSTOMIZE APPEARANCE", 
                new Vector2(0.5f, 0.9f), new Vector2(800, 60), 40, Color.white);
            
            CreateText(panel.transform, "Gender:", new Vector2(0.25f, 0.7f), 
                new Vector2(150, 30), 20, Color.white);
            CreateButton(panel.transform, "Male", new Vector2(0.4f, 0.7f), new Vector2(120, 40), 
                () => { _characterData.gender = Gender.Male; });
            CreateButton(panel.transform, "Female", new Vector2(0.55f, 0.7f), new Vector2(120, 40), 
                () => { _characterData.gender = Gender.Female; });
            
            CreateText(panel.transform, "Build:", new Vector2(0.25f, 0.6f), 
                new Vector2(150, 30), 20, Color.white);
            CreateButton(panel.transform, "Slim", new Vector2(0.35f, 0.6f), new Vector2(100, 40), 
                () => { _characterData.bodyType = BodyType.Slim; });
            CreateButton(panel.transform, "Average", new Vector2(0.5f, 0.6f), new Vector2(100, 40), 
                () => { _characterData.bodyType = BodyType.Average; });
            CreateButton(panel.transform, "Muscular", new Vector2(0.65f, 0.6f), new Vector2(100, 40), 
                () => { _characterData.bodyType = BodyType.Muscular; });
            
            return panel;
        }
        
        private GameObject CreateAttributesPanel()
        {
            GameObject panel = CreatePanel("AttributesPanel");
            
            CreateText(panel.transform, "ALLOCATE ATTRIBUTES", 
                new Vector2(0.5f, 0.9f), new Vector2(800, 60), 40, Color.white);
            
            CreateText(panel.transform, $"Points Remaining: {_attributePoints}", 
                new Vector2(0.5f, 0.8f), new Vector2(400, 40), 24, Color.yellow);
            
            float yStart = 0.7f;
            float yStep = 0.1f;
            
            CreateAttributeRow(panel.transform, "Strength", yStart, 
                v => _characterData.strength = (int)v);
            CreateAttributeRow(panel.transform, "Dexterity", yStart - yStep, 
                v => _characterData.dexterity = (int)v);
            CreateAttributeRow(panel.transform, "Intelligence", yStart - yStep * 2, 
                v => _characterData.intelligence = (int)v);
            CreateAttributeRow(panel.transform, "Vitality", yStart - yStep * 3, 
                v => _characterData.vitality = (int)v);
            CreateAttributeRow(panel.transform, "Endurance", yStart - yStep * 4, 
                v => _characterData.endurance = (int)v);
            CreateAttributeRow(panel.transform, "Luck", yStart - yStep * 5, 
                v => _characterData.luck = (int)v);
            
            return panel;
        }
        
        private void CreateAttributeRow(Transform parent, string name, float yPos, 
            UnityEngine.Events.UnityAction<float> onChanged)
        {
            CreateText(parent, name + ":", new Vector2(0.25f, yPos), 
                new Vector2(150, 30), 18, Color.white);
            
            CreateButton(parent, "-", new Vector2(0.4f, yPos), new Vector2(40, 30), 
                () => AdjustAttribute(name, -1));
            
            CreateText(parent, "10", new Vector2(0.5f, yPos), 
                new Vector2(60, 30), 20, Color.cyan).name = name + "Value";
            
            CreateButton(parent, "+", new Vector2(0.6f, yPos), new Vector2(40, 30), 
                () => AdjustAttribute(name, 1));
        }
        
        private void AdjustAttribute(string attributeName, int delta)
        {
            int currentValue = attributeName switch
            {
                "Strength" => _characterData.strength,
                "Dexterity" => _characterData.dexterity,
                "Intelligence" => _characterData.intelligence,
                "Vitality" => _characterData.vitality,
                "Endurance" => _characterData.endurance,
                "Luck" => _characterData.luck,
                _ => 10
            };
            
            int newValue = Mathf.Clamp(currentValue + delta, 5, 25);
            
            if (newValue == currentValue) return;
            
            int totalUsed = (_characterData.strength + _characterData.dexterity + 
                           _characterData.intelligence + _characterData.vitality + 
                           _characterData.endurance + _characterData.luck) - 60;
            
            if (delta > 0 && totalUsed >= _attributePoints) return;
            
            switch (attributeName)
            {
                case "Strength": _characterData.strength = newValue; break;
                case "Dexterity": _characterData.dexterity = newValue; break;
                case "Intelligence": _characterData.intelligence = newValue; break;
                case "Vitality": _characterData.vitality = newValue; break;
                case "Endurance": _characterData.endurance = newValue; break;
                case "Luck": _characterData.luck = newValue; break;
            }
            
            UpdateAttributeDisplay();
        }
        
        private void UpdateAttributeDisplay()
        {
            UpdateValueText("StrengthValue", _characterData.strength);
            UpdateValueText("DexterityValue", _characterData.dexterity);
            UpdateValueText("IntelligenceValue", _characterData.intelligence);
            UpdateValueText("VitalityValue", _characterData.vitality);
            UpdateValueText("EnduranceValue", _characterData.endurance);
            UpdateValueText("LuckValue", _characterData.luck);
            
            int used = (_characterData.strength + _characterData.dexterity + 
                       _characterData.intelligence + _characterData.vitality + 
                       _characterData.endurance + _characterData.luck) - 60;
            
            Text pointsText = FindTextByContent("Points Remaining:");
            if (pointsText != null)
            {
                int remaining = _attributePoints - used;
                pointsText.text = $"Points Remaining: {remaining}";
                pointsText.color = remaining < 0 ? Color.red : Color.yellow;
            }
        }
        
        private void UpdateValueText(string objName, int value)
        {
            GameObject obj = GameObject.Find(objName);
            if (obj != null)
            {
                Text text = obj.GetComponent<Text>();
                if (text != null) text.text = value.ToString();
            }
        }
        
        private Text FindTextByContent(string content)
        {
            foreach (Text text in FindObjectsByType<Text>(FindObjectsSortMode.None))
            {
                if (text.text.Contains(content)) return text;
            }
            return null;
        }
        
        private GameObject CreateConfirmPanel()
        {
            GameObject panel = CreatePanel("ConfirmPanel");
            
            CreateText(panel.transform, "CONFIRM YOUR HERO", 
                new Vector2(0.5f, 0.9f), new Vector2(800, 60), 40, Color.white);
            
            Text summary = CreateText(panel.transform, "Summary will appear here", 
                new Vector2(0.5f, 0.5f), new Vector2(700, 600), 18, Color.white);
            summary.alignment = TextAnchor.UpperLeft;
            summary.name = "SummaryText";
            
            return panel;
        }
        
        private void CreateNavigationButtons()
        {
            _backButton = CreateButton(transform, "← BACK", 
                new Vector2(0.1f, 0.05f), new Vector2(150, 50), () => PreviousStep());
            
            _nextButton = CreateButton(transform, "NEXT →", 
                new Vector2(0.9f, 0.05f), new Vector2(150, 50), () => NextStep());
            
            _startGameButton = CreateButton(transform, "START GAME!", 
                new Vector2(0.5f, 0.1f), new Vector2(300, 60), () => StartGame());
            _startGameButton.gameObject.SetActive(false);
        }
        
        private GameObject CreatePanel(string name)
        {
            GameObject panel = new GameObject(name);
            panel.transform.SetParent(transform, false);
            
            RectTransform rect = panel.AddComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.sizeDelta = Vector2.zero;
            
            Image img = panel.AddComponent<Image>();
            img.color = new Color(0.1f, 0.1f, 0.15f, 0.95f);
            
            panel.SetActive(false);
            return panel;
        }
        
        private Text CreateText(Transform parent, string content, Vector2 anchorPos, 
            Vector2 size, int fontSize, Color color)
        {
            GameObject obj = new GameObject("Text_" + content.Replace(" ", "_"));
            obj.transform.SetParent(parent, false);
            
            RectTransform rect = obj.AddComponent<RectTransform>();
            rect.anchorMin = anchorPos;
            rect.anchorMax = anchorPos;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = size;
            
            Text text = obj.AddComponent<Text>();
            text.text = content;
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = fontSize;
            text.color = color;
            text.alignment = TextAnchor.MiddleCenter;
            
            return text;
        }
        
        private Button CreateButton(Transform parent, string label, Vector2 anchorPos, 
            Vector2 size, UnityEngine.Events.UnityAction onClick)
        {
            GameObject obj = new GameObject("Btn_" + label.Replace(" ", "_"));
            obj.transform.SetParent(parent, false);
            
            RectTransform rect = obj.AddComponent<RectTransform>();
            rect.anchorMin = anchorPos;
            rect.anchorMax = anchorPos;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = size;
            
            Image img = obj.AddComponent<Image>();
            img.color = new Color(0.3f, 0.5f, 0.8f);
            
            Button btn = obj.AddComponent<Button>();
            btn.targetGraphic = img;
            btn.onClick.AddListener(onClick);
            
            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(obj.transform, false);
            
            RectTransform textRect = textObj.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.sizeDelta = Vector2.zero;
            
            Text text = textObj.AddComponent<Text>();
            text.text = label;
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = 20;
            text.color = Color.white;
            text.alignment = TextAnchor.MiddleCenter;
            
            return btn;
        }
        
        private void SelectSpecies(Species species)
        {
            _characterData.species = species;
            Debug.Log($"Selected species: {species}");
        }
        
        private void ShowStep(int step)
        {
            for (int i = 0; i < _allPanels.Count; i++)
            {
                _allPanels[i].SetActive(i == step);
            }
            
            _backButton.gameObject.SetActive(step > 0 && step < 4);
            _nextButton.gameObject.SetActive(step < 4);
            _startGameButton.gameObject.SetActive(step == 4);
            
            if (step == 4) UpdateConfirmationSummary();
        }
        
        private void UpdateConfirmationSummary()
        {
            Text summary = GameObject.Find("SummaryText")?.GetComponent<Text>();
            if (summary != null)
            {
                summary.text = $"<b>SPECIES:</b> {_characterData.species}\n" +
                              $"<b>GENDER:</b> {_characterData.gender}\n" +
                              $"<b>BUILD:</b> {_characterData.bodyType}\n\n" +
                              $"<b>ATTRIBUTES:</b>\n" +
                              $"Strength: {_characterData.strength}\n" +
                              $"Dexterity: {_characterData.dexterity}\n" +
                              $"Intelligence: {_characterData.intelligence}\n" +
                              $"Vitality: {_characterData.vitality}\n" +
                              $"Endurance: {_characterData.endurance}\n" +
                              $"Luck: {_characterData.luck}\n\n" +
                              $"<color=yellow>Ready to begin your adventure?</color>";
            }
        }
        
        private void NextStep()
        {
            _currentStep++;
            if (_currentStep >= _allPanels.Count) _currentStep = _allPanels.Count - 1;
            ShowStep(_currentStep);
        }
        
        private void PreviousStep()
        {
            _currentStep--;
            if (_currentStep < 0) _currentStep = 0;
            ShowStep(_currentStep);
        }
        
        private async void StartGame()
        {
            // Validate character
            int totalUsed = (_characterData.strength + _characterData.dexterity + 
                           _characterData.intelligence + _characterData.vitality + 
                           _characterData.endurance + _characterData.luck) - 60;
            
            if (totalUsed > _attributePoints)
            {
                Debug.LogError("[CharacterCreation] Too many attribute points allocated!");
                return;
            }
            
            // Save character
            string json = JsonUtility.ToJson(_characterData);
            PlayerPrefs.SetString("CurrentCharacter", json);
            PlayerPrefs.Save();
            
            Debug.Log($"[CharacterCreation] Starting game with {_characterData.species} {_characterData.gender}");
            
            // CRITICAL FIX: Check if scene exists in build settings
            int sceneCount = SceneManager.sceneCountInBuildSettings;
            bool foundGameWorld = false;
            
            for (int i = 0; i < sceneCount; i++)
            {
                string scenePath = SceneUtility.GetScenePathByBuildIndex(i);
                string sceneName = System.IO.Path.GetFileNameWithoutExtension(scenePath);
                
                if (sceneName == "GameWorld")
                {
                    foundGameWorld = true;
                    break;
                }
            }
            
            if (!foundGameWorld)
            {
                Debug.LogError("[CharacterCreation] GameWorld scene not in Build Settings!");
                Debug.LogError("Go to File > Build Settings and add GameWorld.unity");
                return;
            }
            
            // Load game world
            AsyncOperation loadOp = SceneManager.LoadSceneAsync("GameWorld", LoadSceneMode.Single);
            
            if (loadOp == null)
            {
                Debug.LogError("[CharacterCreation] Failed to start loading GameWorld!");
                return;
            }
            
            while (!loadOp.isDone)
            {
                await Awaitable.NextFrameAsync();
            }
            
            Debug.Log("[CharacterCreation] GameWorld loaded successfully");
        }
    }
}