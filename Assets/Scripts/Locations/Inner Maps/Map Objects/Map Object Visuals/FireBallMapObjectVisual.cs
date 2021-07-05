using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Inner_Maps;
using Traits;
using UnityEngine;
using UnityEngine.Profiling;
using Random = UnityEngine.Random;

public class FireBallMapObjectVisual : MovingMapObjectVisual {
    
    [SerializeField] private ParticleSystem _coreEffect;
    [SerializeField] private ParticleSystem _flareEffect;

    private string _expiryKey;
    private Tweener _movement;
    private List<ITraitable> _objsInRange;
    private FireBall owner;
    
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
        _objsInRange = new List<ITraitable>();
        owner = obj as FireBall;
    }
    public override void PlaceObjectAt(LocationGridTile tile) {
        base.PlaceObjectAt(tile);
        MoveToRandomDirection();
        _expiryKey = SchedulingManager.Instance.AddEntry(owner.expiryDate, Expire, this);
        Messenger.AddListener(Signals.TICK_ENDED, PerTick);
        Messenger.AddListener<bool>(UISignals.PAUSED, OnGamePaused);
        Messenger.AddListener<PROGRESSION_SPEED>(UISignals.PROGRESSION_SPEED_CHANGED, OnProgressionSpeedChanged);
        isSpawned = true;

        if (GameManager.Instance.isPaused || !GameManager.Instance.gameHasStarted) {
            _movement.Pause();
            StartCoroutine(PlayParticleCoroutineWhenGameIsPaused());
        } else {
            _movement.Play();
            _coreEffect.Play();
            _flareEffect.Play();
        }
    }
    public override void Reset() {
        base.Reset();
        _expiryKey = string.Empty;
        _movement?.Kill();
        _movement = null;
        _objsInRange = null;
        _coreEffect.Clear();
        _flareEffect.Clear();
    }
    #endregion

    #region Movement
    private void MoveToRandomDirection() {
        float baseSpeed = PlayerSkillManager.Instance.GetSkillMovementSpeedPerLevel(PLAYER_SKILL_TYPE.FIRE_BALL) / 100f;
        Vector3 direction = (new Vector3(Random.Range(-1.0f, 1.0f), Random.Range(-1.0f, 1.0f), 0f)).normalized * 50f;
        direction += transform.position;
        _movement = transform.DOMove(direction, baseSpeed).SetSpeedBased(true);
    }
    private void OnGamePaused(bool isPaused) {
        if (isPaused) {
            _movement.Pause();
            _coreEffect.Pause();
            _flareEffect.Pause();
        } else {
            _movement.Play();
            _coreEffect.Play();
            _flareEffect.Play();
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
        if (GameManager.Instance.gameHasStarted) {
            _movement?.Kill();
            _movement = null;
            MoveToRandomDirection();
        }
    }
    #endregion

    #region Effects
    private void PerTick() {
        if (isSpawned == false) {
            return;
        }
#if DEBUG_PROFILER
        Profiler.BeginSample($"Poison Cloud Per Tick");
#endif
        SkillData fireBallData = PlayerSkillManager.Instance.GetSpellData(PLAYER_SKILL_TYPE.FIRE_BALL);
        int processedDamage = -PlayerSkillManager.Instance.GetDamageBaseOnLevel(fireBallData);
        float piercing = PlayerSkillManager.Instance.GetAdditionalPiercePerLevelBaseOnLevel(fireBallData);
        BurningSource bs = null;
        for (int i = 0; i < _objsInRange.Count; i++) {
            ITraitable traitable = _objsInRange[i];
            if (owner != traitable) {
                traitable.AdjustHP(processedDamage, ELEMENTAL_TYPE.Fire, true, showHPBar: true, piercingPower: piercing, isPlayerSource: owner.isPlayerSource, source: owner.isPlayerSource ? fireBallData : null);
            }
            Burning burningTrait = traitable.traitContainer.GetTraitOrStatus<Burning>("Burning");
            if (burningTrait != null && burningTrait.sourceOfBurning == null) {
                if (bs == null) {
                    bs = new BurningSource();
                }
                burningTrait.SetSourceOfBurning(bs, traitable);
            }

            if (traitable is Character character) {
                Messenger.Broadcast(PlayerSignals.PLAYER_HIT_CHARACTER_VIA_SPELL, character, processedDamage);
                if (character != null && character.isDead && character.skillCauseOfDeath == PLAYER_SKILL_TYPE.NONE) {
                    character.skillCauseOfDeath = PLAYER_SKILL_TYPE.FIRE_BALL;
                    //Messenger.Broadcast(PlayerSignals.CREATE_SPIRIT_ENERGY, character.deathTilePosition.centeredWorldLocation, 1, character.deathTilePosition.parentMap);
                    //Messenger.Broadcast(PlayerSignals.CREATE_CHAOS_ORBS, character.deathTilePosition.centeredWorldLocation, 1, character.deathTilePosition.parentMap);
                }
            }
        }
#if DEBUG_PROFILER
        Profiler.EndSample();
#endif
    }
#endregion
    
#region Triggers
    public void OnTriggerEnter2D(Collider2D collision) {
        if (isSpawned == false) { return; }
        BaseVisionTrigger collidedWith = collision.gameObject.GetComponent<BaseVisionTrigger>();
        if (collidedWith != null && collidedWith.damageable is ITraitable traitable && !(traitable is FireBall)) { 
            AddObject(traitable);   
        }
    }
    public void OnTriggerExit2D(Collider2D collision) {
        if (isSpawned == false) { return; }
        BaseVisionTrigger collidedWith = collision.gameObject.GetComponent<BaseVisionTrigger>();
        if (collidedWith != null && collidedWith.damageable is ITraitable traitable && !(traitable is FireBall)) { 
            RemoveObject(traitable);   
        }
    }
#endregion
    
#region POI's
    private void AddObject(ITraitable obj) {
        if (!_objsInRange.Contains(obj)) {
            _objsInRange.Add(obj);
        }
    }
    private void RemoveObject(ITraitable obj) {
        _objsInRange.Remove(obj);
    }
#endregion
    
#region Expiration
    public void Expire() {
#if DEBUG_LOG
        Debug.Log($"{this.name} expired!");
#endif
        _coreEffect.Stop();
        _flareEffect.Stop();
        isSpawned = false;
        if (string.IsNullOrEmpty(_expiryKey) == false) {
            SchedulingManager.Instance.RemoveSpecificEntry(_expiryKey);
        }
        Messenger.RemoveListener(Signals.TICK_ENDED, PerTick);
        Messenger.RemoveListener<bool>(UISignals.PAUSED, OnGamePaused);
        Messenger.RemoveListener<PROGRESSION_SPEED>(UISignals.PROGRESSION_SPEED_CHANGED, OnProgressionSpeedChanged);
        owner.Expire();
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
        _coreEffect.Play();
        _flareEffect.Play();
        yield return new WaitForSeconds(0.1f);
        _coreEffect.Pause();
        _flareEffect.Pause();
    }
#endregion
}
