using System.Collections.Generic;
using UnityEngine;

namespace Environment.Rope
{
    public partial class Rope
    {
        private void CreateRopeFromPath(List<Transform> path, float pointSpacing = 0.25f, bool fixFirstPoint = true)
        {
            if (path == null || path.Count < 2) return;

            Point previousPoint = null;

            // Add points along the path
            for (var i = 0; i < path.Count - 1; i++)
            {
                Vector2 startPos = path[i].position;
                Vector2 endPos = path[i + 1].position;
        
                var distance = Vector2.Distance(startPos, endPos);
                var pointCount = Mathf.Max(2, Mathf.CeilToInt(distance / pointSpacing));
        
                // Create points for this segment
                for (var j = 0; j < pointCount; j++)
                {
                    var t = j / (float)(pointCount - 1);
                    var position = Vector2.Lerp(startPos, endPos, t);
            
                    // Determine if the point should be fixed
                    var shouldFix = (i == 0 && j == 0 && fixFirstPoint);
            
                    var point = AddPointToQueue(position, shouldFix);
                    
                    // Attach body if this point corresponds to a path transform with a RopeAttachment
                    if (previousPoint != null)
                    {
                        AddStickToQueue(previousPoint, point);
                    }
            
                    previousPoint = point;
                }
            }
    
            // Add the last point if it's not exactly on the last path transform
            Vector2 lastPos = path[^1].position;
            if (previousPoint == null || Vector2.Distance(previousPoint.currentPos, lastPos) > 0.01f)
            {
                var lastPoint = AddPointToQueue(lastPos, false);
                if (previousPoint != null)
                {
                    AddStickToQueue(previousPoint, lastPoint);
                }
            }
    
            // Finalize the rope creation
            SetReadyToAddToSimulation();
        }
        
        [ContextMenu("Generate Cloth")]
        public void GenerateCloth()
        {
            ClearRope();

            var clothPoints = new Point[clothWidth, clothHeight];

            // Create points
            for (var x = 0; x < clothWidth; x++)
            {
                for (var y = 0; y < clothHeight; y++)
                {
                    var pos = transform.position + new Vector3(x * clothSpacing, -y * clothSpacing, 0);
                    var point = AddPoint(pos);
                    point.SetFixed(y == 0 && x % 4 == 0); // Fix top row every 4 points
                    clothPoints[x, y] = point;
                }
            }

            // Create horizontal sticks
            for (var y = 0; y < clothHeight; y++)
            {
                for (var x = 0; x < clothWidth - 1; x++)
                {
                    AddStick(clothPoints[x, y], clothPoints[x + 1, y]);
                }
            }

            // Create vertical sticks
            for (var x = 0; x < clothWidth; x++)
            {
                for (var y = 0; y < clothHeight - 1; y++)
                {
                    AddStick(clothPoints[x, y], clothPoints[x, y + 1]);
                }
            }
        }
        
    }
}