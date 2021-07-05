using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Inner_Maps;
using Traits;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Profiling;
using UtilityScripts;
using Random = UnityEngine.Random;

public class PoisonCloudMapObjectVisual : MovingMapObjectVisual {
    
    [SerializeField] private ParticleSystem _cloudEffect;
    [SerializeField] private ParticleSystem _explosionEffect;
    [SerializeField] private int _size;
    
    private string _expiryKey;
    private Tweener _movement;
    private HashSet<ITraitable> _objsInRange;
    private PoisonCloud _poisonCloud;
    
    public bool wasJustPlaced { get; private set; }

    #region Abstract Members Implementation
    public override void UpdateTileObjectVisual(TileObject obj) { }
    #endregion

    #region Monobehaviours
    private void Awake() {
        visionTrigger = transform.GetComponentInChildren<TileObjectVisionTrigger>();
        choices = new List<ITraitable>();
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
        _objsInRange = new HashSet<ITraitable>();
        _poisonCloud = obj as PoisonCloud;
    }
    public override void PlaceObjectAt(LocationGridTile tile) {
        base.PlaceObjectAt(tile);
        wasJustPlaced = true;

        if (_poisonCloud.size > 0) {
            SetSize(_poisonCloud.size);
        }
        
        GameDate dueDate = GameManager.Instance.Today().AddTicks(1);
        SchedulingManager.Instance.AddEntry(dueDate, () => wasJustPlaced = false, this);
        
        _cloudEffect.gameObject.SetActive(true);
        _cloudEffect.Play();
        MoveToRandomDirection();
        _expiryKey = SchedulingManager.Instance.AddEntry(_poisonCloud.expiryDate, Expire, this);
        Messenger.AddListener(Signals.TICK_ENDED, PerTick);
        Messenger.AddListener<bool>(UISignals.PAUSED, OnGamePaused);
        Messenger.AddListener<ITraitable, Trait>(TraitSignals.TRAITABLE_GAINED_TRAIT, OnTraitableGainedTrait);
        Messenger.AddListener<PROGRESSION_SPEED>(UISignals.PROGRESSION_SPEED_CHANGED, OnProgressionSpeedChanged);
        isSpawned = true;

        if (GameManager.Instance.isPaused || !GameManager.Instance.gameHasStarted) {
            _movement.Pause();
            StartCoroutine(PlayParticleCoroutineWhenGameIsPaused());
        } else {
            _movement.Play();
            _cloudEffect.Play();
            _explosionEffect.Play();
        }
    }
    public override void SetWorldPosition(Vector3 worldPosition) {
        base.SetWorldPosition(worldPosition);
        if (GameManager.Instance.gameHasStarted) {
            _movement?.Kill();
            _movement = null;
            MoveToRandomDirection();
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
    private List<ITraitable> choices;
    private void PerTick() {
        if (isSpawned == false) {
            return;
        }
#if DEBUG_PROFILER
        Profiler.BeginSample($"Poison Cloud Per Tick");
#endif
        choices.Clear();
        int size = _size / 2;
        List<LocationGridTile> tiles = RuinarchListPool<LocationGridTile>.Claim();
        gridTileLocation.PopulateTilesInRadius(tiles, size, includeCenterTile: true, includeTilesInDifferentStructure: true);
        for (int i = 0; i < tiles.Count; i++) {
            LocationGridTile tile = tiles[i];
            tile.PerformActionOnTraitables(traitable => choices.Add(traitable));
        }
        RuinarchListPool<LocationGridTile>.Release(tiles);
        Assert.IsTrue(choices.Count > 0);
#if DEBUG_LOG
        string summary = $"{GameManager.Instance.TodayLogString()}Per tick check of poison cloud.";
#endif
        ITraitable chosenTraitable = UtilityScripts.CollectionUtilities.GetRandomElement(choices);
        chosenTraitable.traitContainer.AddTrait(chosenTraitable, "Poisoned");
        Poisoned poisoned = chosenTraitable.traitContainer.GetTraitOrStatus<Poisoned>("Poisoned");
        poisoned?.SetIsPlayerSource(_poisonCloud.isPlayerSource);

#if DEBUG_LOG
        summary = $"{summary}\nChance met! Target is {chosenTraitable.ToString()}";
        Debug.Log(summary);
#endif
#if DEBUG_PROFILER
        Profiler.EndSample();
#endif
    }
    public void Explode() {
#if DEBUG_LOG
        Debug.Log($"{GameManager.Instance.TodayLogString()}{this.name} has exploded!");
#endif
        _cloudEffect.TriggerSubEmitter(0);
        _poisonCloud.SetDoExpireEffect(false);
        Expire();
        float piercing = 0f;
        if (_poisonCloud.isPlayerSource) {
            piercing = PlayerSkillManager.Instance.GetAdditionalPiercePerLevelBaseOnLevel(PLAYER_SKILL_TYPE.POISON_CLOUD);
        }
        List<LocationGridTile> affectedTiles = RuinarchListPool<LocationGridTile>.Claim();
        gridTileLocation.PopulateTilesInRadius(affectedTiles, _size, includeCenterTile: true, includeTilesInDifferentStructure: true);
        for (int i = 0; i < affectedTiles.Count; i++) {
            LocationGridTile tile = affectedTiles[i];
            tile.PerformActionOnTraitables((t) => ApplyExplosionEffect(t, piercing));
        }
        RuinarchListPool<LocationGridTile>.Release(affectedTiles);
    }
    private void ApplyExplosionEffect(ITraitable traitable, float piercing) {
        traitable.AdjustHP(-350, ELEMENTAL_TYPE.Fire, true, showHPBar: true, piercingPower: piercing, isPlayerSource : _poisonCloud.isPlayerSource);
        //if (traitable.currentHP > 0 || traitable is GenericTileObject) {
        //    traitable.traitContainer.AddTrait(traitable, "Poisoned");
        //    Poisoned poisoned = traitable.traitContainer.GetTraitOrStatus<Poisoned>("Poisoned");
        //    poisoned?.SetIsPlayerSource(_poisonCloud.isPlayerSource);
        //}
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
            if(collidedWith.damageable is PoisonCloud otherPoisonCloud && otherPoisonCloud != _poisonCloud) {
                if (wasJustPlaced == false) {
                    CollidedWithPoisonCloud(otherPoisonCloud);    
                }
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
    private void CollidedWithPoisonCloud(PoisonCloud otherPoisonCloud) {
        if(!otherPoisonCloud.hasExpired) {
            if (_poisonCloud.size != _poisonCloud.maxSize) {
                int stacksToCombine = otherPoisonCloud.stacks;
                otherPoisonCloud.mapVisual.transform.DOKill();
                otherPoisonCloud.mapVisual.transform.DOMove(transform.position, 4f);
                otherPoisonCloud.SetDoExpireEffect(false);
                otherPoisonCloud.Neutralize();
                _poisonCloud.SetStacks(_poisonCloud.stacks + stacksToCombine);
            }
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
        if (obj is Character character && character.combatComponent.isInCombat == false && character.isNormalCharacter) {
            //normal characters that are not in combat, but are in range of a poison cloud should flee
            character.combatComponent.Flight(_poisonCloud, "inside a poison cloud");
        }
        if (obj.traitContainer.GetTraitOrStatus<Trait>("Burning") != null) {
            Explode();
        }
    }
#endregion
    
#region Expiration
    public void Expire() {
        if (!isSpawned) {
            //If already despawned, do not expire anymore
            return;
        }
#if DEBUG_LOG
        Debug.Log($"{this.name} expired!");
#endif
        _poisonCloud.OnExpire();
        _cloudEffect.Stop();
        visionTrigger.SetAllCollidersState(false);
        isSpawned = false;
        if (string.IsNullOrEmpty(_expiryKey) == false) {
            SchedulingManager.Instance.RemoveSpecificEntry(_expiryKey);
        }
        Messenger.RemoveListener(Signals.TICK_ENDED, PerTick);
        Messenger.RemoveListener<bool>(UISignals.PAUSED, OnGamePaused);
        Messenger.RemoveListener<ITraitable, Trait>(TraitSignals.TRAITABLE_GAINED_TRAIT, OnTraitableGainedTrait);
        Messenger.RemoveListener<PROGRESSION_SPEED>(UISignals.PROGRESSION_SPEED_CHANGED, OnProgressionSpeedChanged);
        _poisonCloud.Expire();
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
        Vector3 targetSize = new Vector3(_size, _size, _size);
        transform.DOScale(targetSize, 1f);
        _cloudEffect.transform.DOScale(targetSize, 1f);
        // gameObject.transform.localScale = new Vector3(_size, _size, _size);
        // _cloudEffect.transform.localScale = new Vector3(_size, _size, _size);
    }
#endregion
}
