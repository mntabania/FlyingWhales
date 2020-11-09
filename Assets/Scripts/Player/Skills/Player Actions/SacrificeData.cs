using System.Collections;
using System.Collections.Generic;
using Traits;
using UnityEngine;
using UnityEngine.Assertions;
using Inner_Maps.Location_Structures;

public class SacrificeData : PlayerAction {
    public override SPELL_TYPE type => SPELL_TYPE.SACRIFICE;
    public override string name => "Sacrifice";
    public override string description => "This Action allows you to sacrifice a kenneled Monster for 2 Mana Orbs.";
    public SacrificeData() : base() {
        targetTypes = new SPELL_TARGET[] { SPELL_TARGET.CHARACTER };
    }

    #region Overrides
    public override void ActivateAbility(IPointOfInterest targetPOI) {
        if(targetPOI is Summon summon) {
            Messenger.Broadcast(PlayerSignals.CREATE_CHAOS_ORBS, summon.worldPosition, 2, summon.currentRegion.innerMap);
            summon.Death("sacrifice");
            base.ActivateAbility(targetPOI);
        }
    }
    public override bool CanPerformAbilityTowards(Character targetCharacter) {
        bool canPerform = base.CanPerformAbilityTowards(targetCharacter);
        if (canPerform) {
            if (targetCharacter is Summon) {
                if (!targetCharacter.isDead && targetCharacter.gridTileLocation != null && targetCharacter.gridTileLocation.structure != null) {
                    if (targetCharacter.gridTileLocation.structure.structureType == STRUCTURE_TYPE.KENNEL) {
                        return true;
                    } else if (targetCharacter.gridTileLocation.structure.structureType == STRUCTURE_TYPE.TORTURE_CHAMBERS) {
                        return targetCharacter.gridTileLocation.structure.IsTilePartOfARoom(targetCharacter.gridTileLocation, out var room);
                    } else if (targetCharacter.gridTileLocation.structure.structureType == STRUCTURE_TYPE.DEFILER) {
                        return targetCharacter.gridTileLocation.structure.IsTilePartOfARoom(targetCharacter.gridTileLocation, out var room);
                    }
                }
            }
            return false;
        }
        return false;
    }
    public override bool IsValid(IPlayerActionTarget target) {
        if(target is Summon targetCharacter) {
            if (targetCharacter.gridTileLocation != null && targetCharacter.gridTileLocation.structure != null) {
                if (targetCharacter.gridTileLocation.structure.structureType == STRUCTURE_TYPE.KENNEL) {
                    return true;
                } else if (targetCharacter.gridTileLocation.structure.structureType == STRUCTURE_TYPE.TORTURE_CHAMBERS) {
                    return targetCharacter.gridTileLocation.structure.IsTilePartOfARoom(targetCharacter.gridTileLocation, out var room);
                }else if (targetCharacter.gridTileLocation.structure.structureType == STRUCTURE_TYPE.DEFILER) {
                    return targetCharacter.gridTileLocation.structure.IsTilePartOfARoom(targetCharacter.gridTileLocation, out var room);
                }
            }
            return false;
        }
        return false;
    }
    #endregion
}