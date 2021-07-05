using UnityEngine;
using System.Collections.Generic;
using Traits;
using UtilityScripts;
using Inner_Maps.Location_Structures;
using UnityEngine.Assertions;

public class WorkBehaviour : CharacterBehaviourComponent {
    public WorkBehaviour() {
        priority = 16;
        // attributes = new[] { BEHAVIOUR_COMPONENT_ATTRIBUTE.WITHIN_HOME_SETTLEMENT_ONLY };
    }
    
    public override bool TryDoBehaviour(Character character, ref string log, out JobQueueItem producedJob) {
#if DEBUG_LOG
        log = $"{log}\n-{character.name} will try to do work behaviour...";
#endif
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
        DAILY_SCHEDULE currentSchedule = character.dailyScheduleComponent.schedule.GetScheduleType(GameManager.Instance.currentTick);
        if (currentSchedule == DAILY_SCHEDULE.Work) {
#if DEBUG_LOG
            log = $"{log}\n-Character is in Work Schedule";
#endif
            NPCSettlement homeSettlement = character.homeSettlement;

            if (homeSettlement != null && homeSettlement.locationType == LOCATION_TYPE.VILLAGE) {
#if DEBUG_LOG
                log = $"{log}\n-Character lives in a village";
#endif
                if (character.structureComponent.workPlaceStructure == null) {
#if DEBUG_LOG
                    log = $"{log}\n-Character has no work place yet";
#endif
                    STRUCTURE_TYPE workStructureType = CharacterManager.Instance.GetOrCreateCharacterClassData(character.characterClass.className).workStructureType;
#if DEBUG_LOG
                    log = $"{log}\n-Character will find unclaimed work structure type of " + workStructureType.ToString();
#endif
                    if (workStructureType != STRUCTURE_TYPE.NONE) {
                        if (homeSettlement.GetRandomStructureOfTypeThatCanAcceptWorkerAndIsNotReserved(workStructureType) is ManMadeStructure workerStructure) {
#if DEBUG_LOG
                            log = $"{log}\n-Found valid work structure: " + workerStructure.name;
#endif
                            workerStructure.AddAssignedWorker(character);
                            character.interruptComponent.TriggerInterrupt(INTERRUPT.Claim_Work_Structure, character);
                        }
                    }
                }
                //visit hospice
                if (character.currentSettlement != null && character.currentSettlement.HasStructureClaimedByNonEnemyOrSelf(STRUCTURE_TYPE.HOSPICE, character, out LocationStructure foundStructure)) {
#if DEBUG_LOG
                    log = $"{log}\n  -There is a Hospice in the Village claimed by a non-Enemy or by self: ";
#endif
                    Hospice hospice = foundStructure as Hospice;
                    Assert.IsNotNull(hospice);
                    if ((character.traitContainer.HasTrait("Injured") || character.traitContainer.HasTrait("Plagued")) && 
                        !character.traitContainer.HasTrait("Plague Reservoir") && ChanceData.RollChance(CHANCE_TYPE.Plauged_Injured_Visit_Hospice)) {
                        //recuperate
#if DEBUG_LOG
                        log = $"{log}\n  -Actor has Injured or Plagued and there is still an available Bed in the Hospice: Create Recuperate Job";
#endif
                        BedClinic bedClinic = hospice.GetFirstBedToRecuperate();
                        if (bedClinic != null) {
                            if (character.jobComponent.TryRecuperate(bedClinic, out producedJob)) {
                                return true;
                            }
                        }
                    }
                    if (ChanceData.RollChance(CHANCE_TYPE.Vampire_Lycan_Visit_Hospice, ref log) && character.currentStructure != foundStructure && hospice.HasWorkerWithLevel5HealingMagic()) {
#if DEBUG_LOG
                        log = $"{log}\n  -Hospice is claimed by a Villager with Level 5 Healing Magic:";
#endif
                        Traits.Vampire vampire = character.traitContainer.GetTraitOrStatus<Traits.Vampire>("Vampire");
                        if (vampire != null && vampire.dislikedBeingVampire) {
                            //Go to hospice and wait there for 2 hours
#if DEBUG_LOG
                            log = $"{log}\n  -Actor has Vampirism and disliked being a Vampire";
                            character.PlanFixedJob(JOB_TYPE.VISIT_HOSPICE, INTERACTION_TYPE.VISIT, character, out producedJob,
                                new OtherData[] {new LocationStructureOtherData(hospice)});
                            return true;
#endif
                        }
                        if (character.lycanData != null && character.lycanData.dislikesBeingLycan) {
                            //Go to hospice and wait there for 2 hours
#if DEBUG_LOG
                            log = $"{log}\n  -If Actor has Lycanthropy and disliked being a Werewolf";
                            character.PlanFixedJob(JOB_TYPE.VISIT_HOSPICE, INTERACTION_TYPE.VISIT, character, out producedJob,
                                new OtherData[] {new LocationStructureOtherData(hospice)});
                            return true;
#endif
                        }
                    }
                    if (character.currentStructure == foundStructure && character.trapStructure.IsTrappedAndTrapStructureIs(foundStructure)) {
                        //do not take jobs and remain inside hospice
                        producedJob = null;
                        return false;
                    }
                }
                
                if (character.moodComponent.moodState != MOOD_STATE.Bad && character.moodComponent.moodState != MOOD_STATE.Critical) {
#if DEBUG_LOG
                    log = $"{log}\n-{character.name} is NOT in low or critical mood, will do settlement work";
#endif
                    if (character.behaviourComponent.PlanSettlementOrFactionWorkActions(out producedJob)) {
                        return true;
                    }
                }
                
                if (character.moodComponent.moodState != MOOD_STATE.Critical) {
#if DEBUG_LOG
                    log = $"{log}\n-Character is not in critical mood";
#endif

                    if (character.structureComponent.workPlaceStructure != null) {
                        
                        if (character.traitContainer.HasTrait("Lazy")) {
#if DEBUG_LOG
                            log = $"{log}\n-Actor is Lazy, check trigger rate to ignore work place processing";
#endif                            
                            Lazy lazy = character.traitContainer.GetTraitOrStatus<Lazy>("Lazy");
                            float chance = lazy.GetTriggerChance(character);
                            if (GameUtilities.RollChance(chance, ref log)) {
                                lazy.TriggerLazy();
                                producedJob = null;
                                return false;
                            }
                        }
#if DEBUG_LOG
                        log = $"{log}\n-Character has work structure: " + character.structureComponent.workPlaceStructure.name + ", 85% to add Job provided by structure";
#endif
                        if (GameUtilities.RollChance(ChanceData.GetChance(CHANCE_TYPE.Do_Work_Chance), ref log)) {
#if DEBUG_LOG
                            log = $"{log}\n-Character will try to do work structure job";
#endif
                            character.structureComponent.workPlaceStructure.ProcessWorkerBehaviour(character, out producedJob);
                            if (producedJob != null) {
                                Assert.IsNotNull(producedJob.poiTarget, $"Produced job of {character.name} is {producedJob}. But its target is null!");
#if DEBUG_LOG
                                log = $"{log}\n-Character will do work job: " + producedJob.ToString() + " with target: " + producedJob.poiTarget.name;
#endif
                                return true;
                            }
                        }
                    }
                }
            } 
            if (character.moodComponent.moodState != MOOD_STATE.Normal) {
#if DEBUG_LOG
                log = $"{log}\n-{character.name} is not in normal mood, 4% chance - flaw, 4% chance - undermine";
#endif
                bool triggeredFlaw = false;
                if (TraitManager.Instance.CanStillTriggerFlaws(character)) {
                    int roll = Random.Range(0, 100);
#if DEBUG_LOG
                    log = $"{log}\n-Flaw Roll: {roll.ToString()}";
#endif
                    if (roll < 4) {
                        List<Trait> flawTraits = RuinarchListPool<Trait>.Claim();
                        for (int i = 0; i < character.traitContainer.traits.Count; i++) {
                            Trait currTrait = character.traitContainer.traits[i];
                            if (currTrait.type == TRAIT_TYPE.FLAW && currTrait.canBeTriggered) {
                                flawTraits.Add(currTrait);
                            }
                        }
                        if (flawTraits.Count > 0) {
                            Trait chosenFlaw = flawTraits[Random.Range(0, flawTraits.Count)];
                            string logKey = chosenFlaw.TriggerFlaw(character);
                            if (logKey == "flaw_effect") {
#if DEBUG_LOG
                                log = $"{log}\n-{character.name} triggered flaw: {chosenFlaw.name}";
#endif
                                triggeredFlaw = true;
                                //When flaw is triggered, leave from party
                                //if (character.partyComponent.hasParty) {
                                //    character.partyComponent.currentParty.RemoveMember(character);
                                //}
                            } else {
#if DEBUG_LOG
                                log = $"{log}\n-{character.name} failed to trigger flaw: {chosenFlaw.name}";
#endif
                            }
                        } else {
#if DEBUG_LOG
                            log = $"{log}\n-{character.name} has no Flaws to trigger";
#endif
                        }
                        RuinarchListPool<Trait>.Release(flawTraits);
                    }
                } else {
#if DEBUG_LOG
                    log = $"{log}\n-{character.name} can no longer trigger flaws";
#endif
                }

                if (triggeredFlaw) {
                    producedJob = null;
                    return true;
                } else {
                    if (character.traitContainer.HasTrait("Diplomatic") == false && character.characterClass.className != "Hero") {
                        int roll = Random.Range(0, 100);
#if DEBUG_LOG
                        log = $"{log}\n-{character.name} will try to trigger Undermine";
                        log = $"{log}\n-Undermine Roll: {roll.ToString()}";
#endif
                        int chance = 4;
                        if (character.traitContainer.HasTrait("Treacherous")) {
                            chance += 4;
                        }
                        if (roll < chance) {
                            Character chosenEnemy = character.relationshipContainer.GetRandomEnemyCharacter();
                            if (chosenEnemy != null) {
                                if (chosenEnemy.homeSettlement != null) {
                                    if (chosenEnemy.homeSettlement.eventManager.HasActiveEvent(SETTLEMENT_EVENT.Vampire_Hunt)) {
                                        Character spreadRumorOrNegativeInfoTarget = character.rumorComponent.GetRandomSpreadRumorOrNegativeInfoTarget(chosenEnemy);
                                        if (spreadRumorOrNegativeInfoTarget != null) {
                                            Rumor rumor = character.rumorComponent.CreateNewRumor(chosenEnemy, chosenEnemy, INTERACTION_TYPE.IS_VAMPIRE);
                                            if (rumor != null) {
                                                if (character.jobComponent.CreateSpreadRumorJob(spreadRumorOrNegativeInfoTarget, rumor, out producedJob)) {
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
#if DEBUG_LOG
                                log = $"{log}\n-{character.name} does not have enemy or rival";
#endif
                            }
                        }
                    }
                }
            }
        }
        producedJob = null;
        return false;
    }
}
