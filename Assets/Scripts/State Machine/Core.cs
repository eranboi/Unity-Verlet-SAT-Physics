using System;
using System.Collections;
using ArrowPath.Player.Components;
using UnityEngine;

namespace State_Machine
{
    public abstract class Core : MonoBehaviour
    {
        public Rigidbody2D body;
        public GroundSensor groundSensor;
        
        public StateMachine machine;
        public float jumpForce;
        public float moveSpeed;
        public float sprintSpeed;
    
        private int _facingDirection = 1;
        public int FacingDirection => _facingDirection;
        public BaseState state => machine.state;

        public void SetupInstances()
        {
            machine = new StateMachine();
            
            BaseState[] allChildStates = GetComponentsInChildren<BaseState>();

            foreach (var state in allChildStates)
            {
                state.SetCore(this);
            }
        }

        public void Set(BaseState newState, bool forceReset = false)
        {
            if (machine == null)
            {
                Debug.LogError("StateMachine is not initialized. Call SetupInstances() first.");
                return;
            }

            machine.Set(newState, forceReset);
        }

        private void OnDrawGizmos()
        {
#if UNITY_EDITOR
            if (!Application.isPlaying || state == null) return;
            var states = machine.GetActiveStateBranch();
            UnityEditor.Handles.Label(transform.position + Vector3.up - Vector3.right, string.Join(" > ", states));
#endif
        }
        
        public void FaceDirection(Vector2 direction)
        {
            
            switch (direction.x)
            {
                case < 0:
                {
                    if(_facingDirection == 1)
                    {
                        StopAllCoroutines();
                        _facingDirection = -1;
                        StartCoroutine(Rotate());
                    }
                    break;
                }
                case > 0:
                {
                    if(_facingDirection == -1)
                    {
                        StopAllCoroutines();
                        _facingDirection = 1;
                        StartCoroutine(Rotate());
                    }
                    break;
                }
            }
        }

        private IEnumerator Rotate()
        {
            var originalRotation = transform.rotation;
            var targetRotation = Quaternion.Euler(0, _facingDirection == -1 ? 180 : 0, 0);
            var duration = 0.25f;
            var elapsedTime = 0f;
            while (elapsedTime < duration)
            {
                transform.rotation = Quaternion.Lerp(originalRotation, targetRotation, elapsedTime / duration);
                elapsedTime += Time.deltaTime;
                yield return null;
            }
        }
    }
}