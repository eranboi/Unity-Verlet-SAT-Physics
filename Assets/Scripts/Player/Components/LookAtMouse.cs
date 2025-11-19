using UnityEngine;
using UnityEngine.InputSystem;

namespace ArrowPath.Player.Components
{
    /// <summary>
    /// Makes any GameObject continuously look at mouse cursor
    /// Perfect example of composition - can be attached to any object
    /// </summary>
    public class LookAtMouse : MonoBehaviour
    {
        [Header("Look At Mouse Settings")]
        [SerializeField] private bool enableLookAt = true;
        [SerializeField] private float rotationSpeed = 0f; // 0 = instant, > 0 = smooth rotation
        [SerializeField] private Vector3 rotationOffset = Vector3.zero; // Additional rotation offset
        [SerializeField] private bool useLocalRotation = false; // Use local vs world rotation
        
        [Header("Optional Constraints")]
        [SerializeField] private bool constrainToAngle = false;
        [SerializeField] private float minAngle = -180f;
        [SerializeField] private float maxAngle = 180f;
        
        [Header("Aiming Integration")]
        [SerializeField] private bool useAimingDirection = true; // Use aim direction when aiming
        [SerializeField] private AimingComponent aimingComponent;
        
        [Header("Debug")]
        [SerializeField] private bool showDebugLine = false;
        [SerializeField] private Color debugLineColor = Color.red;
        
        private Camera _playerCamera;
        private Vector3 _lastValidDirection = Vector3.right;
        private Mouse _mouse;
        
        public bool EnableLookAt
        {
            get => enableLookAt;
            set => enableLookAt = value;
        }
        
        private void Awake()
        {
            // Find camera - prefer main camera
            _playerCamera = Camera.main;
            if (_playerCamera == null)
            {
                _playerCamera = FindFirstObjectByType<Camera>();
            }
            
            // Get mouse input device
            _mouse = Mouse.current;
            
            // Try to find AimingComponent if not assigned
            if (aimingComponent == null && useAimingDirection)
            {
                aimingComponent = GetComponentInParent<AimingComponent>();
            }
        }
        
        private void Update()
        {
            if (enableLookAt && _playerCamera != null)
            {
                LookAtMouseCursor();
            }
        }
        
        private void LookAtMouseCursor()
        {
            if (_mouse == null) return;
            
            Vector3 _direction;
            
            // Check if we're aiming
            if (useAimingDirection && aimingComponent != null && aimingComponent.IsAiming)
            {
                // When aiming, look in the drag direction (from start click to current mouse)
                var _mouseScreenPosition = _mouse.position.ReadValue();
                var _currentMousePosition = _playerCamera.ScreenToWorldPoint(new Vector3(_mouseScreenPosition.x, _mouseScreenPosition.y, _playerCamera.nearClipPlane));
                
                // Get the drag vector: from where we clicked to where we're dragging
                var _dragVector = _currentMousePosition - aimingComponent.MouseStartPosition;
                
                // Look in the OPPOSITE direction of the drag (like pulling a bow)
                _direction = -_dragVector.normalized;
            }
            else
            {
                // Normal mouse look when not aiming
                var _mouseScreenPosition = _mouse.position.ReadValue();
                var _mouseWorldPosition = _playerCamera.ScreenToWorldPoint(new Vector3(_mouseScreenPosition.x, _mouseScreenPosition.y, _playerCamera.nearClipPlane));
                _mouseWorldPosition.z = transform.position.z;
                
                _direction = (_mouseWorldPosition - transform.position).normalized;
            }
            
            if (_direction.magnitude > 0.1f) // Avoid zero direction
            {
                _lastValidDirection = _direction;
            }
            else
            {
                _direction = _lastValidDirection;
            }
            
            // Calculate target angle
            var _targetAngle = Mathf.Atan2(_direction.y, _direction.x) * Mathf.Rad2Deg;
            
            // Apply rotation offset
            _targetAngle += rotationOffset.z;
            
            // Apply angle constraints if enabled
            if (constrainToAngle)
            {
                _targetAngle = Mathf.Clamp(_targetAngle, minAngle, maxAngle);
            }
            
            // Create target rotation
            var _targetRotation = Quaternion.AngleAxis(_targetAngle, Vector3.forward);
            
            // Apply rotation (instant or smooth)
            if (rotationSpeed <= 0f)
            {
                // Instant rotation
                if (useLocalRotation)
                    transform.localRotation = _targetRotation;
                else
                    transform.rotation = _targetRotation;
            }
            else
            {
                // Smooth rotation
                var _currentRotation = useLocalRotation ? transform.localRotation : transform.rotation;
                var _smoothRotation = Quaternion.Slerp(_currentRotation, _targetRotation, rotationSpeed * Time.deltaTime);
                
                if (useLocalRotation)
                    transform.localRotation = _smoothRotation;
                else
                    transform.rotation = _smoothRotation;
            }
        }
        
        /// <summary>
        /// Manually set the camera reference
        /// </summary>
        public void SetCamera(Camera camera)
        {
            _playerCamera = camera;
        }
        
        /// <summary>
        /// Toggle look at functionality
        /// </summary>
        public void Toggle()
        {
            enableLookAt = !enableLookAt;
        }
        
        /// <summary>
        /// Set rotation constraints
        /// </summary>
        public void SetAngleConstraints(bool enable, float min = -180f, float max = 180f)
        {
            constrainToAngle = enable;
            minAngle = min;
            maxAngle = max;
        }
        
        private void OnDrawGizmos()
        {
            if (showDebugLine && enableLookAt && Application.isPlaying)
            {
                Gizmos.color = debugLineColor;
                var _direction = transform.right; // Default direction after rotation
                Gizmos.DrawLine(transform.position, transform.position + _direction * 2f);
                
                // Draw mouse position
                if (_playerCamera != null && _mouse != null)
                {
                    var _mouseScreenPos = _mouse.position.ReadValue();
                    var _mousePos = _playerCamera.ScreenToWorldPoint(new Vector3(_mouseScreenPos.x, _mouseScreenPos.y, _playerCamera.nearClipPlane));
                    _mousePos.z = transform.position.z;
                    Gizmos.color = Color.yellow;
                    Gizmos.DrawWireSphere(_mousePos, 0.1f);
                }
            }
        }
    }
}