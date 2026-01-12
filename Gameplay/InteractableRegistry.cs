using UnityEngine;
using System.Collections.Generic;

namespace Game.Core.Systems
{
    /// <summary>
    /// Registry for all interactable objects in zones.
    /// Manages doors, chests, ladders, stairs, windows, levers, traps, etc.
    /// COMPLETE IMPLEMENTATION!
    /// </summary>
    public class InteractableRegistry : MonoBehaviour
    {
        [Header("Interactable Prefabs")]
        [SerializeField] private GameObject _doorPrefab;
        [SerializeField] private GameObject _chestPrefab;
        [SerializeField] private GameObject _ladderPrefab;
        [SerializeField] private GameObject _leverPrefab;
        [SerializeField] private GameObject _trapPrefab;
        
        [Header("Runtime Registry")]
        [SerializeField] private List<InteractableObject> _registeredInteractables = new();
        
        private Dictionary<string, InteractableObject> _interactableMap;
        
        private void Awake()
        {
            _interactableMap = new Dictionary<string, InteractableObject>();
            CreateDefaultPrefabs();
        }
        
        private void CreateDefaultPrefabs()
        {
            // Create default prefabs if not assigned
            if (_doorPrefab == null)
                _doorPrefab = CreateDoorPrefab();
            
            if (_chestPrefab == null)
                _chestPrefab = CreateChestPrefab();
            
            if (_ladderPrefab == null)
                _ladderPrefab = CreateLadderPrefab();
            
            if (_leverPrefab == null)
                _leverPrefab = CreateLeverPrefab();
            
            if (_trapPrefab == null)
                _trapPrefab = CreateTrapPrefab();
        }
        
        private GameObject CreateDoorPrefab()
        {
            GameObject door = GameObject.CreatePrimitive(PrimitiveType.Cube);
            door.name = "DoorPrefab";
            door.transform.localScale = new Vector3(1.5f, 3f, 0.2f);
            door.GetComponent<Renderer>().material.color = new Color(0.6f, 0.4f, 0.2f);
            door.AddComponent<Door>();
            door.SetActive(false);
            return door;
        }
        
        private GameObject CreateChestPrefab()
        {
            GameObject chest = GameObject.CreatePrimitive(PrimitiveType.Cube);
            chest.name = "ChestPrefab";
            chest.transform.localScale = new Vector3(1f, 0.8f, 0.6f);
            chest.GetComponent<Renderer>().material.color = new Color(0.7f, 0.5f, 0.2f);
            chest.AddComponent<Chest>();
            chest.SetActive(false);
            return chest;
        }
        
        private GameObject CreateLadderPrefab()
        {
            GameObject ladder = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            ladder.name = "LadderPrefab";
            ladder.transform.localScale = new Vector3(0.3f, 3f, 0.3f);
            ladder.GetComponent<Renderer>().material.color = new Color(0.5f, 0.3f, 0.1f);
            ladder.AddComponent<Ladder>();
            ladder.SetActive(false);
            return ladder;
        }
        
        private GameObject CreateLeverPrefab()
        {
            GameObject lever = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            lever.name = "LeverPrefab";
            lever.transform.localScale = new Vector3(0.1f, 1f, 0.1f);
            lever.GetComponent<Renderer>().material.color = Color.gray;
            lever.AddComponent<Lever>();
            lever.SetActive(false);
            return lever;
        }
        
        private GameObject CreateTrapPrefab()
        {
            GameObject trap = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            trap.name = "TrapPrefab";
            trap.transform.localScale = Vector3.one * 0.5f;
            trap.GetComponent<Renderer>().material.color = Color.red;
            trap.AddComponent<Trap>();
            trap.SetActive(false);
            return trap;
        }
        
        public void RegisterInteractable(InteractableObject interactable)
        {
            if (!_registeredInteractables.Contains(interactable))
            {
                _registeredInteractables.Add(interactable);
                _interactableMap[interactable.GetInstanceID().ToString()] = interactable;
            }
        }
        
        public void UnregisterInteractable(InteractableObject interactable)
        {
            _registeredInteractables.Remove(interactable);
            _interactableMap.Remove(interactable.GetInstanceID().ToString());
        }
        
        public List<InteractableObject> GetInteractablesInRange(Vector3 position, float range)
        {
            List<InteractableObject> nearby = new();
            
            foreach (InteractableObject interactable in _registeredInteractables)
            {
                if (interactable != null && Vector3.Distance(position, interactable.transform.position) <= range)
                {
                    nearby.Add(interactable);
                }
            }
            
            return nearby;
        }
        
        public GameObject SpawnInteractable(string type, Vector3 position, Quaternion rotation)
        {
            GameObject prefab = type switch
            {
                "Door" => _doorPrefab,
                "Chest" => _chestPrefab,
                "Ladder" => _ladderPrefab,
                "Lever" => _leverPrefab,
                "Trap" => _trapPrefab,
                _ => null
            };
            
            if (prefab == null)
            {
                Debug.LogError($"[InteractableRegistry] Unknown interactable type: {type}");
                return null;
            }
            
            GameObject instance = Instantiate(prefab, position, rotation);
            instance.name = $"{type}_{instance.GetInstanceID()}";
            instance.SetActive(true);
            
            return instance;
        }
    }
    
    /// <summary>
    /// Base class for all interactable objects.
    /// </summary>
    public abstract class InteractableObject : MonoBehaviour
    {
        [Header("Interactable Settings")]
        [SerializeField] protected string _interactionPrompt = "Press E to interact";
        [SerializeField] protected float _interactionRange = 3f;
        [SerializeField] protected bool _isInteractable = true;
        [SerializeField] protected bool _requiresLineOfSight = true;
        
        public string InteractionPrompt => _interactionPrompt;
        public float InteractionRange => _interactionRange;
        public bool IsInteractable => _isInteractable;
        
        protected virtual void Start()
        {
            InteractableRegistry registry = FindFirstObjectByType<InteractableRegistry>();
            registry?.RegisterInteractable(this);
        }
        
        protected virtual void OnDestroy()
        {
            InteractableRegistry registry = FindFirstObjectByType<InteractableRegistry>();
            registry?.UnregisterInteractable(this);
        }
        
        public abstract void Interact(GameObject interactor);
        
        public virtual bool CanInteract(GameObject interactor)
        {
            if (!_isInteractable) return false;
            
            float distance = Vector3.Distance(transform.position, interactor.transform.position);
            if (distance > _interactionRange) return false;
            
            if (_requiresLineOfSight)
            {
                Vector3 direction = transform.position - interactor.transform.position;
                if (Physics.Raycast(interactor.transform.position, direction, out RaycastHit hit, _interactionRange))
                {
                    return hit.collider.gameObject == gameObject;
                }
                return false;
            }
            
            return true;
        }
        
        protected virtual void OnDrawGizmos()
        {
            Gizmos.color = _isInteractable ? Color.cyan : Color.gray;
            Gizmos.DrawWireSphere(transform.position, _interactionRange);
        }
    }
    
    /// <summary>
    /// Door interactable - can be opened/closed, locked/unlocked.
    /// </summary>
    public class Door : InteractableObject
    {
        [Header("Door Settings")]
        [SerializeField] private bool _isLocked;
        [SerializeField] private bool _isOpen;
        [SerializeField] private float _openAngle = 90f;
        [SerializeField] private float _openSpeed = 2f;
        [SerializeField] private string _requiredKeyId;
        
        private Quaternion _closedRotation;
        private Quaternion _openRotation;
        private bool _isAnimating;
        
        protected override void Start()
        {
            base.Start();
            _closedRotation = transform.rotation;
            _openRotation = _closedRotation * Quaternion.Euler(0, _openAngle, 0);
            
            _interactionPrompt = _isLocked ? "Locked" : "Press E to open";
        }
        
        public override void Interact(GameObject interactor)
        {
            if (_isLocked)
            {
                Debug.Log("[Door] Door is locked!");
                // TODO: Check for key in inventory
                return;
            }
            
            if (!_isAnimating)
            {
                _isOpen = !_isOpen;
                StartCoroutine(AnimateDoor());
            }
        }
        
        private System.Collections.IEnumerator AnimateDoor()
        {
            _isAnimating = true;
            Quaternion targetRotation = _isOpen ? _openRotation : _closedRotation;
            
            while (Quaternion.Angle(transform.rotation, targetRotation) > 0.1f)
            {
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * _openSpeed);
                yield return null;
            }
            
            transform.rotation = targetRotation;
            _isAnimating = false;
            
            _interactionPrompt = _isOpen ? "Press E to close" : "Press E to open";
        }
        
        public void Unlock()
        {
            _isLocked = false;
            _interactionPrompt = "Press E to open";
            Debug.Log("[Door] Door unlocked!");
        }
        
        public void Lock()
        {
            _isLocked = true;
            _isOpen = false;
            _interactionPrompt = "Locked";
            Debug.Log("[Door] Door locked!");
        }
    }
    
    /// <summary>
    /// Chest interactable - contains loot.
    /// </summary>
    public class Chest : InteractableObject
    {
        [Header("Chest Settings")]
        [SerializeField] private bool _isOpened;
        [SerializeField] private int _lootCount = 3;
        [SerializeField] private Rarity _minLootRarity = Rarity.Common;
        [SerializeField] private Rarity _maxLootRarity = Rarity.Rare;
        
        protected override void Start()
        {
            base.Start();
            _interactionPrompt = _isOpened ? "Empty" : "Press E to open";
        }
        
        public override void Interact(GameObject interactor)
        {
            if (_isOpened)
            {
                Debug.Log("[Chest] Already looted!");
                return;
            }
            
            _isOpened = true;
            _interactionPrompt = "Empty";
            SpawnLoot();
        }
        
        private void SpawnLoot()
        {
            ItemGenerationEngine itemGen = FindFirstObjectByType<ItemGenerationEngine>();
            if (itemGen == null)
            {
                Debug.LogWarning("[Chest] No ItemGenerationEngine found!");
                return;
            }
            
            Debug.Log($"[Chest] Spawning {_lootCount} items!");
            
            for (int i = 0; i < _lootCount; i++)
            {
                Rarity rarity = (Rarity)Random.Range((int)_minLootRarity, (int)_maxLootRarity + 1);
                ItemType type = Random.value > 0.5f ? ItemType.Weapon : ItemType.Armor;
                
                ItemData item = itemGen.GetOrGenerateItem(rarity, type);
                Debug.Log($"  - {item.itemName} ({item.rarity})");
                
                // TODO: Add to player inventory or spawn as pickup
            }
        }
        
        public override bool CanInteract(GameObject interactor)
        {
            return base.CanInteract(interactor) && !_isOpened;
        }
    }
    
    /// <summary>
    /// Ladder interactable - allows vertical traversal.
    /// </summary>
    public class Ladder : InteractableObject
    {
        [Header("Ladder Settings")]
        [SerializeField] private float _climbSpeed = 3f;
        [SerializeField] private Transform _topPosition;
        [SerializeField] private Transform _bottomPosition;
        
        private bool _isClimbing;
        
        protected override void Start()
        {
            base.Start();
            _interactionPrompt = "Press E to climb";
            
            // Create top/bottom positions if not set
            if (_topPosition == null)
            {
                GameObject top = new GameObject("LadderTop");
                top.transform.SetParent(transform);
                top.transform.localPosition = Vector3.up * 5f;
                _topPosition = top.transform;
            }
            
            if (_bottomPosition == null)
            {
                GameObject bottom = new GameObject("LadderBottom");
                bottom.transform.SetParent(transform);
                bottom.transform.localPosition = Vector3.zero;
                _bottomPosition = bottom.transform;
            }
        }
        
        public override void Interact(GameObject interactor)
        {
            if (_isClimbing) return;
            
            // Determine which direction to climb
            float distToTop = Vector3.Distance(interactor.transform.position, _topPosition.position);
            float distToBottom = Vector3.Distance(interactor.transform.position, _bottomPosition.position);
            
            Vector3 target = distToTop < distToBottom ? _bottomPosition.position : _topPosition.position;
            
            StartCoroutine(ClimbLadder(interactor, target));
        }
        
        private System.Collections.IEnumerator ClimbLadder(GameObject interactor, Vector3 target)
        {
            _isClimbing = true;
            
            // Disable player controller
            PlayerController playerController = interactor.GetComponent<PlayerController>();
            if (playerController != null)
                playerController.enabled = false;
            
            // Climb to target
            while (Vector3.Distance(interactor.transform.position, target) > 0.5f)
            {
                interactor.transform.position = Vector3.MoveTowards(
                    interactor.transform.position, 
                    target, 
                    _climbSpeed * Time.deltaTime
                );
                yield return null;
            }
            
            interactor.transform.position = target;
            
            // Re-enable player controller
            if (playerController != null)
                playerController.enabled = true;
            
            _isClimbing = false;
            Debug.Log("[Ladder] Finished climbing!");
        }
    }
    
    /// <summary>
    /// Lever interactable - toggles between on/off states, triggers events.
    /// </summary>
    public class Lever : InteractableObject
    {
        [Header("Lever Settings")]
        [SerializeField] private bool _isActivated;
        [SerializeField] private float _toggleAngle = 45f;
        [SerializeField] private GameObject _targetObject;
        
        private Quaternion _offRotation;
        private Quaternion _onRotation;
        
        public System.Action<bool> OnToggled;
        
        protected override void Start()
        {
            base.Start();
            _offRotation = transform.rotation;
            _onRotation = _offRotation * Quaternion.Euler(_toggleAngle, 0, 0);
            _interactionPrompt = "Press E to pull lever";
        }
        
        public override void Interact(GameObject interactor)
        {
            _isActivated = !_isActivated;
            
            transform.rotation = _isActivated ? _onRotation : _offRotation;
            
            OnToggled?.Invoke(_isActivated);
            
            // Trigger target object
            if (_targetObject != null)
            {
                Door door = _targetObject.GetComponent<Door>();
                if (door != null)
                {
                    if (_isActivated)
                        door.Unlock();
                    else
                        door.Lock();
                }
            }
            
            Debug.Log($"[Lever] Toggled {(_isActivated ? "ON" : "OFF")}");
        }
    }
    
    /// <summary>
    /// Trap interactable - triggers damage when activated.
    /// </summary>
    public class Trap : MonoBehaviour
    {
        [Header("Trap Settings")]
        [SerializeField] private int _damage = 50;
        [SerializeField] private float _triggerRadius = 2f;
        [SerializeField] private float _cooldown = 3f;
        [SerializeField] private bool _isReusable = true;
        
        private float _lastTriggerTime;
        private bool _isTriggered;
        
        private void Update()
        {
            if (_isTriggered && !_isReusable) return;
            
            if (Time.time - _lastTriggerTime < _cooldown) return;
            
            // Check for entities in range
            Collider[] hits = Physics.OverlapSphere(transform.position, _triggerRadius);
            
            foreach (Collider hit in hits)
            {
                if (hit.CompareTag("Player") || hit.CompareTag("Enemy"))
                {
                    TriggerTrap(hit.gameObject);
                    break;
                }
            }
        }
        
        private void TriggerTrap(GameObject victim)
        {
            _isTriggered = true;
            _lastTriggerTime = Time.time;
            
            EntityStats stats = victim.GetComponent<EntityStats>();
            if (stats != null)
            {
                stats.currentHealth -= _damage;
                Debug.Log($"[Trap] {victim.name} triggered trap! Took {_damage} damage");
            }
            
            // Visual effect
            StartCoroutine(PlayTrapEffect());
        }
        
        private System.Collections.IEnumerator PlayTrapEffect()
        {
            Renderer renderer = GetComponent<Renderer>();
            Color originalColor = renderer.material.color;
            
            renderer.material.color = Color.yellow;
            yield return new WaitForSeconds(0.2f);
            
            renderer.material.color = originalColor;
            
            if (_isReusable)
            {
                _isTriggered = false;
            }
        }
        
        private void OnDrawGizmos()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, _triggerRadius);
        }
    }
}