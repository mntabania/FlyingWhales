using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Inner_Maps;
using Traits;
using UnityEngine;
using UnityEngine.Profiling;
using Random = UnityEngine.Random;

public class BallLightningMapObjectVisual : MovingMapObjectVisual<TileObject> {
    
    [SerializeField] private ParticleSystem _ballLightningEffect;
    
    private string _expiryKey;
    private Tweener _movement;
    private List<ITraitable> _objsInRange;
    private BallLightning owner;
    
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
        owner = obj as BallLightning;
    }
    public override void PlaceObjectAt(LocationGridTile tile) {
        base.PlaceObjectAt(tile);
        MoveToRandomDirection();
        _expiryKey = SchedulingManager.Instance.AddEntry(owner.expiryDate, Expire, this);
        Messenger.AddListener(Signals.TICK_ENDED, PerTick);
        Messenger.AddListener<bool>(UISignals.PAUSED, OnGamePaused);
        Messenger.AddListener<PROGRESSION_SPEED>(UISignals.PROGRESSION_SPEED_CHANGED, OnProgressionSpeedChanged);
        isSpawned = true;

        if (GameManager.Instance.isPaused) {
            _movement.Pause();
            StartCoroutine(PlayParticleCoroutineWhenGameIsPaused());
        } else {
            _movement.Play();
            _ballLightningEffect.Play();
        }
    }
    public override void Reset() {
        base.Reset();
        _expiryKey = string.Empty;
        _movement?.Kill();
        _movement = null;
        _objsInRange = null;
        _ballLightningEffect.Clear();
    }
    #endregion

    #region Movement
    private void MoveToRandomDirection() {
        float baseSpeed = PlayerSkillManager.Instance.GetSkillMovementSpeedPerLevel(PLAYER_SKILL_TYPE.BALL_LIGHTNING) / 100f;
        Vector3 direction = (new Vector3(Random.Range(-1.0f, 1.0f), Random.Range(-1.0f, 1.0f), 0f)).normalized * 50f;
        direction += transform.position;
        _movement = transform.DOMove(direction, baseSpeed).SetSpeedBased(true);
    }
    private void OnGamePaused(bool isPaused) {
        if (isPaused) {
            _movement.Pause();
            _ballLightningEffect.Pause();
        } else {
            _movement.Play();
            _ballLightningEffect.Play();
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

    #region Effects
    private void PerTick() {
        if (isSpawned == false) {
            return;
        }
        Profiler.BeginSample($"Ball Lightning Per Tick");
        int processedDamage = -PlayerSkillManager.Instance.GetDamageBaseOnLevel(PLAYER_SKILL_TYPE.BALL_LIGHTNING);
        for (int i = 0; i < _objsInRange.Count; i++) {
            ITraitable traitable = _objsInRange[i];
            if (owner != traitable) {
                traitable.AdjustHP(processedDamage, ELEMENTAL_TYPE.Electric, true, showHPBar: true, isPlayerSource: owner.isPlayerSource);
            }
            if (traitable is Character character) {
                Messenger.Broadcast(PlayerSignals.PLAYER_HIT_CHARACTER_VIA_SPELL, character, processedDamage);
                if (character != null && character.isDead && character.skillCauseOfDeath == PLAYER_SKILL_TYPE.NONE) {
                    character.skillCauseOfDeath = PLAYER_SKILL_TYPE.BALL_LIGHTNING;
                    //Messenger.Broadcast(PlayerSignals.CREATE_SPIRIT_ENERGY, character.deathTilePosition.centeredWorldLocation, 1, character.deathTilePosition.parentMap);
                    //Messenger.Broadcast(PlayerSignals.CREATE_CHAOS_ORBS, character.deathTilePosition.centeredWorldLocation, 1, character.deathTilePosition.parentMap);
                }
            }
        }
        Profiler.EndSample();
    }
    #endregion
    
    #region Triggers
    public void OnTriggerEnter2D(Collider2D collision) {
        if (isSpawned == false) { return; }
        BaseVisionTrigger collidedWith = collision.gameObject.GetComponent<BaseVisionTrigger>();
        if (collidedWith != null && collidedWith.damageable is ITraitable traitable) { 
            AddObject(traitable);   
        }
    }
    public void OnTriggerExit2D(Collider2D collision) {
        if (isSpawned == false) { return; }
        BaseVisionTrigger collidedWith = collision.gameObject.GetComponent<BaseVisionTrigger>();
        if (collidedWith != null && collidedWith.damageable is ITraitable traitable) { 
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
        Debug.Log($"{this.name} expired!");
        _ballLightningEffect.Stop();
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
        _ballLightningEffect.Play();
        yield return new WaitForSeconds(0.1f);
        _ballLightningEffect.Pause();
    }
    #endregion
}
