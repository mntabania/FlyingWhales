using UnityEngine;
using System.Collections.Generic;
using Traits;
using UtilityScripts;

public class WorkBehaviour : CharacterBehaviourComponent {
    public WorkBehaviour() {
        priority = 16;
        attributes = new[] { BEHAVIOUR_COMPONENT_ATTRIBUTE.WITHIN_HOME_SETTLEMENT_ONLY };
    }
    
    public override bool TryDoBehaviour(Character character, ref string log, out JobQueueItem producedJob) {
        log += $"\n-{character.name} will try to do settlement work...";
        if (character.faction != null && character.faction.isMajorNonPlayer && !character.isFactionLeader && !character.isSettlementRuler && !character.crimeComponent.hasReportedCrime) {
            character.crimeComponent.SetHasReportedCrime(true);
            for (int i = 0; i < character.crimeComponent.witnessedCrimes.Count; i++) {
                CrimeData crimeData = character.crimeComponent.witnessedCrimes[i];
                if (!crimeData.isRemoved) {
                    if (!character.crimeComponent.IsReported(crimeData)) {
                        if (character.jobComponent.TryCreateReportCrimeJob(crimeData.criminal, crimeData.target, crimeData, crimeData.crime, out producedJob)) {
                            return true;
                        }
                    }
                }
            }
        }

        if (character.moodComponent.moodState == MOOD_STATE.Normal) {
            log += $"\n-{character.name} is in normal mood, will do settlement work";
            return PlanWorkActions(character, out producedJob);
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
                            //When flaw is triggered, leave from party
                            //if (character.partyComponent.hasParty) {
                            //    character.partyComponent.currentParty.RemoveMember(character);
                            //}
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
                producedJob = null;
                return true;
            } else {
                if (character.traitContainer.HasTrait("Diplomatic") == false && character.characterClass.className != "Hero") {
                    log += $"\n-{character.name} will try to trigger Undermine";
                    int roll = UnityEngine.Random.Range(0, 100);
                    log += $"\n-Undermine Roll: " + roll;
                    int chance = 4;
                    if (character.traitContainer.HasTrait("Treacherous")) {
                        chance += 4;
                    }
                    if (roll < chance) {
                        List<Character> enemies = character.relationshipContainer.GetEnemyCharacters();
                        if (enemies.Count > 0) {
                            Character chosenEnemy = CollectionUtilities.GetRandomElement(enemies);
                            if(chosenEnemy.homeSettlement != null) {
                                if (chosenEnemy.homeSettlement.eventManager.HasActiveEvent(SETTLEMENT_EVENT.Vampire_Hunt)) {
                                    Character spreadRumorOrNegativeInfoTarget = character.rumorComponent.GetRandomSpreadRumorOrNegativeInfoTarget(chosenEnemy);
                                    if (spreadRumorOrNegativeInfoTarget != null) {
                                        Rumor rumor = character.rumorComponent.CreateNewRumor(chosenEnemy, chosenEnemy, INTERACTION_TYPE.IS_VAMPIRE);
                                        if (rumor != null) {
                                            if(character.jobComponent.CreateSpreadRumorJob(spreadRumorOrNegativeInfoTarget, rumor, out producedJob)) {
                                                return true;
                                            }
                                        }
                                    }
                                }
                                if (chosenEnemy.homeSettlement.eventManager.HasActiveEvent(SETTLEMENT_EVENT.Werewolf_Hunt)) {
                                    Character spreadRumorOrNegativeInfoTarget = character.rumorComponent.GetRandomSpreadRumorOrNegativeInfoTarget(chosenEnemy);
                                    if (spreadRumorOrNegativeInfoTarget != null) {
                                        Rumor rumor = character.rumorComponent.CreateNewRumor(chosenEnemy, chosenEnemy, INTERACTION_TYPE.IS_WEREWOLF);
                                        if (rumor != null) {
                                            if (character.jobComponent.CreateSpreadRumorJob(spreadRumorOrNegativeInfoTarget, rumor, out producedJob)) {
                                                return true;
                                            }
                                        }
                                    }
                                }
                            }
                            if (chosenEnemy.HasOwnedItemThatIsOnGroundInSameRegion()) {
                                if (GameUtilities.RollChance(50)) {
                                    //Place Trap
                                    if (character.jobComponent.CreatePlaceTrapJob(chosenEnemy, out producedJob)) {
                                        return true;
                                    }
                                }
                                //Poison Food
                                if (character.jobComponent.CreatePoisonFoodJob(chosenEnemy, out producedJob)) {
                                    return true;
                                }
                            }
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
        producedJob = null;
        return false;
    }
    private bool PlanWorkActions(Character character, out JobQueueItem producedJob) {
        if (character.canTakeJobs) {
            //NOTE: ONLY ADDED FACTION CHECKING BECAUSE OF BUG THAT VAGRANTS ARE STILL PART OF A VILLAGE
            if (character.isAtHomeRegion && character.homeSettlement != null && character.homeSettlement.owner == character.faction) { //&& this.faction.id != FactionManager.Instance.neutralFaction.id
                //check npcSettlement job queue, if it has any jobs that target an object that is in view of the character
                JobQueueItem jobToAssign = character.homeSettlement.GetFirstJobBasedOnVision(character);
                if (jobToAssign != null) {
                    producedJob = jobToAssign;
                    //took job based from vision
                    return true;
                } else {
                    //if none of the jobs targets can be seen by the character, try and get a job from the npcSettlement or faction
                    //regardless of vision instead.
                    if (character.homeSettlement.HasPathTowardsTileInSettlement(character, 2)) {
                        if (character.faction != null) {
                            jobToAssign = character.faction.GetFirstUnassignedJobToCharacterJob(character);
                        }

                        //Characters should only take non-vision settlement jobs if they have a path towards the settlement
                        //Reference: https://trello.com/c/SSYDok6x/1106-characters-should-only-take-non-vision-settlement-jobs-if-they-have-a-path-towards-the-settlement
                        if (jobToAssign == null) {
                            jobToAssign = character.homeSettlement.GetFirstUnassignedJobToCharacterJob(character);
                        }
                    }

                    if (jobToAssign != null) {
                        producedJob = jobToAssign;
                        return true;
                    }
                }
            }
            if (character.faction != null) {
                JobQueueItem jobToAssign = character.faction.GetFirstUnassignedJobToCharacterJob(character);
                if (jobToAssign != null) {
                    producedJob = jobToAssign;
                    return true;
                }
            }
        }
        producedJob = null;
        return false;
    }
}
