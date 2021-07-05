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
            if (character.previousCharacterDataComponent.IsPreviousJobOrActionReturnHome()) {
                int chance = UnityEngine.Random.Range(0, 100);
#if DEBUG_LOG
                log += $"\n-{character.name} is in home structure and just returned home";
                log += "\n-50% chance to Sit if there is still an unoccupied Table or Desk in the current location";
                log += $"\n  -RNG roll: {chance}";
#endif
                if (chance < 50) {
                    TileObject deskOrTable = character.currentStructure.GetUnoccupiedBuiltTileObject(TILE_OBJECT_TYPE.DESK, TILE_OBJECT_TYPE.TABLE);
                    if (deskOrTable != null) {
#if DEBUG_LOG
                        log += $"\n  -{character.name} will do action Sit on {deskOrTable}";
#endif
                        character.PlanFixedJob(JOB_TYPE.IDLE_SIT, INTERACTION_TYPE.SIT, deskOrTable, out producedJob);
                        return true;
                    } else {
#if DEBUG_LOG
                        log += "\n  -No unoccupied table or desk";
#endif
                    }
                }
#if DEBUG_LOG
                log += "\n-Otherwise, stand idle";
                log += $"\n  -{character.name} will do action Stand";
#endif
                character.PlanFixedJob(JOB_TYPE.IDLE_STAND, INTERACTION_TYPE.STAND, character, out producedJob);
                return true;
            } else {
                TIME_IN_WORDS currentTimeOfDay = GameManager.Instance.GetCurrentTimeInWordsOfTick(character);
#if DEBUG_LOG
                log += $"\n-{character.name} is in home structure and previous action is not returned home";
                log += "\n-Otherwise, if it is Lunch Time or Afternoon, 15% chance to nap if there is still an unoccupied Bed in the house";
#endif
                if (currentTimeOfDay == TIME_IN_WORDS.LUNCH_TIME || currentTimeOfDay == TIME_IN_WORDS.AFTERNOON) {
                    int chance = UnityEngine.Random.Range(0, 100);
#if DEBUG_LOG
                    log += $"\n  -Time of Day: {currentTimeOfDay}";
                    log += $"\n  -RNG roll: {chance}";
#endif
                    if (chance < 15) {
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
                log += "\n-Otherwise, if it is Morning or Lunch Time or Afternoon or Early Night, 10% chance to enter Stroll Outside Mode for 1 hour";
#endif
                if (currentTimeOfDay == TIME_IN_WORDS.MORNING || currentTimeOfDay == TIME_IN_WORDS.LUNCH_TIME || currentTimeOfDay == TIME_IN_WORDS.AFTERNOON || currentTimeOfDay == TIME_IN_WORDS.EARLY_NIGHT) {
                    int chance = UnityEngine.Random.Range(0, 100);
#if DEBUG_LOG
                    log += $"\n  -Time of Day: {currentTimeOfDay}";
                    log += $"\n  -RNG roll: {chance}";
#endif
                    if (chance < 10) {
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
                log += "\n-Otherwise, if it is Morning or Afternoon, 15% chance to someone with a positive relationship in current location and then set it as the Base Structure for 2.5 hours";
#endif
                if (currentTimeOfDay == TIME_IN_WORDS.MORNING || currentTimeOfDay == TIME_IN_WORDS.LUNCH_TIME || currentTimeOfDay == TIME_IN_WORDS.AFTERNOON) {
                    int chance = UnityEngine.Random.Range(0, 100);
#if DEBUG_LOG
                    log += $"\n  -Time of Day: {currentTimeOfDay}";
                    log += $"\n  -RNG roll: {chance}";
#endif
                    if (chance < 15) {
                        WeightedDictionary<Character> visitWeights = GetCharacterToVisitWeights(character);
                        if (visitWeights.GetTotalOfWeights() > 0) {
                            Character targetCharacter = visitWeights.PickRandomElementGivenWeights();
                            LocationStructure targetStructure = targetCharacter.homeStructure;
                            Assert.IsNotNull(targetStructure, $"Home structure of visit target {targetCharacter.name} is null!");
#if DEBUG_LOG
                            log += $"\n  -Morning or Afternoon: {character.name} will go to dwelling of character with positive relationship, {targetCharacter.name} and set Base Structure for 2.5 hours";
#endif
                            character.PlanFixedJob(JOB_TYPE.VISIT_FRIEND, INTERACTION_TYPE.VISIT, targetCharacter, out producedJob, 
                                new OtherData[] { new LocationStructureOtherData(targetStructure), new CharacterOtherData(targetCharacter), });
                            return true;
                        } else {
#if DEBUG_LOG
                            log += "\n  -No valid character to visit.";
#endif
                        }
                    }
                } else {
#if DEBUG_LOG
                    log += $"\n  -Time of Day: {currentTimeOfDay}";
#endif
                }
                int sitChance = UnityEngine.Random.Range(0, 100);
#if DEBUG_LOG
                log += "\n-Otherwise, 15% chance to sit if there is still an unoccupied Table or Desk";
                log += $"\n  -RNG roll: {sitChance}";
#endif
                if (sitChance < 15) {
                    TileObject deskOrTable = character.currentStructure.GetUnoccupiedBuiltTileObject(TILE_OBJECT_TYPE.DESK, TILE_OBJECT_TYPE.TABLE);
                    if (deskOrTable != null) {
#if DEBUG_LOG
                        log += $"\n  -{character.name} will do action Sit on {deskOrTable}";
#endif
                        character.PlanFixedJob(JOB_TYPE.IDLE_SIT, INTERACTION_TYPE.SIT, deskOrTable, out producedJob);
                        return true;
                    } else {
#if DEBUG_LOG
                        log += "\n  -No unoccupied Table or Desk";
#endif
                    }
                }
#if DEBUG_LOG
                log += "\n-Otherwise, 15% chance add a Cry Interrupt";
#endif
                if (GameUtilities.RollChance(15, ref log)) {
#if DEBUG_LOG
                    log += $"\n  -{character.name} will do cry";
#endif
                    character.interruptComponent.TriggerInterrupt(INTERRUPT.Cry, character, identifier: "suicidal");
                    producedJob = null;
                    return true;
                }
#if DEBUG_LOG
                log += "\n-Otherwise, Create Commit Suicide Job";
#endif
                if (character.jobComponent.TriggerSuicideJob(out producedJob, "Suicidal Mental Break")) {
                    return true;
                }
            }
        }
        producedJob = null;
        return false;
    }
}