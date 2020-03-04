using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Inner_Maps;
using Traits;
using UnityEngine;
using Random = UnityEngine.Random;

public class PoisonCloudMapObjectVisual : MovingMapObjectVisual<TileObject> {
    
    [SerializeField] private ParticleSystem _cloudEffect;
    [SerializeField] private ParticleSystem _explosionEffect;
    
    private string _expiryKey;
    private Tweener _movement;
    private List<ITraitable> _objsInRange;
    
    
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
    }
    public override void PlaceObjectAt(LocationGridTile tile) {
        base.PlaceObjectAt(tile);
        _cloudEffect.gameObject.SetActive(true);
        _cloudEffect.Play();
        MoveToRandomDirection();
        OnGamePaused(GameManager.Instance.isPaused);
        _expiryKey = SchedulingManager.Instance.AddEntry(GameManager.Instance.Today().AddTicks(GameManager.Instance.GetTicksBasedOnHour(Random.Range(2, 6))), Expire, this);
        Messenger.AddListener(Signals.TICK_ENDED, PerTick);
        Messenger.AddListener<bool>(Signals.PAUSED, OnGamePaused);
        Messenger.AddListener<ITraitable, Trait>(Signals.TRAITABLE_GAINED_TRAIT, OnTraitableGainedTrait);
        Messenger.AddListener<PROGRESSION_SPEED>(Signals.PROGRESSION_SPEED_CHANGED, OnProgressionSpeedChanged);
        isSpawned = true;
    }
    public override void Reset() {
        base.Reset();
        _expiryKey = string.Empty;
        _movement?.Kill();
        _movement = null;
        _objsInRange = null;
        _cloudEffect.Clear();
        _explosionEffect.Clear();
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
        } else {
            _movement.Play();
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
    #endregion

    #region Effects
    private void PerTick() {
        if (isSpawned == false) {
            return;
        }
        int roll = Random.Range(0, 100);
        if (roll < 15 && _objsInRange.Count > 0) {
            string summary = $"{GameManager.Instance.TodayLogString()}Per tick check of poison cloud. Roll is {roll.ToString()}.";
            ITraitable traitable = UtilityScripts.CollectionUtilities.GetRandomElement(_objsInRange);
            traitable.traitContainer.AddTrait(traitable, "Poisoned");
            summary = $"{summary}\nChance met! Target is {traitable.ToString()}";
            Debug.Log(summary);
        }
    }
    private void Explode() {
        Debug.Log($"{GameManager.Instance.TodayLogString()}{this.name} has exploded!");
        List<ITraitable> affectedObjects =
            UtilityScripts.GameUtilities.GetTraitableDiamondTilesFromRadius(_mapLocation.innerMap, gridTileLocation.localPlace, 5);
        for (int i = 0; i < affectedObjects.Count; i++) {
            ITraitable traitable = affectedObjects[i];
            traitable.AdjustHP(-Mathf.FloorToInt(obj.maxHP * 0.75f), ELEMENTAL_TYPE.Normal, true);
            if (traitable.currentHP > 0) {
                traitable.traitContainer.AddTrait(traitable, "Poisoned");
                // if (traitable is GenericTileObject) {
                //     traitable.gridTileLocation.parentMap.groundTilemap.SetColor(traitable.gridTileLocation.localPlace, Color.black);
                // } else {
                //     traitable?.mapObjectVisual.SetColor(Color.black);
                // } 
            }
        }
        _cloudEffect.TriggerSubEmitter(0);
        Expire();
    }
    private void OnTraitableGainedTrait(ITraitable traitable, Trait trait) {
        if (trait is Burning && _objsInRange.Contains(traitable)) {
            Explode();
        }
    }
    // private void OnParticleSystemStopped() {
    //     if (isSpawned) {
    //         _cloudEffect.gameObject.SetActive(false);
    //         Expire();    
    //     }
    // }
    #endregion
    
    #region Triggers
    public void OnTriggerEnter2D(Collider2D collision) {
        if (isSpawned == false) { return; }
        IBaseCollider collidedWith = collision.gameObject.GetComponent<IBaseCollider>();
        if (collidedWith != null && collidedWith.damageable is ITraitable traitable) { 
            AddObject(traitable);   
        }
    }
    public void OnTriggerExit2D(Collider2D collision) {
        if (isSpawned == false) { return; }
        IBaseCollider collidedWith = collision.gameObject.GetComponent<IBaseCollider>();
        if (collidedWith != null && collidedWith.damageable is ITraitable traitable) { 
            RemoveObject(traitable);   
        }
    }
    #endregion
    
    #region POI's
    private void AddObject(ITraitable obj) {
        if (!_objsInRange.Contains(obj)) {
            _objsInRange.Add(obj);
            OnAddPOI(obj);
        }
    }
    private void RemoveObject(ITraitable obj) {
        _objsInRange.Remove(obj);
    }
    private void OnAddPOI(ITraitable obj) {
        if (obj.traitContainer.GetNormalTrait<Trait>("Burning") != null) {
            Explode();
        }
    }
    #endregion
    
    #region Expiration
    private void Expire() {
        Debug.Log($"{this.name} expired!");
        _cloudEffect.Stop();
        isSpawned = false;
        if (string.IsNullOrEmpty(_expiryKey) == false) {
            SchedulingManager.Instance.RemoveSpecificEntry(_expiryKey);
        }
        Messenger.RemoveListener(Signals.TICK_ENDED, PerTick);
        Messenger.RemoveListener<bool>(Signals.PAUSED, OnGamePaused);
        Messenger.RemoveListener<ITraitable, Trait>(Signals.TRAITABLE_GAINED_TRAIT, OnTraitableGainedTrait);
        Messenger.RemoveListener<PROGRESSION_SPEED>(Signals.PROGRESSION_SPEED_CHANGED, OnProgressionSpeedChanged);
        StartCoroutine(DestroyCoroutine());
    }
    private IEnumerator DestroyCoroutine() {
        yield return new WaitForSeconds(0.8f);
        ObjectPoolManager.Instance.DestroyObject(this);
    }
    #endregion
    
        
}
