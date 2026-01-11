using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;

namespace Game.Core.Systems
{
    /// <summary>
    /// Weapon Editor Controller - Create and edit weapons with live preview.
    /// </summary>
    public class WeaponEditorController : MonoBehaviour
    {
        private GameObject _contentPanel;
        private ItemGenerationEngine _itemGenerator;
        private ItemData _currentWeapon;
        
        private Dropdown _archetypeDropdown;
        private Dropdown _rarityDropdown;
        private InputField _damageInput;
        private InputField _attackSpeedInput;
        private InputField _rangeInput;
        private Text _previewText;
        
        public void Initialize(GameObject contentPanel)
        {
            _contentPanel = contentPanel;
            _itemGenerator = FindFirstObjectByType<ItemGenerationEngine>();
            
            if (_itemGenerator == null)
            {
                GameObject genObj = new GameObject("ItemGenerationEngine");
                _itemGenerator = genObj.AddComponent<ItemGenerationEngine>();
            }
            
            CreateUI();
            GenerateRandomWeapon();
        }
        
        private void CreateUI()
        {
            Transform content = CreateScrollableContent();
            
            CreateHeader(content, "WEAPON EDITOR");
            
            CreateDropdown(content, "Archetype:", out _archetypeDropdown, 
                System.Enum.GetNames(typeof(WeaponArchetype)).ToList());
            _archetypeDropdown.onValueChanged.AddListener(v => UpdatePreview());
            
            CreateDropdown(content, "Rarity:", out _rarityDropdown, 
                System.Enum.GetNames(typeof(Rarity)).ToList());
            _rarityDropdown.onValueChanged.AddListener(v => UpdatePreview());
            
            CreateInputField(content, "Damage:", out _damageInput);
            _damageInput.onValueChanged.AddListener(v => UpdatePreview());
            
            CreateInputField(content, "Attack Speed:", out _attackSpeedInput);
            _attackSpeedInput.onValueChanged.AddListener(v => UpdatePreview());
            
            CreateInputField(content, "Range:", out _rangeInput);
            _rangeInput.onValueChanged.AddListener(v => UpdatePreview());
            
            CreateButton(content, "🎲 GENERATE RANDOM", () => GenerateRandomWeapon());
            CreateButton(content, "💾 SAVE WEAPON", () => SaveWeapon());
            
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
            headerText.color = Color.yellow;
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
            
            // Label
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
            
            // Dropdown
            GameObject dropdownObj = new GameObject("Dropdown");
            dropdownObj.transform.SetParent(row.transform, false);
            
            Image dropdownImg = dropdownObj.AddComponent<Image>();
            dropdownImg.color = new Color(0.2f, 0.2f, 0.3f);
            
            dropdown = dropdownObj.AddComponent<Dropdown>();
            dropdown.ClearOptions();
            dropdown.AddOptions(options);
            
            // Dropdown label
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
            
            // Label
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
            
            // Input field
            GameObject inputObj = new GameObject("InputField");
            inputObj.transform.SetParent(row.transform, false);
            
            Image inputImg = inputObj.AddComponent<Image>();
            inputImg.color = new Color(0.2f, 0.2f, 0.3f);
            
            inputField = inputObj.AddComponent<InputField>();
            inputField.textComponent = CreateInputText(inputObj.transform);
        }
        
        private Text CreateInputText(Transform parent)
        {
            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(parent, false);
            
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
            
            return text;
        }
        
        private void CreateButton(Transform parent, string label, UnityEngine.Events.UnityAction onClick)
        {
            GameObject btnObj = new GameObject($"Btn_{label}");
            btnObj.transform.SetParent(parent, false);
            
            LayoutElement btnLayout = btnObj.AddComponent<LayoutElement>();
            btnLayout.preferredHeight = 60;
            
            Image btnImg = btnObj.AddComponent<Image>();
            btnImg.color = new Color(0.3f, 0.6f, 0.3f);
            
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
        
        private void GenerateRandomWeapon()
        {
            Rarity rarity = (Rarity)Random.Range(0, System.Enum.GetValues(typeof(Rarity)).Length);
            _currentWeapon = _itemGenerator.GenerateWeapon(rarity);
            
            LoadWeaponToUI();
            UpdatePreview();
        }
        
        private void LoadWeaponToUI()
        {
            _archetypeDropdown.value = (int)_currentWeapon.weaponArchetype;
            _rarityDropdown.value = (int)_currentWeapon.rarity;
            _damageInput.text = _currentWeapon.damage.ToString();
            _attackSpeedInput.text = _currentWeapon.attackSpeed.ToString("F2");
            _rangeInput.text = _currentWeapon.range.ToString("F1");
        }
        
        private void UpdatePreview()
        {
            if (_currentWeapon == null) return;
            
            // Update weapon from UI
            _currentWeapon.weaponArchetype = (WeaponArchetype)_archetypeDropdown.value;
            _currentWeapon.rarity = (Rarity)_rarityDropdown.value;
            
            if (int.TryParse(_damageInput.text, out int damage))
                _currentWeapon.damage = damage;
            
            if (float.TryParse(_attackSpeedInput.text, out float atkSpeed))
                _currentWeapon.attackSpeed = atkSpeed;
            
            if (float.TryParse(_rangeInput.text, out float range))
                _currentWeapon.range = range;
            
            // Generate preview
            Color rarityColor = GetRarityColor(_currentWeapon.rarity);
            string rarityHex = ColorUtility.ToHtmlStringRGB(rarityColor);
            
            _previewText.text = $"<b><color=#{rarityHex}>{_currentWeapon.itemName}</color></b>\n" +
                              $"<color=grey>Level {_currentWeapon.level} {_currentWeapon.weaponArchetype}</color>\n\n" +
                              $"<color=orange>Damage:</color> {_currentWeapon.damage}\n" +
                              $"<color=cyan>Attack Speed:</color> {_currentWeapon.attackSpeed:F2}\n" +
                              $"<color=yellow>Range:</color> {_currentWeapon.range:F1}\n\n" +
                              $"<b>Affixes:</b>\n";
            
            foreach (ItemAffix affix in _currentWeapon.affixes)
            {
                _previewText.text += $"+ {affix.affixType}: {affix.value}\n";
            }
        }
        
        private Color GetRarityColor(Rarity rarity)
        {
            return rarity switch
            {
                Rarity.Common => Color.white,
                Rarity.Uncommon => Color.green,
                Rarity.Rare => Color.blue,
                Rarity.Epic => new Color(0.6f, 0.3f, 1f),
                Rarity.Legendary => new Color(1f, 0.5f, 0f),
                Rarity.Mythic => new Color(1f, 0f, 0f),
                _ => Color.white
            };
        }
        
        private void SaveWeapon()
        {
            string json = JsonUtility.ToJson(_currentWeapon, true);
            Debug.Log($"[WeaponEditor] Saved weapon:\n{json}");
            
            PlayerPrefs.SetString($"Weapon_{_currentWeapon.itemId}", json);
            PlayerPrefs.Save();
        }
    }
}