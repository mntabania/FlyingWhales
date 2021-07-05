using Traits;
using UtilityScripts;

public class TrollBehaviour : BaseMonsterBehaviour {
	public TrollBehaviour() {
		priority = 8;
	}
	protected override bool WildBehaviour(Character character, ref string log, out JobQueueItem producedJob) {
        producedJob = null;
#if DEBUG_LOG
        log += $"\n-{character.name} is a troll";
#endif
        if (character.IsAtHome()) {
            int roll = UnityEngine.Random.Range(0, 100);
#if DEBUG_LOG
            log += $"\n-10% chance to create a cooking cauldron if it does not have one";
            log += $"\n-Roll: " + roll;
#endif
            if (roll < 10) {
                bool hasCookingCauldron = false;
                if(character.homeSettlement != null) {
                    hasCookingCauldron = character.homeSettlement.HasTileObjectOfType(TILE_OBJECT_TYPE.TROLL_CAULDRON);
                } else if (character.homeStructure != null) {
                    hasCookingCauldron = character.homeStructure.HasTileObjectOfType(TILE_OBJECT_TYPE.TROLL_CAULDRON);
                }
                if (!hasCookingCauldron) {
#if DEBUG_LOG
                    log += $"\n-No cooking cauldron, will build one";
#endif
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

        TIME_IN_WORDS timeInWords = GameManager.Instance.GetCurrentTimeInWordsOfTick(null);
        if (timeInWords == TIME_IN_WORDS.EARLY_NIGHT || timeInWords == TIME_IN_WORDS.LATE_NIGHT /*|| timeInWords == TIME_IN_WORDS.AFTER_MIDNIGHT*/) {
#if DEBUG_LOG
            log += $"\n-Night time, will try to visit adjacent hextiles";
#endif
            if (character.isAtHomeStructure || character.IsInHomeSettlement()) {
                Area adjacentArea = null;
                if(character.homeSettlement != null) {
                    adjacentArea = character.homeSettlement.GetAPlainAdjacentArea();
                } else {
                    adjacentArea = character.areaLocation.neighbourComponent.GetRandomAdjacentNoSettlementHextileWithinRegion();
                }
                if(adjacentArea != null) {
#if DEBUG_LOG
                    log += $"\n-Target hex: " + adjacentArea.name;
#endif
                    return character.jobComponent.CreateGoToSpecificTileJob(adjacentArea.gridTileComponent.GetRandomPassableTile(), out producedJob);

                }
            } else {
                int roll = UnityEngine.Random.Range(0, 100);
#if DEBUG_LOG
                log += $"\n-Already outside home, 30% chance to roam, 70% chance to go to another hex adjacent to home";
                log += $"\n-Roll: " + roll;
#endif
                if (roll < 30) {
                    return character.jobComponent.TriggerRoamAroundStructure(out producedJob);
                } else {
                    Area adjacentArea = null;
                    if (character.homeSettlement != null) {
                        adjacentArea = character.homeSettlement.GetAPlainAdjacentArea();
                    } else {
                        adjacentArea = character.areaLocation.neighbourComponent.GetRandomAdjacentNoSettlementHextileWithinRegion();
                    }
                    if (adjacentArea != null) {
#if DEBUG_LOG
                        log += $"\n-Target hex: " + adjacentArea.name;
#endif
                        return character.jobComponent.CreateGoToSpecificTileJob(adjacentArea.gridTileComponent.GetRandomPassableTile(), out producedJob);
                    } else {
                        return character.jobComponent.TriggerRoamAroundStructure(out producedJob);
                    }
                }
            }
        }
        if (character.IsAtHome()) {
#if DEBUG_LOG
            log += $"\n-Already in home, 25% chance to eat a meat pile if there is one";
#endif
            if (GameUtilities.RollChance(25)) {
                FoodPile meat = null;
                if (character.homeSettlement != null) {
                    meat = character.homeSettlement.GetFirstTileObjectOfType<FoodPile>(TILE_OBJECT_TYPE.HUMAN_MEAT, TILE_OBJECT_TYPE.ELF_MEAT, TILE_OBJECT_TYPE.ANIMAL_MEAT, TILE_OBJECT_TYPE.RAT_MEAT);
                } else if (character.homeStructure != null) {
                    meat = character.homeStructure.GetFirstTileObjectOfType<FoodPile>(TILE_OBJECT_TYPE.HUMAN_MEAT, TILE_OBJECT_TYPE.ELF_MEAT, TILE_OBJECT_TYPE.ANIMAL_MEAT, TILE_OBJECT_TYPE.RAT_MEAT);
                }
                if (meat != null) {
                    if (character.jobComponent.CreateFullnessRecoveryOnSight(meat, false, out producedJob)) {
                        return true;
                    }
                }
            }
            int roll = UnityEngine.Random.Range(0, 100);
#if DEBUG_LOG
            log += $"\n-Already in home, 35% chance to cook a character if there is one";
            log += $"\n-Roll: {roll}";
#endif
            if (roll < 35) {
                Character chosenCharacter = null;
                TrollCauldron cauldron = null;
                if (character.homeSettlement != null) {
                    chosenCharacter = character.homeSettlement.GetRandomCharacterThatIsVillagerAndNotSeizedOrCarriedAndNotTargetedByProduceFoodAndIsRestrainedAndNot(character);
                    cauldron = character.homeSettlement.GetFirstTileObjectOfType<TrollCauldron>(TILE_OBJECT_TYPE.TROLL_CAULDRON);
                } else if (character.homeStructure != null) {
                    chosenCharacter = character.homeStructure.GetRandomCharacterThatIsVillagerAndNotSeizedOrCarriedAndNotTargetedByProduceFoodAndIsRestrainedAndNot(character);
                    cauldron = character.homeStructure.GetFirstTileObjectOfType<TrollCauldron>(TILE_OBJECT_TYPE.TROLL_CAULDRON);
                }
                if (chosenCharacter != null && cauldron != null) {
#if DEBUG_LOG
                    log += $"\n-Chosen character: " + chosenCharacter.name;
#endif
                    if (character.jobComponent.TriggerCookJob(chosenCharacter, cauldron, out producedJob)) {
                        return true;
                    }
                }
            }
            roll = UnityEngine.Random.Range(0, 100);
#if DEBUG_LOG
            log += $"\n-Already in home, 10% chance to butcher a character if there is one";
            log += $"\n-Roll: {roll}";
#endif
            if (roll < 10) {
                Character chosenCharacter = null;
                if (character.homeSettlement != null) {
                    chosenCharacter = character.homeSettlement.GetRandomCharacterThatIsAliveVillagerAndNotSeizedOrCarriedAndNotTargetedByProduceFoodAndIsRestrainedAndNot(character);
                } else if (character.homeStructure != null) {
                    chosenCharacter = character.homeStructure.GetRandomCharacterThatIsAliveVillagerAndNotSeizedOrCarriedAndNotTargetedByProduceFoodAndIsRestrainedAndNot(character);
                }
                if (chosenCharacter != null) {
#if DEBUG_LOG
                    log += $"\n-Chosen character: " + chosenCharacter.name;
#endif
                    if (character.jobComponent.CreateButcherJob(chosenCharacter, JOB_TYPE.MONSTER_BUTCHER, out producedJob)) {
                        return true;
                    }
                }
            }
            return character.jobComponent.TriggerRoamAroundStructure(out producedJob);
        } else {
#if DEBUG_LOG
            log += $"\n-Not in home, go to home";
#endif
            return character.jobComponent.PlanReturnHome(JOB_TYPE.IDLE_RETURN_HOME, out producedJob);
        }
        //return true;
	}
}
