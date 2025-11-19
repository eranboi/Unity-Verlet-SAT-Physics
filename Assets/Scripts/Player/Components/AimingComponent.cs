using UnityEngine;
using UnityEngine.InputSystem;

namespace ArrowPath.Player.Components
{
    /// <summary>
    /// Handles aiming and trajectory visualization
    /// Can be used by any character that needs to aim
    /// </summary>
    public class AimingComponent : MonoBehaviour
    {
        [Header("Aiming Settings")]
        [SerializeField] private float maxAimDistance = 10f;
        [SerializeField] private float maxDrawDistance = 3f; // How far back you can draw
        [SerializeField] private bool showTrajectory = true;
        [SerializeField] private Color trajectoryColor = Color.white;
        [SerializeField] private bool invertAiming = true; // Pull back to aim forward
        
        [Header("Components")]
        [SerializeField] private Transform aimPivot;
        [SerializeField] private LineRenderer trajectoryLine;
        
        private Camera _playerCamera;
        private bool _isAiming = false;
        private Vector3 _aimDirection = Vector3.right;
        private float _aimPower = 0f;
        private Vector3 _aimStartPosition;
        private Vector3 _currentAimPosition;
        private Vector3 _drawPosition; // Where the bow string is pulled to
        private Vector3 _mouseStartPosition; // Where mouse was first clicked
        
        public bool IsAiming => _isAiming;
        public Vector3 AimDirection => _aimDirection;
        public float AimPower => _aimPower;
        public float MaxAimDistance => maxAimDistance;
        public Transform AimPivot => aimPivot;
        public Vector3 DrawPosition => _drawPosition; // For arrow visual feedback
        public Vector3 MouseStartPosition => _mouseStartPosition; // For bow rotation
        
        private void Awake()
        {
            _playerCamera = FindFirstObjectByType<Camera>();
            SetupTrajectoryLine();
        }
        
        private void SetupTrajectoryLine()
        {
            if (trajectoryLine == null)
            {
                var _trajectoryObj = new GameObject("TrajectoryLine");
                _trajectoryObj.transform.SetParent(transform);
                trajectoryLine = _trajectoryObj.AddComponent<LineRenderer>();
            }
            
            if (trajectoryLine != null)
            {
                trajectoryLine.material = new Material(Shader.Find("Sprites/Default"));
                trajectoryLine.material.color = trajectoryColor;
                trajectoryLine.startWidth = 0.05f;
                trajectoryLine.endWidth = 0.05f;
                trajectoryLine.positionCount = 2;
                trajectoryLine.enabled = false;
            }
        }
        
        public void StartAiming()
        {
            _isAiming = true;
            _aimStartPosition = transform.position;
            _drawPosition = _aimStartPosition; // Initialize draw position
            
            // Store where mouse was clicked
            if (_playerCamera != null && Mouse.current != null)
            {
                var _mouseScreenPos = Mouse.current.position.ReadValue();
                _mouseStartPosition = _playerCamera.ScreenToWorldPoint(new Vector3(_mouseScreenPos.x, _mouseScreenPos.y, _playerCamera.nearClipPlane));
            }
            
            if (trajectoryLine != null && showTrajectory)
            {
                trajectoryLine.enabled = true;
            }
        }
        
        public void StopAiming()
        {
            _isAiming = false;
            _aimPower = 0f;
            
            if (trajectoryLine != null)
            {
                trajectoryLine.enabled = false;
            }
        }
        
        public void UpdateAiming(Vector2 mousePosition)
        {
            if (!_isAiming || _playerCamera == null) return;
            
            _currentAimPosition = _playerCamera.ScreenToWorldPoint(new Vector3(mousePosition.x, mousePosition.y, _playerCamera.nearClipPlane));
            
            if (invertAiming)
            {
                // Calculate drag vector from mouse start to current position
                var _dragVector = _currentAimPosition - _mouseStartPosition;
                var _dragDistance = Mathf.Min(_dragVector.magnitude, maxDrawDistance);
                
                // The aim direction is OPPOSITE of drag direction (bow physics)
                _aimDirection = -_dragVector.normalized;
                
                // Power is based on how far you drag
                _aimPower = _dragDistance / maxDrawDistance;
                
                // Draw position follows mouse
                _drawPosition = _currentAimPosition;
                
                
            }
            else
            {
                // Normal aiming (point and shoot)
                _aimDirection = (_currentAimPosition - _aimStartPosition).normalized;
                var _aimDistance = Mathf.Min(Vector3.Distance(_currentAimPosition, _aimStartPosition), maxAimDistance);
                _aimPower = _aimDistance / maxAimDistance;
                _drawPosition = _currentAimPosition;
            }
            
            // Update trajectory visualization
            UpdateTrajectoryVisualization();
        }
        
        public void UpdateAimData(Vector3 direction, float power)
        {
            _aimDirection = direction;
            _aimPower = power;
        }
        
        private void UpdateTrajectoryVisualization()
        {
            if (trajectoryLine == null || !showTrajectory) return;
            
            if (invertAiming)
            {
                // Trajectory from mouse start to current mouse position (drag visualization)
                trajectoryLine.SetPosition(0, _mouseStartPosition);
                trajectoryLine.SetPosition(1, _currentAimPosition);
            }
            else
            {
                // Normal trajectory from player in aim direction
                var _startPosition = _aimStartPosition;
                var _endPosition = _startPosition + (_aimDirection * _aimPower * maxAimDistance);
                
                trajectoryLine.SetPosition(0, _startPosition);
                trajectoryLine.SetPosition(1, _endPosition);
            }
        }
    }
}