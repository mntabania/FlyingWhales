using System.Collections;
using System.Collections.Generic;
using Traits;
using UnityEngine;
using UnityEngine.Assertions;
using Inner_Maps.Location_Structures;

public class SeizeObjectData : PlayerAction {
    public override PLAYER_SKILL_TYPE type => PLAYER_SKILL_TYPE.SEIZE_OBJECT;
    public override string name => "Seize Object";
    public override string description => "This Ability can be used to take an object and then transfer it to an unoccupied tile." +
        "\nTaking a resource pile from a Village city center will produce a Chaos Orb.";
    public SeizeObjectData() : base() {
        targetTypes = new SPELL_TARGET[] { SPELL_TARGET.TILE_OBJECT };
    }

    #region Overrides
    public override void ActivateAbility(IPointOfInterest targetPOI) {
        PlayerManager.Instance.player.seizeComponent.SeizePOI(targetPOI);
        base.ActivateAbility(targetPOI);
    }
    public override bool CanPerformAbilityTowards(TileObject targetTileObject) {
        bool canPerform = base.CanPerformAbilityTowards(targetTileObject);
        if (canPerform) {
            if(targetTileObject is AnkhOfAnubis ankh && ankh.isActivated) {
                return false;
            }
            if(targetTileObject is WurmHole) {
                return false;
            }
            if(targetTileObject is CultAltar) {
                return false;
            }
            return !PlayerManager.Instance.player.seizeComponent.hasSeizedPOI && (targetTileObject.mapVisual != null || targetTileObject.isBeingCarriedBy != null);
        }
        return false;
    }
    public override bool IsValid(IPlayerActionTarget target) {
        bool isValid = base.IsValid(target);
        if (isValid || (target is TileObject targetTileObject && targetTileObject.isBeingCarriedBy != null)) {
            if (target is AnkhOfAnubis ankh && ankh.isActivated) {
                return false;
            }
            if (target is WurmHole) {
                return false;
            }
            return true; //allow tile object to be seized even if it does not yet have a map visual, but make sure it hasn't been destroyed yet  
        }
        return false;
    }
    #endregion
}