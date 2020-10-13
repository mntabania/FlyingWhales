using System.Collections;
using System.Collections.Generic;
using Inner_Maps.Location_Structures;
using UnityEngine;
using Traits;
using UnityEngine.Assertions;
using UtilityScripts;

public class SuicidalBehaviour : CharacterBehaviourComponent {
    public SuicidalBehaviour() {
        priority = 8;
        //attributes = new BEHAVIOUR_COMPONENT_ATTRIBUTE[] { BEHAVIOUR_COMPONENT_ATTRIBUTE.WITHIN_HOME_SETTLEMENT_ONLY };
    }
    public override bool TryDoBehaviour(Character character, ref string log, out JobQueueItem producedJob) {
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
                        character.PlanIdle(JOB_TYPE.IDLE_SIT, INTERACTION_TYPE.SIT, deskOrTable, out producedJob);
                        return true;
                    } else {
                        log += "\n  -No unoccupied table or desk";
                    }
                }
                log += "\n-Otherwise, stand idle";
                log += $"\n  -{character.name} will do action Stand";
                character.PlanIdle(JOB_TYPE.IDLE_STAND, INTERACTION_TYPE.STAND, character, out producedJob);
                return true;
            } else {
                log += $"\n-{character.name} is in home structure and previous action is not returned home";
                TIME_IN_WORDS currentTimeOfDay = GameManager.GetCurrentTimeInWordsOfTick(character);

                log += "\n-Otherwise, if it is Lunch Time or Afternoon, 15% chance to nap if there is still an unoccupied Bed in the house";
                if (currentTimeOfDay == TIME_IN_WORDS.LUNCH_TIME || currentTimeOfDay == TIME_IN_WORDS.AFTERNOON) {
                    log += $"\n  -Time of Day: {currentTimeOfDay}";
                    int chance = UnityEngine.Random.Range(0, 100);
                    log += $"\n  -RNG roll: {chance}";
                    if (chance < 15) {
                        TileObject bed = character.currentStructure.GetUnoccupiedTileObject(TILE_OBJECT_TYPE.BED);
                        if (bed != null) {
                            if (character.traitContainer.HasTrait("Vampire")) {
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
                log += "\n-Otherwise, if it is Morning or Lunch Time or Afternoon or Early Night, 10% chance to enter Stroll Outside Mode for 1 hour";
                if (currentTimeOfDay == TIME_IN_WORDS.MORNING || currentTimeOfDay == TIME_IN_WORDS.LUNCH_TIME || currentTimeOfDay == TIME_IN_WORDS.AFTERNOON || currentTimeOfDay == TIME_IN_WORDS.EARLY_NIGHT) {
                    log += $"\n  -Time of Day: {currentTimeOfDay}";
                    int chance = UnityEngine.Random.Range(0, 100);
                    log += $"\n  -RNG roll: {chance}";
                    if (chance < 10) {
                        log +=
                            $"\n  -Morning, Afternoon, or Early Night: {character.name} will enter Stroll Outside Mode";
                        character.jobComponent.PlanIdleStrollOutside(out producedJob); //character.currentStructure
                        return true;
                    }
                } else {
                    log += $"\n  -Time of Day: {currentTimeOfDay}";
                }
                log += "\n-Otherwise, if it is Morning or Afternoon, 15% chance to someone with a positive relationship in current location and then set it as the Base Structure for 2.5 hours";
                if (currentTimeOfDay == TIME_IN_WORDS.MORNING || currentTimeOfDay == TIME_IN_WORDS.LUNCH_TIME || currentTimeOfDay == TIME_IN_WORDS.AFTERNOON) {
                    log += $"\n  -Time of Day: {currentTimeOfDay}";
                    int chance = UnityEngine.Random.Range(0, 100);
                    log += $"\n  -RNG roll: {chance}";
                    if (chance < 15) {
                        WeightedDictionary<Character> visitWeights = GetCharacterToVisitWeights(character);
                        if (visitWeights.GetTotalOfWeights() > 0) {
                            Character targetCharacter = visitWeights.PickRandomElementGivenWeights();
                            LocationStructure targetStructure = targetCharacter.homeStructure;
                            Assert.IsNotNull(targetStructure, $"Home structure of visit target {targetCharacter.name} is null!");
                            log += $"\n  -Morning or Afternoon: {character.name} will go to dwelling of character with positive relationship, {targetCharacter.name} and set Base Structure for 2.5 hours";
                            character.PlanIdle(JOB_TYPE.VISIT_FRIEND, INTERACTION_TYPE.VISIT, targetCharacter, out producedJob, 
                                new OtherData[] { new LocationStructureOtherData(targetStructure), new CharacterOtherData(targetCharacter), });
                        } else {
                            log += "\n  -No valid character to visit.";
                        }
                    }
                } else {
                    log += $"\n  -Time of Day: {currentTimeOfDay}";
                }
                log += "\n-Otherwise, 15% chance to sit if there is still an unoccupied Table or Desk";
                int sitChance = UnityEngine.Random.Range(0, 100);
                log += $"\n  -RNG roll: {sitChance}";
                if (sitChance < 15) {
                    TileObject deskOrTable = character.currentStructure.GetUnoccupiedTileObject(TILE_OBJECT_TYPE.DESK, TILE_OBJECT_TYPE.TABLE);
                    if (deskOrTable != null) {
                        log += $"\n  -{character.name} will do action Sit on {deskOrTable}";
                        character.PlanIdle(JOB_TYPE.IDLE_SIT, INTERACTION_TYPE.SIT, deskOrTable, out producedJob);
                        return true;
                    } else {
                        log += "\n  -No unoccupied Table or Desk";
                    }
                }

                log += "\n-Otherwise, 15% chance add a Cry Interrupt";
                if (GameUtilities.RollChance(15, ref log)) {
                    log += $"\n  -{character.name} will do cry";
                    character.interruptComponent.TriggerInterrupt(INTERRUPT.Cry, character, identifier: "suicidal");
                    producedJob = null;
                    return true;
                }
                log += "\n-Otherwise, Create Commit Suicide Job";
                if (character.jobComponent.TriggerSuicideJob(out producedJob)) {
                    return true;
                }
            }
        }
        producedJob = null;
        return false;
    }
}