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
                if (!targetCharacter.isDead) {
                    if (targetCharacter.currentStructure is Kennel kennel && kennel.occupyingSummon == targetCharacter) {
                        return true;
                    }
                    if (targetCharacter.gridTileLocation != null && targetCharacter.currentStructure is TortureChambers tortureChambers && 
                        targetCharacter.currentStructure.IsTilePartOfARoom(targetCharacter.gridTileLocation, out var room) && room.parentStructure == tortureChambers) {
                        return true;
                    }    
                }
            } else if (target is DemonicStructure demonicStructure) {
                if (demonicStructure is Kennel kennel && kennel.occupyingSummon != null) {
                    return true;
                }
                if (demonicStructure is TortureChambers tortureChambers && tortureChambers.rooms.Length > 0 && tortureChambers.rooms[0] is PrisonCell prisonCell && 
                    prisonCell.HasValidOccupant()) {
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
            if (kennel.occupyingSummon == null) {
                return false;
            }
            if (!CanPerformAbilityTowards(kennel.occupyingSummon)) {
                return false;
            }
        } else if (targetStructure is TortureChambers tortureChambers) {
            if (tortureChambers.rooms.Length <= 0) {
                return false;
            }
            if (tortureChambers.rooms[0] is PrisonCell prisonCell && !prisonCell.charactersInRoom.Any(CanPerformAbilityTowards)) {
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
            if (kennel.occupyingSummon == null) {
                reasons += "No Character at Kennel.";
            } else {
                reasons += GetReasonsWhyCannotPerformAbilityTowards(kennel.occupyingSummon);
            }
        } else if (p_targetStructure is TortureChambers tortureChambers) {
            if (tortureChambers.rooms.Length <= 0) {
                reasons += "Prison has no Prison Cell.";
            } else if (tortureChambers.rooms[0] is PrisonCell prisonCell && !prisonCell.charactersInRoom.Any(CanPerformAbilityTowards)) {
                reasons += "Cannot let go of anybody. Cannot let go of characters being Brainwashed or Tortured";
            }
        }
        return reasons;
    }
    public override void ActivateAbility(LocationStructure targetStructure) {
        if (targetStructure is Kennel kennel) {
            Assert.IsNotNull(kennel.occupyingSummon);
            ActivateAbility(kennel.occupyingSummon);    
        } else if (targetStructure is TortureChambers tortureChambers) {
            Assert.IsFalse(tortureChambers.rooms.Length <= 0);
            PrisonCell prisonCell = tortureChambers.rooms[0] as PrisonCell;
            Assert.IsNotNull(prisonCell);
            Character firstCharacterThatCanLetGoOf = prisonCell.charactersInRoom.FirstOrDefault(CanPerformAbilityTowards);
            Assert.IsNotNull(firstCharacterThatCanLetGoOf);
            ActivateAbility(firstCharacterThatCanLetGoOf);
        }
    }
    public override void ActivateAbility(IPointOfInterest targetPOI) {
        if (targetPOI is Character targetCharacter) {
            LocationStructure letGoFrom = targetCharacter.currentStructure;
            //Make character dazed (if not summon) and teleport him/her on a random spot outside
            List<LocationGridTile> allTilesOutside = RuinarchListPool<LocationGridTile>.Claim();
            List<LocationGridTile> passableTilesOutside = RuinarchListPool<LocationGridTile>.Claim();
            for (int i = 0; i < targetCharacter.currentStructure.tiles.Count; i++) {
                LocationGridTile tileInStructure = targetCharacter.currentStructure.tiles.ElementAt(i);
                for (int j = 0; j < tileInStructure.neighbourList.Count; j++) {
                    LocationGridTile neighbour = tileInStructure.neighbourList[j];
                    if (neighbour.structure is Wilderness && !allTilesOutside.Contains(neighbour)) {
                        allTilesOutside.Add(neighbour);
                        if (neighbour.IsPassable()) {
                            passableTilesOutside.Add(neighbour);
                        }
                    }
                }
            }
            Assert.IsTrue(allTilesOutside.Count > 0);
            var targetTile = CollectionUtilities.GetRandomElement(passableTilesOutside.Count > 0 ? passableTilesOutside : allTilesOutside);
            if (targetCharacter is Summon == false) {
                targetCharacter.traitContainer.AddTrait(targetCharacter, "Dazed");    
            }
            CharacterManager.Instance.Teleport(targetCharacter, targetTile);
            GameManager.Instance.CreateParticleEffectAt(targetTile, PARTICLE_EFFECT.Minion_Dissipate);
            targetPOI.traitContainer.RemoveRestrainAndImprison(targetPOI);
            RuinarchListPool<LocationGridTile>.Release(allTilesOutside);

            Log log = GameManager.CreateNewLog(GameManager.Instance.Today(), "InterventionAbility", "Let Go", "activated");
            log.AddToFillers(targetCharacter, targetCharacter.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
            log.AddToFillers(letGoFrom, letGoFrom?.name, LOG_IDENTIFIER.LANDMARK_1);
            log.AddLogToDatabase();
            PlayerManager.Instance.player.ShowNotificationFromPlayer(log, true);
            
            base.ActivateAbility(targetPOI);    
        }
    }
}
