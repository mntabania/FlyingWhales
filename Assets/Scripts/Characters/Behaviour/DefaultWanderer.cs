using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Inner_Maps.Location_Structures;
using UnityEngine.Assertions;

public class DefaultWanderer : CharacterBehaviourComponent {
	public DefaultWanderer() {
		priority = 25;
		// attributes = new[] { BEHAVIOUR_COMPONENT_ATTRIBUTE.WITHIN_HOME_SETTLEMENT_ONLY };
	}
	public override bool TryDoBehaviour(Character character, ref string log, out JobQueueItem producedJob) {
        producedJob = null;
        log += $"\n-{character.name} is wanderer";
        if (character.gridTileLocation != null) {
            if ((character.homeStructure == null || character.homeStructure.hasBeenDestroyed) && !character.HasTerritory()) {
                log += "\n-No home structure and territory";
                log += "\n-50% chance to Roam Around Tile";
                int roll = UnityEngine.Random.Range(0, 100);
                log += "\n-Roll: " + roll;
                if (roll < 50) {
                    character.jobComponent.TriggerRoamAroundTile(out producedJob);
                } else {
                    log += "\n-Otherwise, Visit Different Region";
                    if (!character.jobComponent.TriggerVisitDifferentRegion()) {
                        log += "\n-Cannot perform Visit Different Region, Roam Around Tile";
                        character.jobComponent.TriggerRoamAroundTile(out producedJob);
                    }
                }
                //log += "\n-Trigger Set Home interrupt";
                //character.interruptComponent.TriggerInterrupt(INTERRUPT.Set_Home, null);
                //if (character.homeStructure == null && !character.HasTerritory()) {
                //    log += "\n-Still no home structure and territory";
                //    log += "\n-50% chance to Roam Around Tile";
                //    int roll = UnityEngine.Random.Range(0, 100);
                //    log += "\n-Roll: " + roll;
                //    if (roll < 50) {
                //        character.jobComponent.TriggerRoamAroundTile(out producedJob);
                //    } else {
                //        log += "\n-Otherwise, Visit Different Region";
                //        if (!character.jobComponent.TriggerVisitDifferentRegion()) {
                //            log += "\n-Cannot perform Visit Different Region, Roam Around Tile";
                //            character.jobComponent.TriggerRoamAroundTile(out producedJob);
                //        }
                //    }
                //    return true;
                //}
                return true;
            } else {
                log += "\n-Has home structure or territory";
                if (character.isAtHomeStructure || character.IsInTerritory()) {
                    log += "\n-Is in home structure or territory";
                    if (character.previousCurrentActionNode != null && character.previousCurrentActionNode.action.goapType == INTERACTION_TYPE.RETURN_HOME) {
                        log += $"\n-Just returned home";
                        TileObject deskOrTable = character.currentStructure.GetUnoccupiedTileObject(TILE_OBJECT_TYPE.DESK, TILE_OBJECT_TYPE.TABLE);
                        log += "\n-Sit if there is still an unoccupied Table or Desk in the current location";
                        if (deskOrTable != null) {
                            log += $"\n-{character.name} will do action Sit on {deskOrTable}";
                            character.PlanIdle(JOB_TYPE.IDLE_SIT, INTERACTION_TYPE.SIT, deskOrTable, out producedJob);
                        } else {
                            log += "\n-Otherwise, stand idle";
                            log += $"\n-{character.name} will do action Stand";
                            character.PlanIdle(JOB_TYPE.IDLE_STAND, INTERACTION_TYPE.STAND, character, out producedJob);
                        }
                        return true;
                    } else {
                        TIME_IN_WORDS currentTimeOfDay = GameManager.GetCurrentTimeInWordsOfTick(character);

                        log += $"\n-Previous job is not returned home";
                        log += "\n-If it is Lunch Time or Afternoon, 25% chance to nap if there is still an unoccupied Bed in the house";
                        if (currentTimeOfDay == TIME_IN_WORDS.LUNCH_TIME || currentTimeOfDay == TIME_IN_WORDS.AFTERNOON) {
                            log += $"\n  -Time of Day: {currentTimeOfDay}";
                            int chance = Random.Range(0, 100);
                            log += $"\n  -RNG roll: {chance.ToString()}";
                            if (chance < 25) {
                                TileObject bed = character.currentStructure.GetUnoccupiedTileObject(TILE_OBJECT_TYPE.BED);
                                if (bed != null) {
                                    if (character.traitContainer.HasTrait("Vampiric")) {
                                        log += "\n  -Character is vampiric, cannot do nap action";
                                    } else {
                                        log += $"\n  -Afternoon: {character.name} will do action Nap on {bed}";
                                        character.PlanIdle(JOB_TYPE.IDLE_NAP, INTERACTION_TYPE.NAP, bed, out producedJob);
                                        return true;
                                    }
                                } else {
                                    log += "\n  -No unoccupied bed in the current structure";
                                }
                            }
                        } else {
                            log += $"\n  -Time of Day: {currentTimeOfDay}";
                        }
                        log += "\n-Otherwise, if it is Morning or Afternoon, 25% chance to add Obtain Personal Item Job if the character's Inventory is not yet full";
                        if (currentTimeOfDay == TIME_IN_WORDS.MORNING || currentTimeOfDay == TIME_IN_WORDS.AFTERNOON) {
                            log += $"\n  -Time of Day: {currentTimeOfDay}";
                            int chance = Random.Range(0, 100);
                            log += $"\n  -RNG roll: {chance.ToString()}";
                            if (chance < 25) {
                                if (character.jobComponent.TryCreateObtainPersonalItemJob(out producedJob)) {
                                    log += $"\n  -Created Obtain Personal Item Job";
                                    return true;
                                } else {
                                    log += $"\n  -Could not create Obtain Personal Item Job. Either the inventory has reached full capacity or character has no items that he/she is interested";
                                }
                            }
                        } else {
                            log += $"\n  -Time of Day: {currentTimeOfDay}";
                        }

                        log += "\n-Otherwise, if it is Morning or Lunch Time or Afternoon or Early Night, 25% chance to Stroll";
                        if ((currentTimeOfDay == TIME_IN_WORDS.MORNING || currentTimeOfDay == TIME_IN_WORDS.LUNCH_TIME ||
                             currentTimeOfDay == TIME_IN_WORDS.AFTERNOON || currentTimeOfDay == TIME_IN_WORDS.EARLY_NIGHT)
                            && character.trapStructure.IsTrapped() == false) {
                            log += $"\n  -Time of Day: {currentTimeOfDay}";
                            int chance = Random.Range(0, 100);
                            log += $"\n  -RNG roll: {chance.ToString()}";
                            if (chance < 25) {
                                log +=
                                    $"\n  -Morning, Afternoon, or Early Night: {character.name} will enter Stroll Outside Mode";
                                character.jobComponent.PlanIdleStrollOutside(out producedJob); //character.currentStructure
                                return true;
                            }
                        } else {
                            log += $"\n  -Time of Day: {currentTimeOfDay}";
                        }


                        log += "\n-Otherwise, if it is Morning or Afternoon, 25% chance to someone with a positive relationship in current location and then set it as the Base Structure for 2.5 hours";
                        if (currentTimeOfDay == TIME_IN_WORDS.MORNING || currentTimeOfDay == TIME_IN_WORDS.LUNCH_TIME || currentTimeOfDay == TIME_IN_WORDS.AFTERNOON) {
                            log += $"\n  -Time of Day: {currentTimeOfDay}";
                            int chance = Random.Range(0, 100);
                            log += $"\n  -RNG roll: {chance.ToString()}";
                            if (chance < 25 && character.trapStructure.IsTrapped() == false) {
                                WeightedDictionary<Character> visitWeights = GetCharacterToVisitWeights(character);
                                if (visitWeights.GetTotalOfWeights() > 0) {
                                    Character targetCharacter = visitWeights.PickRandomElementGivenWeights();
                                    LocationStructure targetStructure = targetCharacter.homeStructure;
                                    Assert.IsNotNull(targetStructure, $"Home structure of visit target {targetCharacter.name} is null!");
                                    log += $"\n  -Morning or Afternoon: {character.name} will go to dwelling of character with positive relationship, {targetCharacter.name} and set Base Structure for 2.5 hours";
                                    character.PlanIdle(JOB_TYPE.VISIT_FRIEND, INTERACTION_TYPE.VISIT, targetCharacter, out producedJob, new object[] { targetStructure, targetCharacter });
                                } else {
                                    log += "\n  -No valid character to visit.";
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
                            log += $"\n  -Time of Day: {currentTimeOfDay}";
                        }

                        log += "\n-Otherwise, sit if there is still an unoccupied Table or Desk";
                        TileObject deskOrTable = character.currentStructure.GetUnoccupiedTileObject(TILE_OBJECT_TYPE.DESK, TILE_OBJECT_TYPE.TABLE);
                        if (deskOrTable != null) {
                            log += $"\n  -{character.name} will do action Sit on {deskOrTable}";
                            character.PlanIdle(JOB_TYPE.IDLE_SIT, INTERACTION_TYPE.SIT, deskOrTable, out producedJob);
                            return true;
                        }
                        log += "\n  -No unoccupied Table or Desk";

                        log += "\n-Otherwise, stand idle";
                        log += $"\n  -{character.name} will do action Stand";
                        character.PlanIdle(JOB_TYPE.IDLE_STAND, INTERACTION_TYPE.STAND, character, out producedJob);
                        return true;
                    }
                } else {
                    log += "\n-Is not in home structure or territory";
                    if (character.currentHP < (character.maxHP * 0.5f)) {
                        log += "\n-HP is less than 50% of max hp, Return Home/Territory";
                        if (character.homeStructure != null) {
                            character.jobComponent.PlanIdleReturnHome(out producedJob);
                            return true;
                        } else if (character.HasTerritory()) {
                            character.jobComponent.TriggerReturnTerritory(out producedJob);
                            return true;
                        } else {
                            log += "\n-No home structure or territory: THIS MUST NOT HAPPEN!";
                        }
                    } else {
                        log += "\n-50% chance to Roam Around Tile";
                        int roll = UnityEngine.Random.Range(0, 100);
                        log += "\n-Roll: " + roll;
                        if (roll < 50) {
                            character.jobComponent.TriggerRoamAroundTile(out producedJob);
                            return true;
                        } else {
                            log += "\n-Otherwise, Return Home/Territory";
                            if (character.homeStructure != null) {
                                character.jobComponent.PlanIdleReturnHome(out producedJob);
                                return true;
                            } else if (character.HasTerritory()) {
                                character.jobComponent.TriggerReturnTerritory(out producedJob);
                                return true;
                            } else {
                                log += "\n-No home structure or territory: THIS MUST NOT HAPPEN!";
                            }
                        }
                    }
                }
            }
        }
        return false;
    }
}
