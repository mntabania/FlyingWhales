using System;
using UnityEngine;
using UnityEngine.Serialization;
using UtilityScripts;

/// <summary>
/// This is the base class for all vision triggers (Colliders that can be seen by characters and some objects)
/// </summary>
[RequireComponent(typeof(Collider2D))]
public abstract class BaseVisionTrigger : MonoBehaviour{
    [FormerlySerializedAs("_projectileReciever")] 
    [SerializeField] protected ProjectileReceiver _projectileReceiver;
    [FormerlySerializedAs("mainCollider")] [SerializeField] protected Collider2D _mainCollider;
    private int _filterVotes; //How many things has voted to make this part of the filtered layer?
    public IDamageable damageable { get; private set; }

    #region getters
    public int filterVotes => _filterVotes;
    public ProjectileReceiver projectileReceiver => _projectileReceiver;
    public Collider2D mainCollider => _mainCollider;
    #endregion


    public virtual void Initialize(IDamageable damageable) {
        this.name = $"{damageable} collision trigger";
        this.damageable = damageable;
        _projectileReceiver.gameObject.SetActive(true);
        _projectileReceiver.Initialize(damageable);
        _mainCollider.isTrigger = true; //vision triggers should always be set as triggers.
        SetAllCollidersState(true);
    }
    /// <summary>
    /// Set the active state of both, this collider and the projectile
    /// receiver that is attached to this.
    /// </summary>
    /// <param name="state">The active state to put the colliders in.</param>
    public void SetAllCollidersState(bool state) {
        _mainCollider.enabled = state;
        _projectileReceiver.SetColliderState(state);
    }
    /// <summary>
    /// Set if this vision trigger should be active.
    /// </summary>
    /// <param name="state">The state to set the collider in.</param>
    public void SetVisionTriggerCollidersState(bool state) {
        _mainCollider.enabled = state;
    }
    public void Reset() {
        _filterVotes = 0;
        _mainCollider.enabled = true;
    }

    #region Layers
    public void SetFilterVotes(int votes) {
        _filterVotes = votes;
        DetermineLayerBasedOnVotes();
    }
    /// <summary>
    /// Vote to transfer this object to the filtered layer.
    /// Normally, all characters vision only see filtered objects.
    /// Whenever votes are submitted, a function (<see cref="DetermineLayerBasedOnVotes"/>)
    /// determines whether or not this object should be part of the filtered layer or not.
    /// </summary>
    public void VoteToMakeVisibleToCharacters() {
        _filterVotes = filterVotes + 1;
        DetermineLayerBasedOnVotes();
    }
    /// <summary>
    /// Vote to transfer this object to the unfiltered layer.
    /// Normally, all characters vision only see filtered objects.
    /// Whenever votes are submitted, a function (<see cref="DetermineLayerBasedOnVotes"/>)
    /// determines whether or not this object should be part of the filtered layer or not.
    /// </summary>
    public void VoteToMakeInvisibleToCharacters() {
        _filterVotes = filterVotes - 1;
        DetermineLayerBasedOnVotes();
    }
    /// <summary>
    /// If objects filtered votes is higher than it's default value.
    /// Then this object should 
    /// </summary>
    private void DetermineLayerBasedOnVotes() {
        if (filterVotes > 0) {
            TransferToFilteredLayer();
        } else {
            TransferToNonFilteredLayer();
        }
    }
    private void TransferToFilteredLayer() {
        gameObject.layer = LayerMask.NameToLayer(GameUtilities.Filtered_Object_Layer);
    }
    private void TransferToNonFilteredLayer() {
        gameObject.layer = LayerMask.NameToLayer(GameUtilities.Unfiltered_Object_Layer);
    }
    #endregion
}