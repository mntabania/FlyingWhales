using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Inner_Maps;
using Traits;
using UnityEngine;
using UnityEngine.Profiling;
using Random = UnityEngine.Random;

public class FrostyFogMapObjectVisual : MovingMapObjectVisual {
    
    [SerializeField] private ParticleSystem _frostyFogEffect;
    [SerializeField] private ParticleSystem _snowFlakesEffect;
    [SerializeField] private ParticleSystem _waveEffect;

    private string _expiryKey;
    private Tweener _movement;
    private List<ITraitable> _objsInRange;
    private FrostyFog owner;
    private int _size;

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
        owner = obj as FrostyFog;
    }
    public override void PlaceObjectAt(LocationGridTile tile) {
        base.PlaceObjectAt(tile);
        if (owner.size > 0) {
            SetSize(owner.size);
        }
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
            _frostyFogEffect.Play();
            _snowFlakesEffect.Play();
            _waveEffect.Play();
        }
    }
    public override void Reset() {
        base.Reset();
        _expiryKey = string.Empty;
        _movement?.Kill();
        _movement = null;
        _objsInRange = null;
        _frostyFogEffect.Clear();
        _snowFlakesEffect.Clear();
        _waveEffect.Clear();
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
            _frostyFogEffect.Pause();
            _snowFlakesEffect.Pause();
            _waveEffect.Pause();
        } else {
            _movement.Play();
            _frostyFogEffect.Play();
            _snowFlakesEffect.Play();
            _waveEffect.Play();
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
        Profiler.BeginSample($"Frosty Fog Per Tick");
#endif
        for (int i = 0; i < _objsInRange.Count; i++) {
            _objsInRange[i].traitContainer.AddTrait(_objsInRange[i], "Freezing");
            Freezing freezing = _objsInRange[i].traitContainer.GetTraitOrStatus<Freezing>("Freezing");
            freezing?.SetIsPlayerSource(owner.isPlayerSource);
        }
#if DEBUG_PROFILER
        Profiler.EndSample();
#endif
    }
#endregion

#region Size
    public void SetSize(int size) {
        _size = size;
        ChangeScaleBySize();
    }
    private void ChangeScaleBySize() {
        Vector3 targetSize = new Vector3(_size, _size, _size);
        transform.DOScale(new Vector3(_size, _size, 1f), 1f);
        _frostyFogEffect.transform.DOScale(targetSize, 1f);
        _snowFlakesEffect.transform.DOScale(targetSize, 1f);
        _waveEffect.transform.DOScale(targetSize, 1f);
        // this.gameObject.transform.localScale = new Vector3(_size, _size, 1f);
        // _frostyFogEffect.transform.localScale = new Vector3(_size, _size, _size);
        // _snowFlakesEffect.transform.localScale = new Vector3(_size, _size, _size);
        // _waveEffect.transform.localScale = new Vector3(_size, _size, _size);
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
#if DEBUG_LOG
        Debug.Log($"{this.name} expired!");
#endif
        _frostyFogEffect.Stop();
        _snowFlakesEffect.Stop();
        _waveEffect.Stop();
        visionTrigger.SetAllCollidersState(false);
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
        yield return new WaitForSeconds(4.5f);
        ObjectPoolManager.Instance.DestroyObject(this);
    }
#endregion

#region Particles
    private IEnumerator PlayParticleCoroutineWhenGameIsPaused() {
        //Playing particle effect is done in a coroutine so that it will wait one frame before pausing the particles if the game is paused when the particle is activated
        //This will make sure that the particle effect will show but it will be paused right away
        _frostyFogEffect.Play();
        _snowFlakesEffect.Play();
        _waveEffect.Play();
        yield return new WaitForSeconds(0.1f);
        _frostyFogEffect.Pause();
        _snowFlakesEffect.Pause();
        _waveEffect.Pause();
    }
#endregion
}
