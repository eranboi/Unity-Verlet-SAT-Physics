using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace Environment.Rope
{
    [System.Serializable]
    public class Point
    {
        public Vector2 currentPos;
        public Vector2 prevPos;
        public GameObject gameObject;
        public bool isFixed;
        
        private readonly HashSet<Stick> _connectedSticks = new();
        private readonly Color _fixedColor = new(1f, 0f, 0.4f);
        private readonly Color _normalColor = new(0.99f, 0.67f, 0.8f);
        private Transform _parent;
        private SpriteRenderer _renderer;

        public Point(Vector2 currentPos, Vector2 prevPos, float size, GameObject pointPrefab, Transform parent)
        {
            this.currentPos = currentPos;
            this.prevPos = prevPos;
            
            gameObject = Object.Instantiate(pointPrefab, parent, true);
            
            gameObject.transform.position = this.currentPos;
            gameObject.transform.localScale = Vector3.one * size * 2;
            
            _renderer = gameObject.GetComponent<SpriteRenderer>();

            SetFixed(false);
        }

        public void Connect(Stick stick)
        {
            _connectedSticks.Add(stick);
        }

        public void Disconnect(Stick stick)
        {
            _connectedSticks.Remove(stick);
        }

        public bool IsConnectedToAnyStick()
        {
            return _connectedSticks.Count > 0;
        }

        public void UpdateRenderer()
        {
            if (gameObject != null)
                gameObject.transform.position = currentPos;
        }

        public void SetFixed(bool value)
        {
            isFixed = value;
            
            if (_renderer != null)
            {
                _renderer.color = isFixed ? _fixedColor : _normalColor;
            }
        }

        public void Destroy()
        {
            if (gameObject != null)
            {
#if UNITY_EDITOR
                Object.DestroyImmediate(gameObject);
#else
                Object.Destroy(gameObject);
#endif  
            }
        }

        
    }
    
}