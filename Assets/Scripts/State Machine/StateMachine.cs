using System.Collections.Generic;
using UnityEngine;

namespace State_Machine
{
    public class StateMachine
    {
        public BaseState state;
        
        public void Set(BaseState newState, bool forceReset = false)
        {
            if (newState == null) return;

            if (state == newState && !forceReset) return;
            
            state?.Exit();
            state = newState;
            state.Initialise(this);
            state.Enter();
        }
        
        public List<string> GetActiveStateBranch(List<string> list = null)
        {
            list ??= new List<string>();
            
            if(state == null)
            {
                return list;
            }

            var _stateName = state.GetType().Name;
            list.Add(_stateName);
            return state.Machine.GetActiveStateBranch(list);
        }
    }
}