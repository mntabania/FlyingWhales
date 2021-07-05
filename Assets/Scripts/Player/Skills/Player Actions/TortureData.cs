using System.Collections;
using System.Collections.Generic;
using Traits;
using UnityEngine;
using UnityEngine.Assertions;
using Inner_Maps.Location_Structures;

public class TortureData : PlayerAction {
    public override PLAYER_SKILL_TYPE type => PLAYER_SKILL_TYPE.TORTURE;
    public override string name => "Torture";
    public override string description => $"This Ability will commit unspeakable suffering to the target. After the deed, the Villager will gain a negative Trait.";
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
    public override void ActivateAbility(LocationStructure targetStructure) {
        if (targetStructure is TortureChambers tortureChambers && tortureChambers.rooms.Length > 0 && tortureChambers.rooms[0] is PrisonCell prisonCell) {
            prisonCell.BeginTorture();
            base.ActivateAbility(targetStructure);    
        }
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
    public override bool CanPerformAbilityTowards(LocationStructure targetStructure) {
        bool canPerform = base.CanPerformAbilityTowards(targetStructure);
        if (canPerform) {
            if (targetStructure is TortureChambers tortureChambers) {
                if (tortureChambers.rooms.Length > 0 && tortureChambers.rooms[0] is PrisonCell prisonCell) {
                    if (prisonCell.currentBrainwashTarget == null && prisonCell.currentTortureTarget == null && prisonCell.HasValidTortureTarget()) {
                        return true;
                    }
                }
                return false;
            }
            return true;
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
    public override string GetReasonsWhyCannotPerformAbilityTowards(LocationStructure p_targetStructure) {
        string reasons = base.GetReasonsWhyCannotPerformAbilityTowards(p_targetStructure);
        if (p_targetStructure is TortureChambers tortureChambers) {
            if (tortureChambers.rooms.Length > 0 && tortureChambers.rooms[0] is PrisonCell prisonCell) {
                if (prisonCell.currentBrainwashTarget != null) {
                    reasons += "A character is currently being Brainwashed,";
                } else if (prisonCell.currentTortureTarget != null) {
                    reasons += "A character is currently being Tortured,";
                } else if (!prisonCell.HasValidTortureTarget()) {
                    reasons += "Could not find a valid Torture target,";
                }
            }
        }
        return reasons;
    }
    public override bool IsValid(IPlayerActionTarget target) {
        bool isValid = base.IsValid(target);
        if (isValid) {
            if (target is Character targetCharacter) {
                if (targetCharacter.gridTileLocation != null && 
                    targetCharacter.gridTileLocation.structure.IsTilePartOfARoom(targetCharacter.gridTileLocation, out var room) && room is PrisonCell prisonCell) {
                    return true; //tortureRoom.IsValidTortureTarget(targetCharacter);
                }
                return false;
            } else if (target is TortureChambers tortureChambers) {
                if (tortureChambers.rooms.Length > 0 && tortureChambers.rooms[0] is PrisonCell prisonCell) {
                    if (prisonCell.HasValidOccupant()) {
                        return true;
                    }
                }
                return false;  
            }
            return true;
        }
        return false;
    }
    #endregion
}