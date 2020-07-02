using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using EZObjectPools;
using Inner_Maps;
using UnityEngine;
using Random = System.Random;

public class Projectile : PooledObject {
    
    [SerializeField] private Collider2D _collider;
    [SerializeField] private ParticleSystem projectileParticles;
    [SerializeField] private ParticleSystem collisionParticles;
    [SerializeField] private ParticleCallback collisionParticleCallback;
    [SerializeField] private TrailRenderer _lineRenderer;
    
    public IDamageable targetObject { get; private set; }
    public System.Action<IDamageable, CombatState> onHitAction;

    private Vector3 _pausedVelocity;
    private float _pausedAngularVelocity;
    private CombatState createdBy;
    private Tweener tween;
    private bool _hasHit;
    private float _timeAlive;
    
    #region Monobehaviours
    private void OnDestroy() {
        // Messenger.RemoveListener<bool>(Signals.PAUSED, OnGamePaused);
        Messenger.RemoveListener<Party>(Signals.PARTY_STARTED_TRAVELLING, OnCharacterAreaTravelling);
        Messenger.RemoveListener<Character>(Signals.CHARACTER_DEATH, OnCharacterDied);
        Messenger.RemoveListener<TileObject, Character, LocationGridTile>(Signals.TILE_OBJECT_REMOVED, OnTileObjectRemoved);
        // Messenger.RemoveListener<SpecialToken, LocationGridTile>(Signals.ITEM_REMOVED_FROM_TILE, OnItemRemovedFromTile);
    }
    private void Update() {
        _timeAlive += Time.deltaTime;
        if (_timeAlive > 1f) {
            //destroy projectile
            DestroyProjectile();
        }
    }
    #endregion

    public void SetTarget(Transform target, IDamageable targetObject, CombatState createdBy, Character shooter) {
        // Vector3 diff = target.position - transform.position;
        // diff.Normalize();
        // float rot_z = Mathf.Atan2(diff.y, diff.x) * Mathf.Rad2Deg;
        // transform.rotation = Quaternion.Euler(0f, 0f, rot_z - 90);
        _hasHit = false;
        name = $"Projectile from {shooter.name} targeting {targetObject.name}";
        this.targetObject = targetObject;
        this.createdBy = createdBy;
        _timeAlive = 0f;
        if (projectileParticles != null) {
            projectileParticles.Play();    
        }
        if (targetObject is Character) {
            Messenger.AddListener<Party>(Signals.PARTY_STARTED_TRAVELLING, OnCharacterAreaTravelling);
            Messenger.AddListener<Character>(Signals.CHARACTER_DEATH, OnCharacterDied);
        } else if (targetObject is TileObject) {
            Messenger.AddListener<TileObject, Character, LocationGridTile>(Signals.TILE_OBJECT_REMOVED, OnTileObjectRemoved);
        }
        // Messenger.AddListener<bool>(Signals.PAUSED, OnGamePaused);
        collisionParticleCallback.SetAction(DestroyProjectile); //when the collision particles have successfully stopped. Destroy this object.

        Vector2 targetPoint = target.position;
        if (targetObject is Character character && character.marker.isMoving 
            && character.marker.pathfindingAI.currentPath != null) {
            List<Vector3> trimmedPath = InnerMapManager.Instance.GetTrimmedPath(character);
            if (trimmedPath.Count > 0) {
                targetPoint = trimmedPath.Count > 1 ? trimmedPath[1] : trimmedPath[0];    
            }
        }

        tween = transform.DOMove(targetPoint, 25f).SetSpeedBased(true).SetEase(Ease.Linear).SetAutoKill(true);
        _lineRenderer.enabled = true;
    }

    public void OnProjectileHit(IDamageable poi) {
        _hasHit = true;
        tween?.Kill();
        if (projectileParticles != null) { projectileParticles.Stop(); }
        onHitAction?.Invoke(poi, createdBy);
        _collider.enabled = false;
        collisionParticles.Play(true);
    }

    private void DestroyProjectile() {
        // GameObject.Destroy(this.gameObject);
        ObjectPoolManager.Instance.DestroyObject(this);
    }

    #region Object Pool
    public override void Reset() {
        base.Reset();
        Messenger.RemoveListener<Party>(Signals.PARTY_STARTED_TRAVELLING, OnCharacterAreaTravelling);
        Messenger.RemoveListener<Character>(Signals.CHARACTER_DEATH, OnCharacterDied);
        Messenger.RemoveListener<TileObject, Character, LocationGridTile>(Signals.TILE_OBJECT_REMOVED, OnTileObjectRemoved);
        // Messenger.RemoveListener<bool>(Signals.PAUSED, OnGamePaused);
        _collider.enabled = true;
        // rigidBody.velocity = Vector2.zero;
        // rigidBody.angularVelocity = 0f;
        tween?.Kill();
        tween = null;
        if (projectileParticles != null) {
            projectileParticles.Stop();
            projectileParticles.Clear();
        }
        collisionParticles.Clear();
        onHitAction = null;
        _timeAlive = 0f;
        _lineRenderer.Clear();
        _lineRenderer.enabled = false;
    }
    #endregion
    
    #region Listeners
    // private void OnGamePaused(bool isPaused) {
    //     if (isPaused) {
    //         // _pausedVelocity = rigidBody.velocity;
    //         // _pausedAngularVelocity = rigidBody.angularVelocity;
    //         // rigidBody.velocity = Vector2.zero;
    //         // rigidBody.angularVelocity = 0f;
    //         // rigidBody.isKinematic = true;
    //         tween.Pause();
    //     } else {
    //         // rigidBody.isKinematic = false;
    //         // rigidBody.velocity = _pausedVelocity;
    //         // rigidBody.angularVelocity = _pausedAngularVelocity;
    //         tween.Play();
    //     }
    // }
    private void OnCharacterAreaTravelling(Party party) {
        if (targetObject is Character) {
            if (party.owner == targetObject || party.carriedPOI == targetObject) { //party.characters.Contains(targetPOI as Character)
                DestroyProjectile();
            }
        }
    }
    private void OnCharacterDied(Character character) {
        if (character == targetObject) {
            DestroyProjectile();
        }
    }
    private void OnTileObjectRemoved(TileObject obj, Character removedBy, LocationGridTile removedFrom) {
        if (obj == targetObject) {
            DestroyProjectile();
        }
    }
    #endregion


}
