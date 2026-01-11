using UnityEngine;
using Game.Core.Systems;

namespace Game.Core
{
    /// <summary>
    /// First-person player controller with movement, combat, and interaction.
    /// Attach this to the player character GameObject.
    /// </summary>
    [RequireComponent(typeof(CharacterController))]
    public class PlayerController : MonoBehaviour
    {
        [Header("Movement Settings")]
        [SerializeField] private float _moveSpeed = 5f;
        [SerializeField] private float _sprintMultiplier = 1.5f;
        [SerializeField] private float _jumpForce = 5f;
        [SerializeField] private float _gravity = -9.81f;
        
        [Header("Camera Settings")]
        [SerializeField] private float _mouseSensitivity = 2f;
        [SerializeField] private float _maxLookAngle = 80f;
        
        [Header("Combat Settings")]
        [SerializeField] private float _attackRange = 10f;
        [SerializeField] private float _interactRange = 3f;
        [SerializeField] private int _baseDamage = 25;
        
        [Header("Runtime State")]
        [SerializeField] private bool _isGrounded;
        [SerializeField] private float _currentSpeed;
        
        private CharacterController _characterController;
        private Camera _playerCamera;
        private EntityStats _playerStats;
        private Vector3 _moveVelocity;
        private float _cameraRotationX;
        
        #region Unity Lifecycle
        
        private void Awake()
        {
            InitializeComponents();
            SetupCamera();
            LockCursor();
        }
        
        private void Update()
        {
            UpdateMovement();
            UpdateCamera();
            UpdateInput();
        }
        
        #endregion
        
        #region Initialization
        
        private void InitializeComponents()
        {
            _characterController = GetComponent<CharacterController>();
            if (_characterController == null)
            {
                _characterController = gameObject.AddComponent<CharacterController>();
            }
            
            // Configure character controller
            _characterController.height = 2f;
            _characterController.radius = 0.5f;
            _characterController.center = new Vector3(0, 1f, 0);
            _characterController.slopeLimit = 45f;
            _characterController.stepOffset = 0.3f;
            
            _playerStats = GetComponent<EntityStats>();
            if (_playerStats == null)
            {
                Debug.LogWarning("[PlayerController] No EntityStats found! Adding default.");
                _playerStats = gameObject.AddComponent<EntityStats>();
            }
            
            _playerCamera = GetComponentInChildren<Camera>();
        }
        
        private void SetupCamera()
        {
            if (_playerCamera == null)
            {
                // Use main camera if available
                _playerCamera = Camera.main;
                
                if (_playerCamera != null)
                {
                    _playerCamera.transform.SetParent(transform);
                    _playerCamera.transform.localPosition = new Vector3(0, 1.6f, 0);
                    _playerCamera.transform.localRotation = Quaternion.identity;
                }
                else
                {
                    // Create new camera
                    GameObject cameraObj = new GameObject("PlayerCamera");
                    cameraObj.transform.SetParent(transform);
                    cameraObj.transform.localPosition = new Vector3(0, 1.6f, 0);
                    cameraObj.transform.localRotation = Quaternion.identity;
                    
                    _playerCamera = cameraObj.AddComponent<Camera>();
                    _playerCamera.tag = "MainCamera";
                }
            }
            
            // Configure camera
            if (_playerCamera != null)
            {
                _playerCamera.nearClipPlane = 0.1f;
                _playerCamera.farClipPlane = 1000f;
                _playerCamera.fieldOfView = 75f;
            }
        }
        
        private void LockCursor()
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        
        public void Initialize(CharacterCreationData creationData)
        {
            if (_playerStats != null)
            {
                // Stats are already set by GameWorldInitializer
                Debug.Log($"[PlayerController] Initialized {creationData.species} {creationData.gender} player");
            }
        }
        
        #endregion
        
        #region Movement
        
        private void UpdateMovement()
        {
            _isGrounded = _characterController.isGrounded;
            
            // Reset fall velocity when grounded
            if (_isGrounded && _moveVelocity.y < 0)
            {
                _moveVelocity.y = -2f;
            }
            
            // Get input
            float horizontalInput = Input.GetAxis("Horizontal");
            float verticalInput = Input.GetAxis("Vertical");
            bool isSprinting = Input.GetKey(KeyCode.LeftShift) && _playerStats.currentStamina > 0;
            
            // Calculate move direction relative to camera
            Vector3 moveDirection = transform.right * horizontalInput + transform.forward * verticalInput;
            
            // Apply speed
            float currentMoveSpeed = isSprinting ? _moveSpeed * _sprintMultiplier : _moveSpeed;
            _currentSpeed = currentMoveSpeed;
            
            // Move character
            _characterController.Move(moveDirection * currentMoveSpeed * Time.deltaTime);
            
            // Handle stamina
            if (isSprinting && moveDirection.magnitude > 0.1f)
            {
                _playerStats.currentStamina = Mathf.Max(0, 
                    _playerStats.currentStamina - Mathf.RoundToInt(10f * Time.deltaTime));
            }
            else if (_playerStats.currentStamina < _playerStats.maxStamina)
            {
                _playerStats.currentStamina = Mathf.Min(_playerStats.maxStamina, 
                    _playerStats.currentStamina + Mathf.RoundToInt(5f * Time.deltaTime));
            }
            
            // Handle jumping
            if (Input.GetButtonDown("Jump") && _isGrounded)
            {
                _moveVelocity.y = Mathf.Sqrt(_jumpForce * -2f * _gravity);
            }
            
            // Apply gravity
            _moveVelocity.y += _gravity * Time.deltaTime;
            _characterController.Move(_moveVelocity * Time.deltaTime);
        }
        
        #endregion
        
        #region Camera Control
        
        private void UpdateCamera()
        {
            if (_playerCamera == null) return;
            
            float mouseXInput = Input.GetAxis("Mouse X") * _mouseSensitivity;
            float mouseYInput = Input.GetAxis("Mouse Y") * _mouseSensitivity;
            
            // Rotate player body left/right
            transform.Rotate(Vector3.up * mouseXInput);
            
            // Rotate camera up/down (clamped)
            _cameraRotationX -= mouseYInput;
            _cameraRotationX = Mathf.Clamp(_cameraRotationX, -_maxLookAngle, _maxLookAngle);
            _playerCamera.transform.localRotation = Quaternion.Euler(_cameraRotationX, 0, 0);
        }
        
        #endregion
        
        #region Input Handling
        
        private void UpdateInput()
        {
            // Attack
            if (Input.GetMouseButtonDown(0))
            {
                PerformAttack();
            }
            
            // Interact
            if (Input.GetKeyDown(KeyCode.E))
            {
                AttemptInteraction();
            }
            
            // Toggle cursor
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                ToggleCursor();
            }
            
            // Quick inventory (example)
            if (Input.GetKeyDown(KeyCode.I))
            {
                UISystemManager uiManager = CoreSystemManager.UIManager;
                if (uiManager != null)
                {
                    uiManager.ToggleInventory();
                }
            }
            
            // Character sheet (example)
            if (Input.GetKeyDown(KeyCode.C))
            {
                UISystemManager uiManager = CoreSystemManager.UIManager;
                if (uiManager != null)
                {
                    uiManager.ToggleCharacterSheet();
                }
            }
        }
        
        private void ToggleCursor()
        {
            if (Cursor.lockState == CursorLockMode.Locked)
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
            else
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
        }
        
        #endregion
        
        #region Combat
        
        private void PerformAttack()
        {
            if (_playerCamera == null) return;
            
            // Raycast from center of screen
            Ray attackRay = _playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
            
            if (Physics.Raycast(attackRay, out RaycastHit hitInfo, _attackRange))
            {
                // Check if we hit an entity with stats
                EntityStats targetStats = hitInfo.collider.GetComponent<EntityStats>();
                
                if (targetStats != null && targetStats != _playerStats)
                {
                    // Get combat manager
                    CombatSystemManager combatManager = CoreSystemManager.CombatManager;
                    
                    if (combatManager != null)
                    {
                        // Calculate damage
                        DamageResult damageResult = combatManager.CalculateDamage(
                            _playerStats, 
                            targetStats, 
                            _baseDamage, 
                            DamageType.Physical
                        );
                        
                        // Apply damage
                        combatManager.ApplyDamage(
                            hitInfo.collider.gameObject, 
                            damageResult, 
                            gameObject
                        );
                        
                        Debug.Log($"[PlayerController] Hit {hitInfo.collider.name} for {damageResult.finalDamage} damage" +
                                 (damageResult.isCritical ? " (CRITICAL!)" : ""));
                    }
                }
                else
                {
                    Debug.Log($"[PlayerController] Hit {hitInfo.collider.name} (non-entity)");
                }
            }
        }
        
        #endregion
        
        #region Interaction
        
        private void AttemptInteraction()
        {
            if (_playerCamera == null) return;
            
            // Raycast from center of screen
            Ray interactRay = _playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
            
            if (Physics.Raycast(interactRay, out RaycastHit hitInfo, _interactRange))
            {
                // Check for interactable object
                InteractableObject interactable = hitInfo.collider.GetComponent<InteractableObject>();
                
                if (interactable != null && interactable.IsInteractable)
                {
                    interactable.Interact(gameObject);
                    Debug.Log($"[PlayerController] Interacted with {interactable.name}: {interactable.InteractionPrompt}");
                }
                else
                {
                    Debug.Log($"[PlayerController] {hitInfo.collider.name} is not interactable");
                }
            }
        }
        
        #endregion
        
        #region Public API
        
        public void TakeDamage(int damage)
        {
            if (_playerStats != null)
            {
                _playerStats.currentHealth = Mathf.Max(0, _playerStats.currentHealth - damage);
                
                if (_playerStats.currentHealth <= 0)
                {
                    OnPlayerDeath();
                }
            }
        }
        
        public void Heal(int amount)
        {
            if (_playerStats != null)
            {
                _playerStats.currentHealth = Mathf.Min(_playerStats.maxHealth, 
                    _playerStats.currentHealth + amount);
            }
        }
        
        public void RestoreMana(int amount)
        {
            if (_playerStats != null)
            {
                _playerStats.currentMana = Mathf.Min(_playerStats.maxMana, 
                    _playerStats.currentMana + amount);
            }
        }
        
        public void RestoreStamina(int amount)
        {
            if (_playerStats != null)
            {
                _playerStats.currentStamina = Mathf.Min(_playerStats.maxStamina, 
                    _playerStats.currentStamina + amount);
            }
        }
        
        private void OnPlayerDeath()
        {
            Debug.Log("[PlayerController] Player died!");
            // TODO: Implement respawn/game over logic
            enabled = false;
        }
        
        public EntityStats GetStats()
        {
            return _playerStats;
        }
        
        public bool IsMoving()
        {
            return _currentSpeed > 0.1f;
        }
        
        public bool IsGrounded()
        {
            return _isGrounded;
        }
        
        #endregion
        
        #region Debug
        
        private void OnDrawGizmos()
        {
            if (_playerCamera == null) return;
            
            // Draw attack range
            Ray attackRay = _playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
            Gizmos.color = Color.red;
            Gizmos.DrawRay(attackRay.origin, attackRay.direction * _attackRange);
            
            // Draw interact range
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position, _interactRange);
        }
        
        #endregion
    }
}