using System.Collections.Generic;
using Inner_Maps;
using UnityEngine;

public class EyeWard : TileObject {
    public const int EYE_WARD_VISION_RANGE = 7;
    private List<LocationGridTile> tilesInRadius;
    public EyeWard() {
        tilesInRadius = new List<LocationGridTile>();
        Initialize(TILE_OBJECT_TYPE.EYE_WARD, false);
        AddPlayerAction(PLAYER_SKILL_TYPE.DESTROY_EYE_WARD);
        traitContainer.RemoveTrait(this, "Flammable");
        traitContainer.AddTrait(this, "Indestructible");
        hiddenComponent.SetIsHidden(true);
    }
    public EyeWard(SaveDataTileObject data) {
        tilesInRadius = new List<LocationGridTile>();
        AddPlayerAction(PLAYER_SKILL_TYPE.DESTROY_EYE_WARD);
    }

    #region Overrides
    protected override void OnSetGridTileLocation() {
        base.OnSetGridTileLocation();
        if (gridTileLocation != null && previousTile != gridTileLocation) {
            //Add Eye Ward Effect
            tilesInRadius.Clear();
            gridTileLocation.PopulateTilesInRadius(tilesInRadius, EYE_WARD_VISION_RANGE, includeCenterTile: true, includeTilesInDifferentStructure: true);
            for (int i = 0; i < tilesInRadius.Count; i++) {
                tilesInRadius[i].tileObjectComponent.SetIsSeenByEyeWard(true);
            }
        } else if (previousTile != null && gridTileLocation == null) {
            //Remove eye ward effect
            for (int i = 0; i < tilesInRadius.Count; i++) {
                tilesInRadius[i].tileObjectComponent.SetIsSeenByEyeWard(false);
            }
        }
    }
    protected override void SubscribeListeners() {
        base.SubscribeListeners();
        Messenger.AddListener(Signals.HOUR_STARTED, HourStarted);
    }
    protected override void UnsubscribeListeners() {
        base.UnsubscribeListeners();
        Messenger.RemoveListener(Signals.HOUR_STARTED, HourStarted);
    }
    #endregion

    #region Listeners
    private void HourStarted() {
        ReduceHPBypassEverything(1);
    }
    #endregion

    #region Utilities
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
    #endregion
}
