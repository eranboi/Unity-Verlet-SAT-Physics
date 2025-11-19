using UnityEngine;

namespace ArrowPath.Player.Components
{
    /// <summary>
    /// Handles jumping mechanics with variable height and modern gravity
    /// </summary>
    public class JumpComponent : MonoBehaviour
    {
        [Header("Jump Settings")]
        [SerializeField] private float jumpSpeed = 8f; // Direct jump velocity
        [SerializeField] private float apexGravityMultiplier = 0.5f; // Slower gravity at apex for floaty feel
        [SerializeField] private float apexThreshold = 2f; // Velocity threshold to consider "at apex"
        [SerializeField] private float fallGravityMultiplier = 2.5f; // Faster falling
        [SerializeField] private float lowJumpMultiplier = 2f; // Quick fall when jump released early
        [SerializeField] private float maxFallSpeed = 15f; // Terminal velocity
        
        private Rigidbody2D _rb2d;
        private GroundSensor _groundSensor;
        private bool _canJump = true;
        
        // Jump state
        private bool _isJumping = false;
        private bool _jumpInputHeld = false;
        
        public bool CanJump => _canJump;
        public bool IsJumping => _isJumping;
        public bool JumpInputHeld => _jumpInputHeld;
        
        private void Awake()
        {
            _rb2d = GetComponent<Rigidbody2D>();
            _groundSensor = GetComponent<GroundSensor>();
        }
        
        private void Update()
        {
            ApplyModernGravity();
        }
        
        private void ApplyModernGravity()
        {
            if (_rb2d == null) return;
            
            var _velocity = _rb2d.linearVelocity;
            var _yVel = _velocity.y;
            
            // Check if we're at the apex of the jump (low vertical velocity)
            var _isAtApex = Mathf.Abs(_yVel) < apexThreshold && _isJumping;
            
            // Apply different gravity based on state
            if (_isAtApex)
            {
                // At apex - apply lower gravity for floaty feel
                _rb2d.gravityScale = apexGravityMultiplier;
            }
            else if (_yVel < 0) // Falling
            {
                _rb2d.gravityScale = fallGravityMultiplier;
                _isJumping = false; // No longer in jump state when falling
            }
            else if (_yVel > 0 && !_jumpInputHeld) // Rising but jump not held
            {
                _rb2d.gravityScale = lowJumpMultiplier;
            }
            else // Normal gravity (1.0) for all other states
            {
                _rb2d.gravityScale = 1f;
            }
            
            // Clamp fall speed
            if (_yVel < -maxFallSpeed)
            {
                _velocity.y = -maxFallSpeed;
                _rb2d.linearVelocity = _velocity;
            }
        }
        
        public void SetCanJump(bool value)
        {
            _canJump = value;
        }
        
        public void SetJumpSpeed(float newSpeed)
        {
            jumpSpeed = newSpeed;
        }
        
        public void Jump()
        {
            if (!_canJump || _rb2d == null) return;
            if (_groundSensor != null && !_groundSensor.IsGrounded) return;
            
            // Speed-based jump
            _rb2d.linearVelocity = new Vector2(_rb2d.linearVelocity.x, jumpSpeed);
            _isJumping = true;
            _jumpInputHeld = true;
        }
        
        public void OnJumpInputReleased()
        {
            _jumpInputHeld = false;
        }
        
        private void OnTriggerEnter2D(Collider2D other)
        {
            // Reset jump state when landing
            if (_groundSensor != null && _groundSensor.IsGrounded)
            {
                _isJumping = false;
                _jumpInputHeld = false;
            }
        }
    }
}