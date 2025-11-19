using State_Machine.States;
using UnityEngine;

namespace State_Machine.Super_States
{
    public class PlayerInAirState : PlayerState
    {
        [Header("States")]
        public PlayerJumpState jumpState;
        public PlayerFallState fallState;
        
        private bool JumpInput => input.JumpInput;
        private Vector2 MoveInput => input.MoveInput;

        private bool canJump = true;

        public override void FixedDo()
        {
            // Movement in-air
            Core.body.linearVelocity = new Vector2(MoveInput.x * Core.moveSpeed, Core.body.linearVelocity.y);
            
            HandleJump();

            if (Core.groundSensor.IsGrounded && Time > 0.5f)
            {
                İsComplete = true;
                canJump = true;
            }
        }

        private void HandleJump()
        {
            if (JumpInput && canJump && Core.groundSensor.IsGrounded)
            {
                Set(jumpState);
                canJump = false;
            }

            if (State == jumpState && State.İsComplete)
            {
                Debug.Log("Jump state completed, transitioning to fall state");
                Set(fallState);
            }
            
        }
    }
}