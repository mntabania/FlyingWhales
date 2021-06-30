using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using Inner_Maps;
using Inner_Maps.Location_Structures;
using UnityEngine.Assertions;
using UtilityScripts;
public class ForlornSpirit : TileObject {

    public Character possessionTarget { get; private set; }
    private SpiritGameObject _spiritGO;
    private int _duration;
    private int _currentDuration;
    
    #region getters
    public int currentDuration => _currentDuration;
    public override System.Type serializedData => typeof(SaveDataForlornSpirit);
    #endregion
    
    public ForlornSpirit() {
        _duration = GameManager.Instance.GetTicksBasedOnHour(1);
        Initialize(TILE_OBJECT_TYPE.FORLORN_SPIRIT, false);
        traitContainer.AddTrait(this, "Forlorn");
    }
    public ForlornSpirit(SaveDataForlornSpirit data) : base(data) {
        _duration = GameManager.Instance.GetTicksBasedOnHour(1);
        //SaveDataForlornSpirit saveDataForlornSpirit = data as SaveDataForlornSpirit;
        Assert.IsNotNull(data);
        _currentDuration = data.currentDuration;
    }
    
    #region Overrides
    public override string ToString() {
        return $"Forlorn Spirit {id.ToString()}";
    }
    public override void OnPlacePOI() {
        base.OnPlacePOI();
        Messenger.AddListener<PROGRESSION_SPEED>(UISignals.PROGRESSION_SPEED_CHANGED, OnProgressionSpeedChanged);
        Messenger.AddListener<bool>(UISignals.PAUSED, OnGamePaused);
        Messenger.AddListener(Signals.TICK_ENDED, OnTickEnded);
        UpdateSpeed();
        _spiritGO.SetIsRoaming(true);
        GoToRandomTileInRadius();
        _spiritGO.SetRegion(gridTileLocation.parentMap.region);
    }
    public override void OnDestroyPOI() {
        base.OnDestroyPOI();
        Messenger.RemoveListener<PROGRESSION_SPEED>(UISignals.PROGRESSION_SPEED_CHANGED, OnProgressionSpeedChanged);
        Messenger.RemoveListener<bool>(UISignals.PAUSED, OnGamePaused);
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
        if (possessionTarget == null) {
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
            // mapVisual.transform.do
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
            ForlornEffect();
            DonePossession();
        } else {
            _spiritGO.SetIsRoaming(true);
        }
    }
    public void GoToRandomTileInRadius() {
        List<LocationGridTile> tilesInRadius = RuinarchListPool<LocationGridTile>.Claim();
        gridTileLocation.PopulateTilesInRadius(tilesInRadius, 3, includeCenterTile: false, includeTilesInDifferentStructure: true);
        LocationGridTile chosen = tilesInRadius[Random.Range(0, tilesInRadius.Count)];
        _spiritGO.SetDestinationTile(chosen);
        InnerMapManager.Instance.FaceTarget(this, chosen);
        RuinarchListPool<LocationGridTile>.Release(tilesInRadius);
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

    private void ForlornEffect() {
        int baseChance = 100;
        SkillData spiritData = PlayerSkillManager.Instance.GetSpellData(PLAYER_SKILL_TYPE.FORLORN_SPIRIT);
        RESISTANCE resistanceType = PlayerSkillManager.Instance.GetScriptableObjPlayerSkillData<PlayerSkillData>(PLAYER_SKILL_TYPE.FORLORN_SPIRIT).resistanceType;
        float piercing = PlayerSkillManager.Instance.GetAdditionalPiercePerLevelBaseOnLevel(spiritData);
        float resistanceValue = possessionTarget.piercingAndResistancesComponent.GetResistanceValue(resistanceType);
        CombatManager.ModifyValueByPiercingAndResistance(ref baseChance, piercing, resistanceValue);
        if (GameUtilities.RollChance(baseChance)) {
            //Triggers Effect
            float processedHappinessDrain = -PlayerSkillManager.Instance.GetIncreaseStatsPercentagePerLevel(spiritData);
            possessionTarget.needsComponent.AdjustHappiness(processedHappinessDrain);
        } else {
            possessionTarget.reactionComponent.ResistRuinarchPower();
        }
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
public class SaveDataForlornSpirit : SaveDataTileObject {
    public int currentDuration;
    public override void Save(TileObject tileObject) {
        base.Save(tileObject);
        ForlornSpirit forlornSpirit = tileObject as ForlornSpirit;
        Assert.IsNotNull(forlornSpirit);
        currentDuration = forlornSpirit.currentDuration;
    }
}
#endregion