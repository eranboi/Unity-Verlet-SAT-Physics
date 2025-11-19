using ArrowPath.Player;
using ArrowPath.Player.Components;
using ArrowPath.Utils; // Assuming Helpers lives here, though we'll use standard Mathf
using UnityEngine;

namespace ArrowPath.Player
{
    /// <summary>
    /// Handles the input-to-action logic for aiming and firing arrows.
    /// Visualization is handled via LineRenderer.
    /// </summary>
    public class ArrowShooter : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private ArrowManager arrowManager;
        
        [Header("Aiming Settings")]
        [SerializeField] private float maxDragDistance = 100f;
        [SerializeField] private float minDragThreshold = 10f; // Minimum drag to register a shot
        [SerializeField] private float visualLineLengthMultiplier = 0.2f;
        
        // Private components
        private InputHandler _inputHandler;
        private LineRenderer _lineRenderer;
        private LookAtMouse _lookAtMouse;
        
        // State
        private bool _isAiming;
        private Vector2 _aimDirection;
        private float _normalizedPower; // 0 to 1
        
        // Shortcuts for readability
        private Vector2 AimStartPosition => _inputHandler.AimStartPosition;
        private Vector2 AimEndPosition => _inputHandler.AimInput;

        private void Awake()
        {
            _inputHandler = GetComponent<InputHandler>();
            _lineRenderer = GetComponent<LineRenderer>();
            _lookAtMouse = GetComponentInChildren<LookAtMouse>();
            
            // Validate references
            if (arrowManager == null) arrowManager = GetComponent<ArrowManager>();
        }

        private void Update()
        {
            // Case 1: Currently Dragging/Aiming
            if (_inputHandler.StartAimingInput)
            {
                ProcessAiming();
            }
            // Case 2: Just Released (Fire)
            else if (_isAiming)
            {
                TryFireArrow();
                ResetAiming();
            }
        }

        private void ProcessAiming()
        {
            if (!_isAiming)
            {
                // First frame of aiming
                _isAiming = true;
                if (_lookAtMouse != null) _lookAtMouse.EnableLookAt = false;
                if (_lineRenderer != null) _lineRenderer.enabled = true;
            }

            // Calculate raw vector and distance
            Vector2 dragVector = AimStartPosition - AimEndPosition;
            float dragDistance = dragVector.magnitude;
            
            // Calculate Direction
            _aimDirection = dragVector.normalized;
            
            // Calculate Power (Normalized 0-1)
            // ArrowManager handles the actual Force interpolation (10 to 20)
            _normalizedPower = Mathf.Clamp01(dragDistance / maxDragDistance);

            // Visuals: Line Renderer
            if (_lineRenderer != null)
            {
                _lineRenderer.SetPosition(0, transform.position);
                // Draw line based on power
                Vector3 endPos = transform.position + (Vector3)_aimDirection * (dragDistance * visualLineLengthMultiplier);
                _lineRenderer.SetPosition(1, endPos);
            }

            // Visuals: Rotate Character/Bow
            if (_lookAtMouse != null)
            {
                Vector3 lookDirection = new Vector3(_aimDirection.x, _aimDirection.y, 0f);
                _lookAtMouse.transform.right = lookDirection;
            }
        }

        private void TryFireArrow()
        {
            // Calculate drag distance again to ensure we are above threshold
            float dragDistance = (AimStartPosition - AimEndPosition).magnitude;

            if (dragDistance > minDragThreshold)
            {
                // Call the new signature: FireArrow(Vector3 direction, float power01)
                // Note: removed 'false' (ricochet bool) as it was removed from Manager
                arrowManager.FireArrow(_aimDirection, _normalizedPower);
            }
        }

        private void ResetAiming()
        {
            _isAiming = false;
            
            if (_lookAtMouse != null) _lookAtMouse.EnableLookAt = true;
            if (_lineRenderer != null) _lineRenderer.enabled = false;
        }
    }
}