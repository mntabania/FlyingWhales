using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Inner_Maps;
using UnityEngine;
using Random = UnityEngine.Random;

public sealed class TornadoMapObjectVisual : MovingMapObjectVisual<TileObject> {

    [Header("Particles")]
    [SerializeField] private ParticleSystem[] particles;
    
    [Header("Movement")]
    [SerializeField] private float baseSpeed = 1.0F; // Movement speed in units per second.
    private float _startTime;  // Time when the movement started.
    private float _journeyLength; // Total distance between the markers.
    private Vector3 _startPosition;
    private float _speed;
    private int _radius;
    private List<IDamageable> _damagablesInTornado;
    private TornadoTileObject _tornado;
    private string _expiryKey;

    #region getters/setters
    private LocationGridTile destinationTile { get; set; }
    #endregion

    private void Awake() {
        collisionTrigger = transform.GetComponentInChildren<TileObjectCollisionTrigger>();
    }
    public override void Initialize(TileObject tileObject) {
        base.Initialize(tileObject);
        transform.localPosition = tileObject.gridTileLocation.centeredLocalLocation;
        selectable = tileObject;
        _tornado = tileObject as TornadoTileObject;
        _radius = _tornado.radius;
        //PlayTornadoParticle();
        _damagablesInTornado = new List<IDamageable>();
    }

    #region Particles
    private IEnumerator PlayParticleCoroutineWhenGameIsPaused() {
        //Playing particle effect is done in a coroutine so that it will wait one frame before pausing the particles if the game is paused when the particle is activated
        //This will make sure that the particle effect will show but it will be paused right away
        PlayTornadoParticle();
        yield return null;
        PauseTornadoParticle();
    }
    private void PlayTornadoParticle() {
        for (int i = 0; i < particles.Length; i++) {
            ParticleSystem p = particles[i];
            p.Play();
        }
    }
    private void PauseTornadoParticle() {
        for (int i = 0; i < particles.Length; i++) {
            ParticleSystem p = particles[i];
            p.Pause();
        }
    }
    private void StopTornadoParticle() {
        for (int i = 0; i < particles.Length; i++) {
            ParticleSystem p = particles[i];
            p.Stop();
        }
    }
    private void ClearTornadoParticle() {
        for (int i = 0; i < particles.Length; i++) {
            ParticleSystem p = particles[i];
            p.Clear();
        }
    }
    #endregion

    private void GoToRandomTileInRadius() {
        List<LocationGridTile> tilesInRadius = gridTileLocation.GetTilesInRadius(8, 6, false, true);
        LocationGridTile chosen = tilesInRadius[Random.Range(0, tilesInRadius.Count)];
        GoTo(chosen);
    }

    public override void PlaceObjectAt(LocationGridTile tile) {
        base.PlaceObjectAt(tile);
        // Vector3 worldPos = tile.centeredWorldLocation;
        // var thisTransform = transform;
        // thisTransform.SetParent(tile.parentMap.objectsParent);
        // thisTransform.position = worldPos;

        GoToRandomTileInRadius();
        _expiryKey = SchedulingManager.Instance.AddEntry(GameManager.Instance.Today().AddTicks(_tornado.durationInTicks), Expire, this);
        Messenger.AddListener(Signals.TICK_ENDED, PerTick);
        Messenger.AddListener<PROGRESSION_SPEED>(Signals.PROGRESSION_SPEED_CHANGED, OnProgressionSpeedChanged);
        Messenger.AddListener<bool>(Signals.PAUSED, OnGamePaused);
        Messenger.AddListener<TileObject, Character, LocationGridTile>(Signals.TILE_OBJECT_REMOVED, OnTileObjectRemovedFromTile);
        // Messenger.AddListener<SpecialToken, LocationGridTile>(Signals.ITEM_REMOVED_FROM_TILE, OnItemRemovedFromTile);
        isSpawned = true;

        if (GameManager.Instance.isPaused) {
            StartCoroutine(PlayParticleCoroutineWhenGameIsPaused());
        } else {
            PlayTornadoParticle();
        }
    }

    #region Pathfinding
    private void GoTo(LocationGridTile destinationTile) {
        this.destinationTile = destinationTile;
        UpdateSpeed();
        RecalculatePathingValues();
    }
    private void RecalculatePathingValues() {
        // Keep a note of the time the movement started.
        _startTime = Time.time;
        
        var position = transform.position;
        _startPosition = position;
        // Calculate the journey length.
        _journeyLength = Vector3.Distance(position, destinationTile.centeredWorldLocation);
    }
    private void UpdateSpeed() {
        _speed = baseSpeed;
        if (GameManager.Instance.currProgressionSpeed == PROGRESSION_SPEED.X2) {
            _speed *= 1.5f;
        } else if (GameManager.Instance.currProgressionSpeed == PROGRESSION_SPEED.X4) {
            _speed *= 2f;
        }
    }
    private void OnProgressionSpeedChanged(PROGRESSION_SPEED prog) {
        UpdateSpeed();
        RecalculatePathingValues();
    }
    private void OnGamePaused(bool paused) {
        if (paused) {
            PauseTornadoParticle();
        } else {
            PlayTornadoParticle();
        }
        UpdateSpeed();
        RecalculatePathingValues();
    }
    #endregion

    public void Expire() {
        StopTornadoParticle();
        SchedulingManager.Instance.RemoveSpecificEntry(_expiryKey);
        GameManager.Instance.StartCoroutine(ExpireCoroutine());
    }
    private IEnumerator ExpireCoroutine() {
        yield return new WaitForSeconds(1f);
        ObjectPoolManager.Instance.DestroyObject(this);
        _tornado.OnExpire();
    }

    #region Object Pooling
    public override void Reset() {
        base.Reset();
        isSpawned = false;
        destinationTile = null;
        _journeyLength = 0f;
        _startPosition = Vector3.zero;
        _startTime = 0f;
        ClearTornadoParticle();
        Messenger.RemoveListener(Signals.TICK_ENDED, PerTick);
        Messenger.RemoveListener<PROGRESSION_SPEED>(Signals.PROGRESSION_SPEED_CHANGED, OnProgressionSpeedChanged);
        Messenger.RemoveListener<bool>(Signals.PAUSED, OnGamePaused);
        Messenger.RemoveListener<TileObject, Character, LocationGridTile>(Signals.TILE_OBJECT_REMOVED, OnTileObjectRemovedFromTile);
    }
    #endregion
    
    #region Monobehaviours
    protected override void Update() {
        base.Update();
        if (destinationTile == null) {
            return;
        }
        if (gameObject.activeSelf == false) {
            return;
        }
        if (GameManager.Instance.isPaused) {
            RecalculatePathingValues();
            return;
        }
        // Distance moved equals elapsed time times speed..
        float distCovered = (Time.time - _startTime) * _speed;

        // Fraction of journey completed equals current distance divided by total distance.
        float fractionOfJourney = distCovered / _journeyLength;

        // Set our position as a fraction of the distance between the markers.
        transform.position = Vector3.Lerp(_startPosition, destinationTile.centeredWorldLocation, fractionOfJourney);
        if (Mathf.Approximately(transform.position.x, destinationTile.centeredWorldLocation.x) 
            && Mathf.Approximately(transform.position.y, destinationTile.centeredWorldLocation.y)) {
            destinationTile = null;
            GoToRandomTileInRadius();
        }
    }
    #endregion

    #region Triggers
    public void OnTriggerEnter2D(Collider2D collision) {
        //if(collision.tag == "Spell") { return; }
        IBaseCollider collidedWith = collision.gameObject.GetComponent<IBaseCollider>();
        if (collidedWith != null) {
            if (collidedWith.damageable == null) {
                throw new System.Exception($"Tornado collided with {collidedWith} but damagable was null!");
            }
            // Debug.Log($"Tornado collision enter with {collidedWith.damageable.name}");
            AddDamageable(collidedWith.damageable);
        }
    }
    public void OnTriggerExit2D(Collider2D collision) {
        //if (collision.tag == "Spell") { return; }
        IBaseCollider collidedWith = collision.gameObject.GetComponent<IBaseCollider>();
        if (collidedWith != null) {
            // Debug.Log($"Tornado collision exit with {collidedWith.damageable.name}");
            RemoveDamageable(collidedWith.damageable);
        }
    }
    #endregion

    #region POI's
    private void AddDamageable(IDamageable poi) {
        if (!_damagablesInTornado.Contains(poi)) {
            _damagablesInTornado.Add(poi);
            OnAddPoiActions(poi);
        }
    }
    private void RemoveDamageable(IDamageable poi) {
        _damagablesInTornado.Remove(poi);
        DOTween.Kill(this);
    }
    private void OnAddPoiActions(IDamageable poi) {
        //DealDamage(poi);
        poi.mapObjectVisual.transform.DOShakeRotation(20f, new Vector3(0f, 0f, 5f));
    }
    #endregion

    private void PerTick() {
        if (isSpawned == false) {
            return;
        }
        if (gameObject.activeSelf == false) {
            return;
        }
        List<LocationGridTile> tiles = gridTileLocation.GetTilesInRadius(_radius, includeCenterTile: true, includeTilesInDifferentStructure: true);
        for (int i = 0; i < tiles.Count; i++) {
            LocationGridTile tile = tiles[i];
            if (tile.genericTileObject.CanBeDamaged()) {
                tile.genericTileObject.AdjustHP(-(int)(tile.genericTileObject.maxHP * 0.35f), ELEMENTAL_TYPE.Normal, true, this);
            }
        }
        for (int i = 0; i < _damagablesInTornado.Count; i++) {
            IDamageable damageable = _damagablesInTornado[i];
            if (damageable.mapObjectVisual != null) {
                Vector3 distance = transform.position - damageable.mapObjectVisual.gameObjectVisual.transform.position;
                if (distance.magnitude < 3f) {
                    DealDamage(damageable);
                } else {
                    //check for suck in
                    TrySuckIn(damageable);
                }
            }
        }
    }
    private void DealDamage(IDamageable damageable) {
        if (damageable.CanBeDamaged()) {
            //0.35f
            damageable.AdjustHP(-(int)(damageable.maxHP * 0.55f), ELEMENTAL_TYPE.Normal, true, _tornado);
        }
    }
    private void TrySuckIn(IDamageable damageable) {
        if (CanBeSuckedIn(damageable) && Random.Range(0, 100) < 35) {
            damageable.mapObjectVisual.TweenTo(transform, 0.5f, () => OnDamagableReachedThis(damageable));
            if (damageable is IPointOfInterest poi) {
                poi.SetPOIState(POI_STATE.INACTIVE);
            }
        }
    }
    private void OnDamagableReachedThis(IDamageable damageable) {
        damageable.mapObjectVisual?.OnReachTarget();
        damageable.AdjustHP(-damageable.maxHP, ELEMENTAL_TYPE.Normal, true, _tornado);
    }
    private bool CanBeSuckedIn(IDamageable damageable) {
        return damageable.CanBeDamaged() && (damageable is GenericTileObject) == false 
            && (damageable is Character) == false && damageable.mapObjectVisual.IsTweening() == false;
    }

    #region Abstract Member Implementation
    public override void UpdateTileObjectVisual(TileObject obj) { }
    public override void ApplyFurnitureSettings(FurnitureSetting furnitureSetting) { }
    public override bool IsMapObjectMenuVisible() {
        return true;
    }
    public override void UpdateCollidersState(TileObject obj) { }
    #endregion

    #region Listeners
    private void OnTileObjectRemovedFromTile(TileObject tileObject, Character removedBy, LocationGridTile removedFrom) {
       RemoveDamageable(tileObject);
    }
    #endregion
}
