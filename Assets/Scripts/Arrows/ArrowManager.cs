using UnityEngine;
using System.Collections.Generic;

namespace ArrowPath.Player
{
    /// <summary>
    /// Handles the spawning and firing logic of arrows.
    /// Simplified to focus on physics interaction for the portfolio demo.
    /// </summary>
    public class ArrowManager : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private GameObject arrowPrefab;
        [SerializeField] private Transform firePoint;
        
        [Header("Firing Settings")]
        [SerializeField] private float baseFireForce = 10f;
        [SerializeField] private float maxFireForce = 20f;
        
        [Header("Performance")]
        [SerializeField] private int maxActiveArrows = 10;
        
        // Track active arrows to prevent scene clutter
        private List<GameObject> _activeArrows = new List<GameObject>();

        /// <summary>
        /// Instantiates and launches an arrow.
        /// </summary>
        /// <param name="direction">Normalized direction vector.</param>
        /// <param name="power">Value between 0 and 1 (0 = min force, 1 = max force).</param>
        public void FireArrow(Vector3 direction, float power)
        {
            if (arrowPrefab == null || firePoint == null)
            {
                Debug.LogWarning("ArrowManager: References missing!");
                return;
            }
            
            // Calculate the final force based on charge power
            var finalForce = Mathf.Lerp(baseFireForce, maxFireForce, Mathf.Clamp01(power));
            
            // Instantiate the projectile
            var arrowInstance = Instantiate(arrowPrefab, firePoint.position, Quaternion.identity);
            _activeArrows.Add(arrowInstance);
            
            // Initialize the Arrow component
            var arrowScript = arrowInstance.GetComponent<Arrow>();
            if (arrowScript != null)
            {
                // Matches the simplified Arrow.cs signature
                arrowScript.Initialize(direction, finalForce);
            }
            else
            {
                // Fallback if script is missing, just push the Rigidbody
                var rb = arrowInstance.GetComponent<Rigidbody2D>();
                if (rb != null)
                {
                    rb.AddForce(direction * finalForce, ForceMode2D.Impulse);
                }
            }
            
            // Maintain performance by removing old arrows
            ManageActiveArrows();
        }
        
        /// <summary>
        /// Ensures we don't exceed the maximum number of arrows in the scene.
        /// </summary>
        private void ManageActiveArrows()
        {
            // Remove any null references (destroyed arrows)
            _activeArrows.RemoveAll(a => a == null);
            
            // Destroy oldest arrows if we exceed the limit
            while (_activeArrows.Count > maxActiveArrows)
            {
                if (_activeArrows[0] != null)
                {
                    Destroy(_activeArrows[0]);
                }
                _activeArrows.RemoveAt(0);
            }
        }
        
        /// <summary>
        /// Clears all arrows from the scene (Useful for resetting the demo).
        /// </summary>
        public void ClearAllArrows()
        {
            foreach (var arrow in _activeArrows)
            {
                if (arrow != null) Destroy(arrow);
            }
            _activeArrows.Clear();
        }
    }
}