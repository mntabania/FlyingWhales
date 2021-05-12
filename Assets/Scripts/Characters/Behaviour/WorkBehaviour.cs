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
        log = $"{log}\n-{character.name} will try to do settlement work...";
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
            log = $"{log}\n-{character.name} is in normal mood, will do settlement work";
            return character.behaviourComponent.PlanWorkActions(out producedJob);
        } else {
            log = $"{log}\n-{character.name} is low/critical mood, 4% chance - flaw, 4% chance - undermine";
            bool triggeredFlaw = false;
            if (TraitManager.Instance.CanStillTriggerFlaws(character)) {
                int roll = Random.Range(0, 100);
                log = $"{log}\n-Flaw Roll: {roll.ToString()}";
                if (roll < 4) {
                    List<Trait> flawTraits = RuinarchListPool<Trait>.Claim();
                    for (int i = 0; i < character.traitContainer.traits.Count; i++) {
                        Trait currTrait = character.traitContainer.traits[i];
                        if (currTrait.type == TRAIT_TYPE.FLAW && currTrait.canBeTriggered) {
                            flawTraits.Add(currTrait);
                        }
                    }
                    if(flawTraits.Count > 0) {
                        Trait chosenFlaw = flawTraits[Random.Range(0, flawTraits.Count)];
                        string logKey = chosenFlaw.TriggerFlaw(character);
                        if (logKey == "flaw_effect") {
                            log = $"{log}\n-{character.name} triggered flaw: {chosenFlaw.name}";
                            triggeredFlaw = true;
                            //When flaw is triggered, leave from party
                            //if (character.partyComponent.hasParty) {
                            //    character.partyComponent.currentParty.RemoveMember(character);
                            //}
                        } else {
                            log = $"{log}\n-{character.name} failed to trigger flaw: {chosenFlaw.name}";
                        }
                    } else {
                        log = $"{log}\n-{character.name} has no Flaws to trigger";
                    }
                    RuinarchListPool<Trait>.Release(flawTraits);
                }
            } else {
                log = $"{log}\n-{character.name} can no longer trigger flaws";
            }

            if (triggeredFlaw) {
                producedJob = null;
                return true;
            } else {
                if (character.traitContainer.HasTrait("Diplomatic") == false && character.characterClass.className != "Hero") {
                    log = $"{log}\n-{character.name} will try to trigger Undermine";
                    int roll = Random.Range(0, 100);
                    log = $"{log}\n-Undermine Roll: {roll.ToString()}";
                    int chance = 4;
                    if (character.traitContainer.HasTrait("Treacherous")) {
                        chance += 4;
                    }
                    if (roll < chance) {
                        Character chosenEnemy = character.relationshipContainer.GetRandomEnemyCharacter();
                        if (chosenEnemy != null) {
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
                            log = $"{log}\n-{character.name} does not have enemy or rival";
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
}
