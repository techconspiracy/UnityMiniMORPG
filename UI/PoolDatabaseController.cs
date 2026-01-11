using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

namespace Game.Core.Systems
{
    /// <summary>
    /// Pool Database Controller - View and edit all object pools in real-time.
    /// Shows active/inactive counts, allows clearing pools, adjusting limits.
    /// </summary>
    public class PoolDatabaseController : MonoBehaviour
    {
        private GameObject _contentPanel;
        private ObjectPoolManager _poolManager;
        private List<PoolStatsDisplay> _poolDisplays = new();
        private ScrollRect _scrollRect;
        
        public void Initialize(GameObject contentPanel)
        {
            _contentPanel = contentPanel;
            _poolManager = CoreSystemManager.PoolManager;
            
            CreateUI();
            RefreshPoolList();
        }
        
        private void CreateUI()
        {
            // Clear existing content
            foreach (Transform child in _contentPanel.transform)
            {
                if (child.name != "Content") Destroy(child.gameObject);
            }
            
            // Get or create scroll rect
            _scrollRect = _contentPanel.GetComponent<ScrollRect>();
            if (_scrollRect == null)
            {
                _scrollRect = _contentPanel.AddComponent<ScrollRect>();
            }
            
            // Create content container
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
            layout.spacing = 10;
            layout.padding = new RectOffset(20, 20, 20, 20);
            
            ContentSizeFitter fitter = content.AddComponent<ContentSizeFitter>();
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            
            _scrollRect.content = contentRect;
            _scrollRect.horizontal = false;
            _scrollRect.vertical = true;
            
            // Header
            CreateHeader(content.transform);
            
            // Refresh button
            CreateRefreshButton(content.transform);
        }
        
        private void CreateHeader(Transform parent)
        {
            GameObject header = new GameObject("Header");
            header.transform.SetParent(parent, false);
            
            LayoutElement headerLayout = header.AddComponent<LayoutElement>();
            headerLayout.preferredHeight = 80;
            
            Text headerText = header.AddComponent<Text>();
            headerText.text = "OBJECT POOL DATABASE\nReal-time pool monitoring and management";
            headerText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            headerText.fontSize = 24;
            headerText.color = Color.cyan;
            headerText.alignment = TextAnchor.MiddleCenter;
        }
        
        private void CreateRefreshButton(Transform parent)
        {
            GameObject btnObj = new GameObject("RefreshButton");
            btnObj.transform.SetParent(parent, false);
            
            LayoutElement btnLayout = btnObj.AddComponent<LayoutElement>();
            btnLayout.preferredHeight = 50;
            
            Image btnImg = btnObj.AddComponent<Image>();
            btnImg.color = new Color(0.2f, 0.6f, 0.2f);
            
            Button btn = btnObj.AddComponent<Button>();
            btn.targetGraphic = btnImg;
            btn.onClick.AddListener(RefreshPoolList);
            
            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(btnObj.transform, false);
            
            RectTransform textRect = textObj.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.sizeDelta = Vector2.zero;
            
            Text text = textObj.AddComponent<Text>();
            text.text = "🔄 REFRESH POOLS";
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = 20;
            text.color = Color.white;
            text.alignment = TextAnchor.MiddleCenter;
        }
        
        public void RefreshPoolList()
        {
            if (_poolManager == null)
            {
                Debug.LogWarning("[PoolDatabaseController] PoolManager not available");
                return;
            }
            
            // Clear old displays
            foreach (PoolStatsDisplay display in _poolDisplays)
            {
                if (display.gameObject != null)
                    Destroy(display.gameObject);
            }
            _poolDisplays.Clear();
            
            // Get all pool keys via reflection
            System.Reflection.FieldInfo poolsField = typeof(ObjectPoolManager).GetField("_pools", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            
            if (poolsField != null)
            {
                var pools = poolsField.GetValue(_poolManager) as System.Collections.IDictionary;
                if (pools != null)
                {
                    Transform contentTransform = _scrollRect.content;
                    
                    foreach (System.Collections.DictionaryEntry entry in pools)
                    {
                        string poolKey = entry.Key.ToString();
                        CreatePoolDisplay(contentTransform, poolKey);
                    }
                }
            }
            
            Debug.Log($"[PoolDatabaseController] Refreshed {_poolDisplays.Count} pools");
        }
        
        private void CreatePoolDisplay(Transform parent, string poolKey)
        {
            GameObject displayObj = new GameObject($"Pool_{poolKey}");
            displayObj.transform.SetParent(parent, false);
            
            LayoutElement layout = displayObj.AddComponent<LayoutElement>();
            layout.preferredHeight = 120;
            
            Image bg = displayObj.AddComponent<Image>();
            bg.color = new Color(0.15f, 0.15f, 0.2f);
            
            PoolStatsDisplay display = displayObj.AddComponent<PoolStatsDisplay>();
            display.Initialize(poolKey, _poolManager);
            
            _poolDisplays.Add(display);
        }
        
        private void Update()
        {
            // Auto-refresh every second
            if (Time.frameCount % 60 == 0)
            {
                foreach (PoolStatsDisplay display in _poolDisplays)
                {
                    if (display != null)
                        display.UpdateStats();
                }
            }
        }
    }
    
    /// <summary>
    /// Individual pool stats display component.
    /// </summary>
    public class PoolStatsDisplay : MonoBehaviour
    {
        private string _poolKey;
        private ObjectPoolManager _poolManager;
        private Text _statsText;
        
        public void Initialize(string poolKey, ObjectPoolManager poolManager)
        {
            _poolKey = poolKey;
            _poolManager = poolManager;
            
            CreateUI();
            UpdateStats();
        }
        
        private void CreateUI()
        {
            // Stats text
            GameObject textObj = new GameObject("StatsText");
            textObj.transform.SetParent(transform, false);
            
            RectTransform textRect = textObj.AddComponent<RectTransform>();
            textRect.anchorMin = new Vector2(0, 0);
            textRect.anchorMax = new Vector2(0.7f, 1);
            textRect.offsetMin = new Vector2(10, 10);
            textRect.offsetMax = new Vector2(-10, -10);
            
            _statsText = textObj.AddComponent<Text>();
            _statsText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            _statsText.fontSize = 16;
            _statsText.color = Color.white;
            _statsText.alignment = TextAnchor.MiddleLeft;
            
            // Clear button
            CreateButton("Clear", new Vector2(0.75f, 0.5f), new Vector2(100, 40), 
                () => ClearPool());
        }
        
        private void CreateButton(string label, Vector2 anchorPos, Vector2 size, 
            UnityEngine.Events.UnityAction onClick)
        {
            GameObject btnObj = new GameObject($"Btn_{label}");
            btnObj.transform.SetParent(transform, false);
            
            RectTransform btnRect = btnObj.AddComponent<RectTransform>();
            btnRect.anchorMin = anchorPos;
            btnRect.anchorMax = anchorPos;
            btnRect.pivot = new Vector2(0.5f, 0.5f);
            btnRect.sizeDelta = size;
            
            Image btnImg = btnObj.AddComponent<Image>();
            btnImg.color = new Color(0.6f, 0.2f, 0.2f);
            
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
            text.fontSize = 14;
            text.color = Color.white;
            text.alignment = TextAnchor.MiddleCenter;
        }
        
        public void UpdateStats()
        {
            if (_poolManager == null) return;
            
            ObjectPoolManager.PoolStats stats = _poolManager.GetPoolStats(_poolKey);
            
            string statusColor = stats.activeCount > stats.maxPoolSize * 0.8f ? "red" : "white";
            
            _statsText.text = $"<b><color=cyan>{_poolKey}</color></b>\n" +
                            $"Active: <color={statusColor}>{stats.activeCount}</color> | " +
                            $"Inactive: {stats.inactiveCount} | " +
                            $"Total: {stats.totalCount}\n" +
                            $"Max: {stats.maxPoolSize}";
        }
        
        private void ClearPool()
        {
            if (_poolManager != null)
            {
                _poolManager.ClearPool(_poolKey);
                UpdateStats();
                Debug.Log($"[PoolDatabase] Cleared pool: {_poolKey}");
            }
        }
    }
}