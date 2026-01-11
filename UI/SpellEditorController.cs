using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

namespace Game.Core.Systems
{
    /// <summary>
    /// Spell Editor Controller - Create and edit magic spells.
    /// </summary>
    public class SpellEditorController : MonoBehaviour
    {
        private GameObject _contentPanel;
        private SpellData _currentSpell;
        
        private InputField _spellNameInput;
        private Dropdown _spellTypeDropdown;
        private InputField _manaCostInput;
        private InputField _cooldownInput;
        private InputField _damageInput;
        private InputField _rangeInput;
        private Dropdown _elementDropdown;
        private Text _previewText;
        
        private List<SpellData> _spellLibrary = new();
        
        public void Initialize(GameObject contentPanel)
        {
            _contentPanel = contentPanel;
            LoadSpellLibrary();
            CreateUI();
            CreateNewSpell();
        }
        
        private void CreateUI()
        {
            Transform content = CreateScrollableContent();
            
            CreateHeader(content, "SPELL EDITOR");
            
            CreateInputField(content, "Spell Name:", out _spellNameInput);
            _spellNameInput.onValueChanged.AddListener(v => UpdatePreview());
            
            CreateDropdown(content, "Type:", out _spellTypeDropdown, 
                new List<string> { "Damage", "Healing", "Buff", "Debuff", "Summon", "Utility" });
            _spellTypeDropdown.onValueChanged.AddListener(v => UpdatePreview());
            
            CreateDropdown(content, "Element:", out _elementDropdown, 
                System.Enum.GetNames(typeof(DamageElement)));
            _elementDropdown.onValueChanged.AddListener(v => UpdatePreview());
            
            CreateInputField(content, "Mana Cost:", out _manaCostInput);
            _manaCostInput.onValueChanged.AddListener(v => UpdatePreview());
            
            CreateInputField(content, "Cooldown (s):", out _cooldownInput);
            _cooldownInput.onValueChanged.AddListener(v => UpdatePreview());
            
            CreateInputField(content, "Damage:", out _damageInput);
            _damageInput.onValueChanged.AddListener(v => UpdatePreview());
            
            CreateInputField(content, "Range:", out _rangeInput);
            _rangeInput.onValueChanged.AddListener(v => UpdatePreview());
            
            CreateButton(content, "✨ NEW SPELL", () => CreateNewSpell());
            CreateButton(content, "💾 SAVE SPELL", () => SaveSpell());
            CreateButton(content, "📚 VIEW LIBRARY", () => ShowSpellLibrary());
            
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
            headerText.color = new Color(0.6f, 0.3f, 1f);
            headerText.alignment = TextAnchor.MiddleCenter;
        }
        
        private void CreateDropdown(Transform parent, string label, out Dropdown dropdown, System.Array options)
        {
            List<string> optionsList = new List<string>();
            foreach (var option in options)
            {
                optionsList.Add(option.ToString());
            }
            CreateDropdown(parent, label, out dropdown, optionsList);
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
            btnImg.color = new Color(0.4f, 0.2f, 0.6f);
            
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
        
        private void CreateNewSpell()
        {
            _currentSpell = new SpellData
            {
                spellId = System.Guid.NewGuid().ToString(),
                spellName = "New Spell",
                manaCost = 50,
                cooldown = 5f,
                damage = 100,
                range = 20f,
                element = DamageElement.Fire
            };
            
            _spellNameInput.text = _currentSpell.spellName;
            _manaCostInput.text = _currentSpell.manaCost.ToString();
            _cooldownInput.text = _currentSpell.cooldown.ToString();
            _damageInput.text = _currentSpell.damage.ToString();
            _rangeInput.text = _currentSpell.range.ToString();
            _elementDropdown.value = (int)_currentSpell.element;
            
            UpdatePreview();
        }
        
        private void UpdatePreview()
        {
            if (_currentSpell == null) return;
            
            _currentSpell.spellName = _spellNameInput.text;
            
            if (int.TryParse(_manaCostInput.text, out int mana))
                _currentSpell.manaCost = mana;
            
            if (float.TryParse(_cooldownInput.text, out float cd))
                _currentSpell.cooldown = cd;
            
            if (int.TryParse(_damageInput.text, out int dmg))
                _currentSpell.damage = dmg;
            
            if (float.TryParse(_rangeInput.text, out float range))
                _currentSpell.range = range;
            
            _currentSpell.element = (DamageElement)_elementDropdown.value;
            
            Color elementColor = GetElementColor(_currentSpell.element);
            string elementHex = ColorUtility.ToHtmlStringRGB(elementColor);
            
            _previewText.text = $"<b><color=#{elementHex}>{_currentSpell.spellName}</color></b>\n" +
                              $"<color=grey>{_currentSpell.element} Spell</color>\n\n" +
                              $"<color=cyan>Mana Cost:</color> {_currentSpell.manaCost}\n" +
                              $"<color=yellow>Cooldown:</color> {_currentSpell.cooldown}s\n" +
                              $"<color=orange>Damage:</color> {_currentSpell.damage}\n" +
                              $"<color=green>Range:</color> {_currentSpell.range}\n\n" +
                              $"<i>Cast this spell to unleash {_currentSpell.element} magic!</i>";
        }
        
        private Color GetElementColor(DamageElement element)
        {
            return element switch
            {
                DamageElement.Fire => new Color(1f, 0.3f, 0f),
                DamageElement.Ice => new Color(0.3f, 0.7f, 1f),
                DamageElement.Lightning => new Color(1f, 1f, 0.3f),
                DamageElement.Poison => new Color(0.4f, 1f, 0.3f),
                DamageElement.Holy => new Color(1f, 1f, 0.8f),
                DamageElement.Dark => new Color(0.5f, 0.2f, 0.7f),
                _ => Color.white
            };
        }
        
        private void SaveSpell()
        {
            string json = JsonUtility.ToJson(_currentSpell, true);
            Debug.Log($"[SpellEditor] Saved spell:\n{json}");
            
            PlayerPrefs.SetString($"Spell_{_currentSpell.spellId}", json);
            PlayerPrefs.Save();
            
            if (!_spellLibrary.Contains(_currentSpell))
            {
                _spellLibrary.Add(_currentSpell);
            }
        }
        
        private void LoadSpellLibrary()
        {
            int spellCount = PlayerPrefs.GetInt("SpellCount", 0);
            
            for (int i = 0; i < spellCount; i++)
            {
                string json = PlayerPrefs.GetString($"SpellLibrary_{i}", "");
                if (!string.IsNullOrEmpty(json))
                {
                    SpellData spell = JsonUtility.FromJson<SpellData>(json);
                    _spellLibrary.Add(spell);
                }
            }
        }
        
        private void ShowSpellLibrary()
        {
            Debug.Log($"[SpellEditor] Spell Library ({_spellLibrary.Count} spells)");
            foreach (SpellData spell in _spellLibrary)
            {
                Debug.Log($"  - {spell.spellName} ({spell.element})");
            }
        }
    }
    
    /// <summary>
    /// Spell data structure for serialization.
    /// </summary>
    [System.Serializable]
    public class SpellData
    {
        public string spellId;
        public string spellName;
        public int manaCost;
        public float cooldown;
        public int damage;
        public float range;
        public DamageElement element;
        public string description;
    }
}