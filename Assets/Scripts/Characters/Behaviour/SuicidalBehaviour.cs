using System.Collections;
using System.Collections.Generic;
using Inner_Maps.Location_Structures;
using UnityEngine;
using Traits;

public class SuicidalBehaviour : CharacterBehaviourComponent {
    public SuicidalBehaviour() {
        priority = 50;
        //attributes = new BEHAVIOUR_COMPONENT_ATTRIBUTE[] { BEHAVIOUR_COMPONENT_ATTRIBUTE.WITHIN_HOME_SETTLEMENT_ONLY };
    }
    public override bool TryDoBehaviour(Character character, ref string log) {
        if (character.currentStructure == character.homeStructure) {
            if (character.previousCurrentActionNode != null && character.previousCurrentActionNode.action.goapType == INTERACTION_TYPE.RETURN_HOME) {
                log += $"\n-{character.name} is in home structure and just returned home";
                log += "\n-50% chance to Sit if there is still an unoccupied Table or Desk in the current location";
                int chance = UnityEngine.Random.Range(0, 100);
                log += $"\n  -RNG roll: {chance}";
                if (chance < 50) {
                    TileObject deskOrTable = character.currentStructure.GetUnoccupiedTileObject(TILE_OBJECT_TYPE.DESK, TILE_OBJECT_TYPE.TABLE);
                    if (deskOrTable != null) {
                        log += $"\n  -{character.name} will do action Sit on {deskOrTable}";
                        character.PlanIdle(JOB_TYPE.IDLE_SIT, INTERACTION_TYPE.SIT, deskOrTable);
                        return true;
                    } else {
                        log += "\n  -No unoccupied table or desk";
                    }
                }
                log += "\n-Otherwise, stand idle";
                log += $"\n  -{character.name} will do action Stand";
                character.PlanIdle(JOB_TYPE.IDLE_STAND, INTERACTION_TYPE.STAND, character);
                return true;
            } else {
                log += $"\n-{character.name} is in home structure and previous action is not returned home";
                TIME_IN_WORDS currentTimeOfDay = GameManager.GetCurrentTimeInWordsOfTick(character);

                log += "\n-Otherwise, if it is Lunch Time or Afternoon, 25% chance to nap if there is still an unoccupied Bed in the house";
                if (currentTimeOfDay == TIME_IN_WORDS.LUNCH_TIME || currentTimeOfDay == TIME_IN_WORDS.AFTERNOON) {
                    log += $"\n  -Time of Day: {currentTimeOfDay}";
                    int chance = UnityEngine.Random.Range(0, 100);
                    log += $"\n  -RNG roll: {chance}";
                    if (chance < 25) {
                        TileObject bed = character.currentStructure.GetUnoccupiedTileObject(TILE_OBJECT_TYPE.BED);
                        if (bed != null) {
                            if (character.traitContainer.HasTrait("Vampiric")) {
                                log += "\n  -Character is vampiric, cannot do nap action";
                            } else {
                                log += $"\n  -Afternoon: {character.name} will do action Nap on {bed}";
                                character.PlanIdle(JOB_TYPE.IDLE_NAP, INTERACTION_TYPE.NAP, bed);
                                return true;
                            }
                        } else {
                            log += "\n  -No unoccupied bed in the current structure";
                        }
                    }
                } else {
                    log += $"\n  -Time of Day: {currentTimeOfDay}";
                }
                log += "\n-Otherwise, if it is Morning or Afternoon or Early Night, and the character has a positive relationship with someone currently Paralyzed or Catatonic, 30% chance to Check Out one at random";
                if (currentTimeOfDay == TIME_IN_WORDS.MORNING || currentTimeOfDay == TIME_IN_WORDS.AFTERNOON || currentTimeOfDay == TIME_IN_WORDS.EARLY_NIGHT) {
                    log += $"\n  -Time of Day: {currentTimeOfDay}";
                    int chance = UnityEngine.Random.Range(0, 100);
                    log += $"\n  -RNG roll: {chance}";
                    if (chance < 30) {
                        Character chosenCharacter = character.GetDisabledCharacterToCheckOut();
                        if (chosenCharacter != null) {
                            if (chosenCharacter.homeStructure != null) {
                                log += $"\n  -Will visit house of Disabled Character {chosenCharacter.name}";
                                character.PlanIdle(JOB_TYPE.CHECK_PARALYZED_FRIEND, INTERACTION_TYPE.VISIT, character, new object[] { chosenCharacter.homeStructure, chosenCharacter });
                            } else {
                                log += $"\n  -{chosenCharacter.name} has no house. Will check out character instead";
                                character.PlanIdle(JOB_TYPE.CHECK_PARALYZED_FRIEND,  new GoapEffect(GOAP_EFFECT_CONDITION.IN_VISION, string.Empty, false, GOAP_EFFECT_TARGET.TARGET), chosenCharacter);
                            }
                            return true;
                        } else {
                            log += "\n  -No available character to check out ";
                        }
                    }
                } else {
                    log += $"\n  -Time of Day: {currentTimeOfDay}";
                }
                log += "\n-Otherwise, if it is Morning or Lunch Time or Afternoon or Early Night, 25% chance to enter Stroll Outside Mode for 1 hour";
                if (currentTimeOfDay == TIME_IN_WORDS.MORNING || currentTimeOfDay == TIME_IN_WORDS.LUNCH_TIME || currentTimeOfDay == TIME_IN_WORDS.AFTERNOON || currentTimeOfDay == TIME_IN_WORDS.EARLY_NIGHT) {
                    log += $"\n  -Time of Day: {currentTimeOfDay}";
                    int chance = UnityEngine.Random.Range(0, 100);
                    log += $"\n  -RNG roll: {chance}";
                    if (chance < 25) {
                        log +=
                            $"\n  -Morning, Afternoon, or Early Night: {character.name} will enter Stroll Outside Mode";
                        character.PlanIdleStrollOutside(); //character.currentStructure
                        return true;
                    }
                } else {
                    log += $"\n  -Time of Day: {currentTimeOfDay}";
                }
                log += "\n-Otherwise, if it is Morning or Afternoon, 25% chance to someone with a positive relationship in current location and then set it as the Base Structure for 2.5 hours";
                if (currentTimeOfDay == TIME_IN_WORDS.MORNING || currentTimeOfDay == TIME_IN_WORDS.LUNCH_TIME || currentTimeOfDay == TIME_IN_WORDS.AFTERNOON) {
                    log += $"\n  -Time of Day: {currentTimeOfDay}";
                    int chance = UnityEngine.Random.Range(0, 100);
                    log += $"\n  -RNG roll: {chance}";
                    if (chance < 25) {
                        List<Character> positiveRelatables = character.relationshipContainer.GetFriendCharacters();
                        if (positiveRelatables.Count > 0) {
                            LocationStructure targetStructure = null;
                            Character targetCharacter = null;
                            while (positiveRelatables.Count > 0 && targetStructure == null) {
                                int index = UnityEngine.Random.Range(0, positiveRelatables.Count);
                                Character chosenRelatable = positiveRelatables[index];
                                targetCharacter = chosenRelatable;
                                targetStructure = chosenRelatable.homeStructure.GetLocationStructure();
                                if (targetStructure == null) {
                                    positiveRelatables.RemoveAt(index);
                                } else if (targetStructure == character.homeStructure) {
                                    targetStructure = null;
                                    positiveRelatables.RemoveAt(index);
                                } else if (chosenRelatable.isDead || chosenRelatable.isMissing) {
                                    targetStructure = null;
                                    positiveRelatables.RemoveAt(index);
                                } else if (PathfindingManager.Instance.HasPathEvenDiffRegion(character.gridTileLocation, targetStructure.GetRandomTile()) == false) {
                                    targetStructure = null;
                                    positiveRelatables.RemoveAt(index);
                                }
                            }
                            if (targetStructure != null) {
                                log +=
                                    $"\n  -Morning or Afternoon: {character.name} will go to dwelling of character with positive relationship and set Base Structure for 2.5 hours";
                                character.PlanIdle(JOB_TYPE.VISIT_FRIEND,  INTERACTION_TYPE.VISIT, character, new object[] { targetStructure, targetCharacter });
                                return true;
                            } else {
                                log += "\n  -No positive relationship with home structure";
                            }
                        } else {
                            log += "\n  -No character with positive relationship";
                        }
                    }
                } else {
                    log += $"\n  -Time of Day: {currentTimeOfDay}";
                }
                log += "\n-Otherwise, 25% chance to sit if there is still an unoccupied Table or Desk";
                int sitChance = UnityEngine.Random.Range(0, 100);
                log += $"\n  -RNG roll: {sitChance}";
                if (sitChance < 25) {
                    TileObject deskOrTable = character.currentStructure.GetUnoccupiedTileObject(TILE_OBJECT_TYPE.DESK, TILE_OBJECT_TYPE.TABLE);
                    if (deskOrTable != null) {
                        log += $"\n  -{character.name} will do action Sit on {deskOrTable}";
                        character.PlanIdle(JOB_TYPE.IDLE_SIT, INTERACTION_TYPE.SIT, deskOrTable);
                        return true;
                    } else {
                        log += "\n  -No unoccupied Table or Desk";
                    }
                }

                log += "\n-Otherwise, 25% chance to stand idle";
                int standChance = UnityEngine.Random.Range(0, 100);
                log += $"\n  -RNG roll: {standChance}";
                if (standChance < 25) {
                    log += $"\n  -{character.name} will do action Stand";
                    character.PlanIdle(JOB_TYPE.IDLE_STAND, INTERACTION_TYPE.STAND, character);
                    return true;
                }
                character.jobComponent.TriggerSuicideJob();
                //PlanIdleStroll(currentStructure);
                return true;
            }
        }
        return false;
    }
}