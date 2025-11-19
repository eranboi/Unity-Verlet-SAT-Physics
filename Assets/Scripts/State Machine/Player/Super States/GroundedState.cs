using State_Machine.States;
using UnityEngine;

namespace State_Machine.Super_States
{
    public class GroundedState : PlayerState
    {
        public IdleState idleState;
        public PlayerRunState run;

        private Vector2 MoveInput => input.MoveInput;


        public override void Enter()
        {
            Set(idleState);
        }

        public override void Do()
        {
        }

        public override void FixedDo()
        {
            if(Mathf.Abs(MoveInput.x) > 0.1f)
            {
                Set(run);
            }
            else
            {
                Set(idleState);
            }
        }
    }
}