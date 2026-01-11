using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace Game.Core.Systems
{
    /// <summary>
    /// Quest System Manager - Complete quest management with objectives and rewards.
    /// Supports multiple objective types: Kill, Collect, Talk, Explore, Escort.
    /// FIXED: Data structures moved to QuestSystemData.cs
    /// </summary>
    public class QuestSystemManager : MonoBehaviour
    {
        [Header("Quest Database")]
        [SerializeField] private List<QuestData> _availableQuests = new();
        [SerializeField] private List<QuestData> _activeQuests = new();
        [SerializeField] private List<QuestData> _completedQuests = new();
        
        [Header("Configuration")]
        [SerializeField] private int _maxActiveQuests = 10;
        
        private Dictionary<string, QuestData> _questDatabase;
        
        #region Events
        
        public System.Action<QuestData> OnQuestStarted;
        public System.Action<QuestData> OnQuestCompleted;
        public System.Action<QuestData> OnQuestFailed;
        public System.Action<QuestData, QuestObjective> OnObjectiveCompleted;
        
        #endregion
        
        #region Initialization
        
        private void Awake()
        {
            _questDatabase = new Dictionary<string, QuestData>();
            InitializeQuests();
            SubscribeToEvents();
            Debug.Log("[QuestSystemManager] Initialized");
        }
        
        private void OnDestroy()
        {
            UnsubscribeFromEvents();
        }
        
        private void InitializeQuests()
        {
            CreateDefaultQuests();
        }
        
        private void CreateDefaultQuests()
        {
            // Quest 1: Kill Quest
            QuestData killQuest = new QuestData
            {
                questId = "quest_001",
                questName = "Zombie Slayer",
                questDescription = "The local village is being overrun by zombies. Help clear them out!",
                questType = QuestType.Kill,
                level = 1,
                experienceReward = 100,
                goldReward = 50
            };
            killQuest.objectives.Add(new QuestObjective
            {
                objectiveId = "obj_001",
                objectiveType = ObjectiveType.Kill,
                targetName = "Zombie",
                requiredCount = 10,
                currentCount = 0,
                description = "Kill 10 Zombies"
            });
            AddQuest(killQuest);
            
            // Quest 2: Collection Quest
            QuestData collectQuest = new QuestData
            {
                questId = "quest_002",
                questName = "Herb Gathering",
                questDescription = "The village healer needs healing herbs. Collect them from the wilderness.",
                questType = QuestType.Collect,
                level = 1,
                experienceReward = 75,
                goldReward = 30
            };
            collectQuest.objectives.Add(new QuestObjective
            {
                objectiveId = "obj_002",
                objectiveType = ObjectiveType.Collect,
                targetName = "HealingHerb",
                requiredCount = 5,
                currentCount = 0,
                description = "Collect 5 Healing Herbs"
            });
            AddQuest(collectQuest);
            
            // Quest 3: Talk Quest
            QuestData talkQuest = new QuestData
            {
                questId = "quest_003",
                questName = "The Village Elder",
                questDescription = "Speak with the village elder to learn about the ancient prophecy.",
                questType = QuestType.Talk,
                level = 1,
                experienceReward = 50,
                goldReward = 20
            };
            talkQuest.objectives.Add(new QuestObjective
            {
                objectiveId = "obj_003",
                objectiveType = ObjectiveType.Talk,
                targetName = "VillageElder",
                requiredCount = 1,
                currentCount = 0,
                description = "Talk to the Village Elder"
            });
            AddQuest(talkQuest);
            
            // Quest 4: Exploration Quest
            QuestData exploreQuest = new QuestData
            {
                questId = "quest_004",
                questName = "Scout the Dark Forest",
                questDescription = "Explore the Dark Forest and report back on any dangers.",
                questType = QuestType.Explore,
                level = 2,
                experienceReward = 150,
                goldReward = 75
            };
            exploreQuest.objectives.Add(new QuestObjective
            {
                objectiveId = "obj_004",
                objectiveType = ObjectiveType.Explore,
                targetName = "DarkForest",
                requiredCount = 1,
                currentCount = 0,
                description = "Explore the Dark Forest"
            });
            AddQuest(exploreQuest);
        }
        
        private void SubscribeToEvents()
        {
            CombatSystemManager combatManager = CoreSystemManager.CombatManager;
            if (combatManager != null)
            {
                combatManager.OnEntityDeath += OnEntityKilled;
            }
            
            InventorySystemManager inventoryManager = CoreSystemManager.InventoryManager;
            if (inventoryManager != null)
            {
                inventoryManager.OnInventoryChanged += OnInventoryChanged;
            }
        }
        
        private void UnsubscribeFromEvents()
        {
            CombatSystemManager combatManager = CoreSystemManager.CombatManager;
            if (combatManager != null)
            {
                combatManager.OnEntityDeath -= OnEntityKilled;
            }
            
            InventorySystemManager inventoryManager = CoreSystemManager.InventoryManager;
            if (inventoryManager != null)
            {
                inventoryManager.OnInventoryChanged -= OnInventoryChanged;
            }
        }
        
        #endregion
        
        #region Quest Management
        
        public void AddQuest(QuestData quest)
        {
            if (!_questDatabase.ContainsKey(quest.questId))
            {
                _questDatabase[quest.questId] = quest;
                _availableQuests.Add(quest);
                Debug.Log($"[QuestSystemManager] Added quest: {quest.questName}");
            }
        }
        
        public bool StartQuest(string questId)
        {
            if (!_questDatabase.ContainsKey(questId))
            {
                Debug.LogWarning($"[QuestSystemManager] Quest not found: {questId}");
                return false;
            }
            
            if (_activeQuests.Count >= _maxActiveQuests)
            {
                Debug.LogWarning("[QuestSystemManager] Max active quests reached!");
                return false;
            }
            
            QuestData quest = _questDatabase[questId];
            
            if (quest.status != QuestStatus.Available)
            {
                Debug.LogWarning($"[QuestSystemManager] Quest not available: {quest.questName}");
                return false;
            }
            
            quest.status = QuestStatus.Active;
            _availableQuests.Remove(quest);
            _activeQuests.Add(quest);
            
            OnQuestStarted?.Invoke(quest);
            
            Debug.Log($"[QuestSystemManager] Started quest: {quest.questName}");
            return true;
        }
        
        public bool CompleteQuest(string questId)
        {
            QuestData quest = _activeQuests.FirstOrDefault(q => q.questId == questId);
            
            if (quest == null)
            {
                Debug.LogWarning($"[QuestSystemManager] Active quest not found: {questId}");
                return false;
            }
            
            if (!quest.IsComplete())
            {
                Debug.LogWarning($"[QuestSystemManager] Quest not complete: {quest.questName}");
                return false;
            }
            
            quest.status = QuestStatus.Completed;
            _activeQuests.Remove(quest);
            _completedQuests.Add(quest);
            
            GiveRewards(quest);
            
            OnQuestCompleted?.Invoke(quest);
            
            Debug.Log($"[QuestSystemManager] Completed quest: {quest.questName}");
            return true;
        }
        
        public bool AbandonQuest(string questId)
        {
            QuestData quest = _activeQuests.FirstOrDefault(q => q.questId == questId);
            
            if (quest == null)
                return false;
            
            quest.status = QuestStatus.Available;
            quest.ResetProgress();
            
            _activeQuests.Remove(quest);
            _availableQuests.Add(quest);
            
            Debug.Log($"[QuestSystemManager] Abandoned quest: {quest.questName}");
            return true;
        }
        
        #endregion
        
        #region Objective Tracking
        
        private void OnEntityKilled(GameObject deadEntity, GameObject killer)
        {
            if (!killer.CompareTag("Player"))
                return;
            
            string entityName = GetEntityName(deadEntity);
            
            foreach (QuestData quest in _activeQuests)
            {
                foreach (QuestObjective objective in quest.objectives)
                {
                    if (objective.objectiveType == ObjectiveType.Kill && 
                        objective.targetName == entityName &&
                        !objective.isCompleted)
                    {
                        objective.currentCount++;
                        
                        if (objective.currentCount >= objective.requiredCount)
                        {
                            objective.isCompleted = true;
                            OnObjectiveCompleted?.Invoke(quest, objective);
                            Debug.Log($"[QuestSystemManager] Objective completed: {objective.description}");
                        }
                        
                        if (quest.IsComplete())
                        {
                            CompleteQuest(quest.questId);
                        }
                    }
                }
            }
        }
        
        private void OnInventoryChanged(GameObject entity)
        {
            if (!entity.CompareTag("Player"))
                return;
            
            InventorySystemManager inventoryManager = CoreSystemManager.InventoryManager;
            if (inventoryManager == null)
                return;
            
            InventoryData inventory = inventoryManager.GetInventory(entity);
            
            foreach (QuestData quest in _activeQuests)
            {
                foreach (QuestObjective objective in quest.objectives)
                {
                    if (objective.objectiveType == ObjectiveType.Collect && !objective.isCompleted)
                    {
                        int count = inventory.GetItemCount(objective.targetName);
                        objective.currentCount = count;
                        
                        if (objective.currentCount >= objective.requiredCount)
                        {
                            objective.isCompleted = true;
                            OnObjectiveCompleted?.Invoke(quest, objective);
                            Debug.Log($"[QuestSystemManager] Objective completed: {objective.description}");
                        }
                        
                        if (quest.IsComplete())
                        {
                            CompleteQuest(quest.questId);
                        }
                    }
                }
            }
        }
        
        public void UpdateObjective(string questId, string objectiveId, int progress)
        {
            QuestData quest = _activeQuests.FirstOrDefault(q => q.questId == questId);
            if (quest == null)
                return;
            
            QuestObjective objective = quest.objectives.FirstOrDefault(o => o.objectiveId == objectiveId);
            if (objective == null)
                return;
            
            objective.currentCount += progress;
            
            if (objective.currentCount >= objective.requiredCount)
            {
                objective.isCompleted = true;
                OnObjectiveCompleted?.Invoke(quest, objective);
            }
            
            if (quest.IsComplete())
            {
                CompleteQuest(quest.questId);
            }
        }
        
        public void CompleteObjective(string questId, string objectiveId)
        {
            QuestData quest = _activeQuests.FirstOrDefault(q => q.questId == questId);
            if (quest == null)
                return;
            
            QuestObjective objective = quest.objectives.FirstOrDefault(o => o.objectiveId == objectiveId);
            if (objective == null)
                return;
            
            objective.isCompleted = true;
            objective.currentCount = objective.requiredCount;
            
            OnObjectiveCompleted?.Invoke(quest, objective);
            
            if (quest.IsComplete())
            {
                CompleteQuest(quest.questId);
            }
        }
        
        #endregion
        
        #region Rewards
        
        private void GiveRewards(QuestData quest)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player == null)
                return;
            
            EntityStats stats = player.GetComponent<EntityStats>();
            if (stats != null && quest.experienceReward > 0)
            {
                stats.AddExperience(quest.experienceReward);
                Debug.Log($"[QuestSystemManager] Awarded {quest.experienceReward} XP");
            }
            
            if (quest.goldReward > 0)
            {
                Debug.Log($"[QuestSystemManager] Awarded {quest.goldReward} gold");
            }
            
            InventorySystemManager inventoryManager = CoreSystemManager.InventoryManager;
            if (inventoryManager != null && quest.itemRewards != null)
            {
                foreach (ItemData item in quest.itemRewards)
                {
                    inventoryManager.AddItem(player, item);
                    Debug.Log($"[QuestSystemManager] Awarded item: {item.itemName}");
                }
            }
        }
        
        #endregion
        
        #region Queries
        
        public List<QuestData> GetAvailableQuests()
        {
            return new List<QuestData>(_availableQuests);
        }
        
        public List<QuestData> GetActiveQuests()
        {
            return new List<QuestData>(_activeQuests);
        }
        
        public List<QuestData> GetCompletedQuests()
        {
            return new List<QuestData>(_completedQuests);
        }
        
        public QuestData GetQuest(string questId)
        {
            return _questDatabase.ContainsKey(questId) ? _questDatabase[questId] : null;
        }
        
        public bool IsQuestActive(string questId)
        {
            return _activeQuests.Any(q => q.questId == questId);
        }
        
        public bool IsQuestCompleted(string questId)
        {
            return _completedQuests.Any(q => q.questId == questId);
        }
        
        #endregion
        
        #region Helpers
        
        private string GetEntityName(GameObject entity)
        {
            string name = entity.name.ToLower();
            
            if (name.Contains("zombie")) return "Zombie";
            if (name.Contains("skeleton")) return "Skeleton";
            if (name.Contains("orc")) return "Orc";
            if (name.Contains("boss")) return "Boss";
            
            return entity.name;
        }
        
        #endregion
        
        #region Save/Load
        
        public void SaveQuests()
        {
            for (int i = 0; i < _activeQuests.Count; i++)
            {
                string json = JsonUtility.ToJson(_activeQuests[i]);
                PlayerPrefs.SetString($"ActiveQuest_{i}", json);
            }
            PlayerPrefs.SetInt("ActiveQuestCount", _activeQuests.Count);
            
            for (int i = 0; i < _completedQuests.Count; i++)
            {
                PlayerPrefs.SetString($"CompletedQuest_{i}", _completedQuests[i].questId);
            }
            PlayerPrefs.SetInt("CompletedQuestCount", _completedQuests.Count);
            
            PlayerPrefs.Save();
            Debug.Log("[QuestSystemManager] Quests saved");
        }
        
        public void LoadQuests()
        {
            int activeCount = PlayerPrefs.GetInt("ActiveQuestCount", 0);
            for (int i = 0; i < activeCount; i++)
            {
                string json = PlayerPrefs.GetString($"ActiveQuest_{i}", "");
                if (!string.IsNullOrEmpty(json))
                {
                    QuestData quest = JsonUtility.FromJson<QuestData>(json);
                    if (!_activeQuests.Contains(quest))
                    {
                        _activeQuests.Add(quest);
                    }
                }
            }
            
            int completedCount = PlayerPrefs.GetInt("CompletedQuestCount", 0);
            for (int i = 0; i < completedCount; i++)
            {
                string questId = PlayerPrefs.GetString($"CompletedQuest_{i}", "");
                if (!string.IsNullOrEmpty(questId) && _questDatabase.ContainsKey(questId))
                {
                    QuestData quest = _questDatabase[questId];
                    quest.status = QuestStatus.Completed;
                    if (!_completedQuests.Contains(quest))
                    {
                        _completedQuests.Add(quest);
                    }
                }
            }
            
            Debug.Log($"[QuestSystemManager] Loaded {_activeQuests.Count} active quests, {_completedQuests.Count} completed");
        }
        
        #endregion
    }
}