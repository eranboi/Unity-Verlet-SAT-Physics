using State_Machine.Super_States;
using UnityEngine;

namespace State_Machine
{
    public class Player : Core
    {
        public InputHandler inputHandler;
        
        [Header("States")]
        public GroundedState grounded;
        public PlayerInAirState playerInAir;

        public Vector2 MoveInput => inputHandler.MoveInput;
        
        private void Awake()
        {
            SetupInstances();
            Set(grounded);
        }
        
        private void Update()
        {
            state?.DoBranch();

            HandleStateChanges();
        }

        private void FixedUpdate()
        {
            state?.FixedDoBranch();
        }

        private void HandleStateChanges()
        {
            if (inputHandler.JumpInput && groundSensor.IsGrounded)
            {
                Set(playerInAir);
            }
            else if(groundSensor.IsGrounded && state != grounded)
            {
                if (state != playerInAir) return;
                if(state.İsComplete) Set(grounded);

            }
            else if (!groundSensor.IsGrounded && state == grounded)
            {
                Set(playerInAir);
            }
            
        }
    }
}