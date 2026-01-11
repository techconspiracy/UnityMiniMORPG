using UnityEngine;
using System.Collections.Generic;

namespace Game.Core.Systems
{
    /// <summary>
    /// Complete combat system with damage calculation, abilities, and magic.
    /// RPG-tanky combat with fun early game pacing.
    /// </summary>
    public partial class CombatSystemManager : MonoBehaviour
    {
        [Header("Combat Settings")]
        [SerializeField] private float _globalDamageMultiplier = 1.0f;
        [SerializeField] private float _criticalHitMultiplier = 2.0f;
        
        public void Shutdown() { }
        
        /// <summary>
        /// Calculate final damage with all modifiers.
        /// </summary>
        public DamageResult CalculateDamage(EntityStats attacker, EntityStats defender, int baseDamage, DamageType damageType)
        {
            DamageResult result = new();
            
            // Get relevant stat for damage type
            float attackerStat = damageType switch
            {
                DamageType.Physical => attacker.strength,
                DamageType.Ranged => attacker.dexterity,
                DamageType.Magic => attacker.intelligence,
                _ => 0
            };
            
            // Base damage calculation
            float damage = baseDamage * (1 + attackerStat / 100f);
            
            // Apply global multiplier
            damage *= _globalDamageMultiplier;
            
            // Check for critical hit
            float critRoll = Random.Range(0f, 1f);
            if (critRoll <= attacker.criticalChance)
            {
                damage *= _criticalHitMultiplier;
                result.isCritical = true;
            }
            
            // Apply elemental multiplier
            damage *= GetElementalMultiplier(attacker.damageElement, defender.resistances);
            
            // Apply armor reduction (diminishing returns formula)
            float armorReduction = defender.armor / (defender.armor + 100f);
            damage *= (1f - armorReduction);
            
            // Ensure minimum damage
            damage = Mathf.Max(1, damage);
            
            result.finalDamage = Mathf.RoundToInt(damage);
            result.damageType = damageType;
            result.wasBlocked = false; // TODO: Implement blocking
            
            return result;
        }
        
        private float GetElementalMultiplier(DamageElement element, EntityResistances resistances)
        {
            return element switch
            {
                DamageElement.Fire => 1f - resistances.fireResistance,
                DamageElement.Ice => 1f - resistances.iceResistance,
                DamageElement.Lightning => 1f - resistances.lightningResistance,
                DamageElement.Poison => 1f - resistances.poisonResistance,
                DamageElement.Holy => 1f - resistances.holyResistance,
                DamageElement.Dark => 1f - resistances.darkResistance,
                _ => 1f
            };
        }
        
        /// <summary>
        /// Apply damage to an entity and trigger effects.
        /// </summary>
        public void ApplyDamage(GameObject target, DamageResult damage, GameObject attacker)
        {
            EntityStats targetStats = target.GetComponent<EntityStats>();
            if (targetStats == null) return;
            
            // Apply damage
            targetStats.currentHealth -= damage.finalDamage;
            targetStats.currentHealth = Mathf.Max(0, targetStats.currentHealth);
            
            // Trigger damage events
            OnDamageDealt?.Invoke(attacker, target, damage);
            
            // Check for death
            if (targetStats.currentHealth <= 0)
            {
                OnEntityDeath?.Invoke(target, attacker);
            }
            
            // Spawn damage number (pooled)
            SpawnDamageNumber(target.transform.position, damage);
        }
        
        private void SpawnDamageNumber(Vector3 position, DamageResult damage)
        {
            ObjectPoolManager poolManager = CoreSystemManager.PoolManager;
            if (poolManager == null) return;
            
            DamageNumber damageNum = poolManager.Get<DamageNumber>("DamageNumber", position, Quaternion.identity);
            if (damageNum != null)
            {
                damageNum.Initialize(damage.finalDamage, damage.isCritical, damage.damageType);
            }
        }
        
        // Events
        public System.Action<GameObject, GameObject, DamageResult> OnDamageDealt;
        public System.Action<GameObject, GameObject> OnEntityDeath;
    }
    
    /// <summary>
    /// Entity stats component for all combat entities.
    /// </summary>
    [System.Serializable]
    public class EntityStats : MonoBehaviour
    {
        [Header("Primary Stats")]
        public int strength = 10;
        public int dexterity = 10;
        public int intelligence = 10;
        public int vitality = 10;
        public int endurance = 10;
        public int luck = 10;
        
        [Header("Combat Stats")]
        public int currentHealth = 100;
        public int maxHealth = 100;
        public int currentMana = 50;
        public int maxMana = 50;
        public int currentStamina = 100;
        public int maxStamina = 100;
        
        [Header("Defense")]
        public int armor = 0;
        public EntityResistances resistances;
        
        [Header("Offense")]
        public DamageElement damageElement = DamageElement.None;
        public float criticalChance = 0.05f;
        
        [Header("Level")]
        public int level = 1;
        public int experience = 0;
        public int experienceToNextLevel = 100;
        
        private void Start()
        {
            RecalculateStats();
        }
        
        public void RecalculateStats()
        {
            maxHealth = 100 + (vitality * 10);
            maxMana = 100 + (intelligence * 10);
            maxStamina = 100 + (endurance * 10);
            
            currentHealth = Mathf.Min(currentHealth, maxHealth);
            currentMana = Mathf.Min(currentMana, maxMana);
            currentStamina = Mathf.Min(currentStamina, maxStamina);
        }
        
        public void AddExperience(int amount)
        {
            experience += amount;
            
            while (experience >= experienceToNextLevel)
            {
                LevelUp();
            }
        }
        
        private void LevelUp()
        {
            level++;
            experience -= experienceToNextLevel;
            experienceToNextLevel = Mathf.RoundToInt(experienceToNextLevel * 1.2f);
            
            // Grant attribute points
            strength += 2;
            dexterity += 2;
            intelligence += 2;
            vitality += 2;
            endurance += 2;
            luck += 1;
            
            RecalculateStats();
            
            Debug.Log($"[EntityStats] Level up! Now level {level}");
        }
    }
    
    [System.Serializable]
    public struct EntityResistances
    {
        public float fireResistance;
        public float iceResistance;
        public float lightningResistance;
        public float poisonResistance;
        public float holyResistance;
        public float darkResistance;
    }
    
    public struct DamageResult
    {
        public int finalDamage;
        public DamageType damageType;
        public bool isCritical;
        public bool wasBlocked;
    }
    
    public enum DamageType { Physical, Ranged, Magic }
    public enum DamageElement { None, Fire, Ice, Lightning, Poison, Holy, Dark }
    
    /// <summary>
    /// Poolable damage number display.
    /// </summary>
    public class DamageNumber : Game.Core.Pooling.PoolableObject
    {
        [SerializeField] private TextMesh _textMesh;
        [SerializeField] private float _floatSpeed = 2f;
        [SerializeField] private float _lifetime = 1.5f;
        
        private float _spawnTime;
        
        private void Awake()
        {
            if (_textMesh == null)
            {
                _textMesh = GetComponentInChildren<TextMesh>();
                if (_textMesh == null)
                {
                    GameObject textObj = new("Text");
                    textObj.transform.SetParent(transform);
                    _textMesh = textObj.AddComponent<TextMesh>();
                    _textMesh.fontSize = 32;
                    _textMesh.anchor = TextAnchor.MiddleCenter;
                }
            }
        }
        
        public void Initialize(int damage, bool isCrit, DamageType type)
        {
            _textMesh.text = isCrit ? $"{damage}!" : damage.ToString();
            _textMesh.color = isCrit ? Color.yellow : Color.white;
            _spawnTime = Time.time;
        }
        
        private void Update()
        {
            if (!IsActiveInPool) return;
            
            // Float upward
            transform.position += Vector3.up * _floatSpeed * Time.deltaTime;
            
            // Fade out
            float alpha = 1f - ((Time.time - _spawnTime) / _lifetime);
            Color color = _textMesh.color;
            color.a = alpha;
            _textMesh.color = color;
            
            // Return to pool when lifetime expires
            if (Time.time - _spawnTime >= _lifetime)
            {
                ReturnToPool();
            }
        }
        
        public override void OnReturnToPool()
        {
            base.OnReturnToPool();
            _textMesh.color = Color.white;
        }
    }
    
    /// <summary>
    /// Ability system controller.
    /// </summary>
    public class AbilitySystemController : MonoBehaviour
    {
        [SerializeField] private List<Ability> _equippedAbilities = new();
        [SerializeField] private int _maxAbilitySlots = 10;
        
        public bool UseAbility(int slotIndex, GameObject caster, Vector3 targetPosition)
        {
            if (slotIndex < 0 || slotIndex >= _equippedAbilities.Count)
                return false;
            
            Ability ability = _equippedAbilities[slotIndex];
            if (ability == null) return false;
            
            // Check cooldown
            if (ability.IsOnCooldown()) return false;
            
            // Check resource cost
            EntityStats stats = caster.GetComponent<EntityStats>();
            if (stats == null) return false;
            
            if (stats.currentMana < ability.manaCost || stats.currentStamina < ability.staminaCost)
                return false;
            
            // Execute ability
            ability.Execute(caster, targetPosition);
            
            // Consume resources
            stats.currentMana -= ability.manaCost;
            stats.currentStamina -= ability.staminaCost;
            
            // Start cooldown
            ability.StartCooldown();
            
            return true;
        }
    }
    
    [System.Serializable]
    public class Ability
    {
        public string abilityName;
        public AbilityType abilityType;
        public int manaCost;
        public int staminaCost;
        public float cooldown;
        public float baseDamage;
        public float aoeRadius;
        
        private float _cooldownEndTime;
        
        public bool IsOnCooldown() => Time.time < _cooldownEndTime;
        
        public void StartCooldown() => _cooldownEndTime = Time.time + cooldown;
        
        public virtual void Execute(GameObject caster, Vector3 targetPosition)
        {
            Debug.Log($"[Ability] {caster.name} used {abilityName}");
        }
    }
    
    public enum AbilityType { Damage, Heal, Buff, Debuff, Summon, Movement, Utility }
}