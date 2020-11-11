using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Inner_Maps;
using Inner_Maps.Location_Structures;
using Locations.Settlements;
using Traits;

public class TrollBehaviour : CharacterBehaviourComponent {
	public TrollBehaviour() {
		priority = 8;
		// attributes = new[] { BEHAVIOUR_COMPONENT_ATTRIBUTE.WITHIN_HOME_SETTLEMENT_ONLY };
	}
	public override bool TryDoBehaviour(Character character, ref string log, out JobQueueItem producedJob) {
        producedJob = null;
        log += $"\n-{character.name} is a troll";
        if (character.isAtHomeStructure || character.IsInHomeSettlement()) {
            log += $"\n-10% chance to create a cooking cauldron if it does not have one";
            int roll = UnityEngine.Random.Range(0, 100);
            log += $"\n-Roll: " + roll;
            if (roll < 10) {
                bool hasCookingCauldron = false;
                if(character.homeSettlement != null) {
                    hasCookingCauldron = character.homeSettlement.HasTileObjectOfType(TILE_OBJECT_TYPE.TROLL_CAULDRON);
                } else if (character.homeStructure != null) {
                    hasCookingCauldron = character.homeStructure.HasTileObjectOfType(TILE_OBJECT_TYPE.TROLL_CAULDRON);
                }
                if (!hasCookingCauldron) {
                    log += $"\n-No cooking cauldron, will build one";
                    return character.jobComponent.TriggerBuildTrollCauldronJob(out producedJob);
                }
            }
        }

        if(character.homeStructure != null) {
            if (!character.isAtHomeStructure && !character.jobQueue.HasJob(JOB_TYPE.CAPTURE_CHARACTER)) {
                if (character.marker) {
                    Character chosenCharacter = null;
                    Character characterThatCanBeKidnapped = null;
                    for (int i = 0; i < character.marker.inVisionCharacters.Count; i++) {
                        Character potentialCharacter = character.marker.inVisionCharacters[i];
                        if (potentialCharacter.isNormalCharacter) {
                            if (!potentialCharacter.limiterComponent.canPerform || !potentialCharacter.limiterComponent.canMove) {
                                if (characterThatCanBeKidnapped == null) {
                                    characterThatCanBeKidnapped = potentialCharacter;
                                }
                                if (potentialCharacter.traitContainer.HasTrait("Unconscious")) {
                                    Unconscious unconsciousTrait = potentialCharacter.traitContainer.GetTraitOrStatus<Unconscious>("Unconscious");
                                    if (unconsciousTrait.IsResponsibleForTrait(character)) {
                                        chosenCharacter = potentialCharacter;
                                        break;
                                    }
                                }
                            }
                        }
                    }
                    if (chosenCharacter == null) {
                        chosenCharacter = characterThatCanBeKidnapped;
                    }
                    if (chosenCharacter != null) {
                        if (character.jobComponent.TryTriggerCaptureCharacter(chosenCharacter, character.homeStructure, out producedJob, true)){
                            return true;
                        }
                    }
                }
            }
        }

        TIME_IN_WORDS timeInWords = GameManager.GetCurrentTimeInWordsOfTick(null);
        if (timeInWords == TIME_IN_WORDS.EARLY_NIGHT || timeInWords == TIME_IN_WORDS.LATE_NIGHT /*|| timeInWords == TIME_IN_WORDS.AFTER_MIDNIGHT*/) {
            log += $"\n-Night time, will try to visit adjacent hextiles";
            if (character.isAtHomeStructure || character.IsInHomeSettlement()) {
                HexTile adjacentHextile = null;
                if(character.homeSettlement != null) {
                    adjacentHextile = character.homeSettlement.GetAPlainAdjacentHextile();
                } else {
                    adjacentHextile = character.gridTileLocation.collectionOwner.partOfHextile.hexTileOwner.GetRandomAdjacentNoSettlementHextileWithinRegion();
                }
                if(adjacentHextile != null) {
                    log += $"\n-Target hex: " + adjacentHextile.name;
                    return character.jobComponent.CreateGoToJob(adjacentHextile.GetRandomTile(), out producedJob);

                }
            } else {
                log += $"\n-Already outside home, 30% chance to roam, 70% chance to go to another hex adjacent to home";
                int roll = UnityEngine.Random.Range(0, 100);
                log += $"\n-Roll: " + roll;
                if(roll < 30) {
                    return character.jobComponent.TriggerRoamAroundStructure(out producedJob);
                } else {
                    HexTile adjacentHextile = null;
                    if (character.homeSettlement != null) {
                        adjacentHextile = character.homeSettlement.GetAPlainAdjacentHextile();
                    } else {
                        adjacentHextile = character.gridTileLocation.collectionOwner.partOfHextile.hexTileOwner.GetRandomAdjacentNoSettlementHextileWithinRegion();
                    }
                    if (adjacentHextile != null) {
                        log += $"\n-Target hex: " + adjacentHextile.name;
                        return character.jobComponent.CreateGoToJob(adjacentHextile.GetRandomTile(), out producedJob);
                    } else {
                        return character.jobComponent.TriggerRoamAroundStructure(out producedJob);
                    }
                }
            }
        }
        if (character.isAtHomeStructure || character.IsInHomeSettlement()) {
            log += $"\n-Already in home, 20% chance to cook a dead character if there is one";
            int roll = UnityEngine.Random.Range(0, 100);
            log += $"\n-Roll: {roll}";
            if (roll < 20) {
                Character chosenCharacter = null;
                TrollCauldron cauldron = null;
                if (character.homeSettlement != null) {
                    chosenCharacter = character.homeSettlement.GetRandomCharacterThatMeetCriteria(x => x.isNormalCharacter && !x.isBeingSeized && x.isBeingCarriedBy == null && x.isDead && !x.HasJobTargetingThis(JOB_TYPE.PRODUCE_FOOD));
                    cauldron = character.homeStructure.GetTileObjectOfType<TrollCauldron>(TILE_OBJECT_TYPE.TROLL_CAULDRON);
                } else if (character.homeStructure != null) {
                    chosenCharacter = character.homeStructure.GetRandomCharacterThatMeetCriteria(x => x.isNormalCharacter && !x.isBeingSeized && x.isBeingCarriedBy == null && x.isDead && !x.HasJobTargetingThis(JOB_TYPE.PRODUCE_FOOD));
                    cauldron = character.homeStructure.GetTileObjectOfType<TrollCauldron>(TILE_OBJECT_TYPE.TROLL_CAULDRON);
                }
                if (chosenCharacter != null && cauldron != null) {
                    log += $"\n-Chosen character: " + chosenCharacter.name;
                    if (character.jobComponent.TriggerCookJob(chosenCharacter, cauldron, out producedJob)) {
                        return true;
                    }
                }
            }
            log += $"\n-Already in home, 10% chance to butcher a character if there is one";
            roll = UnityEngine.Random.Range(0, 100);
            log += $"\n-Roll: {roll}";
            if (roll < 50) {
                Character chosenCharacter = null;
                if (character.homeSettlement != null) {
                    chosenCharacter = character.homeSettlement.GetRandomCharacterThatMeetCriteria(x => x.isNormalCharacter && !x.isBeingSeized && x.isBeingCarriedBy == null && !x.isDead && !x.HasJobTargetingThis(JOB_TYPE.PRODUCE_FOOD) && x.traitContainer.HasTrait("Restrained"));
                } else if (character.homeStructure != null) {
                    chosenCharacter = character.homeStructure.GetRandomCharacterThatMeetCriteria(x => x.isNormalCharacter && !x.isBeingSeized && x.isBeingCarriedBy == null && !x.isDead && !x.HasJobTargetingThis(JOB_TYPE.PRODUCE_FOOD) && x.traitContainer.HasTrait("Restrained"));
                }
                if (chosenCharacter != null) {
                    log += $"\n-Chosen character: " + chosenCharacter.name;
                    if (character.jobComponent.CreateButcherJob(chosenCharacter, out producedJob)) {
                        return true;
                    }
                }
            }
            return character.jobComponent.TriggerRoamAroundStructure(out producedJob);
        } else {
            log += $"\n-Not in home, go to home";
            return character.jobComponent.PlanIdleReturnHome(out producedJob);
        }
        //return true;
	}
}
