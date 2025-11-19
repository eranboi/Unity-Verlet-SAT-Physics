using System;
using UnityEngine;
using ArrowPath.Utils;

namespace ArrowPath.Player
{
    public class Arrow : MonoBehaviour
    {
        [Header("Arrow Components")]
        [SerializeField] private Transform arrowHead;
        
        [Header("Lifetime Management")]
        [SerializeField] private float lifeTime = 2f; // Time to remain after sticking
        [SerializeField] private float fadeOutDuration = 2f; // Duration of the fade-out effect
        
        // Arrow state
        private bool _isStuck = false;
        private Rigidbody2D _rb;
        private FixedJoint2D _joint;
        
        // Lifetime management
        private float _stuckTime;
        private FadeAndDestroy _fadeComponent;
        private Camera _mainCamera;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
            _joint = GetComponent<FixedJoint2D>();
            
            // Get or add FadeAndDestroy component
            _fadeComponent = GetComponent<FadeAndDestroy>();
            if (_fadeComponent == null)
            {
                _fadeComponent = gameObject.AddComponent<FadeAndDestroy>();
            }

            _mainCamera = Camera.main ?? FindFirstObjectByType<Camera>();
        }

        public void Initialize(Vector3 dir, float force)
        {
            _isStuck = false;
            
            if (_rb != null)
            {
                _rb.bodyType = RigidbodyType2D.Dynamic;
                _rb.gravityScale = 1f; 
                _rb.linearDamping = 0f;
                _rb.angularDamping = 0f;
                
                // Apply initial launch force
                _rb.AddForce(dir * force, ForceMode2D.Impulse);
            }
            
            // Rotate arrow to face launch direction
            RotateToDirection(dir);
        }
        
        private void Update()
        {
            HandleLifetime();
            
            // If not stuck and moving, rotate towards velocity direction
            if (_rb != null && _rb.linearVelocity.magnitude > 0.1f && !_isStuck)
            {
                RotateToDirection(_rb.linearVelocity);
            }
        }

        private void RotateToDirection(Vector3 direction)
        {
            var angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
        }
        
        private void OnCollisionEnter2D(Collision2D collision)
        {
            // Ignore collision with the player
            if (collision.gameObject.CompareTag("Player")) return;
            
            // Only stick to the "Ground" layer
            if (collision.gameObject.layer == LayerMask.NameToLayer("Ground"))
            {
                StickToSurface(collision);
            }
        }
        
        private void StickToSurface(Collision2D collision)
        {
            if (_isStuck) return;
            
            _isStuck = true;
            _stuckTime = Time.time;

            // Stop physics simulation
            if (_rb != null)
            {
                _rb.linearVelocity = Vector2.zero;
                _rb.angularVelocity = 0f;
            }
            
            // Slight visual penetration effect
            if (collision.contacts.Length > 0)
            {
                var penetrationDirection = -collision.contacts[0].normal;
                transform.position += (Vector3)(penetrationDirection * 0.1f);
            }
            
            // Anchor the arrow to the surface using the Joint
            if (_joint != null)
            {
                _joint.enabled = true;
                _joint.connectedBody = collision.rigidbody;
            }
            
            // Note: We don't disable the script completely because Update is needed for Lifetime management.
        }
        
        private void HandleLifetime()
        {
            // Timer only runs after sticking
            if (!_isStuck) 
            {
                // If off-screen, destroy immediately (optional safety check)
                if (IsOffScreen()) StartFadeOut();
                return;
            }

            if (_fadeComponent.IsDestroyed) return;
            
            var timeStuck = Time.time - _stuckTime;
            
            // Start fade out if lifetime expired or went off-screen
            if ((!_fadeComponent.IsFading && timeStuck >= lifeTime) || IsOffScreen())
            {
                StartFadeOut();
            }
        }
        
        private bool IsOffScreen()
        {
            if (_mainCamera == null) return false;
            var screenPoint = _mainCamera.WorldToViewportPoint(transform.position);
            
            // Check if out of viewport bounds (with small buffer)
            return screenPoint.x < -0.1f || screenPoint.x > 1.1f || 
                   screenPoint.y < -0.1f || screenPoint.y > 1.1f;
        }
        
        private void StartFadeOut()
        {
            if (_fadeComponent.IsFading) return;
            _fadeComponent.StartFade(fadeOutDuration);
        }
    }
}