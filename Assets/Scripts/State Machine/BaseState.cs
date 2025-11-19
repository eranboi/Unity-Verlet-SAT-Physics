using ArrowPath.Player.Components;
using UnityEngine;

namespace State_Machine
{
    public abstract class BaseState : MonoBehaviour
    {
        public bool İsComplete { get; protected set; }
        public float Time => UnityEngine.Time.time - StartTime;
        
        protected Core Core;
        protected float StartTime;
        
        protected Rigidbody2D Body => Core.body;
        protected GroundSensor GroundSensor => Core.groundSensor;

        public StateMachine Machine;
        public StateMachine Parent;
        public BaseState State => Machine.state;

        public virtual void Enter() {}
        public virtual void Do() {}
        public virtual void FixedDo() {}
        public virtual void Exit() {}

        public void DoBranch()
        {
            Do();
            if (State == null) return;
            
            State?.DoBranch();
        }
        
        public void FixedDoBranch()
        {
            FixedDo();
            if (State == null) return;
            
            State?.FixedDoBranch();
        }

        public void Set(BaseState newState, bool forceReset = false)
        {
            Machine.Set(newState, forceReset);
        }
        
        public virtual void SetCore(Core core)
        {
            Machine = new StateMachine();
            Core = core;
        }

        public void Initialise(StateMachine parent)
        {
            Parent = parent;
            İsComplete = false;
            StartTime = UnityEngine.Time.time;
        }
    }
}