using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Inner_Maps;
using Traits;
using UnityEngine;
using Random = UnityEngine.Random;

public class VaporMapObjectVisual : MovingMapObjectVisual<TileObject> {
    
    [SerializeField] private ParticleSystem _vaporEffect;
    
    private string _expiryKey;
    private Tweener _movement;
    private int _size;
    
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
    protected override void Update() {
        base.Update();
        if (isSpawned && gridTileLocation == null) {
            Expire();
        }
    }
    #endregion

    #region Overrides
    public override void PlaceObjectAt(LocationGridTile tile) {
        base.PlaceObjectAt(tile);
        MoveToRandomDirection();
        _expiryKey = SchedulingManager.Instance.AddEntry(GameManager.Instance.Today().AddTicks(GameManager.Instance.GetTicksBasedOnHour(2)), Expire, this);
        Messenger.AddListener<bool>(Signals.PAUSED, OnGamePaused);
        Messenger.AddListener<PROGRESSION_SPEED>(Signals.PROGRESSION_SPEED_CHANGED, OnProgressionSpeedChanged);
        isSpawned = true;

        if (GameManager.Instance.isPaused) {
            _movement.Pause();
            StartCoroutine(PlayParticleCoroutineWhenGameIsPaused());
        } else {
            _movement.Play();
            _vaporEffect.Play();
        }
    }
    public override void Reset() {
        base.Reset();
        _expiryKey = string.Empty;
        _movement?.Kill();
        _movement = null;
        _vaporEffect.Clear();
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
            _vaporEffect.Pause();
        } else {
            _movement.Play();
            _vaporEffect.Play();
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

    #region Utilities
    public void SetSize(int size) {
        _size = size;
        ChangeScaleBySize();
    }
    private void ChangeScaleBySize() {
        //int scaleSize = 1;
        //if(_size > 1) {
        //    scaleSize = _size + 1;
        //}
        this.gameObject.transform.localScale = new Vector3(_size, _size, 1f);
        _vaporEffect.transform.localScale = new Vector3(_size, _size, _size);
    }
    private void Effect() {
        if (gridTileLocation != null) {
            int radius = 0;
            if (UtilityScripts.Utilities.IsEven(_size)) {
                //-3 because (-1 + -2), wherein -1 is the lower odd number, and -2 is the radius
                //So if size is 4, that means that 4-3 is 1, the radius for the 4x4 is 1 meaning we will get the neighbours of the tile.
                radius = _size - 3;
            } else {
                //-2 because we will not need to get the lower odd number since this is already an odd number, just get the radius, hence, -2
                radius = _size - 2;
            }
            //If the radius is less than or equal to zero this means we will only get the gridTileLocation itself
            if (radius <= 0) {
                gridTileLocation.genericTileObject.traitContainer.AddTrait(gridTileLocation.genericTileObject, "Wet");
            } else {
                List<LocationGridTile> tiles = gridTileLocation.GetTilesInRadius(radius, includeCenterTile: true, includeTilesInDifferentStructure: true);
                for (int i = 0; i < tiles.Count; i++) {
                    tiles[i].genericTileObject.traitContainer.AddTrait(tiles[i].genericTileObject, "Wet");
                }
            }
        }
    }
    #endregion
    
    #region Expiration
    public void Expire() {
        Debug.Log($"{this.name} expired!");
        Effect();
        _vaporEffect.Stop();
        isSpawned = false;
        if (string.IsNullOrEmpty(_expiryKey) == false) {
            SchedulingManager.Instance.RemoveSpecificEntry(_expiryKey);
        }
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
        _vaporEffect.Play();
        yield return new WaitForSeconds(0.1f);
        _vaporEffect.Pause();
    }
    #endregion
}
