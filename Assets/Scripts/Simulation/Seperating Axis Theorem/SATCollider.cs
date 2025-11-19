using System;
using UnityEngine;

public abstract class SatCollider : MonoBehaviour
{
    [SerializeField] private bool isStatic;
    public bool tearRope;
    public float friction;
    public float bounciness;
    public bool IsStatic => isStatic;

    private SpriteRenderer _renderer;
    
    private void Awake()
    {
        _renderer = GetComponent<SpriteRenderer>();
    }

    private void Start()
    {
        SatEngine.Instance.RegisterCollider(this);
        RopeSimulator.Instance.AddCollider(this);
    }

    private void OnDisable()
    {
        if(RopeSimulator.Instance != null)
            RopeSimulator.Instance.RemoveCollider(this);
        if(SatEngine.Instance != null)
            SatEngine.Instance.UnregisterCollider(this);
    }
    public virtual void SetSize(Vector2 size){}
    public virtual void SetSize(float radius){}

    public virtual Vector2[] GetWorldVertices(){
        return default;
    }

    public virtual Vector2[] GetAxes(){
        return default;
    }
}
