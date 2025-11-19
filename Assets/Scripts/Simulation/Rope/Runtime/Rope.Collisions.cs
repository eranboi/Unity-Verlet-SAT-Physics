using Environment.Seperating_Axis_Theorem;
using UnityEngine;

namespace Environment.Rope
{
    public partial class Rope
    {
        private bool CheckCircleVsBoxCollision(Point colA, SatBoxCollider colB)
        {
            var axes = colB.GetAxes();
            var vertices = colB.GetWorldVertices();

            var closestVertex = vertices[0];
            var distanceToVertex = Vector2.Distance(colA.currentPos, closestVertex);
            foreach (var vertex in vertices)
            {
                if (vertex == closestVertex) continue;
                
                var testVertexDist = Vector2.Distance(colA.currentPos, vertex);

                if (testVertexDist < distanceToVertex)
                {
                    distanceToVertex = testVertexDist;
                    closestVertex = vertex;
                }
            }

            var newAxis = (closestVertex - (Vector2)colA.currentPos).normalized;
            var temp = new Vector2[axes.Length + 1];
            axes.CopyTo(temp, 0);

            temp[^1] = newAxis;
            axes = temp;

            
            var minOverlap = float.PositiveInfinity;
            var mtvAxis = Vector2.zero;
            
            foreach (var axis in axes)
            {
                var boxProjection = ProjectShapeOntoAxis(axis, vertices);
                var circleProjection = ProjectCircleOntoAxis(axis, colA);

                if (boxProjection.y < circleProjection.x || circleProjection.y < boxProjection.x) return false;

                var overlap = Mathf.Min(boxProjection.y, circleProjection.y) -
                               Mathf.Max(boxProjection.x, circleProjection.x);
                
                if (!(overlap < minOverlap)) continue;
                
                minOverlap = overlap;
                mtvAxis = axis;
            }
            
            Vector2 centerBox = colB.transform.position;
            Vector2 centerCircle = colA.currentPos;
            var direction = centerBox - centerCircle;

            if (Vector2.Dot(direction, mtvAxis) < 0)
            {
                mtvAxis = -mtvAxis;
            }

            if (colB.tearRope)
            {
                TearAtPoint(colA);
                _pointsToRemove.Add(colA);
            }
            ResolveCollision(colA, colB, minOverlap, mtvAxis.normalized);

            return true;
        }
        
        private Vector2 ProjectShapeOntoAxis(Vector2 axis, Vector2[] vertices)
        {
            // Get the dot product of the first vertex to initialize min and max.
            var min = Vector2.Dot(vertices[0], axis);
            var max = min;

            // Iterate through all vertices to find the overall min and max projections.
            for (var i = 1; i < vertices.Length; i++)
            {
                // Get the dot product of the vertex with the axis.
                var p = Vector2.Dot(vertices[i], axis);

                // Check if the projection is less than min or greater than max and update accordingly.
                if (p < min)
                    min = p;
                else if (p > max)
                    max = p;
            }

            // Return the min and max as a Vector2.
            return new Vector2(min, max);
        }

        private Vector2 ProjectCircleOntoAxis(Vector2 axis, Point point)
        {
            var center = point.currentPos;
            var radius = pointSize;

            var centerProjection = Vector2.Dot(center, axis);

            var min = centerProjection - radius;
            var max = centerProjection + radius;

            return new Vector2(min, max);
        }
        
        private void ResolveCollision(Point colA, SatCollider colB, float overlap, Vector2 normal)
        {
            // Get the minimum translation vector
            Vector3 mtv = normal * overlap;
            
            // Resolve based on static/movable status
            if (colA.isFixed)
            {
                // If A is fixed, only move B
                if (!colB.IsStatic)
                    colB.transform.position += mtv;
            }
            else if (colB.IsStatic)
            {
                // If B is fixed, only move A
                if (!colA.isFixed)
                    colA.currentPos -= (Vector2)mtv;
            }
            else
            {
                // If both are movable, move both half the distance
                colA.currentPos -= (Vector2)(mtv / 2f);
                colB.transform.position += mtv / 2f;
            }

            
            if (colB.friction > 0)
            {
                ApplyFriction(colA, colB, normal);
            }
            
            ApplyBounce(colA, colB, normal);
            
            
        }

        private void ApplyFriction(Point colA, SatCollider colB, Vector2 normal)
        {
            // Find the collision velocity
            var collisionVelocity = colA.currentPos - colA.prevPos;

            // Get the speed in the normal direction
            var normalSpeed = Vector2.Dot(collisionVelocity, normal);
            
            // What part of the velocity is normal to the surface
            var normalVelocity = normal * normalSpeed;
            
            // Get the tangent velocity (parallel to the surface)
            var tangentVelocity = collisionVelocity - normalVelocity;

            // Decrease tangent velocity based on friction
            tangentVelocity *= (1f - colB.friction); // friction 0-1 arası

            // Combine the velocities back together
            Vector2 newVelocity = normalVelocity + tangentVelocity;

            // Update prevPos for Verlet integration
            colA.prevPos = colA.currentPos - newVelocity;
        }

        private void ApplyBounce(Point colA, SatCollider colB, Vector2 normal)
        {
            var velocity = colA.currentPos - colA.prevPos;
    
            // Get the speed in the normal direction
            var normalSpeed = Vector2.Dot(velocity, normal);

            // What part of the velocity is normal to the surface
            var normalVelocity = normalSpeed * normal;
            
            // Get the tangent velocity (parallel to the surface)
            var tangentVelocity = velocity - normalVelocity;
    
            // Apply bounce by inverting normal velocity and scaling by bounciness
            // bounciness = 1: perfect bounce, 0: no bounce
            normalVelocity = -normalVelocity * colB.bounciness;
    
            // Combine the velocities back together
            var newVelocity = normalVelocity + tangentVelocity;
    
            // Update prevPos for Verlet integration
            colA.prevPos = colA.currentPos - newVelocity;
        }

        private void TearAtPoint(Point point)
        {
            foreach (var stick in _sticks)
            {
                if (stick.pointA == point || stick.pointB == point)
                    _sticksToRemove.Add(stick);
            }
        }
        
    }
}