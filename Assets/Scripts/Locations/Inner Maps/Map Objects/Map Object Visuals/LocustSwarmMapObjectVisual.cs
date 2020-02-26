using System.Collections.Generic;
using DG.Tweening;
using Inner_Maps;
using Traits;
using UnityEngine;
using Random = UnityEngine.Random;

public class LocustSwarmMapObjectVisual : MovingMapObjectVisual<TileObject> {

    private string _expiryKey;
    private string _movementKey;
    private Tweener _movement;
    private List<ITraitable> _objsInRange;
    private LocustSwarmTileObject _locustSwarm;

    #region Abstract Members Implementation
    public override void ApplyFurnitureSettings(FurnitureSetting furnitureSetting) { }
    public override bool IsMapObjectMenuVisible() {
        return true;
    }
    public override void UpdateTileObjectVisual(TileObject obj) { }
    public override void UpdateCollidersState(TileObject obj) { }
    #endregion

    #region Monobehaviours
    private void Awake() {
        collisionTrigger = transform.GetComponentInChildren<TileObjectCollisionTrigger>();
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
        _objsInRange = new List<ITraitable>();
        _locustSwarm = obj as LocustSwarmTileObject;
    }
    public override void PlaceObjectAt(LocationGridTile tile) {
        base.PlaceObjectAt(tile);
        isSpawned = true;
        RandomizeDirection();
        OnGamePaused(GameManager.Instance.isPaused);
        _expiryKey = SchedulingManager.Instance.AddEntry(GameManager.Instance.Today().AddTicks(GameManager.Instance.GetTicksBasedOnHour(6)), Expire, this);
        Messenger.AddListener<bool>(Signals.PAUSED, OnGamePaused);
        Messenger.AddListener<PROGRESSION_SPEED>(Signals.PROGRESSION_SPEED_CHANGED, OnProgressionSpeedChanged);
        Messenger.AddListener<ITraitable, Trait>(Signals.TRAITABLE_GAINED_TRAIT, OnTraitableGainedTrait);
    }
    public override void Reset() {
        base.Reset();
        isSpawned = false;
        _expiryKey = string.Empty;
        _movementKey = string.Empty;
        _movement = null;
        _objsInRange = null;
        Messenger.RemoveListener<bool>(Signals.PAUSED, OnGamePaused);
        Messenger.RemoveListener<PROGRESSION_SPEED>(Signals.PROGRESSION_SPEED_CHANGED, OnProgressionSpeedChanged);
        Messenger.RemoveListener<ITraitable, Trait>(Signals.TRAITABLE_GAINED_TRAIT, OnTraitableGainedTrait);
    }
    #endregion

    #region Movement
    private void OnGamePaused(bool isPaused) {
        if (isPaused) {
            _movement.Pause();    
        } else {
            _movement.Play();
        }
    }
    private void RandomizeDirection() {
        Vector3 direction = (new Vector3(Random.Range(-1.0f, 1.0f), 0.0f, Random.Range(-1.0f, 1.0f))).normalized;
        _movement = transform.DOMove(direction, 0.3f).SetSpeedBased(true);
        OnGamePaused(GameManager.Instance.isPaused);
        //schedule change direction after 1 hour
        _movementKey =
            SchedulingManager.Instance.AddEntry(GameManager.Instance.Today().AddTicks(GameManager.ticksPerHour),
                RandomizeDirection, this);
    }
    #endregion

    #region Triggers
    public void OnTriggerEnter2D(Collider2D collision) {
        IBaseCollider collidedWith = collision.gameObject.GetComponent<IBaseCollider>();
        if (collidedWith != null && collidedWith.damageable is ITraitable traitable 
            && CanBeAffectedByLocustSwarm(traitable)) { 
            AddObject(traitable);   
        }
    }
    public void OnTriggerExit2D(Collider2D collision) {
        IBaseCollider collidedWith = collision.gameObject.GetComponent<IBaseCollider>();
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
        if (obj.traitContainer.GetNormalTrait<Trait>("Edible") != null) {
            obj.AdjustHP(-obj.currentHP, ELEMENTAL_TYPE.Normal, true, _locustSwarm);
        }
        if (obj.traitContainer.GetNormalTrait<Trait>("Burning") != null) {
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
        ObjectPoolManager.Instance.DestroyObject(this);
    }
    #endregion
    
        
}
