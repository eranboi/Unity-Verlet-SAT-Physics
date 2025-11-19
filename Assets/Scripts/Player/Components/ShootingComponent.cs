using UnityEngine;

namespace ArrowPath.Player.Components
{
    /// <summary>
    /// Handles the interaction between aiming and the ArrowManager.
    /// Acts as the trigger mechanism for the character.
    /// </summary>
    public class ShootingComponent : MonoBehaviour
    {
        // Dependencies
        private AimingComponent _aimingComponent;
        private ArrowManager _arrowManager;
        
        // State
        private bool _isShooting = false;
        public bool IsShooting => _isShooting;
        
        private void Awake()
        {
            _aimingComponent = GetComponent<AimingComponent>();
            _arrowManager = GetComponent<ArrowManager>();
        }
        
        /// <summary>
        /// Prepares the shooting state (can be used for animations).
        /// </summary>
        public void StartShooting()
        {
            if (_aimingComponent == null || !_aimingComponent.IsAiming) return;
            
            _isShooting = true;
        }
        
        /// <summary>
        /// Resets the shooting state.
        /// </summary>
        public void EndShooting()
        {
            _isShooting = false;
        }
        
        /// <summary>
        /// Executes the shot by taking data from AimingComponent and passing it to ArrowManager.
        /// </summary>
        public void FireArrow()
        {
            if (_arrowManager == null || _aimingComponent == null)
            {
                Debug.LogWarning("ShootingComponent: Missing dependencies!");
                return;
            }
            
            var direction = _aimingComponent.AimDirection;
            var power = _aimingComponent.AimPower;
            
            // Fire the arrow using the simplified ArrowManager signature
            _arrowManager.FireArrow(direction, power);
            
            // Reset aiming state after a successful shot
            _aimingComponent.StopAiming();
            
            EndShooting();
        }
    }
}