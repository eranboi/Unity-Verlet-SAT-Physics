using UnityEngine;

namespace State_Machine.States
{
    public class PlayerJumpState : PlayerState
    {
        private bool JumpInput => input.JumpInput;
        private bool IsGrounded => Core.groundSensor.IsGrounded;
        
        [SerializeField] private float maxJumpTime = 0.5f; // Maximum time the player can hold the jump button
        
        public override void FixedDo()
        {
            if(JumpInput && Time < maxJumpTime)
            {
                Core.body.linearVelocity = new Vector2(Core.body.linearVelocity.x, Core.jumpForce);
            } 
            
            if (!JumpInput || Time >= maxJumpTime || (IsGrounded && Time > .1f))
            {
                Core.body.linearVelocity = new Vector2(Core.body.linearVelocityX, Core.body.linearVelocityY / 2); // Stop upward movement when jump is released or max time reached
                İsComplete = true;
            }
        }
    }
}