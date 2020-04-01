using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Inner_Maps;
using Traits;
using UnityEngine;
using Random = UnityEngine.Random;

public class FireBallMapObjectVisual : MovingMapObjectVisual<TileObject> {
    
    [SerializeField] private ParticleSystem _coreEffect;
    [SerializeField] private ParticleSystem _flareEffect;

    private string _expiryKey;
    private Tweener _movement;
    private List<ITraitable> _objsInRange;
    private FireBallTileObject owner;
    
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
        owner = obj as FireBallTileObject;
    }
    public override void PlaceObjectAt(LocationGridTile tile) {
        base.PlaceObjectAt(tile);
        MoveToRandomDirection();
        //OnGamePaused(GameManager.Instance.isPaused);
        _expiryKey = SchedulingManager.Instance.AddEntry(GameManager.Instance.Today().AddTicks(GameManager.Instance.GetTicksBasedOnHour(2)), Expire, this);
        Messenger.AddListener(Signals.TICK_ENDED, PerTick);
        Messenger.AddListener<bool>(Signals.PAUSED, OnGamePaused);
        Messenger.AddListener<PROGRESSION_SPEED>(Signals.PROGRESSION_SPEED_CHANGED, OnProgressionSpeedChanged);
        isSpawned = true;

        if (GameManager.Instance.isPaused) {
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
        Vector3 direction = (new Vector3(Random.Range(-1.0f, 1.0f), Random.Range(-1.0f, 1.0f), 0f)).normalized * 50f;
        direction += transform.position;
        _movement = transform.DOMove(direction, 0.3f).SetSpeedBased(true);
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
    #endregion

    #region Effects
    private void PerTick() {
        if (isSpawned == false) {
            return;
        }
        BurningSource bs = null;
        for (int i = 0; i < _objsInRange.Count; i++) {
            ITraitable traitable = _objsInRange[i];
            traitable.AdjustHP(-10, ELEMENTAL_TYPE.Fire, true, showHPBar: true);
            Burning burningTrait = traitable.traitContainer.GetNormalTrait<Burning>("Burning");
            if (burningTrait != null && burningTrait.sourceOfBurning == null) {
                if (bs == null) {
                    bs = new BurningSource(traitable.gridTileLocation.parentMap.region);
                }
                burningTrait.SetSourceOfBurning(bs, traitable);
            }
        }
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
        _coreEffect.Stop();
        _flareEffect.Stop();
        isSpawned = false;
        if (string.IsNullOrEmpty(_expiryKey) == false) {
            SchedulingManager.Instance.RemoveSpecificEntry(_expiryKey);
        }
        Messenger.RemoveListener(Signals.TICK_ENDED, PerTick);
        Messenger.RemoveListener<bool>(Signals.PAUSED, OnGamePaused);
        Messenger.RemoveListener<PROGRESSION_SPEED>(Signals.PROGRESSION_SPEED_CHANGED, OnProgressionSpeedChanged);
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
