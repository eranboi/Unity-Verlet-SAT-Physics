using System;
using System.Collections.Generic;
using Environment.Seperating_Axis_Theorem;
using UnityEngine;

namespace Environment.Rope
{
    /// <summary>
    /// Individual rope that can be simulated independently
    /// </summary>
    public partial class Rope : MonoBehaviour
    {
        [Header("Rope Settings")] [SerializeField]
        private float pointSize = 0.1f;

        [SerializeField] private float stickWidth = 0.1f;
        [SerializeField] private Material lineMaterial;
        [SerializeField] private GameObject pointPrefab;

        [Header("Cloth Generation")] 
        [SerializeField] private bool generateCloth;
        [SerializeField] private int clothWidth = 10;
        [SerializeField] private int clothHeight = 10;
        [SerializeField] private float clothSpacing = 0.5f;

        [SerializeField] private List<Transform> path;
        
        // Rope components
        private readonly HashSet<Point> _points = new();
        private readonly HashSet<Stick> _sticks = new();

        // Components to remove
        private readonly HashSet<Point> _pointsToRemove = new();
        private readonly HashSet<Stick> _sticksToRemove = new();

        // Points and sticks that are being created (not yet in simulation)
        private readonly HashSet<Point> _pointsToAdd = new();
        private readonly HashSet<Stick> _sticksToAdd = new();
        private bool _readyToAddToSimulation;
        
        // Rope Bounds
        private float _minX;
        private float _maxX;
        private float _minY;
        private float _maxY;

        // Runtime state
        private bool _isInitialized;

        public HashSet<Point> Points => _points;
        public HashSet<Stick> Sticks => _sticks;
        public float PointSize => pointSize;
        public float StickWidth => stickWidth;
        public Material LineMaterial => lineMaterial;
        public GameObject PointPrefab => pointPrefab;
        
        
        private float _gravity;
        private float _tearLength;
        private int _numIterations;
        
        private void Start()
        {
            Initialize();
            var ropeSimulator = RopeSimulator.Instance;
            _tearLength = ropeSimulator.TearLength;
            _numIterations = ropeSimulator.NumIterations;
            _gravity = ropeSimulator.Gravity;
        }

        private void OnEnable()
        {
            if (RopeSimulator.Instance != null)
                RopeSimulator.Instance.RegisterRope(this);
        }

        private void OnDisable()
        {
            if (RopeSimulator.Instance != null)
                RopeSimulator.Instance.UnregisterRope(this);
        }

        private void OnDestroy()
        {
            ClearRope();
        }

        private void Initialize()
        {
            if (_isInitialized) return;

            _isInitialized = true;

            if (generateCloth)
            {
                GenerateCloth();
            }

            if (path.Count > 0)
            {
                CreateRopeFromPath(path);
            }
        }

        public void Simulate()
        {
            if (!RopeSimulator.Instance.IsSimulating) return;

            _sticksToRemove.Clear();
            _pointsToRemove.Clear();
            
            // Simulate points
            foreach (var point in _points)
            {
                if (point.isFixed) continue;
                var posBeforeSimulation = point.currentPos;
                var velocity = point.currentPos - point.prevPos;

                // Apply motion
                point.currentPos += velocity;

                // Apply gravity
                var gravityForce = Vector2.down * (_gravity * Time.fixedDeltaTime * Time.fixedDeltaTime);
                point.currentPos += gravityForce;

                point.prevPos = posBeforeSimulation;
                
                
            }
            ApplyConstraints();
            HandleCollisions();
            RemoveGarbage();

            // Add pending points and sticks to simulation
            if (_readyToAddToSimulation)
            {
                foreach (var point in _pointsToAdd)
                {
                    _points.Add(point);
                }

                foreach (var stick in _sticksToAdd)
                {
                    _sticks.Add(stick);
                }

                _pointsToAdd.Clear();
                _sticksToAdd.Clear();
                _readyToAddToSimulation = false;
            }

            // Update renderers
            UpdateRenderers();

            SetBoundaries();
        }

        private void SetBoundaries()
        {
            _minX = float.PositiveInfinity;
            _minY = float.PositiveInfinity;

            _maxX = float.NegativeInfinity;
            _maxY = float.NegativeInfinity;
            
            foreach (var point in _points)
            {
                if (point.currentPos.x < _minX) _minX = point.currentPos.x;
                if (point.currentPos.x > _maxX) _maxX = point.currentPos.x;
                
                if (point.currentPos.y < _minY) _minY = point.currentPos.y;
                if (point.currentPos.y > _maxY) _maxY = point.currentPos.y;

            }
        }

        private void ApplyConstraints()
        {
            for (int i = 0; i < _numIterations; i++)
            {
                foreach (var stick in _sticks)
                {
                    var distance = Vector2.Distance(stick.pointA.currentPos, stick.pointB.currentPos);
                    var difference = stick.length - distance;
                    var correction = difference / 2f;
                    var direction = (stick.pointA.currentPos - stick.pointB.currentPos).normalized;

                    if (!stick.pointA.isFixed)
                        stick.pointA.currentPos += direction * correction;

                    if (!stick.pointB.isFixed)
                        stick.pointB.currentPos -= direction * correction;
                }
            }
        }

        private void HandleCollisions()
        {
            var colliders = RopeSimulator.Instance.GetColliders();
            if (colliders.Count == 0) return;
            foreach (var col in colliders)
            {
                foreach (var point in _points)
                {
                    if(col.GetType() == typeof(SatBoxCollider))
                    {
                        var boxCol = col as SatBoxCollider;
                        if (Vector2.Distance(point.currentPos, col.transform.position) > boxCol.size.x + .5f) continue;
                        CheckCircleVsBoxCollision(point, boxCol);
                    }
                }
            }
            
            
        }
        
        private void RemoveGarbage()
        {
            foreach (var stick in _sticksToRemove)
            {
                _sticks.Remove(stick);
                stick.pointA.Disconnect(stick);
                stick.pointB.Disconnect(stick);
                stick.Destroy();
            }

            foreach (var point in _points)
            {
                if (!point.IsConnectedToAnyStick())
                {
                    _pointsToRemove.Add(point);
                }
                
            }

            foreach (var point in _pointsToRemove)
            {
                _points.Remove(point);
                point.Destroy();
            }
        }

        private void UpdateRenderers()
        {
            foreach (var point in _points)
            {
                point.UpdateRenderer();
            }

            foreach (var stick in _sticks)
            {
                stick.UpdatePositions();
            }
        }

        private Point AddPoint(Vector2 position, bool isFixed = false)
        {
            var point = new Point(position, position, pointSize, pointPrefab, transform);
            point.SetFixed(isFixed);
            _points.Add(point);
            return point;
        }

        public Point AddPointToQueue(Vector2 position, bool isFixed = false)
        {
            var point = new Point(position, position, pointSize, pointPrefab, transform);
            point.SetFixed(isFixed);
            _pointsToAdd.Add(point);
            return point;
        }

        public Stick AddStick(Point pointA, Point pointB)
        {
            // Check if stick already exists
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
            return stick;
        }

        public Stick AddStickToQueue(Point pointA, Point pointB)
        {
            // Check if stick already exists in queue
            foreach (var existingStick in _sticksToAdd)
            {
                if ((existingStick.pointA == pointA && existingStick.pointB == pointB) ||
                    (existingStick.pointA == pointB && existingStick.pointB == pointA))
                {
                    return existingStick;
                }
            }

            var stick = new Stick(pointA, pointB, stickWidth, lineMaterial, transform);
            _sticksToAdd.Add(stick);
            pointA.Connect(stick);
            pointB.Connect(stick);
            return stick;
        }

        public void SetReadyToAddToSimulation()
        {
            _readyToAddToSimulation = true;
        }

        public void RemoveStick(Stick stick)
        {
            _sticksToRemove.Add(stick);
        }
        
        public void ClearRope()
        {
            foreach (var point in _points)
            {
                point.Destroy();
            }

            foreach (var stick in _sticks)
            {
                stick.Destroy();
            }

            _points.Clear();
            _sticks.Clear();
            _pointsToRemove.Clear();
            _sticksToRemove.Clear();
        }

        private void OnDrawGizmos()
        {
            var topLeft = new Vector2(_minX, _maxY);
            var topRight = new Vector2(_maxX, _maxY);
            var bottomLeft = new Vector2(_minX, _minY);
            var bottomRight = new Vector2(_maxX, _minY);
            Gizmos.DrawLine(topLeft, topRight);
            Gizmos.DrawLine(topRight, bottomRight);
            Gizmos.DrawLine(bottomRight, bottomLeft);
            Gizmos.DrawLine(bottomLeft, topLeft);
            
        }
    }
}
