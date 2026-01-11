using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

namespace Game.Core.Systems
{
    /// <summary>
    /// Loot Table Editor for Admin Console - Create and edit loot tables.
    /// </summary>
    public class LootTableEditorController : MonoBehaviour
    {
        private GameObject _contentPanel;
        private LootSystemManager _lootManager;
        
        private InputField _tableNameInput;
        private InputField _minItemsInput;
        private InputField _maxItemsInput;
        private InputField _minLevelInput;
        private InputField _maxLevelInput;
        private InputField _goldMinInput;
        private InputField _goldMaxInput;
        private Text _previewText;
        private Dropdown _tableDropdown;
        
        private LootTable _currentTable;
        
        public void Initialize(GameObject contentPanel)
        {
            _contentPanel = contentPanel;
            _lootManager = FindFirstObjectByType<LootSystemManager>();
            
            if (_lootManager == null)
            {
                GameObject lootObj = new GameObject("LootSystemManager");
                _lootManager = lootObj.AddComponent<LootSystemManager>();
            }
            
            CreateUI();
            CreateNewTable();
        }
        
        private void CreateUI()
        {
            Transform content = CreateScrollableContent();
            
            CreateHeader(content, "LOOT TABLE EDITOR");
            
            // Table selector
            CreateDropdown(content, "Select Table:", out _tableDropdown, GetTableNames());
            _tableDropdown.onValueChanged.AddListener(v => LoadTable(v));
            
            CreateInputField(content, "Table Name:", out _tableNameInput);
            _tableNameInput.onValueChanged.AddListener(v => UpdatePreview());
            
            CreateInputField(content, "Min Items:", out _minItemsInput);
            _minItemsInput.onValueChanged.AddListener(v => UpdatePreview());
            
            CreateInputField(content, "Max Items:", out _maxItemsInput);
            _maxItemsInput.onValueChanged.AddListener(v => UpdatePreview());
            
            CreateInputField(content, "Min Level:", out _minLevelInput);
            _minLevelInput.onValueChanged.AddListener(v => UpdatePreview());
            
            CreateInputField(content, "Max Level:", out _maxLevelInput);
            _maxLevelInput.onValueChanged.AddListener(v => UpdatePreview());
            
            CreateInputField(content, "Gold Min:", out _goldMinInput);
            _goldMinInput.onValueChanged.AddListener(v => UpdatePreview());
            
            CreateInputField(content, "Gold Max:", out _goldMaxInput);
            _goldMaxInput.onValueChanged.AddListener(v => UpdatePreview());
            
            CreateButton(content, "✨ NEW TABLE", () => CreateNewTable());
            CreateButton(content, "💾 SAVE TABLE", () => SaveTable());
            CreateButton(content, "🎲 TEST DROP", () => TestDrop());
            
            CreatePreviewPanel(content);
        }
        
        private Transform CreateScrollableContent()
        {
            ScrollRect scroll = _contentPanel.GetComponent<ScrollRect>();
            if (scroll == null) scroll = _contentPanel.AddComponent<ScrollRect>();
            
            GameObject content = new GameObject("Content");
            content.transform.SetParent(_contentPanel.transform, false);
            
            RectTransform contentRect = content.AddComponent<RectTransform>();
            contentRect.anchorMin = new Vector2(0, 1);
            contentRect.anchorMax = new Vector2(1, 1);
            contentRect.pivot = new Vector2(0.5f, 1);
            contentRect.sizeDelta = new Vector2(0, 2000);
            
            VerticalLayoutGroup layout = content.AddComponent<VerticalLayoutGroup>();
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;
            layout.spacing = 15;
            layout.padding = new RectOffset(20, 20, 20, 20);
            
            ContentSizeFitter fitter = content.AddComponent<ContentSizeFitter>();
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            
            scroll.content = contentRect;
            scroll.horizontal = false;
            scroll.vertical = true;
            
            return content.transform;
        }
        
        private void CreateHeader(Transform parent, string text)
        {
            GameObject header = new GameObject("Header");
            header.transform.SetParent(parent, false);
            
            LayoutElement layout = header.AddComponent<LayoutElement>();
            layout.preferredHeight = 60;
            
            Text headerText = header.AddComponent<Text>();
            headerText.text = text;
            headerText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            headerText.fontSize = 28;
            headerText.color = new Color(1f, 0.8f, 0.2f);
            headerText.alignment = TextAnchor.MiddleCenter;
        }
        
        private void CreateDropdown(Transform parent, string label, out Dropdown dropdown, List<string> options)
        {
            GameObject row = new GameObject($"Row_{label}");
            row.transform.SetParent(parent, false);
            
            LayoutElement rowLayout = row.AddComponent<LayoutElement>();
            rowLayout.preferredHeight = 50;
            
            HorizontalLayoutGroup rowHLayout = row.AddComponent<HorizontalLayoutGroup>();
            rowHLayout.childForceExpandWidth = true;
            rowHLayout.spacing = 10;
            
            GameObject labelObj = new GameObject("Label");
            labelObj.transform.SetParent(row.transform, false);
            
            LayoutElement labelLayout = labelObj.AddComponent<LayoutElement>();
            labelLayout.preferredWidth = 200;
            
            Text labelText = labelObj.AddComponent<Text>();
            labelText.text = label;
            labelText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            labelText.fontSize = 18;
            labelText.color = Color.white;
            labelText.alignment = TextAnchor.MiddleLeft;
            
            GameObject dropdownObj = new GameObject("Dropdown");
            dropdownObj.transform.SetParent(row.transform, false);
            
            Image dropdownImg = dropdownObj.AddComponent<Image>();
            dropdownImg.color = new Color(0.2f, 0.2f, 0.3f);
            
            dropdown = dropdownObj.AddComponent<Dropdown>();
            dropdown.ClearOptions();
            dropdown.AddOptions(options);
            
            GameObject dropdownLabel = new GameObject("Label");
            dropdownLabel.transform.SetParent(dropdownObj.transform, false);
            
            RectTransform dropdownLabelRect = dropdownLabel.AddComponent<RectTransform>();
            dropdownLabelRect.anchorMin = Vector2.zero;
            dropdownLabelRect.anchorMax = Vector2.one;
            dropdownLabelRect.offsetMin = new Vector2(10, 2);
            dropdownLabelRect.offsetMax = new Vector2(-25, -2);
            
            Text dropdownLabelText = dropdownLabel.AddComponent<Text>();
            dropdownLabelText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            dropdownLabelText.fontSize = 16;
            dropdownLabelText.color = Color.white;
            dropdownLabelText.alignment = TextAnchor.MiddleLeft;
            
            dropdown.captionText = dropdownLabelText;
        }
        
        private void CreateInputField(Transform parent, string label, out InputField inputField)
        {
            GameObject row = new GameObject($"Row_{label}");
            row.transform.SetParent(parent, false);
            
            LayoutElement rowLayout = row.AddComponent<LayoutElement>();
            rowLayout.preferredHeight = 50;
            
            HorizontalLayoutGroup rowHLayout = row.AddComponent<HorizontalLayoutGroup>();
            rowHLayout.childForceExpandWidth = true;
            rowHLayout.spacing = 10;
            
            GameObject labelObj = new GameObject("Label");
            labelObj.transform.SetParent(row.transform, false);
            
            LayoutElement labelLayout = labelObj.AddComponent<LayoutElement>();
            labelLayout.preferredWidth = 200;
            
            Text labelText = labelObj.AddComponent<Text>();
            labelText.text = label;
            labelText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            labelText.fontSize = 18;
            labelText.color = Color.white;
            labelText.alignment = TextAnchor.MiddleLeft;
            
            GameObject inputObj = new GameObject("InputField");
            inputObj.transform.SetParent(row.transform, false);
            
            Image inputImg = inputObj.AddComponent<Image>();
            inputImg.color = new Color(0.2f, 0.2f, 0.3f);
            
            inputField = inputObj.AddComponent<InputField>();
            
            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(inputObj.transform, false);
            
            RectTransform textRect = textObj.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = new Vector2(10, 5);
            textRect.offsetMax = new Vector2(-10, -5);
            
            Text text = textObj.AddComponent<Text>();
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = 16;
            text.color = Color.white;
            text.alignment = TextAnchor.MiddleLeft;
            
            inputField.textComponent = text;
        }
        
        private void CreateButton(Transform parent, string label, UnityEngine.Events.UnityAction onClick)
        {
            GameObject btnObj = new GameObject($"Btn_{label}");
            btnObj.transform.SetParent(parent, false);
            
            LayoutElement btnLayout = btnObj.AddComponent<LayoutElement>();
            btnLayout.preferredHeight = 60;
            
            Image btnImg = btnObj.AddComponent<Image>();
            btnImg.color = new Color(0.6f, 0.4f, 0.2f);
            
            Button btn = btnObj.AddComponent<Button>();
            btn.targetGraphic = btnImg;
            btn.onClick.AddListener(onClick);
            
            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(btnObj.transform, false);
            
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
        }
        
        private void CreatePreviewPanel(Transform parent)
        {
            GameObject panel = new GameObject("PreviewPanel");
            panel.transform.SetParent(parent, false);
            
            LayoutElement panelLayout = panel.AddComponent<LayoutElement>();
            panelLayout.preferredHeight = 300;
            
            Image panelImg = panel.AddComponent<Image>();
            panelImg.color = new Color(0.1f, 0.1f, 0.15f);
            
            GameObject textObj = new GameObject("PreviewText");
            textObj.transform.SetParent(panel.transform, false);
            
            RectTransform textRect = textObj.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = new Vector2(15, 15);
            textRect.offsetMax = new Vector2(-15, -15);
            
            _previewText = textObj.AddComponent<Text>();
            _previewText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            _previewText.fontSize = 16;
            _previewText.color = Color.white;
            _previewText.alignment = TextAnchor.UpperLeft;
        }
        
        private List<string> GetTableNames()
        {
            List<string> names = new List<string> { "New Table" };
            
            if (_lootManager != null)
            {
                names.AddRange(_lootManager.GetAvailableLootTables());
            }
            
            return names;
        }
        
        private void LoadTable(int index)
        {
            if (index == 0)
            {
                CreateNewTable();
                return;
            }
            
            List<string> tables = _lootManager.GetAvailableLootTables();
            if (index - 1 < tables.Count)
            {
                // TODO: Load existing table
                Debug.Log($"[LootTableEditor] Load table: {tables[index - 1]}");
            }
        }
        
        private void CreateNewTable()
        {
            _currentTable = new LootTable
            {
                tableName = "NewLootTable",
                minItems = 1,
                maxItems = 3,
                levelRange = new Vector2Int(1, 10),
                goldMin = 10,
                goldMax = 50
            };
            
            _tableNameInput.text = _currentTable.tableName;
            _minItemsInput.text = _currentTable.minItems.ToString();
            _maxItemsInput.text = _currentTable.maxItems.ToString();
            _minLevelInput.text = _currentTable.levelRange.x.ToString();
            _maxLevelInput.text = _currentTable.levelRange.y.ToString();
            _goldMinInput.text = _currentTable.goldMin.ToString();
            _goldMaxInput.text = _currentTable.goldMax.ToString();
            
            UpdatePreview();
        }
        
        private void UpdatePreview()
        {
            if (_currentTable == null) return;
            
            _currentTable.tableName = _tableNameInput.text;
            
            if (int.TryParse(_minItemsInput.text, out int minItems))
                _currentTable.minItems = minItems;
            
            if (int.TryParse(_maxItemsInput.text, out int maxItems))
                _currentTable.maxItems = maxItems;
            
            if (int.TryParse(_minLevelInput.text, out int minLevel))
                _currentTable.levelRange.x = minLevel;
            
            if (int.TryParse(_maxLevelInput.text, out int maxLevel))
                _currentTable.levelRange.y = maxLevel;
            
            if (int.TryParse(_goldMinInput.text, out int goldMin))
                _currentTable.goldMin = goldMin;
            
            if (int.TryParse(_goldMaxInput.text, out int goldMax))
                _currentTable.goldMax = goldMax;
            
            _previewText.text = $"<b><color=yellow>{_currentTable.tableName}</color></b>\n\n" +
                              $"<color=cyan>Items:</color> {_currentTable.minItems} - {_currentTable.maxItems}\n" +
                              $"<color=green>Level Range:</color> {_currentTable.levelRange.x} - {_currentTable.levelRange.y}\n" +
                              $"<color=yellow>Gold:</color> {_currentTable.goldMin} - {_currentTable.goldMax}\n\n" +
                              $"<i>This table will generate between {_currentTable.minItems} and {_currentTable.maxItems} items</i>";
        }
        
        private void SaveTable()
        {
            if (_lootManager != null && _currentTable != null)
            {
                _lootManager.AddLootTable(_currentTable);
                Debug.Log($"[LootTableEditor] Saved table: {_currentTable.tableName}");
                
                // Refresh dropdown
                _tableDropdown.ClearOptions();
                _tableDropdown.AddOptions(GetTableNames());
            }
        }
        
        private void TestDrop()
        {
            if (_lootManager == null || _currentTable == null) return;
            
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            Vector3 dropPosition = player != null ? player.transform.position + Vector3.forward * 3f : Vector3.zero;
            
            // Generate test loot
            List<ItemData> loot = _lootManager.GenerateLoot(_currentTable.tableName);
            int gold = Random.Range(_currentTable.goldMin, _currentTable.goldMax + 1);
            
            // Drop it
            _lootManager.DropLoot(loot, dropPosition, gold);
            
            Debug.Log($"[LootTableEditor] Test drop: {loot.Count} items, {gold} gold at {dropPosition}");
        }
    }
}