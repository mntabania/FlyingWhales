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
    [SerializeField] private int _size;
    
    private string _expiryKey;
    private Tweener _movement;
    private List<ITraitable> _objsInRange;
    private PoisonCloudTileObject _poisonCloud;

    #region Abstract Members Implementation
    public override void ApplyFurnitureSettings(FurnitureSetting furnitureSetting) { }
    public virtual bool IsMapObjectMenuVisible() {
        return true;
    }
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
        _objsInRange = new List<ITraitable>();
        _poisonCloud = obj as PoisonCloudTileObject;
    }
    public override void PlaceObjectAt(LocationGridTile tile) {
        base.PlaceObjectAt(tile);
        _cloudEffect.gameObject.SetActive(true);
        _cloudEffect.Play();
        MoveToRandomDirection();
        _expiryKey = SchedulingManager.Instance.AddEntry(GameManager.Instance.Today().
            AddTicks(_poisonCloud.durationInTicks), Expire, this);
        Messenger.AddListener(Signals.TICK_ENDED, PerTick);
        Messenger.AddListener<bool>(Signals.PAUSED, OnGamePaused);
        Messenger.AddListener<ITraitable, Trait>(Signals.TRAITABLE_GAINED_TRAIT, OnTraitableGainedTrait);
        Messenger.AddListener<PROGRESSION_SPEED>(Signals.PROGRESSION_SPEED_CHANGED, OnProgressionSpeedChanged);
        isSpawned = true;

        if (GameManager.Instance.isPaused) {
            _movement.Pause();
            StartCoroutine(PlayParticleCoroutineWhenGameIsPaused());
        } else {
            _movement.Play();
            _cloudEffect.Play();
            _explosionEffect.Play();
        }
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
        _movement = transform.DOMove(direction, 0.15f).SetSpeedBased(true);
    }
    private void OnGamePaused(bool isPaused) {
        if (isPaused) {
            _movement.Pause();
            _cloudEffect.Pause();
            _explosionEffect.Pause();
        } else {
            _movement.Play();
            _cloudEffect.Play();
            _explosionEffect.Play();
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
    public void Explode() {
        Debug.Log($"{GameManager.Instance.TodayLogString()}{this.name} has exploded!");
        _cloudEffect.TriggerSubEmitter(0);
        Expire();
        List<LocationGridTile> affectedTiles =
            gridTileLocation.GetTilesInRadius(_size, includeCenterTile: true, includeTilesInDifferentStructure: true);
        for (int i = 0; i < affectedTiles.Count; i++) {
            LocationGridTile tile = affectedTiles[i];
            tile.PerformActionOnTraitables(ApplyExplosionEffect);
        }
    }
    private void ApplyExplosionEffect(ITraitable traitable) {
        traitable.AdjustHP(-250, ELEMENTAL_TYPE.Fire, true, showHPBar: true);
        if (traitable.currentHP > 0) {
            traitable.traitContainer.AddTrait(traitable, "Poisoned");
        }
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
        BaseVisionTrigger collidedWith = collision.gameObject.GetComponent<BaseVisionTrigger>();
        if (collidedWith != null) {
            if(collidedWith.damageable is PoisonCloudTileObject otherPoisonCloud && otherPoisonCloud != _poisonCloud) {
                CollidedWithPoisonCloud(otherPoisonCloud);
            } else if (collidedWith.damageable is ITraitable traitable) {
                AddObject(traitable);
            }
        }
        
    }
    public void OnTriggerExit2D(Collider2D collision) {
        if (isSpawned == false) { return; }
        BaseVisionTrigger collidedWith = collision.gameObject.GetComponent<BaseVisionTrigger>();
        if (collidedWith != null && collidedWith.damageable is ITraitable traitable) { 
            RemoveObject(traitable);   
        }
    }
    private void CollidedWithPoisonCloud(PoisonCloudTileObject otherPoisonCloud) {
        if (_poisonCloud.size != _poisonCloud.maxSize) {
            int stacksToCombine = otherPoisonCloud.stacks;
            otherPoisonCloud.Neutralize();
            _poisonCloud.SetStacks(_poisonCloud.stacks + stacksToCombine);
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
        } else if (obj is PoisonCloudTileObject otherPoisonCloud) {
            if(_poisonCloud.size != _poisonCloud.maxSize) {
                int stacksToCombine = otherPoisonCloud.stacks;
                otherPoisonCloud.Neutralize();
                _poisonCloud.SetStacks(_poisonCloud.stacks + stacksToCombine);
            }
        }
    }
    #endregion
    
    #region Expiration
    public void Expire() {
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
        _poisonCloud.Expire();
        StartCoroutine(DestroyCoroutine());
    }
    private IEnumerator DestroyCoroutine() {
        yield return new WaitForSeconds(0.8f);
        ObjectPoolManager.Instance.DestroyObject(this);
    }
    #endregion

    #region Particles
    private IEnumerator PlayParticleCoroutineWhenGameIsPaused() {
        //Playing particle effect is done in a coroutine so that it will wait one frame before pausing the particles if the game is paused when the particle is activated
        //This will make sure that the particle effect will show but it will be paused right away
        _cloudEffect.Play();
        _explosionEffect.Play();
        yield return new WaitForSeconds(0.1f);
        _cloudEffect.Pause();
        _explosionEffect.Pause();
    }
    #endregion

    #region Size
    public void SetSize(int size) {
        _size = size;
        ChangeScaleBySize();
    }
    private void ChangeScaleBySize() {
        gameObject.transform.localScale = new Vector3(_size, _size, _size);
        _cloudEffect.transform.localScale = new Vector3(_size, _size, _size);
        // _cloudEffect.transform.localScale = new Vector3(_size, _size, _size);
        //ParticleSystem.MainModule mainModule = _cloudEffect.main;
        //mainModule.startSpeed = (_size + 1) / 10f;
        //mainModule.startLifetime = _size;
        //ParticleSystem.EmissionModule emissionModule = _cloudEffect.emission;
        //emissionModule.rateOverTime = (_size + 1) * 10;
    }
    #endregion
}
