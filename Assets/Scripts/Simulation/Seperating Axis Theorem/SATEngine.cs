using System.Collections.Generic;
using ArrowPath.Utils;
using Environment.Seperating_Axis_Theorem;
using UnityEngine;

public class SatEngine : Singleton<SatEngine>
{
    private List<SatCollider> _allColliders = new();

    public void RegisterCollider(SatCollider col)
    {
        _allColliders.Add(col);
    }
    
    public void UnregisterCollider(SatCollider col)
    {
        if (_allColliders.Contains(col))
            _allColliders.Remove(col);
    }
    
    /// <summary>
    /// Check all colliders against each other in the physics loop.
    /// </summary>
    private void Update()
    {
        // Remove null objects from the list backwards to avoid index issues.
        for (int i = _allColliders.Count - 1; i >= 0; i--)
        {
            if (_allColliders[i] == null)
                _allColliders.RemoveAt(i);
        }

        
        // Loop 'i' goes through all colliders.
        for (int i = 0; i < _allColliders.Count; i++)
        {
            // Loop 'j' always starts AFTER 'i' (j = i + 1).
            // This prevents checking a pair (A, B) twice (as B, A) and 
            // prevents checking an object against itself (A == B).
            for (int j = i + 1; j < _allColliders.Count; j++)
            {
                var colA = _allColliders[i];
                var colB = _allColliders[j];

                // Optimization: Static objects do not interact with each other, skip calculation.
                if (colA.IsStatic && colB.IsStatic) continue;

                CheckCollision(colA, colB);
            }
        }
    }

    /// <summary>
    /// Checks if there is a collision between two SATColliders using SAT.
    /// </summary>
    /// <returns>Returns true if collision exists, otherwise false.</returns>
    private bool CheckCollision(SatCollider colA, SatCollider colB)
    {
        var typeA = colA.GetType();
        var typeB = colB.GetType();
        if (typeA == typeof(SatBoxCollider) && typeB == typeof(SatBoxCollider))
            return CheckBoxVsBoxCollision(colA, colB);
        
        if (typeA == typeof(SatCircleCollider) && typeB == typeof(SatCircleCollider))
            return CheckCircleVsCircleCollision(colA as SatCircleCollider, colB as SatCircleCollider);

        if(typeA == typeof(SatCircleCollider) && typeB == typeof(SatBoxCollider) || 
           typeA == typeof(SatBoxCollider) && typeB == typeof(SatCircleCollider))
            return CheckCircleVsBoxCollision(colA, colB);

        return false;
    }

    private void ResolveCollision(SatCollider colA, SatCollider colB, float overlap, Vector2 normal)
    {
        // Calculate the push (MTV) vector.
        Vector3 mtv = normal * overlap;

        // Distribute the push amount based on the situation.
        if (colA.IsStatic)
        {
            // If only A is static, push B by the full amount.
            if (!colB.IsStatic)
                colB.transform.position += mtv;
        }
        else if (colB.IsStatic)
        {
            // If only B is static, push A by the full amount in the opposite direction.
            if (!colA.IsStatic)
                colA.transform.position -= mtv;
        }
        else
        {
            // If both are dynamic, split the push amount evenly between them.
            colA.transform.position -= mtv / 2f;
            colB.transform.position += mtv / 2f;
        }
    }

    private bool CheckCircleVsCircleCollision(SatCircleCollider colA, SatCircleCollider colB)
    {
        var colACircle = colA;
        var colBCircle = colB;
        
        var posA = colA.transform.position;
        var posB = colB.transform.position;

        var radiusA = colACircle.radius;
        var radiusB = colBCircle.radius;

        var distance = Vector2.Distance(posA, posB);
        var totalRadius = radiusA + radiusB;

        if (distance > totalRadius) return false;

        var overlap = totalRadius - distance;
        var mtvAxis = (posB - posA).normalized;
        ResolveCollision(colA, colB, overlap, mtvAxis);
        return true;
    }

    private bool CheckCircleVsBoxCollision(SatCollider colA, SatCollider colB)
    {
        SatCircleCollider circleCollider;
        SatBoxCollider boxCollider;

        if (colA.GetType() == typeof(SatCircleCollider))
        {
            circleCollider = colA as SatCircleCollider;
            boxCollider = colB as SatBoxCollider;
        }
        else
        {
            circleCollider = colB as SatCircleCollider;
            boxCollider = colA as SatBoxCollider;
        }

        if (circleCollider == null || boxCollider == null) return false;

        var axes = boxCollider.GetAxes();
        var vertices = boxCollider.GetWorldVertices();

        var closestVertex = vertices[0];
        var distanceToVertex = Vector2.Distance(circleCollider.transform.position, closestVertex);
        foreach (var vertex in vertices)
        {
            if (vertex == closestVertex) continue;
            
            var testVertexDist = Vector2.Distance(circleCollider.transform.position, vertex);

            if (testVertexDist < distanceToVertex)
            {
                distanceToVertex = testVertexDist;
                closestVertex = vertex;
            }
        }

        var newAxis = (closestVertex - (Vector2)circleCollider.transform.position).normalized;
        var temp = new Vector2[axes.Length + 1];
        axes.CopyTo(temp, 0);

        temp[^1] = newAxis;
        axes = temp;

        
        var minOverlap = float.PositiveInfinity;
        var mtvAxis = Vector2.zero;
        
        foreach (var axis in axes)
        {
            var boxProjection = ProjectShapeOntoAxis(axis, vertices);
            var circleProjection = ProjectCircleOntoAxis(axis, circleCollider);

            if (boxProjection.y < circleProjection.x || circleProjection.y < boxProjection.x) return false;

            var overlap = Mathf.Min(boxProjection.y, circleProjection.y) -
                           Mathf.Max(boxProjection.x, circleProjection.x);
            
            if (!(overlap < minOverlap)) continue;
            
            minOverlap = overlap;
            mtvAxis = axis;
        }
        
        Vector2 centerBox = boxCollider.transform.position;
        Vector2 centerCircle = circleCollider.transform.position;
        var direction = centerBox - centerCircle;

        if (Vector2.Dot(direction, mtvAxis) < 0)
        {
            mtvAxis = -mtvAxis;
        }
    
        ResolveCollision(colA, colB, minOverlap, mtvAxis.normalized);

        return true;
    }

    private bool CheckBoxVsBoxCollision(SatCollider colA, SatCollider colB)
    {
        // Get vertices and axes of both boxes in world coordinates.
        var verticesA = colA.GetWorldVertices();
        var verticesB = colB.GetWorldVertices();
        
        var axesA = colA.GetAxes();
        var axesB = colB.GetAxes();

        // Combine all axes into a single list. For 2D boxes, a total of 4 axes are checked.
        var allAxes = new List<Vector2>();
        allAxes.AddRange(axesA);
        allAxes.AddRange(axesB);

        
        var minDistance = float.PositiveInfinity;
        var mtvAxis = Vector2.zero;

        // Loop through each axis.
        foreach (var axis in allAxes)
        {
            // Project the 'shadow' of both shapes onto the current axis.
            // This projection consists of a minimum and a maximum point.
            var projectionA = ProjectShapeOntoAxis(axis, verticesA);
            var projectionB = ProjectShapeOntoAxis(axis, verticesB);

            // Check if there is a gap between the two shadows.
            // If the maximum point of one is less than the minimum point of the other, the shadows do not intersect.
            // projectionA.y = maxA, projectionB.x = minB
            if (projectionA.y < projectionB.x || projectionB.y < projectionA.x)
            {
                // If we find even one gap, this is a 'Separating Axis'.
                // According to the theorem, if there is even one separating axis, there is no collision.
                // Therefore, we can return false immediately.
                return false;
            }

            
            var overlap = Mathf.Min(projectionA.y, projectionB.y) - Mathf.Max(projectionB.x, projectionA.x);

            if (overlap < minDistance)
            {
                minDistance = overlap;
                mtvAxis = axis.normalized;
            }
        }
        
        // FIX 2: Ensure the push direction (MTV axis) is correct.
        Vector2 centerA = colA.transform.position;
        Vector2 centerB = colB.transform.position;
        var direction = centerB - centerA;

        // Check if the MTV axis opposes the direction connecting the object centers.
        if (Vector2.Dot(direction, mtvAxis) < 0)
        {
            // If it opposes, reverse the push direction.
            mtvAxis = -mtvAxis;
        }
        
        
        ResolveCollision(colA, colB, minDistance, mtvAxis);

        // If the loop finishes and no gaps are found on any axis,
        // it means shadows intersect on all axes.
        // Therefore, there is definitely a collision.
        return true;
    }
    
    /// <summary>
    /// Projects the vertices of a shape onto a specific axis.
    /// Consequently returns the minimum and maximum projection values on that axis.
    /// This is where the DOT PRODUCT is used!
    /// </summary>
    /// <returns>A Vector2 containing x = min, y = max values.</returns>
    private Vector2 ProjectShapeOntoAxis(Vector2 axis, Vector2[] vertices)
    {
        // Initially set min and max values to the projection of the first vertex.
        var min = Vector2.Dot(vertices[0], axis);
        var max = min;

        // Loop through all other vertices.
        for (var i = 1; i < vertices.Length; i++)
        {
            // Get the projection of the vertex onto the axis using dot product.
            var p = Vector2.Dot(vertices[i], axis);

            // Check if it is a new min or max value.
            if (p < min)
                min = p;
            else if (p > max)
                max = p;
        }

        // Return Min and Max values as a single Vector2.
        return new Vector2(min, max);
    }

    private Vector2 ProjectCircleOntoAxis(Vector2 axis, SatCircleCollider circle)
    {
        var center = circle.transform.position;
        var radius = circle.radius;

        var centerProjection = Vector2.Dot(center, axis);

        var min = centerProjection - radius;
        var max = centerProjection + radius;

        return new Vector2(min, max);
    }
}