using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;

namespace Game.Core.Systems
{
    /// <summary>
    /// Armor Editor Controller - Create and edit armor pieces with live preview.
    /// </summary>
    public class ArmorEditorController : MonoBehaviour
    {
        private GameObject _contentPanel;
        private ItemGenerationEngine _itemGenerator;
        private ItemData _currentArmor;
        
        private Dropdown _slotDropdown;
        private Dropdown _archetypeDropdown;
        private Dropdown _rarityDropdown;
        private InputField _armorInput;
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
            GenerateRandomArmor();
        }
        
        private void CreateUI()
        {
            Transform content = CreateScrollableContent();
            
            CreateHeader(content, "ARMOR EDITOR");
            
            CreateDropdown(content, "Slot:", out _slotDropdown, 
                System.Enum.GetNames(typeof(ArmorSlot)).ToList());
            _slotDropdown.onValueChanged.AddListener(v => UpdatePreview());
            
            CreateDropdown(content, "Archetype:", out _archetypeDropdown, 
                System.Enum.GetNames(typeof(ArmorArchetype)).ToList());
            _archetypeDropdown.onValueChanged.AddListener(v => UpdatePreview());
            
            CreateDropdown(content, "Rarity:", out _rarityDropdown, 
                System.Enum.GetNames(typeof(Rarity)).ToList());
            _rarityDropdown.onValueChanged.AddListener(v => UpdatePreview());
            
            CreateInputField(content, "Armor Value:", out _armorInput);
            _armorInput.onValueChanged.AddListener(v => UpdatePreview());
            
            CreateButton(content, "🎲 GENERATE RANDOM", () => GenerateRandomArmor());
            CreateButton(content, "💾 SAVE ARMOR", () => SaveArmor());
            
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
        
        private void GenerateRandomArmor()
        {
            Rarity rarity = (Rarity)Random.Range(0, System.Enum.GetValues(typeof(Rarity)).Length);
            _currentArmor = _itemGenerator.GenerateArmor(rarity);
            
            _slotDropdown.value = (int)_currentArmor.armorSlot;
            _archetypeDropdown.value = (int)_currentArmor.armorArchetype;
            _rarityDropdown.value = (int)_currentArmor.rarity;
            _armorInput.text = _currentArmor.armorValue.ToString();
            
            UpdatePreview();
        }
        
        private void UpdatePreview()
        {
            if (_currentArmor == null) return;
            
            _currentArmor.armorSlot = (ArmorSlot)_slotDropdown.value;
            _currentArmor.armorArchetype = (ArmorArchetype)_archetypeDropdown.value;
            _currentArmor.rarity = (Rarity)_rarityDropdown.value;
            
            if (int.TryParse(_armorInput.text, out int armor))
                _currentArmor.armorValue = armor;
            
            Color rarityColor = GetRarityColor(_currentArmor.rarity);
            string rarityHex = ColorUtility.ToHtmlStringRGB(rarityColor);
            
            _previewText.text = $"<b><color=#{rarityHex}>{_currentArmor.itemName}</color></b>\n" +
                              $"<color=grey>Level {_currentArmor.level} {_currentArmor.armorArchetype} {_currentArmor.armorSlot}</color>\n\n" +
                              $"<color=orange>Armor:</color> {_currentArmor.armorValue}\n\n" +
                              $"<b>Affixes:</b>\n";
            
            foreach (ItemAffix affix in _currentArmor.affixes)
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
        
        private void SaveArmor()
        {
            string json = JsonUtility.ToJson(_currentArmor, true);
            Debug.Log($"[ArmorEditor] Saved armor:\n{json}");
            
            PlayerPrefs.SetString($"Armor_{_currentArmor.itemId}", json);
            PlayerPrefs.Save();
        }
    }
}