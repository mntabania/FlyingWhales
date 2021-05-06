using System.Collections;
using System.Collections.Generic;
using Traits;
using UnityEngine;
using UnityEngine.Assertions;
using Inner_Maps.Location_Structures;

public class SacrificeData : PlayerAction {
    public override PLAYER_SKILL_TYPE type => PLAYER_SKILL_TYPE.SACRIFICE;
    public override string name => "Sacrifice";
    public override string description => "This Action allows you to sacrifice an imprisoned Monster for Chaotic Orbs.";
    public SacrificeData() : base() {
        targetTypes = new SPELL_TARGET[] { SPELL_TARGET.CHARACTER };
    }

    #region Overrides
    public override void ActivateAbility(IPointOfInterest targetPOI) {
        if(targetPOI is Summon summon) {
            int orbCount = UtilityScripts.GameUtilities.RandomBetweenTwoNumbers(2, 5);
            Messenger.Broadcast(PlayerSignals.CREATE_CHAOS_ORBS, summon.gridTileLocation.centeredWorldLocation, orbCount, summon.gridTileLocation.parentMap);
            summon.Death("sacrifice");
            base.ActivateAbility(targetPOI);
        }
    }
    public override string GetReasonsWhyCannotPerformAbilityTowards(Character targetCharacter) {
        string reasons = base.GetReasonsWhyCannotPerformAbilityTowards(targetCharacter);
        if (targetCharacter.traitContainer.HasTrait("Being Drained")) {
            reasons += "Characters being drained cannot be Sacrificed.";
        }
        return reasons;
    }
    public override bool CanPerformAbilityTowards(Character targetCharacter) {
        bool canPerform = base.CanPerformAbilityTowards(targetCharacter);
        if (canPerform) {
            if (targetCharacter.traitContainer.HasTrait("Being Drained")) {
                return false;
            }
            if (targetCharacter is Summon) {
                if (!targetCharacter.isDead && targetCharacter.gridTileLocation != null && targetCharacter.gridTileLocation.structure != null) {
                    if (targetCharacter.gridTileLocation.structure.structureType == STRUCTURE_TYPE.KENNEL) {
                        return true;
                    } else if (targetCharacter.gridTileLocation.structure.structureType == STRUCTURE_TYPE.TORTURE_CHAMBERS) {
                        return targetCharacter.gridTileLocation.structure.IsTilePartOfARoom(targetCharacter.gridTileLocation, out var room);
                    } 
                    //else if (targetCharacter.gridTileLocation.structure.structureType == STRUCTURE_TYPE.DEFILER) {
                    //    return targetCharacter.gridTileLocation.structure.IsTilePartOfARoom(targetCharacter.gridTileLocation, out var room);
                    //}
                }
            }
            return false;
        }
        return false;
    }
    public override bool IsValid(IPlayerActionTarget target) {
        bool isValid = base.IsValid(target);
        if (isValid) {
            if (target is Summon targetCharacter) {
                if (targetCharacter.gridTileLocation != null && targetCharacter.gridTileLocation.structure != null) {
                    if (targetCharacter.gridTileLocation.structure.structureType == STRUCTURE_TYPE.KENNEL) {
                        return true;
                    } else if (targetCharacter.gridTileLocation.structure.structureType == STRUCTURE_TYPE.TORTURE_CHAMBERS) {
                        return targetCharacter.gridTileLocation.structure.IsTilePartOfARoom(targetCharacter.gridTileLocation, out var room);
                    }
                    //else if (targetCharacter.gridTileLocation.structure.structureType == STRUCTURE_TYPE.DEFILER) {
                    //    return targetCharacter.gridTileLocation.structure.IsTilePartOfARoom(targetCharacter.gridTileLocation, out var room);
                    //}
                }
                return false;
            }
        }
        return false;
    }
    #endregion
}