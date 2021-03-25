﻿using Inner_Maps.Location_Structures;

public class BrainwashData : PlayerAction {
    public override PLAYER_SKILL_TYPE type => PLAYER_SKILL_TYPE.BRAINWASH;
    public override string name => "Brainwash";
    public override string description => $"This Ability will attempt to turn the target into a Cultist. The target's Mood as well as some of its Traits will affect the success rate. Leaders are also much more difficult to brainwash.";
    
    public BrainwashData() : base() {
        targetTypes = new SPELL_TARGET[] { SPELL_TARGET.ROOM, SPELL_TARGET.CHARACTER };
    }

    #region Overrides
    public override void ActivateAbility(StructureRoom room) {
        if (room is PrisonCell defilerRoom) {
            defilerRoom.StartBrainwash();
        }
        base.ActivateAbility(room);
    }
    public override void ActivateAbility(IPointOfInterest targetPOI) {
        if (targetPOI is Character targetCharacter) {
            if (targetCharacter.gridTileLocation != null && 
                targetCharacter.gridTileLocation.structure.IsTilePartOfARoom(targetCharacter.gridTileLocation, out var room) && room is PrisonCell defilerRoom) {
                defilerRoom.StartBrainwash(targetCharacter);
            }
        }
        base.ActivateAbility(targetPOI);
    }
    public override bool CanPerformAbilityTowards(StructureRoom room) {
        bool canPerform = base.CanPerformAbilityTowards(room);
        if (canPerform) {
            if (room is PrisonCell defilerRoom) {
                return defilerRoom.currentBrainwashTarget == null && defilerRoom.currentTortureTarget == null && defilerRoom.HasValidBrainwashTarget();
            }
            return false;
        }
        return false;
    }
    public override bool CanPerformAbilityTowards(Character targetCharacter) {
        bool canPerform = base.CanPerformAbilityTowards(targetCharacter);
        if (canPerform) {
            if (targetCharacter.gridTileLocation != null && 
                targetCharacter.gridTileLocation.structure.IsTilePartOfARoom(targetCharacter.gridTileLocation, out var room) && room is PrisonCell defilerRoom) {
                return defilerRoom.currentBrainwashTarget == null && defilerRoom.currentTortureTarget == null && defilerRoom.IsValidBrainwashTarget(targetCharacter);
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
                    targetCharacter.gridTileLocation.structure.IsTilePartOfARoom(targetCharacter.gridTileLocation, out var room) && room is PrisonCell defilerRoom) {
                    return true;
                }
                return false;
            }
            return true;
        }
        return false;
    }
    #endregion
}