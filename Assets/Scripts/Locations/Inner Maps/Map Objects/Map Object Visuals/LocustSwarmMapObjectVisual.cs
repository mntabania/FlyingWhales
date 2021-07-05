﻿using System.Collections.Generic;
using DG.Tweening;
using Inner_Maps;
using Traits;
using UnityEngine;
using Random = UnityEngine.Random;
using System.Collections;

public class LocustSwarmMapObjectVisual : MovingMapObjectVisual {

    private string _expiryKey;
    private string _movementKey;
    private Tweener _movement;
    private List<ITraitable> _objsInRange;
    private LocustSwarm _locustSwarm;

    public ParticleSystem locustSwarmParticle;

    #region Abstract Members Implementation
    public override void UpdateTileObjectVisual(TileObject obj) { }
    #endregion

    #region Monobehaviours
    private void Awake() {
        visionTrigger = transform.GetComponentInChildren<TileObjectVisionTrigger>();
        _objsInRange = new List<ITraitable>();
    }
    private void LateUpdate() {
        if (isSpawned && gridTileLocation == null) {
            Expire();
        }
    }
    #endregion

    #region Overrides
    public override void Initialize(TileObject obj) {
        base.Initialize(obj);
        _locustSwarm = obj as LocustSwarm;
    }
    public override void PlaceObjectAt(LocationGridTile tile) {
        base.PlaceObjectAt(tile);
        RandomizeDirection();
        _expiryKey = SchedulingManager.Instance.AddEntry(_locustSwarm.expiryDate, Expire, this);
        Messenger.AddListener<bool>(UISignals.PAUSED, OnGamePaused);
        Messenger.AddListener<PROGRESSION_SPEED>(UISignals.PROGRESSION_SPEED_CHANGED, OnProgressionSpeedChanged);
        Messenger.AddListener<ITraitable, Trait>(TraitSignals.TRAITABLE_GAINED_TRAIT, OnTraitableGainedTrait);
        if (GameManager.Instance.isPaused || !GameManager.Instance.gameHasStarted) {
            _movement.Pause();
            StartCoroutine(PlayParticleCoroutineWhenGameIsPaused());
        } else {
            _movement.Play();
            locustSwarmParticle.Play();
        }
        isSpawned = true;
    }
    public override void Reset() {
        base.Reset();
        isSpawned = false;
        _expiryKey = string.Empty;
        if (string.IsNullOrEmpty(_movementKey) == false) {
            SchedulingManager.Instance.RemoveSpecificEntry(_movementKey);    
        }
        _movementKey = string.Empty;
        if (_movement != null) {
            DOTween.Kill(_movement);
            _movement = null;    
        }
        DOTween.Kill(this);
        DOTween.Kill(transform);
        _objsInRange.Clear();
        Messenger.RemoveListener<bool>(UISignals.PAUSED, OnGamePaused);
        Messenger.RemoveListener<PROGRESSION_SPEED>(UISignals.PROGRESSION_SPEED_CHANGED, OnProgressionSpeedChanged);
        Messenger.RemoveListener<ITraitable, Trait>(TraitSignals.TRAITABLE_GAINED_TRAIT, OnTraitableGainedTrait);
    }
    #endregion

    #region Movement
    private void OnGamePaused(bool isPaused) {
        if (isPaused) {
            _movement.Pause();
            locustSwarmParticle.Pause();
        } else {
            _movement.Play();
            locustSwarmParticle.Play();
        }
    }
    private void RandomizeDirection() {
        float processedSpeed = PlayerSkillManager.Instance.GetSkillMovementSpeedPerLevel(PLAYER_SKILL_TYPE.LOCUST_SWARM) / 100f;
        Vector3 position = transform.position;
        Vector3 direction = (new Vector3(Random.Range(-1.0f, 1.0f), Random.Range(-1.0f, 1.0f), 0f)).normalized * 50f;
        direction += position;
        if (_movement != null) { _movement.Kill(); }
        _movement = transform.DOMove(direction, processedSpeed).SetSpeedBased(true);
        OnGamePaused(GameManager.Instance.isPaused);
        //schedule change direction after 1 hour
        _movementKey = SchedulingManager.Instance.AddEntry(GameManager.Instance.Today().AddTicks(GameManager.ticksPerHour), RandomizeDirection, this);
    }
    #endregion

    #region Triggers
    public void OnTriggerEnter2D(Collider2D collision) {
        BaseVisionTrigger collidedWith = collision.gameObject.GetComponent<BaseVisionTrigger>();
        if (collidedWith != null && collidedWith.damageable is ITraitable traitable 
            && CanBeAffectedByLocustSwarm(traitable)) { 
            AddObject(traitable);   
        }
    }
    public void OnTriggerExit2D(Collider2D collision) {
        BaseVisionTrigger collidedWith = collision.gameObject.GetComponent<BaseVisionTrigger>();
        if (collidedWith != null && collidedWith.damageable is ITraitable traitable
            && CanBeAffectedByLocustSwarm(traitable)) { 
            RemoveObject(traitable);   
        }
    }
    #endregion
    
    #region POI's
    private void AddObject(ITraitable obj) {
        if (!_objsInRange.Contains(obj)) {
            _objsInRange.Add(obj);
            CheckObjectForEffects(obj);
        }
    }
    private void RemoveObject(ITraitable obj) {
        _objsInRange.Remove(obj);
    }
    #endregion

    #region Effects
    private void OnTraitableGainedTrait(ITraitable traitable, Trait trait) {
        if (CanBeAffectedByLocustSwarm(traitable) && _objsInRange.Contains(traitable) && 
            (trait is Burning || trait.name == "Edible")) {
            CheckObjectForEffects(traitable);
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
    private void CheckObjectForEffects(ITraitable obj) {
        if (obj.traitContainer.GetTraitOrStatus<Trait>("Edible") != null || obj is Crops) {
            obj.AdjustHP(-obj.currentHP, ELEMENTAL_TYPE.Normal, true, _locustSwarm, showHPBar: true, isPlayerSource: true);
        }
        if (obj.traitContainer.GetTraitOrStatus<Trait>("Burning") != null) {
            _locustSwarm.AdjustHP(-Mathf.FloorToInt(_locustSwarm.maxHP * 0.2f), ELEMENTAL_TYPE.Fire, true, obj);
            obj.traitContainer.RemoveTrait(obj, "Burning");
        }
    }
    private bool CanBeAffectedByLocustSwarm(ITraitable traitable) {
        if (traitable is Character) {
            return false;
        }
        return true;
    }
    #endregion
    
    #region Expiration
    public void Expire() {
        if (string.IsNullOrEmpty(_expiryKey) == false) {
            SchedulingManager.Instance.RemoveSpecificEntry(_expiryKey);    
        }
        if (string.IsNullOrEmpty(_movementKey) == false) {
            SchedulingManager.Instance.RemoveSpecificEntry(_movementKey);    
        }
        isSpawned = false;
        _locustSwarm.Expire();
        ObjectPoolManager.Instance.DestroyObject(this);
    }
    #endregion

    #region Particles
    private IEnumerator PlayParticleCoroutineWhenGameIsPaused() {
        //Playing particle effect is done in a coroutine so that it will wait one frame before pausing the particles if the game is paused when the particle is activated
        //This will make sure that the particle effect will show but it will be paused right away
        locustSwarmParticle.Play();
        yield return new WaitForSeconds(0.1f);
        locustSwarmParticle.Pause();
    }
    #endregion
}
