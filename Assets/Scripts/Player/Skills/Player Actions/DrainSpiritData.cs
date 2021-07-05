using System.Collections.Generic;
using System.Linq;
using Inner_Maps.Location_Structures;
using UnityEngine.Assertions;
using Inner_Maps;
public class DrainSpiritData : PlayerAction {
    public override PLAYER_SKILL_TYPE type => PLAYER_SKILL_TYPE.DRAIN_SPIRIT;
    public override string name => "Drain Spirit";
    public override string description => "This Ability slowly kills the target to produce Chaos Orbs.";
    public DrainSpiritData() : base() {
        targetTypes = new SPELL_TARGET[] { SPELL_TARGET.CHARACTER };
    }
    public override void ActivateAbility(IPointOfInterest targetPOI) {
        if (targetPOI is Character targetCharacter) {
            targetCharacter.traitContainer.AddTrait(targetCharacter, "Being Drained");
            base.ActivateAbility(targetPOI);    
        }
    }
    public override void ActivateAbility(LocationStructure targetStructure) {
        if (targetStructure is Kennel kennel) {
            ActivateAbility(kennel.occupyingSummon);
        } else if (targetStructure is TortureChambers tortureChambers) {
            PrisonCell prisonCell = tortureChambers.rooms[0] as PrisonCell;
            Assert.IsNotNull(prisonCell);
            //Character chosenCharacter = prisonCell.charactersInRoom.FirstOrDefault(c =>  CanPerformAbilityTowards(c) && IsValid(c));
            Character chosenCharacter = GetFirstCharacterInPrisonCellThatIsValid(prisonCell);
            Assert.IsNotNull(chosenCharacter);
            ActivateAbility(chosenCharacter);
        }
    }
    public override bool IsValid(IPlayerActionTarget target) {
        bool isValid = base.IsValid(target);
        if (isValid) {
            if (target is Character targetCharacter) {
                if (targetCharacter.isDead) {
                    return false;
                }
                if (targetCharacter is Summon) {
                    if (targetCharacter.currentStructure is Kennel kennel && kennel.occupyingSummon == targetCharacter) {
                        return true;
                    }
                } else {
                    if (targetCharacter.gridTileLocation != null && targetCharacter.currentStructure is TortureChambers tortureChambers && 
                        targetCharacter.currentStructure.IsTilePartOfARoom(targetCharacter.gridTileLocation, out var room) && room.parentStructure == tortureChambers) {
                        return true;
                    }
                }
            } else if (target is DemonicStructure demonicStructure) {
                if (demonicStructure is Kennel kennel && kennel.occupyingSummon != null) {
                    return true;
                } else if (demonicStructure is TortureChambers tortureChambers && tortureChambers.rooms.Length > 0 && tortureChambers.rooms[0] is PrisonCell prisonCell && 
                           prisonCell.HasValidOccupant()) {
                    return true;
                }
            }
        }
        return false;
    }
    public override bool CanPerformAbilityTowards(Character targetCharacter) {
        bool canPerformAbility = base.CanPerformAbilityTowards(targetCharacter);
        if (canPerformAbility) {
            if (targetCharacter.traitContainer.HasTrait("Being Drained")) {
                return false;
            }
            if (targetCharacter.interruptComponent.isInterrupted) {
                if (targetCharacter.interruptComponent.currentInterrupt.interrupt.type == INTERRUPT.Being_Brainwashed ||
                    targetCharacter.interruptComponent.currentInterrupt.interrupt.type == INTERRUPT.Being_Tortured) {
                    //do not allow characters being tortured or brainwashed to be drained
                    return false;
                }
            }
            // if (targetCharacter.gridTileLocation != null && targetCharacter.gridTileLocation.structure.IsTilePartOfARoom(targetCharacter.gridTileLocation, out var room) && room is PrisonCell prisonCell &&
            //     prisonCell.charactersInRoom.Any(c => c.traitContainer.HasTrait("Being Drained"))) {
            //     //a character is already being drained inside the room
            //     return false;
            // }
            return true;
        }
        return false;
    }
    public override bool CanPerformAbilityTowards(LocationStructure targetStructure) {
        bool canPerformAbility = base.CanPerformAbilityTowards(targetStructure);
        if (canPerformAbility) {
            if (targetStructure is Kennel kennel) {
                if (kennel.occupyingSummon != null && !CanPerformAbilityTowards(kennel.occupyingSummon)) {
                    return false;
                }
            } else if (targetStructure is TortureChambers tortureChambers) {
                if (tortureChambers.rooms.Length > 0 && tortureChambers.rooms[0] is PrisonCell prisonCell) {
                    if (!HasCharacterInPrisonCellThatIsValid(prisonCell)) {
                        return false;
                    }
                    //List<Character> charactersInRoom = prisonCell.charactersInRoom;
                    //if (!charactersInRoom.Any(c => CanPerformAbilityTowards(c) && IsValid(c))) { //charactersInRoom.Any(c => c.traitContainer.HasTrait("Being Drained")) || 
                    //    //if cannot drain any character in room or a character in the room is already being drained.
                    //    return false;    
                    //}
                }
            }
            return true;
        }
        return false;
    }
    public override string GetReasonsWhyCannotPerformAbilityTowards(Character targetCharacter) {
        string reasons = base.GetReasonsWhyCannotPerformAbilityTowards(targetCharacter);
        if (targetCharacter.traitContainer.HasTrait("Being Drained")) {
            reasons += "Character is already being drained.";
        }
        if (targetCharacter.interruptComponent.isInterrupted) {
            if (targetCharacter.interruptComponent.currentInterrupt.interrupt.type == INTERRUPT.Being_Brainwashed) {
                reasons += "Character is currently being Brainwashed.";
            }else if (targetCharacter.interruptComponent.currentInterrupt.interrupt.type == INTERRUPT.Being_Tortured) {
                reasons += "Character is currently being Tortured.";
            }
        }
        return reasons;
    }
    public override string GetReasonsWhyCannotPerformAbilityTowards(LocationStructure p_targetStructure) {
        string reasons = base.GetReasonsWhyCannotPerformAbilityTowards(p_targetStructure);
        if (p_targetStructure is Kennel kennel) {
            if (kennel.occupyingSummon != null && !CanPerformAbilityTowards(kennel.occupyingSummon)) {
                reasons += GetReasonsWhyCannotPerformAbilityTowards(kennel.occupyingSummon);
            }
        } else if (p_targetStructure is TortureChambers tortureChambers) {
            if (tortureChambers.rooms.Length > 0 && tortureChambers.rooms[0] is PrisonCell prisonCell) {
                if (!HasCharacterInPrisonCellThatIsValid(prisonCell)) {
                    reasons += "Cannot find valid Drain target. Cannot drain characters that are currently being Brainwashed or Tortured.";
                }
                //List<Character> charactersInRoom = prisonCell.charactersInRoom;
                //if (!charactersInRoom.Any(c => CanPerformAbilityTowards(c) && IsValid(c))) {
                //    reasons += "Cannot find valid Drain target. Cannot drain characters that are currently being Brainwashed or Tortured.";
                //}


                // else if (charactersInRoom.Any(c => c.traitContainer.HasTrait("Being Drained"))) {
                //     reasons += "A character is already being drained.";
                // }
            }
        }
        return reasons;
    }
    private bool HasCharacterInPrisonCellThatIsValid(PrisonCell prisonCell) {
        return GetFirstCharacterInPrisonCellThatIsValid(prisonCell) != null;
    }
    private Character GetFirstCharacterInPrisonCellThatIsValid(PrisonCell prisonCell) {
        for (int i = 0; i < prisonCell.tilesInRoom.Count; i++) {
            LocationGridTile t = prisonCell.tilesInRoom[i];
            for (int j = 0; j < t.charactersHere.Count; j++) {
                Character c = t.charactersHere[j];
                if (CanPerformAbilityTowards(c) && IsValid(c)) {
                    return c;
                }
            }
        }
        return null;
    }
}
