using System.Collections.Generic;
using ArrowPath.Utils;
using Environment.Rope;
using Environment.Seperating_Axis_Theorem;
using UnityEngine;

/// <summary>
    /// Main simulator that manages all ropes and their physics simulation
    /// </summary>
    public class RopeSimulator : Singleton<RopeSimulator>
    {


        [Header("Simulation Settings")]
        [SerializeField] private float gravity = 9.81f;
        [SerializeField] [Range(1, 50)] private int numIterations = 10;
        [SerializeField] private bool simulate = true;
        [SerializeField] private float tearLength = 5f;
        
        [Header("Colliders")]
        private readonly List<SatCollider> _colliders = new();
        
        // All ropes in the simulation
        private readonly HashSet<Rope> _allRopes = new();
        
        public float Gravity => gravity;
        public int NumIterations => numIterations;
        public bool IsSimulating => simulate;
        public float TearLength => tearLength;

        private void FixedUpdate()
        {
            if (!simulate) return;

            foreach (var rope in _allRopes)
            {
                if (rope != null && rope.gameObject.activeInHierarchy)
                    rope.Simulate();
            }
        }

        public void RegisterRope(Rope rope)
        {
            _allRopes.Add(rope);
        }

        public void UnregisterRope(Rope rope)
        {
            _allRopes.Remove(rope);
        }

        public void AddCollider(SatCollider col)
        {
            if (!_colliders.Contains(col))
                _colliders.Add(col);
        }

        public void RemoveCollider(SatCollider col)
        {
            _colliders.Remove(col);
        }

        public List<SatCollider> GetColliders()
        {
            return new List<SatCollider>(_colliders);
        }

        public void ToggleSimulation()
        {
            simulate = !simulate;
        }

        public void SetSimulation(bool value)
        {
            simulate = value;
        }
    }