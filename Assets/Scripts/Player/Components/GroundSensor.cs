using UnityEngine;

namespace ArrowPath.Player.Components
{
    /// <summary>
    /// Handles ground detection for any character
    /// </summary>
    public class GroundSensor : MonoBehaviour
    {
        [Header("Ground Detection")]
        [SerializeField] private BoxCollider2D groundCheck;
        [SerializeField] private LayerMask groundMask = 1;
        
        public bool IsGrounded { get; private set; }
        
        private void FixedUpdate()
        {
            CheckGrounded();
        }
        
        private void CheckGrounded()
        {
            IsGrounded = Physics2D.OverlapAreaAll(groundCheck.bounds.min, groundCheck.bounds.max, groundMask).Length > 0;
        }
    }
}