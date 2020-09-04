﻿using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using Inner_Maps;
using Inner_Maps.Location_Structures;
using UnityEngine.Assertions;

public class RavenousSpirit : TileObject {

    public Character possessionTarget { get; private set; }
    private SpiritGameObject _spiritGO;
    private int _duration;
    private int _currentDuration;
    
    #region getters
    public int currentDuration => _currentDuration;
    public override System.Type serializedData => typeof(SaveDataRavenousSpirit);
    #endregion
    
    public RavenousSpirit() {
        _duration = GameManager.Instance.GetTicksBasedOnHour(1);
        Initialize(TILE_OBJECT_TYPE.RAVENOUS_SPIRIT, false);
        traitContainer.AddTrait(this, "Ravenous");
    }
    public RavenousSpirit(SaveDataRavenousSpirit data) {
        //SaveDataRavenousSpirit saveDataRavenousSpirit = data as SaveDataRavenousSpirit;
        Assert.IsNotNull(data);
        _currentDuration = data.currentDuration;
    }

    #region Overrides
    public override string ToString() {
        return $"Ravenous Spirit {id.ToString()}";
    }
    public override void OnPlacePOI() {
        base.OnPlacePOI();
        Messenger.AddListener<PROGRESSION_SPEED>(Signals.PROGRESSION_SPEED_CHANGED, OnProgressionSpeedChanged);
        Messenger.AddListener<bool>(Signals.PAUSED, OnGamePaused);
        Messenger.AddListener(Signals.TICK_ENDED, OnTickEnded);
        UpdateSpeed();
        _spiritGO.SetIsRoaming(true);
        GoToRandomTileInRadius();
    }
    public override void OnDestroyPOI() {
        base.OnDestroyPOI();
        Messenger.RemoveListener<PROGRESSION_SPEED>(Signals.PROGRESSION_SPEED_CHANGED, OnProgressionSpeedChanged);
        Messenger.RemoveListener<bool>(Signals.PAUSED, OnGamePaused);
        Messenger.RemoveListener(Signals.TICK_ENDED, OnTickEnded);
    }
    protected override void CreateMapObjectVisual() {
        GameObject obj = InnerMapManager.Instance.mapObjectFactory.CreateNewTileObjectMapVisual(tileObjectType);
        _spiritGO = obj.GetComponent<SpiritGameObject>();
        mapVisual = _spiritGO;
        _spiritGO.SetRegion(InnerMapManager.Instance.currentlyShowingLocation as Region);
    }
    #endregion
    
    #region Listeners
    private void OnProgressionSpeedChanged(PROGRESSION_SPEED prog) {
        UpdateSpeed();
        _spiritGO.RecalculatePathingValues();
    }
    private void OnGamePaused(bool paused) {
        if(possessionTarget == null) {
            _spiritGO.SetIsRoaming(!paused);
            if (!paused) {
                _spiritGO.RecalculatePathingValues();
            }
        }
    }
    #endregion

    public void StartSpiritPossession(Character target) {
        if (possessionTarget == null) {
            _spiritGO.SetIsRoaming(false);
            possessionTarget = target;
            GameManager.Instance.StartCoroutine(CommencePossession());
        }
    }
    private IEnumerator CommencePossession() {
        InnerMapManager.Instance.FaceTarget(this, possessionTarget);
        yield return new WaitForSeconds(0.5f);
        while (possessionTarget.marker.transform.position != mapVisual.gameObject.transform.position && !possessionTarget.marker.IsNear(mapVisual.gameObject.transform.position)) {
            yield return new WaitForFixedUpdate();
            if (!GameManager.Instance.isPaused) {
                if (possessionTarget != null && possessionTarget.marker && possessionTarget.gridTileLocation != null && !possessionTarget.isBeingSeized) {
                    iTween.MoveUpdate(mapVisual.gameObject, possessionTarget.marker.transform.position, 2f);
                } else {
                    possessionTarget = null;
                    iTween.Stop(mapVisual.gameObject);
                    break;
                }
            }
        }
        if (possessionTarget != null) {
            RavenousEffect();
            DonePossession();
        } else {
            _spiritGO.SetIsRoaming(true);
        }
    }
    public void GoToRandomTileInRadius() {
        List<LocationGridTile> tilesInRadius = gridTileLocation.GetTilesInRadius(3, includeCenterTile: false, includeTilesInDifferentStructure: true);
        LocationGridTile chosen = tilesInRadius[Random.Range(0, tilesInRadius.Count)];
        _spiritGO.SetDestinationTile(chosen);
        InnerMapManager.Instance.FaceTarget(this, chosen);
    }
    private void UpdateSpeed() {
        _spiritGO.SetSpeed(1f);
        if (GameManager.Instance.currProgressionSpeed == PROGRESSION_SPEED.X2) {
            _spiritGO.SetSpeed(_spiritGO.speed * 1.5f);
        } else if (GameManager.Instance.currProgressionSpeed == PROGRESSION_SPEED.X4) {
            _spiritGO.SetSpeed(_spiritGO.speed * 2f);
        }
    }

    private void OnTickEnded() {
        if (_spiritGO != null && _spiritGO.isRoaming) {
            _currentDuration++;
            if (_currentDuration >= _duration) {
                _spiritGO.SetIsRoaming(false);
                Dissipate();
            }
        }
    }

    private void RavenousEffect() {
        possessionTarget.needsComponent.AdjustFullness(-35);
    }

    private void DonePossession() {
        GameManager.Instance.CreateParticleEffectAt(possessionTarget.gridTileLocation, PARTICLE_EFFECT.Minion_Dissipate);
        DestroySpirit();
    }
    private void Dissipate() {
        GameManager.Instance.CreateParticleEffectAt(gridTileLocation, PARTICLE_EFFECT.Minion_Dissipate);
        DestroySpirit();
    }
    private void DestroySpirit() {
        iTween.Stop(mapVisual.gameObject);
        SetGridTileLocation(null);
        OnDestroyPOI();
        possessionTarget = null;
    }
}

#region Save Data
public class SaveDataRavenousSpirit : SaveDataTileObject {
    public int currentDuration;
    public override void Save(TileObject tileObject) {
        base.Save(tileObject);
        RavenousSpirit ravenousSpirit = tileObject as RavenousSpirit;
        Assert.IsNotNull(ravenousSpirit);
        currentDuration = ravenousSpirit.currentDuration;
    }
}
#endregion