using Inner_Maps;
using Inner_Maps.Location_Structures;
using System.Collections.Generic;
using UnityEngine;

public class DemonEye : TileObject {
    //public const int EYE_WARD_VISION_RANGE = 7;
    private List<LocationGridTile> tilesInRadius;

    private EyeWardHighlight _eyeWardHighlight;
    private Inner_Maps.Location_Structures.Watcher m_owner;
    public DemonEye() {
        tilesInRadius = new List<LocationGridTile>();
        Initialize(TILE_OBJECT_TYPE.DEMON_EYE, false);
        AddPlayerAction(PLAYER_SKILL_TYPE.DESTROY_EYE_WARD);
        traitContainer.RemoveTrait(this, "Flammable");
        traitContainer.AddTrait(this, "Indestructible");
        hiddenComponent.SetIsHidden(this, true, false);
        PlayerManager.Instance.player.tileObjectComponent.AddEyeWard(this);
    }
    public DemonEye(SaveDataTileObject data) : base(data) {
        tilesInRadius = new List<LocationGridTile>();
        //AddPlayerAction(PLAYER_SKILL_TYPE.DESTROY_EYE_WARD);
    }

    public void UpdateRange() {
        if (gridTileLocation != null && previousTile != gridTileLocation) {
            tilesInRadius.Clear();
            gridTileLocation.PopulateTilesInRadius(tilesInRadius, m_owner.GetEyeWardRadius(), includeCenterTile: true, includeTilesInDifferentStructure: true);
            for (int i = 0; i < tilesInRadius.Count; i++) {
                tilesInRadius[i].tileObjectComponent.SetIsSeenByEyeWard(true);
            }
        } else if (previousTile != null && gridTileLocation == null) {
            for (int i = 0; i < tilesInRadius.Count; i++) {
                tilesInRadius[i].tileObjectComponent.SetIsSeenByEyeWard(false);
            }
        }
    }

    #region Overrides

    protected override void OnSetGridTileLocation() {
        base.OnSetGridTileLocation();
        UpdateRange();
    }
    //protected override void SubscribeListeners() {
    //    base.SubscribeListeners();
    //    Messenger.AddListener(Signals.HOUR_STARTED, HourStarted);
    //}
    //protected override void UnsubscribeListeners() {
    //    base.UnsubscribeListeners();
    //    Messenger.RemoveListener(Signals.HOUR_STARTED, HourStarted);
    //}
    #endregion

    //#region Listeners
    //private void HourStarted() {
    //    ReduceHPBypassEverything(1);
    //}
    //#endregion

    #region Utilities
    public override void OnPlacePOI() {
        base.OnPlacePOI();
        if (PlayerManager.Instance.player.IsCurrentActiveSpell(PLAYER_SKILL_TYPE.SPAWN_EYE_WARD)) {
            ShowEyeWardHighlight();
        }
    }
    public override void OnDestroyPOI() {
        base.OnDestroyPOI();
        PlayerManager.Instance.player.tileObjectComponent.RemoveEyeWard(this);
        if(_eyeWardHighlight != null) {
            ObjectPoolManager.Instance.DestroyObject(_eyeWardHighlight.gameObject);
            _eyeWardHighlight = null;
        }
        PlayerSkillManager.Instance.GetPlayerActionData(PLAYER_SKILL_TYPE.SPAWN_EYE_WARD).AdjustCharges(1);
    }
    public void ReduceHPBypassEverything(int amount) {
        if (currentHP == 0 && amount < 0) { return; } //hp is already at minimum, do not allow any more negative adjustments
        if (Mathf.Abs(amount) > currentHP) {
            //if the damage amount is greater than this object's hp, set the damage to this object's
            //hp instead, this is so that if this object contributes to a structure's hp, it will not deal the excess damage
            //to the structure
            amount = -currentHP;
        }
        currentHP -= amount;
        currentHP = Mathf.Clamp(currentHP, 0, maxHP);
        if (mapVisual && mapVisual.hpBarGO) {
            if (mapVisual.hpBarGO.activeSelf) {
                mapVisual.UpdateHP(this);
            } else {
                if (currentHP > 0) {
                    //only show hp bar if hp was reduced and hp is greater than 0
                    mapVisual.QuickShowHPBar(this);
                }
            }
        }
        LocationGridTile tile = gridTileLocation;
        if (currentHP <= 0) {
            if (tile != null) {
                tile.structure.RemovePOI(this);
            }
        }
    }
    public void SetBeholderOwner(Inner_Maps.Location_Structures.Watcher p_beholder) {
        m_owner = p_beholder;
    }
    public void ShowEyeWardHighlight() {
        if(_eyeWardHighlight == null) {
            GameObject go = GameManager.Instance.CreateParticleEffectAt(this, PARTICLE_EFFECT.Eye_Ward_Highlight, false);
            if (go) {
                _eyeWardHighlight = go.GetComponent<EyeWardHighlight>();
            }
        }
        if(_eyeWardHighlight != null) {
            _eyeWardHighlight.HideHighlight();
            _eyeWardHighlight.SetupHighlight(m_owner.GetEyeWardRadius());
            _eyeWardHighlight.ShowHighlight();
        }
    }
    public void HideEyeWardHighlight() {
        if (_eyeWardHighlight != null) {
            _eyeWardHighlight.HideHighlight();

        }
    }
    #endregion

    #region Loading
    public override void LoadSecondWave(SaveDataTileObject data) {
        base.LoadSecondWave(data);
        AddPlayerAction(PLAYER_SKILL_TYPE.DESTROY_EYE_WARD);
        PlayerManager.Instance.player.tileObjectComponent.AddEyeWard(this);
    }
    #endregion
}
