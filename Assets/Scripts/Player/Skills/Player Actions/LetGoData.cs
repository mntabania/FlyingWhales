using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Inner_Maps;
using Inner_Maps.Location_Structures;
using UnityEngine.Assertions;
using UtilityScripts;

public class LetGoData : PlayerAction {

    public override bool canBeCastOnBlessed => true;
    public override PLAYER_SKILL_TYPE type => PLAYER_SKILL_TYPE.LET_GO;
    public override string name => "Let It Go";
    public override string description => "This Ability will move the target out of its cold and bothersome Prison or Kennel.";
    public LetGoData() : base() {
        targetTypes = new SPELL_TARGET[] { SPELL_TARGET.CHARACTER };
    }
    public override bool IsValid(IPlayerActionTarget target) {
        bool isValid = base.IsValid(target);
        if (isValid) {
            if (target is Character targetCharacter) {
                if (targetCharacter.currentStructure is Kennel) { //kennel && kennel.occupyingSummon == targetCharacter
                    return true;
                }
                if (targetCharacter.gridTileLocation != null && targetCharacter.currentStructure is TortureChambers tortureChambers && 
                    targetCharacter.currentStructure.IsTilePartOfARoom(targetCharacter.gridTileLocation, out var room) && room.parentStructure == tortureChambers) {
                    return true;
                }
            } else if (target is DemonicStructure demonicStructure) {
                if (demonicStructure is Kennel kennel && kennel.HasCharactersInside()) {
                    return true;
                }
                if (demonicStructure is TortureChambers tortureChambers && tortureChambers.rooms.ElementAtOrDefault(0) is PrisonCell prisonCell && 
                    prisonCell.HasOccupants()) {
                    return true;
                }
            }
            return false;
        }
        return false;
    }
    public override bool CanPerformAbilityTowards(Character targetCharacter) {
        if (targetCharacter.traitContainer.HasTrait("Being Drained")) {
            return false;
        }
        if (targetCharacter.interruptComponent.isInterrupted) {
            if (targetCharacter.interruptComponent.currentInterrupt.interrupt.type == INTERRUPT.Being_Brainwashed ||
                targetCharacter.interruptComponent.currentInterrupt.interrupt.type == INTERRUPT.Being_Tortured) {
                //do not allow characters being tortured or brainwashed to be seized
                return false;
            }
        }
        return base.CanPerformAbilityTowards(targetCharacter);
    }
    public override bool CanPerformAbilityTowards(LocationStructure targetStructure) {
        if (targetStructure is Kennel kennel) {
            if (!kennel.HasCharactersInside()) {
                return false;
            }
            if (!HasCharacterInKennelThatIsValid(kennel)) {
                return false;
            }
        } else if (targetStructure is TortureChambers tortureChambers) {
            if (tortureChambers.rooms.Length <= 0) {
                return false;
            }
            if (tortureChambers.rooms[0] is PrisonCell prisonCell && !HasCharacterInPrisonCellThatIsValid(prisonCell) /*!prisonCell.charactersInRoom.Any(CanPerformAbilityTowards)*/) {
                return false;
            }
        }
        return base.CanPerformAbilityTowards(targetStructure);
    }
    public override string GetReasonsWhyCannotPerformAbilityTowards(Character targetCharacter) {
        string reasons = base.GetReasonsWhyCannotPerformAbilityTowards(targetCharacter);
        if (targetCharacter.traitContainer.HasTrait("Being Drained")) {
            reasons += "Characters being drained cannot be Let Go.";
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
            if (!kennel.HasCharactersInside()) {
                reasons += "No Character at Kennel.";
            } else {
                reasons += "Cannot let go of anybody. Cannot let go of characters being Brainwashed or Tortured";
            }
        } else if (p_targetStructure is TortureChambers tortureChambers) {
            if (tortureChambers.rooms.Length <= 0) {
                reasons += "Prison has no Prison Cell.";
            } else if (tortureChambers.rooms[0] is PrisonCell prisonCell && !HasCharacterInPrisonCellThatIsValid(prisonCell) /*!prisonCell.charactersInRoom.Any(CanPerformAbilityTowards)*/) {
                reasons += "Cannot let go of anybody. Cannot let go of characters being Brainwashed or Tortured";
            }
        }
        return reasons;
    }
    public override void ActivateAbility(LocationStructure targetStructure) {
        if (targetStructure is Kennel kennel) {
            List<Character> charactersInKennel = RuinarchListPool<Character>.Claim();
            charactersInKennel.AddRange(kennel.charactersHere);
            for (int i = 0; i < charactersInKennel.Count; i++) {
                Character character = charactersInKennel[i];
                if (CanPerformAbilityTowards(character)) {
                    LetGo(character);
                }
            }
            RuinarchListPool<Character>.Release(charactersInKennel);
            // Assert.IsNotNull(kennel.occupyingSummon);
            // ActivateAbility(kennel.occupyingSummon);    
            base.ActivateAbility(targetStructure);
        } else if (targetStructure is TortureChambers tortureChambers) {
            Assert.IsFalse(tortureChambers.rooms.Length <= 0);
            PrisonCell prisonCell = tortureChambers.rooms[0] as PrisonCell;
            Assert.IsNotNull(prisonCell);
            List<Character> charactersInRoom = RuinarchListPool<Character>.Claim();
            prisonCell.PopulateCharactersInRoom(charactersInRoom);
            for (int i = 0; i < charactersInRoom.Count; i++) {
                Character c = charactersInRoom[i];
                if (CanPerformAbilityTowards(c)) {
                    LetGo(c);
                }
            }
            RuinarchListPool<Character>.Release(charactersInRoom);
            base.ActivateAbility(targetStructure);
        }
    }
    public override void ActivateAbility(IPointOfInterest targetPOI) {
        if (targetPOI is Character targetCharacter) {
            LetGo(targetCharacter);
            base.ActivateAbility(targetPOI);    
        }
    }
    private void LetGo(Character targetCharacter) {
        LocationStructure letGoFrom = targetCharacter.currentStructure;
        targetCharacter.movementComponent.LetGo(true);

        Log log = GameManager.CreateNewLog(GameManager.Instance.Today(), "InterventionAbility", "Let Go", "activated");
        log.AddToFillers(targetCharacter, targetCharacter.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
        log.AddToFillers(letGoFrom, letGoFrom?.name, LOG_IDENTIFIER.LANDMARK_1);
        log.AddLogToDatabase();
        PlayerManager.Instance.player.ShowNotificationFromPlayer(log, true);
    }
    private bool HasCharacterInPrisonCellThatIsValid(PrisonCell prisonCell) {
        return GetFirstCharacterInPrisonCellThatIsValid(prisonCell) != null;
    }
    private Character GetFirstCharacterInPrisonCellThatIsValid(PrisonCell prisonCell) {
        for (int i = 0; i < prisonCell.parentStructure.charactersHere.Count; i++) {
            Character character = prisonCell.parentStructure.charactersHere[i];
            if (character.gridTileLocation != null && prisonCell.parentStructure.IsTilePartOfARoom(character.gridTileLocation, out var room) && 
                room == prisonCell && CanPerformAbilityTowards(character)) {
                return character;
            }
        }
        return null;
    }
    private bool HasCharacterInKennelThatIsValid(Kennel kennel) {
        return GetFirstCharacterInKennelThatIsValid(kennel) != null;
    }
    private Character GetFirstCharacterInKennelThatIsValid(Kennel kennel) {
        for (int i = 0; i < kennel.charactersHere.Count; i++) {
            Character c = kennel.charactersHere[i];
            if (CanPerformAbilityTowards(c)) {
                return c;
            }
        }
        return null;
    }
}
