using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Inner_Maps;
using UnityEngine;
using UnityEngine.EventSystems;
using Traits;
using UnityEngine.Profiling;
#if UNITY_EDITOR
using Packages.Rider.Editor;
#endif

public class AreaSpellsComponent : AreaComponent {
    
    #region Earthquake Variables
    public bool hasEarthquake { get; private set; }
    public List<IPointOfInterest> earthquakeTileObjects { get; private set; }
    public List<IPointOfInterest> pendingEarthquakeTileObjects { get; private set; }
    public int currentEarthquakeDuration { get; private set; }
    private LocationGridTile _centerEarthquakeTile;
    private bool _hasEarthquakeStarted;
    #endregion
    
    #region Brimstones Variables
    public bool hasBrimstones { get; private set; }
    public int currentBrimstonesDuration  { get; private set; }
    #endregion

    #region Electric Storm Variables
    public bool hasElectricStorm { get; private set; }
    public int currentElectricStormDuration { get; private set; }
    #endregion
    
    #region Iceteroid Variables
    public bool hasIceteroids { get; private set; }
    public int currentIceteroidsDuration  { get; private set; }
    #endregion

    public AreaSpellsComponent() {
        earthquakeTileObjects = new List<IPointOfInterest>();
        pendingEarthquakeTileObjects = new List<IPointOfInterest>();
        Messenger.AddListener<bool>(UISignals.PAUSED, OnGamePaused);
    }

    public AreaSpellsComponent(SaveDataAreaSpellsComponent saveData) {
        earthquakeTileObjects = new List<IPointOfInterest>();
        pendingEarthquakeTileObjects = new List<IPointOfInterest>();
        Messenger.AddListener<bool>(UISignals.PAUSED, OnGamePaused);
    }

    #region Loading
    public void LoadReferences(SaveDataAreaSpellsComponent saveData) {
        if (saveData.hasEarthquake) {
            SetHasEarthquake(true);
            currentEarthquakeDuration = saveData.remainingEarthquakeDuration;
        }
        if (saveData.hasBrimstones) {
            SetHasBrimstones(true);
            currentBrimstonesDuration = saveData.remainingBrimstoneDuration;
        }
        if (saveData.hasElectricStorm) {
            SetHasElectricStorm(true);
            currentElectricStormDuration = saveData.remainingElectricStormDuration;
        }
        if (saveData.hasIceteroids) {
            SetHasIceteroids(true);
            currentIceteroidsDuration = saveData.remainingIceteroidsDuration;
        }
    }
    #endregion

    #region Listeners
    private void OnGamePaused(bool state) {
        if (hasEarthquake) {
            if (state) {
                PauseEarthquake();
            } else {
                ResumeEarthquake();
            }
        }
    }
    #endregion
    
    #region Processes
    public void OnPlacePOIInHex(IPointOfInterest poi) {
        if (poi.poiType == POINT_OF_INTEREST_TYPE.TILE_OBJECT) {
            OnPlaceTileObjectInHex(poi as TileObject);
        } else if (poi.poiType == POINT_OF_INTEREST_TYPE.CHARACTER) {
            OnPlaceCharacterInHex(poi as Character);
        }
    }
    public void OnRemovePOIInHex(IPointOfInterest poi) {
        if (poi.poiType == POINT_OF_INTEREST_TYPE.TILE_OBJECT) {
            OnRemoveTileObjectInHex(poi as TileObject);
        } else if (poi.poiType == POINT_OF_INTEREST_TYPE.CHARACTER) {
            OnRemoveCharacterInHex(poi as Character);
        }
    }
    private void OnPlaceTileObjectInHex(TileObject tileObject) {
        if (hasEarthquake) {
            AddPendingEarthquakeTileObject(tileObject);
        }
    }
    private void OnPlaceCharacterInHex(Character character) {
        if (hasEarthquake) {
            character.traitContainer.AddTrait(character, "Disoriented");
        }
    }
    private void OnRemoveTileObjectInHex(TileObject tileObject) {
        if (hasEarthquake) {
            RemoveEarthquakeTileObject(tileObject);
        }
    }
    private void OnRemoveCharacterInHex(Character character) {
        
    }
    #endregion
    
    #region Earthquake
    public void SetHasEarthquake(bool state) {
        if (hasEarthquake != state) {
            hasEarthquake = state;
            if (hasEarthquake) {
                StartEarthquake();
            } else {
                StopEarthquake();
            }
        }
    }
    private void StartEarthquake() {
        currentEarthquakeDuration = 0;
        _centerEarthquakeTile = owner.gridTileComponent.centerGridTile;
        earthquakeTileObjects.Clear();
        for (int i = 0; i < owner.gridTileComponent.gridTiles.Count; i++) {
            IPointOfInterest poi = owner.gridTileComponent.gridTiles[i].tileObjectComponent.objHere;
            if (poi != null) {
                AddEarthquakeTileObject(poi);
            }
        }
        if (!GameManager.Instance.isPaused) {
            OnStartEarthquake();
        } else {
            _hasEarthquakeStarted = false;
        }
    }
    private void OnStartEarthquake() {
        _hasEarthquakeStarted = true;

        //Note: I put the PerTickEarthquake listener before awakening the dragon so that the process would be, AddListener => Pause Earthquake(Because of the dragon awaken popup, which would Remove the listener) => Click Ok (Add Listener again)
        //If we put it after the dragon awakening popup, It would double the add listener, that is why we keep getting infinite camera shake, because the PerTickEarthquake is not properly removed
        CameraShake();
        Messenger.AddListener(Signals.TICK_STARTED, PerTickEarthquake);

        List<Character> charactersInsideHex = ObjectPoolManager.Instance.CreateNewCharactersList();
        owner.locationCharacterTracker.PopulateCharacterListInsideHexThatMeetCriteria(charactersInsideHex, c => !c.isDead);
        if (charactersInsideHex != null) {
            for (int i = 0; i < charactersInsideHex.Count; i++) {
                Character character = charactersInsideHex[i];
                if(character is Dragon dragon) {
                    if (!dragon.isAwakened) {
                        dragon.Awaken();
                    }
                } else {
                    charactersInsideHex[i].traitContainer.AddTrait(charactersInsideHex[i], "Disoriented");
                }
            }
        }
        ObjectPoolManager.Instance.ReturnCharactersListToPool(charactersInsideHex);
    }
    private void StopEarthquake() {
        Messenger.RemoveListener(Signals.TICK_STARTED, PerTickEarthquake);
        StopCameraShake();
        // for (int i = 0; i < earthquakeTileObjects.Count; i++) {
        //     IPointOfInterest poi = earthquakeTileObjects[i];
        //     poi.mapObjectVisual.transform.DOKill();
        // }
        earthquakeTileObjects.Clear();
    }
    private void PauseEarthquake() {
        if (_hasEarthquakeStarted) {
            StopCameraShake();
            Messenger.RemoveListener(Signals.TICK_STARTED, PerTickEarthquake);
        }
        // for (int i = 0; i < earthquakeTileObjects.Count; i++) {
        //     IPointOfInterest poi = earthquakeTileObjects[i];
        //     poi.mapObjectVisual.transform.DOPause();
        // }
    }
    private void ResumeEarthquake( ) {
        if (!_hasEarthquakeStarted) {
            OnStartEarthquake();
        } else {
            //CameraShake();
            Messenger.AddListener(Signals.TICK_STARTED, PerTickEarthquake);
        }
        // for (int i = 0; i < earthquakeTileObjects.Count; i++) {
        //     IPointOfInterest poi = earthquakeTileObjects[i];
        //     poi.mapObjectVisual.transform.DOPlay();
        // }
    }
    public void AddEarthquakeTileObject(IPointOfInterest poi) {
        earthquakeTileObjects.Add(poi);
        //POIShake(poi);
    }
    public void AddPendingEarthquakeTileObject(IPointOfInterest poi) {
        pendingEarthquakeTileObjects.Add(poi);
    }
    public void RemoveEarthquakeTileObject(IPointOfInterest poi) {
        if (earthquakeTileObjects.Remove(poi)) {
            // poi.mapObjectVisual.transform.DOKill();
        }
    }
    private void CameraShake() {
        InnerMapCameraMove.Instance.EarthquakeShake();
        //tween.OnComplete(OnCompleteCameraShake);
    }
    private void StopCameraShake() {
        GameManager.Instance.StartCoroutine(StopCameraShakeCoroutine());
    }
    private IEnumerator StopCameraShakeCoroutine() {
        yield return null;
        InnerMapCameraMove.Instance.camera.DOKill();
        InnerMapCameraMove.Instance.camera.transform.rotation = Quaternion.Euler(new Vector3(0f, 0f, 0f));
    }
    private void POIShake(IPointOfInterest poi) {
        Tweener tween = poi.mapObjectVisual.transform.DOShakeRotation(1f, new Vector3(0f, 0f, 5f), 40, fadeOut: false);
        tween.OnComplete(() => OnCompletePOIShake(poi));
    }
    private void OnCompletePOIShake(IPointOfInterest poi) {
        if (hasEarthquake) {
            POIShake(poi);
        }
    }
    private void POIMove(IPointOfInterest poi, LocationGridTile to) {
        if (poi.gridTileLocation != null) {
            poi.gridTileLocation.structure.RemovePOIWithoutDestroying(poi);
        }
        to.structure.AddPOI(poi, to);
        // Tween tween = poi.mapObjectVisual.transform.DOMove(to.centeredWorldLocation,1f, true);
        // tween.OnComplete(() => OnCompletePOIMove(poi, to));
    }
    private void OnCompletePOIMove(IPointOfInterest poi, LocationGridTile to) {
        if (poi.gridTileLocation != null) {
            poi.gridTileLocation.structure.RemovePOIWithoutDestroying(poi);
        }
        to.structure.AddPOI(poi, to);
    }
    private void PerTickEarthquake() {
        Profiler.BeginSample($"Per Tick Earthquake");
        if (InnerMapManager.Instance.isAnInnerMapShowing && InnerMapManager.Instance.currentlyShowingLocation == owner.region) {
            if (InnerMapCameraMove.Instance.CanSee(_centerEarthquakeTile)) {
                if (!DOTween.IsTweening(InnerMapCameraMove.Instance.camera)) {
                    CameraShake();
                }
            } else {
                if (DOTween.IsTweening(InnerMapCameraMove.Instance.camera)) {
                    StopCameraShake();
                }
            }

        } else {
            StopCameraShake();
        }
        
        currentEarthquakeDuration++;
        for (int i = 0; i < earthquakeTileObjects.Count; i++) {
            IPointOfInterest poi = earthquakeTileObjects[i];
            if (poi.gridTileLocation == null) {
                RemoveEarthquakeTileObject(poi);
                i--;
                continue;
            }
            int processedDamage = (-PlayerSkillManager.Instance.GetDamageBaseOnLevel(PLAYER_SKILL_TYPE.EARTHQUAKE));
            poi.AdjustHP(processedDamage, ELEMENTAL_TYPE.Normal, showHPBar: true, piercingPower: PlayerSkillManager.Instance.GetAdditionalPiercePerLevelBaseOnLevel(PLAYER_SKILL_TYPE.EARTHQUAKE));
            if (poi.gridTileLocation != null && !poi.traitContainer.HasTrait("Immovable")) {
                if (!DOTween.IsTweening(poi.mapObjectVisual.transform)) {
                    if (UnityEngine.Random.Range(0, 100) < 30) {
                        List<LocationGridTile> adjacentTiles = poi.gridTileLocation.UnoccupiedNeighboursWithinHex;
                        if (adjacentTiles != null && adjacentTiles.Count > 0) {
                            POIMove(poi, adjacentTiles[UnityEngine.Random.Range(0, adjacentTiles.Count)]);
                        }
                    }
                }
            }
            // else {
            //     RemoveEarthquakeTileObject(poi);
            //     i--;
            // }
        }
        if (pendingEarthquakeTileObjects.Count > 0) {
            for (int i = 0; i < pendingEarthquakeTileObjects.Count; i++) {
                AddEarthquakeTileObject(pendingEarthquakeTileObjects[i]);
            }
            pendingEarthquakeTileObjects.Clear();
        }
        if (currentEarthquakeDuration >= 3) {
            SetHasEarthquake(false);
        }
        Profiler.EndSample();
    }
    #endregion
    
    #region Brimstones
    public void SetHasBrimstones(bool state) {
        if (hasBrimstones != state) {
            hasBrimstones = state;
            if (hasBrimstones) {
                StartBrimstones();
            } else {
                StopBrimstones();
            }
        }
    }
    private void StartBrimstones() {
        currentBrimstonesDuration = 0;
        GameManager.Instance.StartCoroutine(CommenceFallingBrimstones());
        Messenger.AddListener(Signals.TICK_STARTED, PerTickBrimstones);
    }
    private void StopBrimstones() {
        GameManager.Instance.StopCoroutine(CommenceFallingBrimstones());
        Messenger.RemoveListener(Signals.TICK_STARTED, PerTickBrimstones);
    }
    private IEnumerator CommenceFallingBrimstones() {
        while (hasBrimstones) {
            while (GameManager.Instance.isPaused || !GameManager.Instance.gameHasStarted) {
                yield return null;
            }
            yield return new WaitForSeconds(UnityEngine.Random.Range(0.1f, 0.7f));
            LocationGridTile chosenTile = owner.gridTileComponent.gridTiles[UnityEngine.Random.Range(0, owner.gridTileComponent.gridTiles.Count)];
            GameManager.Instance.CreateParticleEffectAt(chosenTile, PARTICLE_EFFECT.Brimstones);
            //Note: Damage is moved in BrimstonesParticleEffect
            //chosenTile.PerformActionOnTraitables(ApplyBrimstoneDamage);
        }
    }
    //private void ApplyBrimstoneDamage(ITraitable traitable) {
    //    traitable.AdjustHP(PlayerSkillManager.Instance.GetAdditionalDamageBaseOnLevel(PLAYER_SKILL_TYPE.BRIMSTONES), ELEMENTAL_TYPE.Fire, true, showHPBar: true,
    //                piercingPower: PlayerSkillManager.Instance.GetAdditionalPiercePerLevelBaseOnLevel(PLAYER_SKILL_TYPE.BRIMSTONES));
    //}
    //private void BrimstoneEffect(ITraitable traitable) {
    //    if (traitable is IPointOfInterest poi) {
    //        poi.AdjustHP(-400, ELEMENTAL_TYPE.Fire, true, showHPBar: true);
    //    }
    //}
    //private IEnumerator CommenceBrimstoneEffect(LocationGridTile targetTile) {
    //    yield return new WaitForSeconds(0.6f);
    //    List<ITraitable> traitables = targetTile.GetTraitablesOnTile();
    //    BurningSource bs = null;
    //    for (int i = 0; i < traitables.Count; i++) {
    //        ITraitable traitable = traitables[i];
    //        if (traitable is TileObject obj) {
    //            if (obj.tileObjectType != TILE_OBJECT_TYPE.GENERIC_TILE_OBJECT) {
    //                obj.AdjustHP(-obj.currentHP, ELEMENTAL_TYPE.Fire);
    //                if (obj.gridTileLocation == null) {
    //                    continue; //object was destroyed, do not add burning trait
    //                }
    //            } else {
    //                obj.AdjustHP(0, ELEMENTAL_TYPE.Fire);
    //            }
    //        } else if (traitable is Character character) {
    //            character.AdjustHP(-(int) (character.maxHP * 0.4f), ELEMENTAL_TYPE.Fire, true);
    //            if (UnityEngine.Random.Range(0, 100) < 25) {
    //                character.traitContainer.AddTrait(character, "Injured");
    //            }
    //        } else {
    //            traitable.AdjustHP(-traitable.currentHP, ELEMENTAL_TYPE.Fire);
    //        }
    //        Burning burningTrait = traitable.traitContainer.GetNormalTrait<Burning>("Burning");
    //        if (burningTrait != null && burningTrait.sourceOfBurning == null) {
    //            if (bs == null) {
    //                bs = new BurningSource(traitable.gridTileLocation.parentMap.location);
    //            }
    //            burningTrait.SetSourceOfBurning(bs, traitable);
    //        }
    //    }
    //}
    private void PerTickBrimstones() {
        Profiler.BeginSample($"Per Tick Brimstones");
        currentBrimstonesDuration++;
        if (currentBrimstonesDuration >= PlayerSkillManager.Instance.GetDurationBonusPerLevel(PLAYER_SKILL_TYPE.BRIMSTONES)) {
            SetHasBrimstones(false);
        }
        Profiler.EndSample();
    }
    public void ResetBrimstoneDuration() {
        currentBrimstonesDuration = 0;
    }
    #endregion

    #region Electric Storm
    public void SetHasElectricStorm(bool state) {
        if (hasElectricStorm != state) {
            hasElectricStorm = state;
            if (hasElectricStorm) {
                StartElectricStorm();
            } else {
                StopElectricStorm();
            }
        }
    }
    private void StartElectricStorm() {
        currentElectricStormDuration = 0;
        GameManager.Instance.StartCoroutine(CommenceElectricStorm());
        Messenger.AddListener(Signals.TICK_STARTED, PerTickElectricStorm);
    }
    private void StopElectricStorm() {
        GameManager.Instance.StopCoroutine(CommenceElectricStorm());
        Messenger.RemoveListener(Signals.TICK_STARTED, PerTickElectricStorm);
    }
    private IEnumerator CommenceElectricStorm() {
        while (hasElectricStorm) {
            while (GameManager.Instance.isPaused || !GameManager.Instance.gameHasStarted) {
                yield return null;
            }
            yield return new WaitForSeconds(UnityEngine.Random.Range(0.1f, 0.7f));
            LocationGridTile chosenTile = owner.gridTileComponent.gridTiles[UnityEngine.Random.Range(0, owner.gridTileComponent.gridTiles.Count)];
            GameManager.Instance.CreateParticleEffectAt(chosenTile, PARTICLE_EFFECT.Lightning_Strike);
            chosenTile.tileObjectComponent.genericTileObject.traitContainer.AddTrait(chosenTile.tileObjectComponent.genericTileObject, "Danger Remnant");
            // List<IPointOfInterest> pois = chosenTile.GetPOIsOnTile();
            // for (int i = 0; i < pois.Count; i++) {
            //     pois[i].AdjustHP(-175, ELEMENTAL_TYPE.Electric, true, showHPBar: true);
            // }
            chosenTile.PerformActionOnTraitables(ElectricStormEffect);
        }
    }
    private void ElectricStormEffect(ITraitable traitable) {
        if (traitable is IPointOfInterest poi) {
            int processedDamage = (-PlayerSkillManager.Instance.GetDamageBaseOnLevel(PLAYER_SKILL_TYPE.ELECTRIC_STORM));
            poi.AdjustHP(processedDamage, ELEMENTAL_TYPE.Electric, true, showHPBar: true, piercingPower: PlayerSkillManager.Instance.GetAdditionalPiercePerLevelBaseOnLevel(PLAYER_SKILL_TYPE.ELECTRIC_STORM));
        }
    }
    private void PerTickElectricStorm() {
        Profiler.BeginSample($"Per Tick Electric Storm");
        currentElectricStormDuration++;
        if (currentElectricStormDuration >= PlayerSkillManager.Instance.GetDurationBonusPerLevel(PLAYER_SKILL_TYPE.ELECTRIC_STORM)) {
            SetHasElectricStorm(false);
        }
        Profiler.EndSample();
    }
    public void ResetElectricStormDuration() {
        currentElectricStormDuration = 0;
    }
    #endregion
    
    #region Iceteroids
    public void SetHasIceteroids(bool state) {
        if (hasIceteroids != state) {
            hasIceteroids = state;
            if (hasIceteroids) {
                StartIceteroids();
            } else {
                StopIceteroids();
            }
        }
    }
    private void StartIceteroids() {
        currentIceteroidsDuration = 0;
        GameManager.Instance.StartCoroutine(CommenceFallingIceteroids());
        Messenger.AddListener(Signals.TICK_STARTED, PerTickIceteroids);
    }
    private void StopIceteroids() {
        GameManager.Instance.StopCoroutine(CommenceFallingIceteroids());
        Messenger.RemoveListener(Signals.TICK_STARTED, PerTickIceteroids);
    }
    private IEnumerator CommenceFallingIceteroids() {
        while (hasIceteroids) {
            while (GameManager.Instance.isPaused || !GameManager.Instance.gameHasStarted) {
                yield return null;
            }
            yield return new WaitForSeconds(UnityEngine.Random.Range(0.1f, 0.7f));
            LocationGridTile chosenTile = owner.gridTileComponent.gridTiles[UnityEngine.Random.Range(0, owner.gridTileComponent.gridTiles.Count)];
            GameManager.Instance.CreateParticleEffectAt(chosenTile, PARTICLE_EFFECT.Iceteroids);
            //Note: Damage is moved in IceteroidParticleEffect
            //chosenTile.PerformActionOnTraitables(ApplyIceteroidDamage);

            //List<IPointOfInterest> pois = chosenTile.GetPOIsOnTile();
            //for (int i = 0; i < pois.Count; i++) {
            //    pois[i].AdjustHP(PlayerSkillManager.Instance.GetAdditionalDamageBaseOnLevel(PLAYER_SKILL_TYPE.ICETEROIDS), ELEMENTAL_TYPE.Ice, true, showHPBar: true,
            //        piercingPower: PlayerSkillManager.Instance.GetAdditionalPiercePerLevelBaseOnLevel(PLAYER_SKILL_TYPE.ICETEROIDS));
            //}
        }
    }
    //private void ApplyIceteroidDamage(ITraitable traitable) {
    //    traitable.AdjustHP(PlayerSkillManager.Instance.GetAdditionalDamageBaseOnLevel(PLAYER_SKILL_TYPE.ICETEROIDS), ELEMENTAL_TYPE.Ice, true, showHPBar: true,
    //                piercingPower: PlayerSkillManager.Instance.GetAdditionalPiercePerLevelBaseOnLevel(PLAYER_SKILL_TYPE.ICETEROIDS));
    //}
    private void PerTickIceteroids() {
        Profiler.BeginSample($"Per Tick Iceteroids");
        currentIceteroidsDuration++;
        if (currentIceteroidsDuration >= PlayerSkillManager.Instance.GetDurationBonusPerLevel(PLAYER_SKILL_TYPE.ICETEROIDS)) {
            SetHasIceteroids(false);
        }
        Profiler.EndSample();
    }
    public void ResetIceteroidDuration() {
        currentIceteroidsDuration = 0;
    }
    #endregion
}

public class SaveDataAreaSpellsComponent : SaveData<AreaSpellsComponent> {
    //earthquake
    public bool hasEarthquake;
    public int remainingEarthquakeDuration;
    //brimstones
    public bool hasBrimstones;
    public int remainingBrimstoneDuration;
    //electric storm
    public bool hasElectricStorm;
    public int remainingElectricStormDuration;
    //iceteroids
    public bool hasIceteroids;
    public int remainingIceteroidsDuration;
    
    public override void Save(AreaSpellsComponent data) {
        base.Save(data);
        hasEarthquake = data.hasEarthquake;
        remainingEarthquakeDuration = data.currentEarthquakeDuration;
        
        hasBrimstones = data.hasBrimstones;
        remainingBrimstoneDuration = data.currentBrimstonesDuration;
            
        hasElectricStorm = data.hasElectricStorm;
        remainingElectricStormDuration = data.currentElectricStormDuration;
        
        hasIceteroids = data.hasIceteroids;
        remainingIceteroidsDuration = data.currentIceteroidsDuration;
    }
    public override AreaSpellsComponent Load() {
        AreaSpellsComponent component = new AreaSpellsComponent(this);
        return component;
    }
}