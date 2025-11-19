using UnityEngine;

namespace Environment.Seperating_Axis_Theorem
{
    public class SatBoxCollider : SatCollider
    {
        public Vector2 size;
        public override void SetSize(Vector2 size)
        {
            this.size = size;
        }

        public override Vector2[] GetWorldVertices()
        {
            var vertices = new Vector2[4];
        
            var boxSize = this.size;

            var halfSize = boxSize / 2f;

            // Get the local points of the vertices.
            var p1 = new Vector2(halfSize.x, halfSize.y);
            var p2 = new Vector2(-halfSize.x, halfSize.y);
            var p3 = new Vector2(-halfSize.x, -halfSize.y);
            var p4 = new Vector2(halfSize.x, -halfSize.y);

            // Transform the points into world points.
            vertices[0] = transform.TransformPoint(p1);
            vertices[1] = transform.TransformPoint(p2);
            vertices[2] = transform.TransformPoint(p3);
            vertices[3] = transform.TransformPoint(p4);
        

            return vertices;
        }

        public override Vector2[] GetAxes()
        {
            var axes = new Vector2[2];

            axes[0] = transform.right;
            axes[1] = transform.up;

            return axes;
        }
        
        private void OnDrawGizmos()
        {
            var vertices = GetWorldVertices();

            for (int i = 0; i < vertices.Length; i++)
            {
                var point = vertices[i];
                var nextPoint = vertices[(i + 1) % vertices.Length];
            
                Gizmos.DrawLine(point, nextPoint);
            }
        }
    }
}