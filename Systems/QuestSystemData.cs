using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace Game.Core.Systems
{
    /// <summary>
    /// Quest data container - Pure data, no logic.
    /// </summary>
    [System.Serializable]
    public class QuestData
    {
        public string questId;
        public string questName;
        public string questDescription;
        public QuestType questType;
        public QuestStatus status = QuestStatus.Available;
        public int level = 1;
        
        public List<QuestObjective> objectives = new();
        
        public int experienceReward;
        public int goldReward;
        public List<ItemData> itemRewards = new();
        
        public bool IsComplete()
        {
            return objectives.All(o => o.isCompleted);
        }
        
        public float GetProgress()
        {
            if (objectives.Count == 0) return 0f;
            
            float totalProgress = objectives.Sum(o => (float)o.currentCount / o.requiredCount);
            return totalProgress / objectives.Count;
        }
        
        public void ResetProgress()
        {
            foreach (QuestObjective objective in objectives)
            {
                objective.currentCount = 0;
                objective.isCompleted = false;
            }
        }
    }
    
    /// <summary>
    /// Quest objective container - Pure data, no logic.
    /// </summary>
    [System.Serializable]
    public class QuestObjective
    {
        public string objectiveId;
        public string description;
        public ObjectiveType objectiveType;
        public string targetName;
        public int requiredCount = 1;
        public int currentCount = 0;
        public bool isCompleted = false;
        
        public float GetProgress()
        {
            return (float)currentCount / requiredCount;
        }
    }
    
    /// <summary>
    /// Quest type enumeration.
    /// </summary>
    public enum QuestType
    {
        Kill,
        Collect,
        Talk,
        Explore,
        Escort,
        Deliver,
        Craft
    }
    
    /// <summary>
    /// Quest status enumeration.
    /// </summary>
    public enum QuestStatus
    {
        Available,
        Active,
        Completed,
        Failed
    }
    
    /// <summary>
    /// Objective type enumeration.
    /// </summary>
    public enum ObjectiveType
    {
        Kill,
        Collect,
        Talk,
        Explore,
        Escort,
        Deliver,
        Interact,
        Craft
    }
}