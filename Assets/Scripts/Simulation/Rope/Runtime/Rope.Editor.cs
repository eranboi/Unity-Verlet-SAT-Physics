using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Environment.Rope
{
    /// <summary>
    /// Helpers for the editor part of the rope system.
    /// </summary>
    public partial class Rope
    {
#if UNITY_EDITOR
        public Point Editor_AddPoint(Vector2 position)
        {
            var point = new Point(position, position, pointSize, pointPrefab, transform);
            _points.Add(point);
            
            // Triggers the serialization
            if (!Application.isPlaying)
            {
                EditorUtility.SetDirty(this);
            }
            
            return point;
        }
        
        public Stick Editor_AddStick(Point pointA, Point pointB)
        {
            // Is there already a stick that connects the same two points?
            foreach (var existingStick in _sticks)
            {
                if ((existingStick.pointA == pointA && existingStick.pointB == pointB) ||
                    (existingStick.pointA == pointB && existingStick.pointB == pointA))
                {
                    return existingStick;
                }
            }
            
            var stick = new Stick(pointA, pointB, stickWidth, lineMaterial, transform);
            _sticks.Add(stick);
            pointA.Connect(stick);
            pointB.Connect(stick);
            
            // Triggers the serialization
            if (!Application.isPlaying)
            {
                EditorUtility.SetDirty(this);
            }
            
            return stick;
        }
#endif
    }
}