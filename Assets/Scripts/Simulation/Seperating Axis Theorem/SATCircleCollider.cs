using System;
using UnityEngine;

namespace Environment.Seperating_Axis_Theorem
{
    public class SatCircleCollider : SatCollider
    {
        public float radius;

        private void OnDrawGizmos()
        {
            Gizmos.DrawWireSphere(transform.position, radius);
        }
    }
}