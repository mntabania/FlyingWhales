using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Inner_Maps;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Profiling;
using UtilityScripts;
using Random = UnityEngine.Random;

public sealed class TornadoMapObjectVisual : MovingMapObjectVisual {

    [Header("Particles")]
    [SerializeField] private ParticleSystem[] particles;
    
    private float _startTime;  // Time when the movement started.
    private float _journeyLength; // Total distance between the markers.
    private Vector3 _startPosition;
    private float _speed;
    private int _radius;
    private List<IDamageable> _damagablesInTornado;
    private Tornado _tornado;
    private string _expiryKey;

    #region getters/setters
    private LocationGridTile destinationTile { get; set; }
    #endregion

    private void Awake() {
        visionTrigger = transform.GetComponentInChildren<TileObjectVisionTrigger>();
    }
    public override void Initialize(TileObject tileObject) {
        base.Initialize(tileObject);
        name = tileObject.ToString();
        transform.localPosition = tileObject.gridTileLocation.centeredLocalLocation;
        selectable = tileObject;
        _tornado = tileObject as Tornado;
        _radius = _tornado.radius;
        //PlayTornadoParticle();
        if(_damagablesInTornado == null) {
            _damagablesInTornado = new List<IDamageable>();
        } else {
            _damagablesInTornado.Clear();
        }
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
        List<LocationGridTile> tilesInRadius = RuinarchListPool<LocationGridTile>.Claim();
        gridTileLocation.PopulateTilesInRadius(tilesInRadius, 8, 6, false, true);
        LocationGridTile chosen = tilesInRadius[Random.Range(0, tilesInRadius.Count)];
        RuinarchListPool<LocationGridTile>.Release(tilesInRadius);
        Assert.IsNotNull(chosen, $"Tornado at {gridTileLocation} cannot find a tile to go to!");
        GoTo(chosen);
    }

    public override void PlaceObjectAt(LocationGridTile tile) {
        base.PlaceObjectAt(tile);

        GoToRandomTileInRadius();
        _expiryKey = SchedulingManager.Instance.AddEntry(_tornado.expiryDate, Expire, this);
        Messenger.AddListener(Signals.TICK_ENDED, PerTick);
        Messenger.AddListener<PROGRESSION_SPEED>(UISignals.PROGRESSION_SPEED_CHANGED, OnProgressionSpeedChanged);
        Messenger.AddListener<bool>(UISignals.PAUSED, OnGamePaused);
        Messenger.AddListener<TileObject, Character, LocationGridTile>(GridTileSignals.TILE_OBJECT_REMOVED, OnTileObjectRemovedFromTile);
        isSpawned = true;

        if (GameManager.Instance.isPaused || !GameManager.Instance.gameHasStarted) {
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
        if (!GameManager.Instance.gameHasStarted || destinationTile == null) {
            return;
        }
        // Keep a note of the time the movement started.
        _startTime = Time.time;
        
        var position = transform.position;
        _startPosition = position;
        // Calculate the journey length.
        _journeyLength = Vector3.Distance(position, destinationTile.centeredWorldLocation);
    }
    private void UpdateSpeed() {
        _speed = PlayerSkillManager.Instance.GetSkillMovementSpeedPerLevel(PLAYER_SKILL_TYPE.TORNADO);
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
        if (!isSpawned) {
            return; //tornado has already been reset
        }
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
        _tornado.Expire();
        ObjectPoolManager.Instance.DestroyObject(this);
    }
    #region Object Pooling
    public override void Reset() {
        base.Reset();
        isSpawned = false;
        destinationTile = null;
        _journeyLength = 0f;
        _startPosition = Vector3.zero;
        _startTime = 0f;
        _damagablesInTornado.Clear();
        ClearTornadoParticle();
        Messenger.RemoveListener(Signals.TICK_ENDED, PerTick);
        Messenger.RemoveListener<PROGRESSION_SPEED>(UISignals.PROGRESSION_SPEED_CHANGED, OnProgressionSpeedChanged);
        Messenger.RemoveListener<bool>(UISignals.PAUSED, OnGamePaused);
        Messenger.RemoveListener<TileObject, Character, LocationGridTile>(GridTileSignals.TILE_OBJECT_REMOVED, OnTileObjectRemovedFromTile);
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
        if (GameManager.Instance.isPaused || !GameManager.Instance.gameHasStarted) {
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
        BaseVisionTrigger collidedWith = collision.gameObject.GetComponent<BaseVisionTrigger>();
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
        BaseVisionTrigger collidedWith = collision.gameObject.GetComponent<BaseVisionTrigger>();
        if (collidedWith != null) {
            // Debug.Log($"Tornado collision exit with {collidedWith.damageable.name}");
            RemoveDamageable(collidedWith.damageable);
        }
    }
    #endregion

    #region POI's
    private void AddDamageable(IDamageable poi) {
        if (poi is MovingTileObject) {
            return;
        }
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
        if (poi is MovingTileObject) {
            return;
        }
        if (poi is Dragon dragon) {
            return;
        }
        poi.mapObjectVisual.transform.DOShakeRotation(20f, new Vector3(0f, 0f, 10f));
    }
    #endregion

    private void PerTick() {
        if (isSpawned == false) {
            return;
        }
        if (gameObject.activeSelf == false) {
            return;
        }
        if (gridTileLocation == null) {
            return;
        }
#if DEBUG_PROFILER
        Profiler.BeginSample($"Tornado Per Tick");
#endif
        SkillData tornadoData = PlayerSkillManager.Instance.GetSpellData(PLAYER_SKILL_TYPE.TORNADO);
        int processedDamage = -PlayerSkillManager.Instance.GetDamageBaseOnLevel(tornadoData);
        float piercing = PlayerSkillManager.Instance.GetAdditionalPiercePerLevelBaseOnLevel(tornadoData);
        List<LocationGridTile> tiles = RuinarchListPool<LocationGridTile>.Claim();
        gridTileLocation.PopulateTilesInRadius(tiles, _radius, includeCenterTile: true, includeTilesInDifferentStructure: true);
        for (int i = 0; i < tiles.Count; i++) {
            LocationGridTile tile = tiles[i];
            tile.tileObjectComponent.genericTileObject.AdjustHP(processedDamage, ELEMENTAL_TYPE.Wind, true, this, piercingPower: piercing, isPlayerSource: _tornado.isPlayerSource);
        }
        RuinarchListPool<LocationGridTile>.Release(tiles);
        for (int i = 0; i < _damagablesInTornado.Count; i++) {
            IDamageable damageable = _damagablesInTornado[i];
            if (damageable.mapObjectVisual != null) {
                //if(damageable is Dragon dragon) {
                //    if (!dragon.isAwakened) {
                //        dragon.Awaken();
                //    }
                //} else {
                    Vector3 distance = transform.position - damageable.mapObjectVisual.gameObjectVisual.transform.position;
                    if (distance.magnitude < 2f) { //3
                        DealDamage(damageable, processedDamage, piercing);
                    } else {
                        //check for suck in
                        TrySuckIn(damageable);
                    }
                //}
            }
        }
#if DEBUG_PROFILER
        Profiler.EndSample();
#endif
    }
    private void DealDamage(IDamageable damageable, int processedDamage, float piercing) {
        if (damageable.CanBeDamaged()) {
            if (damageable is Character character) {
                damageable.AdjustHP(processedDamage, ELEMENTAL_TYPE.Wind, true, _tornado, showHPBar: true, piercingPower: piercing, isPlayerSource: _tornado.isPlayerSource);
                Messenger.Broadcast(PlayerSignals.PLAYER_HIT_CHARACTER_VIA_SPELL, character, processedDamage);
                if (character.isDead && character.skillCauseOfDeath == PLAYER_SKILL_TYPE.NONE) {
                    character.skillCauseOfDeath = PLAYER_SKILL_TYPE.TORNADO;
                    //Messenger.Broadcast(PlayerSignals.CREATE_SPIRIT_ENERGY, character.deathTilePosition.centeredWorldLocation, 1, character.deathTilePosition.parentMap);
                    //Messenger.Broadcast(PlayerSignals.CREATE_CHAOS_ORBS, character.deathTilePosition.centeredWorldLocation, 1, character.deathTilePosition.parentMap);
                }
            } else {
                //Damage to tile objects should be half so that the village structures should not be destroyed easily
                int objectDamage = Mathf.RoundToInt(processedDamage * 0.5f);
                damageable.AdjustHP(objectDamage, ELEMENTAL_TYPE.Wind, true, _tornado, showHPBar: true, piercingPower: piercing, isPlayerSource: _tornado.isPlayerSource);    
            }
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
        damageable.AdjustHP(-damageable.maxHP, ELEMENTAL_TYPE.Wind, true, _tornado, showHPBar: true, isPlayerSource: _tornado.isPlayerSource);
    }
    private bool CanBeSuckedIn(IDamageable damageable) {
        return damageable.CanBeDamaged() && (damageable is GenericTileObject) == false 
            && (damageable is Character) == false && damageable.mapObjectVisual.IsTweening() == false;
    }

#region Abstract Member Implementation
    public override void UpdateTileObjectVisual(TileObject obj) { }
#endregion

#region Listeners
    private void OnTileObjectRemovedFromTile(TileObject tileObject, Character removedBy, LocationGridTile removedFrom) {
       RemoveDamageable(tileObject);
    }
#endregion
}
