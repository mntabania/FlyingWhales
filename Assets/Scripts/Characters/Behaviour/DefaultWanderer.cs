using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Inner_Maps.Location_Structures;
using UnityEngine.Assertions;
using UtilityScripts;

public class DefaultWanderer : CharacterBehaviourComponent {
	public DefaultWanderer() {
		priority = 8;
		// attributes = new[] { BEHAVIOUR_COMPONENT_ATTRIBUTE.WITHIN_HOME_SETTLEMENT_ONLY };
	}
	public override bool TryDoBehaviour(Character character, ref string log, out JobQueueItem producedJob) {
        producedJob = null;
#if DEBUG_LOG
        log += $"\n-{character.name} is wanderer";
#endif
        //needs recovery
        if (character.needsComponent.isStarving) {
#if DEBUG_LOG

            log = $"{log}\n-{character.name} is starving will try to do fullness recovery.";
#endif
            //add fullness recovery
            if (character.needsComponent.PlanFullnessRecoveryActionsForFreeTime(out producedJob)) {
                return true;
            }
        } else if (character.needsComponent.isHungry) {
#if DEBUG_LOG
            log = $"{log}\n-{character.name} is hungry 20% chance to do fullness recovery.";
#endif
            if (GameUtilities.RollChance(20, ref log)) {
                //add fullness recovery
                if (character.needsComponent.PlanFullnessRecoveryActionsForFreeTime(out producedJob)) {
                    return true;
                }
            }
        }
        if (character.needsComponent.isSulking) {
#if DEBUG_LOG
            log = $"{log}\n-{character.name} is sulking will try to do happiness recovery.";
#endif
            //add happiness recovery
            if (CreateHappinessRecoveryJob(character, out producedJob)) {
                return true;
            }
        } else if (character.needsComponent.isBored) {
#if DEBUG_LOG
            log = $"{log}\n-{character.name} is bored 20% chance do happiness recovery.";
#endif
            if (GameUtilities.RollChance(20, ref log)) {
                //add happiness recovery
                if (CreateHappinessRecoveryJob(character, out producedJob)) {
                    return true;
                }
            }
        }

        if (!character.HasTerritory() && character.currentRegion != null) {
            Area initialTerritory = character.currentRegion.GetRandomAreaThatIsNotMountainWaterAndNoStructureAndNoCorruption();
            if (initialTerritory != null) {
                character.SetTerritory(initialTerritory);
            } else {
#if DEBUG_LOG
                character.logComponent.PrintLogIfActive(character.name + " is a wanderer but could not set temporary territory");
#endif
            }
        }
        if (character.gridTileLocation != null) {
            if ((character.homeStructure == null || character.homeStructure.hasBeenDestroyed) && !character.HasTerritory()) {
#if DEBUG_LOG
                log += "\n-No home structure and territory";
                //log += "\n-50% chance to Roam Around Tile";
                log += "\n-Roam Around Tile";
#endif
                return character.jobComponent.TriggerRoamAroundTile(out producedJob);
            } else {
#if DEBUG_LOG
                log += "\n-Has home structure or territory";
#endif
                if (character.isAtHomeStructure || character.IsInTerritory()) {
#if DEBUG_LOG
                    log += "\n-Is in home structure or territory";
#endif
                    if (character.previousCharacterDataComponent.IsPreviousJobOrActionReturnHome()) {
#if DEBUG_LOG
                        log += $"\n-Just returned home";
#endif
                        TileObject deskOrTable = character.currentStructure.GetUnoccupiedBuiltTileObject(TILE_OBJECT_TYPE.DESK, TILE_OBJECT_TYPE.TABLE);
#if DEBUG_LOG
                        log += "\n-Sit if there is still an unoccupied Table or Desk in the current location";
#endif
                        if (deskOrTable != null) {
#if DEBUG_LOG
                            log += $"\n-{character.name} will do action Sit on {deskOrTable}";
#endif
                            character.PlanFixedJob(JOB_TYPE.IDLE_SIT, INTERACTION_TYPE.SIT, deskOrTable, out producedJob);
                        } else {
#if DEBUG_LOG
                            log += "\n-Otherwise, stand idle";
                            log += $"\n-{character.name} will do action Stand";
#endif
                            character.PlanFixedJob(JOB_TYPE.IDLE_STAND, INTERACTION_TYPE.STAND, character, out producedJob);
                        }
                        return true;
                    } else {
                        TIME_IN_WORDS currentTimeOfDay = GameManager.Instance.GetCurrentTimeInWordsOfTick(character);
#if DEBUG_LOG
                        log += $"\n-Previous job is not returned home";
                        log += "\n-If it is Lunch Time or Afternoon, 25% chance to nap if there is still an unoccupied Bed in the house";
#endif
                        if (currentTimeOfDay == TIME_IN_WORDS.LUNCH_TIME || currentTimeOfDay == TIME_IN_WORDS.AFTERNOON) {
                            int chance = Random.Range(0, 100);
#if DEBUG_LOG
                            log += $"\n  -Time of Day: {currentTimeOfDay}";
                            log += $"\n  -RNG roll: {chance.ToString()}";
#endif
                            if (chance < 25) {
                                TileObject bed = character.currentStructure.GetUnoccupiedTileObject(TILE_OBJECT_TYPE.BED);
                                if (bed != null) {
                                    if (character.traitContainer.HasTrait("Vampire")) {
#if DEBUG_LOG
                                        log += "\n  -Character is vampiric, cannot do nap action";
#endif
                                    } else {
#if DEBUG_LOG
                                        log += $"\n  -Afternoon: {character.name} will do action Nap on {bed}";
#endif
                                        character.PlanFixedJob(JOB_TYPE.IDLE_NAP, INTERACTION_TYPE.NAP, bed, out producedJob);
                                        return true;
                                    }
                                } else {
#if DEBUG_LOG
                                    log += "\n  -No unoccupied bed in the current structure";
#endif
                                }
                            }
                        } else {
#if DEBUG_LOG
                            log += $"\n  -Time of Day: {currentTimeOfDay}";
#endif
                        }
#if DEBUG_LOG
                        log += "\n-Otherwise, if it is Morning or Afternoon, 25% chance to add Obtain Personal Item Job if the character's Inventory is not yet full";
#endif
                        if (currentTimeOfDay == TIME_IN_WORDS.MORNING || currentTimeOfDay == TIME_IN_WORDS.AFTERNOON) {
                            int chance = Random.Range(0, 100);
#if DEBUG_LOG
                            log += $"\n  -Time of Day: {currentTimeOfDay}";
                            log += $"\n  -RNG roll: {chance.ToString()}";
#endif
                            if (chance < 25) {
                                if (character.jobComponent.TryCreateObtainPersonalItemJob(out producedJob)) {
#if DEBUG_LOG
                                    log += $"\n  -Created Obtain Personal Item Job";
#endif
                                    return true;
                                } else {
#if DEBUG_LOG
                                    log += $"\n  -Could not create Obtain Personal Item Job. Either the inventory has reached full capacity or character has no items that he/she is interested";
#endif
                                }
                            }
                        } else {
#if DEBUG_LOG
                            log += $"\n  -Time of Day: {currentTimeOfDay}";
#endif
                        }
#if DEBUG_LOG
                        log += "\n-Otherwise, if it is Morning or Lunch Time or Afternoon or Early Night, 25% chance to Stroll";
#endif
                        if ((currentTimeOfDay == TIME_IN_WORDS.MORNING || currentTimeOfDay == TIME_IN_WORDS.LUNCH_TIME ||
                             currentTimeOfDay == TIME_IN_WORDS.AFTERNOON || currentTimeOfDay == TIME_IN_WORDS.EARLY_NIGHT)
                            && character.trapStructure.IsTrapped() == false && character.trapStructure.IsTrappedInArea() == false) {
                            int chance = Random.Range(0, 100);
#if DEBUG_LOG
                            log += $"\n  -Time of Day: {currentTimeOfDay}";
                            log += $"\n  -RNG roll: {chance.ToString()}";
#endif
                            if (chance < 25) {
#if DEBUG_LOG
                                log += $"\n  -Morning, Afternoon, or Early Night: {character.name} will enter Stroll Outside Mode";
#endif
                                character.jobComponent.PlanIdleStrollOutside(out producedJob); //character.currentStructure
                                return true;
                            }
                        } else {
#if DEBUG_LOG
                            log += $"\n  -Time of Day: {currentTimeOfDay}";
#endif
                        }


#if DEBUG_LOG
                        log += "\n-Otherwise, if it is Morning or Afternoon, 25% chance to someone with a positive relationship in current location and then set it as the Base Structure for 2.5 hours";
#endif
                        if (currentTimeOfDay == TIME_IN_WORDS.MORNING || currentTimeOfDay == TIME_IN_WORDS.LUNCH_TIME || currentTimeOfDay == TIME_IN_WORDS.AFTERNOON) {
                            int chance = Random.Range(0, 100);
#if DEBUG_LOG
                            log += $"\n  -Time of Day: {currentTimeOfDay}";
                            log += $"\n  -RNG roll: {chance.ToString()}";
#endif
                            if (chance < 25 && character.trapStructure.IsTrapped() == false && character.trapStructure.IsTrappedInArea() == false) {
                                WeightedDictionary<Character> visitWeights = GetCharacterToVisitWeights(character);
                                if (visitWeights.GetTotalOfWeights() > 0) {
                                    Character targetCharacter = visitWeights.PickRandomElementGivenWeights();
                                    LocationStructure targetStructure = targetCharacter.homeStructure;
                                    Assert.IsNotNull(targetStructure, $"Home structure of visit target {targetCharacter.name} is null!");
#if DEBUG_LOG
                                    log += $"\n  -Morning or Afternoon: {character.name} will go to dwelling of character with positive relationship, {targetCharacter.name} and set Base Structure for 2.5 hours";
#endif
                                    character.PlanFixedJob(JOB_TYPE.VISIT_FRIEND, INTERACTION_TYPE.VISIT, targetCharacter, out producedJob, 
                                        new OtherData[] { new LocationStructureOtherData(targetStructure), new CharacterOtherData(targetCharacter),  });
                                    return true;
                                } else {
#if DEBUG_LOG
                                    log += "\n  -No valid character to visit.";
#endif
                                }
                                // List<Character> positiveRelatables = character.relationshipContainer.GetFriendCharacters();
                                // if (positiveRelatables.Count > 0) {
                                //     Character targetCharacter = null;
                                //     LocationStructure targetStructure = null;
                                //     while (positiveRelatables.Count > 0 && targetStructure == null) {
                                //         int index = Random.Range(0, positiveRelatables.Count);
                                //         Character chosenRelatable = positiveRelatables[index];
                                //         targetCharacter = chosenRelatable;
                                //         targetStructure = chosenRelatable.homeStructure;
                                //         if (targetStructure == null) {
                                //             positiveRelatables.RemoveAt(index);
                                //         } else if (targetStructure == character.homeStructure) {
                                //             targetStructure = null;
                                //             positiveRelatables.RemoveAt(index);
                                //         } else if (chosenRelatable.isDead /*|| chosenRelatable.isMissing*/) {
                                //             targetStructure = null;
                                //             positiveRelatables.RemoveAt(index);
                                //         } else if (character.movementComponent.HasPathToEvenIfDiffRegion(targetStructure.GetRandomTile()) == false) {
                                //             targetStructure = null;
                                //             positiveRelatables.RemoveAt(index);
                                //         }
                                //     }
                                //     if (targetStructure != null) {
                                //         log +=
                                //             $"\n  -Morning or Afternoon: {character.name} will go to dwelling of character with positive relationship, {targetCharacter.name} and set Base Structure for 2.5 hours";
                                //         character.PlanIdle(JOB_TYPE.VISIT_FRIEND, INTERACTION_TYPE.VISIT, targetCharacter, out producedJob, new object[] { targetStructure, targetCharacter });
                                //         return true;
                                //     }
                                //     log += "\n  -No positive relationship with home structure";
                                // } else {
                                //     log += "\n  -No character with positive relationship";
                                // }
                            }
                        } else {
#if DEBUG_LOG
                            log += $"\n  -Time of Day: {currentTimeOfDay}";
#endif
                        }

#if DEBUG_LOG
                        log += "\n-Otherwise, sit if there is still an unoccupied Table or Desk";
#endif
                        TileObject deskOrTable = character.currentStructure.GetUnoccupiedBuiltTileObject(TILE_OBJECT_TYPE.DESK, TILE_OBJECT_TYPE.TABLE);
                        if (deskOrTable != null) {
#if DEBUG_LOG
                            log += $"\n  -{character.name} will do action Sit on {deskOrTable}";
#endif
                            character.PlanFixedJob(JOB_TYPE.IDLE_SIT, INTERACTION_TYPE.SIT, deskOrTable, out producedJob);
                            return true;
                        }
#if DEBUG_LOG
                        log += "\n  -No unoccupied Table or Desk";
                        log += "\n-Otherwise, stand idle";
                        log += $"\n  -{character.name} will do action Stand";
#endif
                        character.PlanFixedJob(JOB_TYPE.IDLE_STAND, INTERACTION_TYPE.STAND, character, out producedJob);
                        return true;
                    }
                } else {
#if DEBUG_LOG
                    log += "\n-Is not in home structure or territory";
#endif
                    if (character.currentHP < (character.maxHP * 0.5f)) {
#if DEBUG_LOG
                        log += "\n-HP is less than 50% of max hp, Return Home/Territory";
#endif
                        if (character.homeStructure != null || character.HasTerritory()) {
                            character.jobComponent.PlanReturnHome(JOB_TYPE.IDLE_RETURN_HOME, out producedJob);
                            return true;
                        } else {
#if DEBUG_LOG
                            log += "\n-No home structure or territory: THIS MUST NOT HAPPEN!";
#endif
                        }
                    } else {
                        int roll = UnityEngine.Random.Range(0, 100);
#if DEBUG_LOG
                        log += "\n-50% chance to Roam Around Tile";
                        log += "\n-Roll: " + roll;
#endif
                        if (roll < 50) {
                            character.jobComponent.TriggerRoamAroundTile(out producedJob);
                            return true;
                        } else {
#if DEBUG_LOG
                            log += "\n-Otherwise, Return Home/Territory";
#endif
                            if (character.homeStructure != null || character.HasTerritory()) {
                                character.jobComponent.PlanReturnHome(JOB_TYPE.IDLE_RETURN_HOME, out producedJob);
                                return true;
                            } else {
#if DEBUG_LOG
                                log += "\n-No home structure or territory: THIS MUST NOT HAPPEN!";
#endif
                            }
                        }
                    }
                }
            }
        }
        return false;
    }

    private bool CreateHappinessRecoveryJob(Character p_character, out JobQueueItem producedJob) {
        GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.HAPPINESS_RECOVERY,
            new GoapEffect(GOAP_EFFECT_CONDITION.HAPPINESS_RECOVERY, string.Empty, false, GOAP_EFFECT_TARGET.ACTOR), p_character, p_character);
        JobUtilities.PopulatePriorityLocationsForHappinessRecovery(p_character, job);
        job.SetDoNotRecalculate(true);
        producedJob = job;
        return true;
    }
}