using UnityEngine;

namespace Environment.Rope
{
    [System.Serializable]
    public class Stick
    {
        public readonly Point pointA;
        public readonly Point pointB;
        public readonly float length;
        
        private readonly LineRenderer _lineRenderer;
        public GameObject gameObject { get; private set; }
        private readonly Color _color = new(0.99f, 0.67f, 0.8f);

        public Stick(Point pointA, Point pointB, float width, Material lineMaterial, Transform parent)
        {
            this.pointA = pointA;
            this.pointB = pointB;
            length = Vector2.Distance(pointA.currentPos, pointB.currentPos);
            
            gameObject = new GameObject("Stick");
            gameObject.transform.SetParent(parent);
            gameObject.transform.position = (pointA.currentPos + pointB.currentPos) / 2f;
            
            _lineRenderer = gameObject.AddComponent<LineRenderer>();
            
            if (lineMaterial != null)
                _lineRenderer.sharedMaterial = lineMaterial;
            else
                _lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
            
            _lineRenderer.startWidth = width;
            _lineRenderer.endWidth = width;
            _lineRenderer.startColor = _color;
            _lineRenderer.endColor = _color;
            _lineRenderer.positionCount = 2;
            
            UpdatePositions();
        }
        
        // An overload for an already existing game object -- Created in the editor.
        public Stick(Point pointA, Point pointB, float width, GameObject existingGameObject)
        {
            this.pointA = pointA;
            this.pointB = pointB;
            this.gameObject = existingGameObject;
            length = Vector2.Distance(pointA.currentPos, pointB.currentPos);
            
            _lineRenderer = gameObject.GetComponent<LineRenderer>();
            if (_lineRenderer != null)
            {
                _lineRenderer.startWidth = width;
                _lineRenderer.endWidth = width;
                UpdatePositions();
            }
        }

        public void UpdatePositions()
        {
            if (_lineRenderer != null)
            {
                _lineRenderer.SetPosition(0, pointA.currentPos);
                _lineRenderer.SetPosition(1, pointB.currentPos);
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