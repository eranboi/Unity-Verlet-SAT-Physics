using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Environment.Rope
{
    /// <summary>
    /// Handles user input for creating and manipulating ropes
    /// </summary>
    public class RopeInput : MonoBehaviour
    {
        [Header("Input Settings")]
        [SerializeField] private float minDistance = 0.3f;
        [SerializeField] private bool acceptInput = true;
        [SerializeField] private Rope targetRope;
        
        [Header("Creation Settings")]
        [SerializeField] private bool createNewRopeOnClick = true;
        [SerializeField] private GameObject ropePrefab;
        
        private Camera _cam;
        private bool _tearMode;
        
        // Dragging state
        private Point _leftDraggingPoint;
        private Point _rightDraggingPoint;
        private Point _hoveredPoint;
        private Point _lastCreatedPoint;
        private Vector2 _lastMousePosition;
        private Vector2 _mouseDownPos;
        
        // Temporary creation storage
        private readonly List<Point> _tempPoints = new();
        private readonly List<Stick> _tempSticks = new();
        private Rope _currentCreationRope;

        private void Awake()
        {
            _cam = Camera.main;
        }

        private void Update()
        {
            if (!acceptInput) return;
            
            HandleInputs();
        }

        private void HandleInputs()
        {
            Vector2 mouseWorldPos = _cam.ScreenToWorldPoint(Mouse.current.position.ReadValue());
            _hoveredPoint = GetHoveredPoint(mouseWorldPos);
            
            // Toggle simulation
            if (Keyboard.current.fKey.wasPressedThisFrame)
            {
                RopeSimulator.Instance.ToggleSimulation();
            }
            
            // Toggle tear mode
            if (Keyboard.current.tKey.wasPressedThisFrame)
            {
                _tearMode = !_tearMode;
            }
            
            HandleRightMouse(mouseWorldPos);
            HandleLeftMouse(mouseWorldPos);
        }

        private void HandleRightMouse(Vector2 mouseWorldPos)
        {
            if (Mouse.current.rightButton.wasPressedThisFrame)
            {
                _mouseDownPos = mouseWorldPos;
                
                if (_hoveredPoint != null)
                {
                    _rightDraggingPoint = _hoveredPoint;
                }
            }
            
            if (Mouse.current.rightButton.isPressed && _rightDraggingPoint != null)
            {
                _rightDraggingPoint.currentPos = mouseWorldPos;
            }
            
            if (Mouse.current.rightButton.wasReleasedThisFrame)
            {
                if (_rightDraggingPoint != null && Vector2.Distance(_mouseDownPos, mouseWorldPos) < 0.2f)
                {
                    _rightDraggingPoint.SetFixed(!_rightDraggingPoint.isFixed);
                }
                
                _rightDraggingPoint = null;
            }
        }

        private void HandleLeftMouse(Vector2 mouseWorldPos)
        {
            if (Mouse.current.leftButton.wasPressedThisFrame)
            {
                if (_tearMode) return;
                
                if (_hoveredPoint != null)
                {
                    _leftDraggingPoint = _hoveredPoint;
                }
                else if (createNewRopeOnClick && targetRope == null)
                {
                    CreateNewRope(mouseWorldPos);
                }
                
                _lastMousePosition = mouseWorldPos;
            }
            
            if (Mouse.current.leftButton.isPressed)
            {
                if (_tearMode)
                {
                    HandleTearing(mouseWorldPos);
                }
                else if (_leftDraggingPoint == null && _hoveredPoint == null)
                {
                    HandleContinuousCreation(mouseWorldPos);
                }
            }
            
            if (Mouse.current.leftButton.wasReleasedThisFrame)
            {
                HandleLeftMouseRelease(mouseWorldPos);
            }
        }

        private void HandleTearing(Vector2 mouseWorldPos)
        {
            var allRopes = FindObjectsOfType<Rope>();
            
            foreach (var rope in allRopes)
            {
                var sticksToRemove = new List<Stick>();
                
                foreach (var stick in rope.Sticks)
                {
                    var distance = DistancePointToLine(mouseWorldPos, stick.pointA.currentPos, stick.pointB.currentPos);
                    
                    if (distance < 0.25f)
                    {
                        sticksToRemove.Add(stick);
                    }
                }
                
                foreach (var stick in sticksToRemove)
                {
                    rope.RemoveStick(stick);
                }
            }
        }

        private void HandleContinuousCreation(Vector2 mouseWorldPos)
        {
            if (Vector2.Distance(mouseWorldPos, _lastMousePosition) > minDistance)
            {
                var rope = GetCurrentRope();
                if (rope == null) return;
                
                var newPoint = rope.AddPointToQueue(mouseWorldPos);
                _tempPoints.Add(newPoint);
                
                if (_lastCreatedPoint != null)
                {
                    var newStick = rope.AddStickToQueue(_lastCreatedPoint, newPoint);
                    _tempSticks.Add(newStick);
                }
                
                _lastCreatedPoint = newPoint;
                _lastMousePosition = mouseWorldPos;
            }
        }

        private void HandleLeftMouseRelease(Vector2 mouseWorldPos)
        {
            // First, tell the rope to add queued items to simulation
            if (_currentCreationRope != null && _tempPoints.Count > 0)
            {
                _currentCreationRope.SetReadyToAddToSimulation();
            }
            else if (targetRope != null && _tempPoints.Count > 0)
            {
                targetRope.SetReadyToAddToSimulation();
            }
            
            if (_leftDraggingPoint != null && _hoveredPoint != null && _hoveredPoint != _leftDraggingPoint)
            {
                var rope = GetRopeContainingPoint(_leftDraggingPoint);
                if (rope != null)
                {
                    rope.AddStick(_leftDraggingPoint, _hoveredPoint);
                }
            }
            
            // Clean up
            _leftDraggingPoint = null;
            _lastCreatedPoint = null;
            _currentCreationRope = null;
            _tempPoints.Clear();
            _tempSticks.Clear();
        }

        private void CreateNewRope(Vector2 position)
        {
            if (ropePrefab != null)
            {
                var newRopeObj = Instantiate(ropePrefab, position, Quaternion.identity);
                _currentCreationRope = newRopeObj.GetComponent<Rope>();
            }
        }

        private Rope GetCurrentRope()
        {
            if (targetRope != null) return targetRope;
            if (_currentCreationRope != null) return _currentCreationRope;
            
            // Find or create a rope
            var existingRope = FindFirstObjectByType<Rope>();
            if (existingRope != null) return existingRope;
            
            // Create new rope if none exists
            var newRope = new GameObject("Rope").AddComponent<Rope>();
            return newRope;
        }

        private Point GetHoveredPoint(Vector2 mouseWorldPos)
        {
            var allRopes = FindObjectsByType<Rope>(FindObjectsSortMode.None);
            
            foreach (var rope in allRopes)
            {
                foreach (var point in rope.Points)
                {
                    if (Vector2.Distance(mouseWorldPos, point.currentPos) < rope.PointSize)
                    {
                        return point;
                    }
                }
            }
            
            return null;
        }

        private Rope GetRopeContainingPoint(Point point)
        {
            var allRopes = FindObjectsByType<Rope>(FindObjectsSortMode.None);
            
            foreach (var rope in allRopes)
            {
                if (rope.Points.Contains(point))
                    return rope;
            }
            
            return null;
        }

        private float DistancePointToLine(Vector2 point, Vector2 lineStart, Vector2 lineEnd)
        {
            var lineVec = lineEnd - lineStart;
            var pointVec = point - lineStart;
            var lineLen = lineVec.magnitude;
            
            if (lineLen == 0) return (point - lineStart).magnitude;
            
            var lineUnitVec = lineVec / lineLen;
            var pointProjected = Vector2.Dot(pointVec, lineUnitVec);
            
            if (pointProjected < 0f) return (point - lineStart).magnitude;
            if (pointProjected > lineLen) return (point - lineEnd).magnitude;
            
            var closest = lineStart + lineUnitVec * pointProjected;
            return (point - closest).magnitude;
        }
    }
}