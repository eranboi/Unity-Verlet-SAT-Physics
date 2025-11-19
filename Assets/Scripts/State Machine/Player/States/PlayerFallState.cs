using UnityEngine;

namespace State_Machine.States
{
    public class PlayerFallState : PlayerState
    {
        public float maxFallSpeed = 15;
        public float fallMultiplier = 1.5f;

        public override void Enter()
        {
            base.Enter();
            Core.body.gravityScale = fallMultiplier; // Increase gravity scale to make falling faster
        }

        public override void Exit()
        {
            base.Exit();
            
            Core.body.gravityScale = 1f; // Reset gravity scale when exiting fall state
        }

        public override void FixedDo()
        {
            base.FixedDo();
            if (Core.body.linearVelocity.y < -maxFallSpeed)
            {
                Core.body.linearVelocity = new Vector2(Core.body.linearVelocityX, -maxFallSpeed);
            }
        }
    }
}