using UnityEngine;

namespace State_Machine.States
{
    public class PlayerRunState : PlayerState
    { 
        private Vector2 MoveInput => input.MoveInput;
        public override void FixedDo()
        {
            Core.body.linearVelocity = new Vector2(MoveInput.x * Core.moveSpeed, Core.body.linearVelocity.y);
            
            Core.FaceDirection(MoveInput);
        }

        public override void Exit()
        {
            base.Exit();
            Core.body.linearVelocityX *= .5f;

        }
    }
}