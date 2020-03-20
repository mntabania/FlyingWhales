using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Inner_Maps;
using Traits;
using UnityEngine;
using Random = UnityEngine.Random;

public class QuicksandMapObjectVisual : MapObjectVisual<TileObject> {
    
    [SerializeField] private ParticleSystem _quicksandEffect;

    private string _expiryKey;
    private Tweener _movement;
    private List<Character> _charactersInRange;
    private int _currentTick;
    
    
    #region Abstract Members Implementation
    public override void ApplyFurnitureSettings(FurnitureSetting furnitureSetting) { }
    public virtual bool IsMapObjectMenuVisible() {
        return true;
    }
    public override void UpdateTileObjectVisual(TileObject obj) { }
    public override void UpdateCollidersState(TileObject obj) { }
    #endregion

    #region Monobehaviours
    private void Awake() {
        collisionTrigger = transform.GetComponentInChildren<TileObjectCollisionTrigger>();
    }
    #endregion

    #region Overrides
    public override void Initialize(TileObject obj) {
        base.Initialize(obj);
        _charactersInRange = new List<Character>();
        _currentTick = 0;
    }
    public override void PlaceObjectAt(LocationGridTile tile) {
        base.PlaceObjectAt(tile);
        _expiryKey = SchedulingManager.Instance.AddEntry(GameManager.Instance.Today().AddTicks(GameManager.Instance.GetTicksBasedOnHour(3)), Expire, this);
        Messenger.AddListener(Signals.TICK_ENDED, PerTick);
        Messenger.AddListener<bool>(Signals.PAUSED, OnGamePaused);
        Messenger.AddListener<PROGRESSION_SPEED>(Signals.PROGRESSION_SPEED_CHANGED, OnProgressionSpeedChanged);

        OnSpawnQuicksand();

        if (GameManager.Instance.isPaused) {
            _movement.Pause();
            StartCoroutine(PlayParticleCoroutineWhenGameIsPaused());
        } else {
            _movement.Play();
            _quicksandEffect.Play();
        }
    }
    public override void Reset() {
        base.Reset();
        _expiryKey = string.Empty;
        _movement?.Kill();
        _movement = null;
        _charactersInRange = null;
        _currentTick = 0;
        _quicksandEffect.Clear();
    }
    #endregion

    #region Game Signals
    private void OnGamePaused(bool isPaused) {
        if (isPaused) {
            _movement.Pause();
            _quicksandEffect.Pause();
        } else {
            _movement.Play();
            _quicksandEffect.Play();
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
    private void OnSpawnQuicksand() {
        //yield return new WaitForSeconds(0.1f);
        PerFifteenMinutes();
    }
    private void PerTick() {
        _currentTick++;
        if(_currentTick >= 3) {
            _currentTick = 0;
            PerFifteenMinutes();
        }
    }
    private void PerFifteenMinutes() {
        BurningSource bs = null;
        for (int i = 0; i < _charactersInRange.Count; i++) {
            Character character = _charactersInRange[i];
            character.AdjustHP(-10, ELEMENTAL_TYPE.Earth, true, showHPBar: true);
            if (!character.isDead) {
                if(UnityEngine.Random.Range(0, 100) < 35) {
                    character.traitContainer.AddTrait(character, "Disoriented");
                }
            }
        }
    }
    #endregion
    
    #region Triggers
    public void OnTriggerEnter2D(Collider2D collision) {
        IBaseCollider collidedWith = collision.gameObject.GetComponent<IBaseCollider>();
        if (collidedWith != null && collidedWith.damageable is Character character) { 
            AddCharacter(character);   
        }
    }
    public void OnTriggerExit2D(Collider2D collision) {
        IBaseCollider collidedWith = collision.gameObject.GetComponent<IBaseCollider>();
        if (collidedWith != null && collidedWith.damageable is Character character) { 
            RemoveCharacter(character);   
        }
    }
    #endregion
    
    #region POI's
    private void AddCharacter(Character character) {
        if (!_charactersInRange.Contains(character)) {
            _charactersInRange.Add(character);
        }
    }
    private void RemoveCharacter(Character character) {
        _charactersInRange.Remove(character);
    }
    #endregion
    
    #region Expiration
    public void Expire() {
        Debug.Log($"{this.name} expired!");
        _quicksandEffect.Stop();
        if (string.IsNullOrEmpty(_expiryKey) == false) {
            SchedulingManager.Instance.RemoveSpecificEntry(_expiryKey);
        }
        Messenger.RemoveListener(Signals.TICK_ENDED, PerFifteenMinutes);
        Messenger.RemoveListener<bool>(Signals.PAUSED, OnGamePaused);
        Messenger.RemoveListener<PROGRESSION_SPEED>(Signals.PROGRESSION_SPEED_CHANGED, OnProgressionSpeedChanged);
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
        _quicksandEffect.Play();
        yield return new WaitForSeconds(0.1f);
        _quicksandEffect.Pause();
    }
    #endregion
}
