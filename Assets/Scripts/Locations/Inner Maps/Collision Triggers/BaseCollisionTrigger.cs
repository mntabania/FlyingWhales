using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

/// <summary>
/// This is the base class for all collision triggers (Basically anything that can accept collisions) NOTE: object that can accept collisions must implement the IDamagable interface.
/// </summary>
public abstract class BaseCollisionTrigger<T> : MonoBehaviour, IBaseCollider where T: IDamageable{

    [FormerlySerializedAs("_projectileReciever")] [SerializeField] private ProjectileReceiver _projectileReceiver;
    [SerializeField] protected Collider2D mainCollider;
    private T owner { get; set; }
    public ProjectileReceiver projectileReceiver => _projectileReceiver;
    public IDamageable damageable { get; private set; }
    public virtual void Initialize(T owner) {
        this.owner = owner;
        this.name = $"{owner.ToString()} collision trigger";
        damageable = owner;
        _projectileReceiver.gameObject.SetActive(true);
        _projectileReceiver.Initialize(owner);
    }

    public void SetCollidersState(bool state) {
        mainCollider.enabled = state;
        _projectileReceiver.SetColliderState(state);
    }
    public void SetColliderLayer(string layerName) {
        int newLayer = LayerMask.NameToLayer(layerName);
        this.gameObject.layer = newLayer;
        _projectileReceiver.gameObject.layer = newLayer;
    }
}

