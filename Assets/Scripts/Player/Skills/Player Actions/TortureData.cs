using System.Collections;
using System.Collections.Generic;
using Traits;
using UnityEngine;
using UnityEngine.Assertions;
using Inner_Maps.Location_Structures;

public class TortureData : PlayerAction {
    public override PLAYER_SKILL_TYPE type => PLAYER_SKILL_TYPE.TORTURE;
    public override string name => "Torture";
    public override string description => $"Torture a Villager to afflict it with a random negative Trait and a random negative Status.";
    public override bool canBeCastOnBlessed => true;
    public TortureData() : base() {
        targetTypes = new SPELL_TARGET[] { SPELL_TARGET.ROOM, SPELL_TARGET.CHARACTER };
    }

    #region Overrides
    public override void ActivateAbility(StructureRoom room) {
        if (room is PrisonCell tortureRoom) {
            tortureRoom.BeginTorture();
        }
        base.ActivateAbility(room);
    }
    public override void ActivateAbility(IPointOfInterest targetPOI) {
        if (targetPOI is Character targetCharacter) {
            if (targetCharacter.gridTileLocation != null && targetCharacter.gridTileLocation.structure.IsTilePartOfARoom(targetCharacter.gridTileLocation, out var room) && room is PrisonCell tortureRoom) {
                tortureRoom.BeginTorture(targetCharacter);
            }
        }
        base.ActivateAbility(targetPOI);
    }
    public override bool CanPerformAbilityTowards(StructureRoom room) {
        bool canPerform = base.CanPerformAbilityTowards(room);
        if (canPerform) {
            if (room is PrisonCell tortureRoom) {
                return tortureRoom.currentTortureTarget == null && tortureRoom.currentBrainwashTarget == null && tortureRoom.HasValidTortureTarget();
            }
            return false;
        }
        return false;
    }
    public override bool CanPerformAbilityTowards(Character targetCharacter) {
        bool canPerform = base.CanPerformAbilityTowards(targetCharacter);
        if (canPerform) {
            if (targetCharacter.gridTileLocation != null && 
                targetCharacter.gridTileLocation.structure.IsTilePartOfARoom(targetCharacter.gridTileLocation, out var room) && room is PrisonCell tortureRoom) {
                return tortureRoom.currentTortureTarget == null && tortureRoom.currentBrainwashTarget == null && tortureRoom.IsValidTortureTarget(targetCharacter);
            }
            return false;
        }
        return false;
    }
    public override bool IsValid(IPlayerActionTarget target) {
        bool isValid = base.IsValid(target);
        if (isValid) {
            if (target is Character targetCharacter) {
                if (targetCharacter.gridTileLocation != null && 
                    targetCharacter.gridTileLocation.structure.IsTilePartOfARoom(targetCharacter.gridTileLocation, out var room) && room is PrisonCell tortureRoom) {
                    return true; //tortureRoom.IsValidTortureTarget(targetCharacter);
                }
                return false;
            }
            return true;
        }
        return false;
    }
    #endregion
}