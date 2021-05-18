﻿using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Inner_Maps;
using Traits;
using UnityEngine;
using Random = UnityEngine.Random;

public class VaporMapObjectVisual : MovingMapObjectVisual<TileObject> {
    
    [SerializeField] private ParticleSystem _vaporEffect;
    
    private string _expiryKey;
    private Tweener _movement;
    private int _size;
    private Vapor _vapor;

    public bool wasJustPlaced { get; private set; }

    #region Abstract Members Implementation
    public override void UpdateTileObjectVisual(TileObject obj) { }
    #endregion

    #region Monobehaviours
    private void Awake() {
        visionTrigger = transform.GetComponentInChildren<TileObjectVisionTrigger>();
    }
    protected override void Update() {
        base.Update();
        if (isSpawned && gridTileLocation == null) {
            Expire();
        }
    }
    #endregion

    #region Overrides
    public override void Initialize(TileObject obj) {
        base.Initialize(obj);
        _vapor = obj as Vapor;
    }
    public override void PlaceObjectAt(LocationGridTile tile) {
        base.PlaceObjectAt(tile);
        wasJustPlaced = true;
        if (_vapor.size > 0) {
            SetSize(_vapor.size);
        }
        GameDate dueDate = GameManager.Instance.Today().AddTicks(1);
        SchedulingManager.Instance.AddEntry(dueDate, () => wasJustPlaced = false, this);
        MoveToRandomDirection();
        _expiryKey = SchedulingManager.Instance.AddEntry(_vapor.expiryDate, Expire, this);
        Messenger.AddListener<bool>(UISignals.PAUSED, OnGamePaused);
        Messenger.AddListener<PROGRESSION_SPEED>(UISignals.PROGRESSION_SPEED_CHANGED, OnProgressionSpeedChanged);
        isSpawned = true;

        if (GameManager.Instance.isPaused) {
            _movement.Pause();
            StartCoroutine(PlayParticleCoroutineWhenGameIsPaused());
        } else {
            _movement.Play();
            _vaporEffect.Play();
        }
    }
    public override void Reset() {
        base.Reset();
        _expiryKey = string.Empty;
        _movement?.Kill();
        _movement = null;
        _vaporEffect.Clear();
    }
    #endregion

    #region Movement
    private void MoveToRandomDirection() {
        Vector3 direction = (new Vector3(Random.Range(-1.0f, 1.0f), Random.Range(-1.0f, 1.0f), 0f)).normalized * 50f;
        direction += transform.position;
        _movement = transform.DOMove(direction, 0.3f).SetSpeedBased(true);
    }
    private void OnGamePaused(bool isPaused) {
        if (isPaused) {
            _movement.Pause();
            _vaporEffect.Pause();
        } else {
            _movement.Play();
            _vaporEffect.Play();
        }
    }
    private void OnProgressionSpeedChanged(PROGRESSION_SPEED progression) {
        if (_movement == null) {
            return;
        }
        switch (progression) {
            case PROGRESSION_SPEED.X1:
                _movement.timeScale = 1f;
                break;
            case PROGRESSION_SPEED.X2:
                _movement.timeScale = 1.2f;
                break;
            case PROGRESSION_SPEED.X4:
                _movement.timeScale = 1.4f;
                break;
        }
    }
    public override void SetWorldPosition(Vector3 worldPosition) {
        base.SetWorldPosition(worldPosition);
        _movement?.Kill();
        _movement = null;
        MoveToRandomDirection();
    }
    #endregion

    #region Utilities
    public void SetSize(int size) {
        _size = size;
        ChangeScaleBySize();
    }
    private void ChangeScaleBySize() {
        //int scaleSize = 1;
        //if(_size > 1) {
        //    scaleSize = _size + 1;
        //}
        // this.gameObject.transform.localScale = new Vector3(_size, _size, 1f);
        // _vaporEffect.transform.localScale = new Vector3(_size, _size, _size);
        transform.DOScale(new Vector3(_size, _size, 1f), 1f);
        _vaporEffect.transform.DOScale(new Vector3(_size, _size, _size), 1f);
    }
    #endregion
    
    #region Expiration
    public void Expire() {
#if DEBUG_LOG
        Debug.Log($"{this.name} expired!");
#endif
        _vapor.OnExpire();
        _vaporEffect.Stop();
        visionTrigger.SetAllCollidersState(false);
        isSpawned = false;
        if (string.IsNullOrEmpty(_expiryKey) == false) {
            SchedulingManager.Instance.RemoveSpecificEntry(_expiryKey);
        }
        Messenger.RemoveListener<bool>(UISignals.PAUSED, OnGamePaused);
        Messenger.RemoveListener<PROGRESSION_SPEED>(UISignals.PROGRESSION_SPEED_CHANGED, OnProgressionSpeedChanged);
        _vapor.Expire();
        StartCoroutine(DestroyCoroutine());
    }
    private IEnumerator DestroyCoroutine() {
        yield return new WaitForSeconds(2f);
        ObjectPoolManager.Instance.DestroyObject(this);
    }
#endregion

#region Particles
    private IEnumerator PlayParticleCoroutineWhenGameIsPaused() {
        //Playing particle effect is done in a coroutine so that it will wait one frame before pausing the particles if the game is paused when the particle is activated
        //This will make sure that the particle effect will show but it will be paused right away
        _vaporEffect.Play();
        yield return new WaitForSeconds(0.1f);
        _vaporEffect.Pause();
    }
#endregion

#region Triggers
    public void OnTriggerEnter2D(Collider2D collision) {
        if (isSpawned == false) { return; }
        BaseVisionTrigger collidedWith = collision.gameObject.GetComponent<BaseVisionTrigger>();
        if (collidedWith != null && collidedWith.damageable is Vapor otherVapor && otherVapor != _vapor) {
            if (wasJustPlaced == false) {
                CollidedWithVapor(otherVapor);
            }
        }
    }
    private void CollidedWithVapor(Vapor otherVapor) {
        if (!otherVapor.hasExpired) {
            if (_vapor.size != _vapor.maxSize) {
                int stacksToCombine = otherVapor.stacks;
                otherVapor.mapVisual.transform.DOKill();
                otherVapor.mapVisual.transform.DOMove(transform.position, 4f);
                otherVapor.SetDoExpireEffect(false);
                otherVapor.Neutralize();
                _vapor.SetStacks(_vapor.stacks + stacksToCombine);
            }
        }
    }
#endregion
}
