﻿using UnityEngine;
using System.Collections.Generic;
using Traits;
using UtilityScripts;

public class WorkBehaviour : CharacterBehaviourComponent {
    public WorkBehaviour() {
        priority = 16;
        //attributes = new[] { BEHAVIOUR_COMPONENT_ATTRIBUTE.WITHIN_HOME_SETTLEMENT_ONLY };
    }
    
    public override bool TryDoBehaviour(Character character, ref string log) {
        log += $"\n-{character.name} will try to do settlement work...";
        if (character.moodComponent.moodState == MOOD_STATE.NORMAL) {
            log += $"\n-{character.name} is in normal mood, will do settlement work";
            return PlanWorkActions(character);
        } else {
            log += $"\n-{character.name} is low/critical mood, 4% chance - flaw, 4% chance - undermine";
            bool triggeredFlaw = false;
            if (TraitManager.Instance.CanStillTriggerFlaws(character)) {
                int roll = UnityEngine.Random.Range(0, 100);
                log += $"\n-Flaw Roll: " + roll;
                if (roll < 4) {
                    List<Trait> flawTraits = new List<Trait>();
                    for (int i = 0; i < character.traitContainer.traits.Count; i++) {
                        Trait currTrait = character.traitContainer.traits[i];
                        if (currTrait.type == TRAIT_TYPE.FLAW && currTrait.canBeTriggered) {
                            flawTraits.Add(currTrait);
                        }
                    }
                    if(flawTraits.Count > 0) {
                        Trait chosenFlaw = flawTraits[UnityEngine.Random.Range(0, flawTraits.Count)];
                        string logKey = chosenFlaw.TriggerFlaw(character);
                        if (logKey == "flaw_effect") {
                            log += $"\n-{character.name} triggered flaw: " + chosenFlaw.name;
                            triggeredFlaw = true;
                        } else {
                            log += $"\n-{character.name} failed to trigger flaw: " + chosenFlaw.name;
                        }
                    } else {
                        log += $"\n-{character.name} has no Flaws to trigger";
                    }
                }
            } else {
                log += $"\n-{character.name} can no longer trigger flaws";
            }

            if (triggeredFlaw) {
                return true;
            } else {
                if (character.traitContainer.HasTrait("Diplomatic") == false) {
                    log += $"\n-{character.name} will try to trigger Undermine";
                    int roll = UnityEngine.Random.Range(0, 100);
                    log += $"\n-Undermine Roll: " + roll;
                    if (roll < 4) {
                        List<Character> enemies = character.relationshipContainer.GetEnemyCharacters();
                        if (enemies.Count > 0) {
                            Character chosenEnemy = CollectionUtilities.GetRandomElement(enemies);
                            if(UnityEngine.Random.Range(0, 2) == 0) {
                                //Place Trap
                                character.jobComponent.CreatePlaceTrapJob(chosenEnemy);
                            } else {
                                //Poison Food
                                character.jobComponent.CreatePoisonFoodJob(chosenEnemy);
                            }
                            //if (chosenEnemy.homeRegion.GetFirstTileObjectOnTheFloorOwnedBy(chosenEnemy) != null) {
                            //    if (character.jobComponent.CreateUndermineJob(chosenEnemy, "normal")) {
                            //        log += $"\n-{character.name} created undermine job for " + chosenEnemy;
                            //        return true;
                            //    }
                            //    else {
                            //        log += $"\n-{character.name} could not create undermine job for " + chosenEnemy;
                            //    }
                            //} else {
                            //    log += $"\n-{chosenEnemy.name} does not have an owned item on the floor ";
                            //}
                        } else {
                            log += $"\n-{character.name} does not have enemy or rival";
                        }
                    }    
                }
            }
        }
        //if (!PlanJobQueueFirst(character)) {
        //    if (!character.needsComponent.PlanFullnessRecoveryActions(character)) {
        //        if (!character.needsComponent.PlanTirednessRecoveryActions(character)) {
        //            if (!character.needsComponent.PlanHappinessRecoveryActions(character)) {
        //                if (!PlanWorkActions(character)) {
        //                    return false;
        //                }
        //            }
        //        }
        //    }
        //}
        return false;
    }
    private bool PlanWorkActions(Character character) {
        if (character.isAtHomeRegion && character.homeSettlement != null && character.canTakeJobs) { //&& this.faction.id != FactionManager.Instance.neutralFaction.id
            //check npcSettlement job queue, if it has any jobs that target an object that is in view of the character
            JobQueueItem jobToAssign = character.homeSettlement.GetFirstJobBasedOnVision(character);
            if (jobToAssign == null) {
                //if there are none, check the characters faction job queue under the same conditions.
                if (character.faction?.activeFactionQuest != null) {
                    jobToAssign = character.faction.activeFactionQuest.GetFirstJobBasedOnVision(character);
                }
            }
            if (jobToAssign != null) {
                character.jobQueue.AddJobInQueue(jobToAssign);
                //took job based from vision
                return true;
            } else {
                //if none of the jobs targets can be seen by the character, try and get a job from the npcSettlement or faction
                //regardless of vision instead.
                jobToAssign = character.homeSettlement.GetFirstUnassignedJobToCharacterJob(character);
                if (jobToAssign == null) {
                    if (character.faction?.activeFactionQuest != null) {
                        jobToAssign = character.faction.activeFactionQuest.GetFirstUnassignedJobToCharacterJob(character);
                    }
                }
                if (jobToAssign != null) {
                    character.jobQueue.AddJobInQueue(jobToAssign);
                    return true;
                }    
            }
        }
        return false;
    }
}
