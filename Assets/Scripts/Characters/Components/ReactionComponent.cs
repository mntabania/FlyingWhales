using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using Traits;
using Inner_Maps;
using Interrupts;
using Inner_Maps.Location_Structures;

public class ReactionComponent {
    public Character owner { get; private set; }

    public ReactionComponent(Character owner) {
        this.owner = owner;
    }

    #region Processes
    public void ReactTo(IPointOfInterest targetTileObject, ref string debugLog) {
        if (targetTileObject.poiType == POINT_OF_INTEREST_TYPE.CHARACTER) {
            ReactTo(targetTileObject as Character, ref debugLog);
        } else if (targetTileObject.poiType == POINT_OF_INTEREST_TYPE.TILE_OBJECT) {
            ReactTo(targetTileObject as TileObject, ref debugLog);
        } 
        // else if (targetTileObject.poiType == POINT_OF_INTEREST_TYPE.ITEM) {
        //     ReactTo(targetTileObject as SpecialToken, ref debugLog);
        // }
        if (owner.minion != null || owner is Summon || owner.faction == FactionManager.Instance.zombieFaction) {
            //Minions or Summons cannot react to its own traits
            return;
        }
        if (owner.isInCombat) {
            return;
        }
        debugLog += "\n-Character will loop through all his/her traits to react to Target";
        for (int i = 0; i < owner.traitContainer.allTraits.Count; i++) {
            debugLog += $"\n - {owner.traitContainer.allTraits[i].name}";
            if (owner.traitContainer.allTraits[i].OnSeePOI(targetTileObject, owner)) {
                debugLog += ": triggered";
            } else {
                debugLog += ": not triggered";
            }
        }
    }
    public string ReactTo(ActualGoapNode node, REACTION_STATUS status) {
        if (owner.minion != null || owner is Summon || owner.faction == FactionManager.Instance.zombieFaction) {
            //Minions or Summons cannot react to actions
            return string.Empty;
        }
        if(status == REACTION_STATUS.WITNESSED) {
            ReactToWitnessedAction(node);
        } else {
            return ReactToInformedAction(node);
        }
        return string.Empty;
    }
    public void ReactTo(Interrupt interrupt, Character actor, IPointOfInterest target, REACTION_STATUS status) {
        if (owner.isInCombat) {
            return;
        }
        if (owner.minion != null || owner is Summon || owner.faction == FactionManager.Instance.zombieFaction) {
            //Minions or Summons cannot react to interrupts
            return;
        }
        if (owner.faction != actor.faction && owner.faction.IsHostileWith(actor.faction)) {
            //Must not react if the faction of the actor of witnessed action is hostile with the faction of the witness
            return;
        }
        if (actor != owner && target != owner) {
            if(actor.interruptComponent.currentInterrupt == interrupt && actor.interruptComponent.currentEffectLog != null) {
                Log witnessLog = new Log(GameManager.Instance.Today(), "Character", "Generic", "witness_event");
                witnessLog.AddToFillers(owner, owner.name, LOG_IDENTIFIER.OTHER);
                witnessLog.AddToFillers(null,  UtilityScripts.Utilities.LogDontReplace(actor.interruptComponent.currentEffectLog), LOG_IDENTIFIER.APPEND);
                witnessLog.AddToFillers(actor.interruptComponent.currentEffectLog.fillers);
                owner.logComponent.AddHistory(witnessLog);
            }
            string emotionsToActor = interrupt.ReactionToActor(owner, actor, target, interrupt, status);
            if (emotionsToActor != string.Empty && !CharacterManager.Instance.EmotionsChecker(emotionsToActor)) {
                string error = "Interrupt Error in Witness Reaction To Actor (Duplicate/Incompatible Emotions Triggered)";
                error += $"\n-Witness: {owner}";
                error += $"\n-Interrupt: {interrupt.name}";
                error += $"\n-Actor: {actor.name}";
                error += $"\n-Target: {target.nameWithID}";
                owner.logComponent.PrintLogErrorIfActive(error);
            }
            string emotionsToTarget = interrupt.ReactionToTarget(owner, actor, target, interrupt, status);
            if (emotionsToTarget != string.Empty && !CharacterManager.Instance.EmotionsChecker(emotionsToTarget)) {
                string error = "Interrupt Error in Witness Reaction To Actor (Duplicate/Incompatible Emotions Triggered)";
                error += $"\n-Witness: {owner}";
                error += $"\n-Interrupt: {interrupt.name}";
                error += $"\n-Actor: {actor.name}";
                error += $"\n-Target: {target.nameWithID}";
                owner.logComponent.PrintLogErrorIfActive(error);
            }
            string response =
                $"Witness interrupt reaction of {owner.name} to {interrupt.name} of {actor.name} with target {target.name}: {emotionsToActor}{emotionsToTarget}";
            owner.logComponent.PrintLogIfActive(response);
        } else if (target == owner) {
            string emotionsOfTarget = interrupt.ReactionOfTarget(actor, target, interrupt, status);
            if (emotionsOfTarget != string.Empty && !CharacterManager.Instance.EmotionsChecker(emotionsOfTarget)) {
                string error = "Interrupt Error in Witness Reaction To Actor (Duplicate/Incompatible Emotions Triggered)";
                error += $"\n-Witness: {owner}";
                error += $"\n-Interrupt: {interrupt.name}";
                error += $"\n-Actor: {actor.name}";
                error += $"\n-Target: {target.nameWithID}";
                owner.logComponent.PrintLogErrorIfActive(error);
            }
            string response =
                $"Witness interrupt reaction of {owner.name} to {interrupt.name} of {actor.name} with target {target.name}: {emotionsOfTarget}";
            owner.logComponent.PrintLogIfActive(response);
        }
    }
    private void ReactToWitnessedAction(ActualGoapNode node) {
        if (owner.isInCombat) {
            return;
        }
        if (owner.faction != node.actor.faction && owner.faction.IsHostileWith(node.actor.faction)) {
            //Must not react if the faction of the actor of witnessed action is hostile with the faction of the witness
            return;
        }
        //if (witnessedEvent.currentStateName == null) {
        //    throw new System.Exception(GameManager.Instance.TodayLogString() + this.name + " witnessed event " + witnessedEvent.action.goapName + " by " + witnessedEvent.actor.name + " but it does not have a current state!");
        //}
        if (string.IsNullOrEmpty(node.currentStateName)) {
            return;
        }
        if (node.descriptionLog == null) {
            throw new Exception(
                $"{GameManager.Instance.TodayLogString()}{owner.name} witnessed event {node.action.goapName} by {node.actor.name} with state {node.currentStateName} but it does not have a description log!");
        }
        IPointOfInterest target = node.poiTarget;
        if(node.poiTarget is TileObject && node.action.goapType == INTERACTION_TYPE.STEAL) {
            TileObject item = node.poiTarget as TileObject;
            if(item.isBeingCarriedBy != null) {
                target = item.isBeingCarriedBy;
            }
        }
        if(node.actor != owner && target != owner) {
            Log witnessLog = new Log(GameManager.Instance.Today(), "Character", "Generic", "witness_event", node);
            witnessLog.AddToFillers(owner, owner.name, LOG_IDENTIFIER.OTHER);
            witnessLog.AddToFillers(null, UtilityScripts.Utilities.LogDontReplace(node.descriptionLog), LOG_IDENTIFIER.APPEND);
            witnessLog.AddToFillers(node.descriptionLog.fillers);
            owner.logComponent.AddHistory(witnessLog);

            string emotionsToActor = node.action.ReactionToActor(owner, node, REACTION_STATUS.WITNESSED);
            if(emotionsToActor != string.Empty && !CharacterManager.Instance.EmotionsChecker(emotionsToActor)) {
                string error = "Action Error in Witness Reaction To Actor (Duplicate/Incompatible Emotions Triggered)";
                error += $"\n-Witness: {owner}";
                error += $"\n-Action: {node.action.goapName}";
                error += $"\n-Actor: {node.actor.name}";
                error += $"\n-Target: {node.poiTarget.nameWithID}";
                owner.logComponent.PrintLogErrorIfActive(error);
            }
            string emotionsToTarget = node.action.ReactionToTarget(owner, node, REACTION_STATUS.WITNESSED);
            if (emotionsToTarget != string.Empty && !CharacterManager.Instance.EmotionsChecker(emotionsToTarget)) {
                string error = "Action Error in Witness Reaction To Target (Duplicate/Incompatible Emotions Triggered)";
                error += $"\n-Witness: {owner}";
                error += $"\n-Action: {node.action.goapName}";
                error += $"\n-Actor: {node.actor.name}";
                error += $"\n-Target: {node.poiTarget.nameWithID}";
                owner.logComponent.PrintLogErrorIfActive(error);
            }
            string response =
                $"Witness action reaction of {owner.name} to {node.action.goapName} of {node.actor.name} with target {node.poiTarget.name}: {emotionsToActor}{emotionsToTarget}";
            owner.logComponent.PrintLogIfActive(response);
        } else if (target == owner) {
            if (!node.isStealth || target.traitContainer.HasTrait("Vigilant")) {
                string emotionsOfTarget = node.action.ReactionOfTarget(node, REACTION_STATUS.WITNESSED);
                if (emotionsOfTarget != string.Empty && !CharacterManager.Instance.EmotionsChecker(emotionsOfTarget)) {
                    string error = "Action Error in Witness Reaction Of Target (Duplicate/Incompatible Emotions Triggered)";
                    error += $"\n-Witness: {owner}";
                    error += $"\n-Action: {node.action.goapName}";
                    error += $"\n-Actor: {node.actor.name}";
                    error += $"\n-Target: {node.poiTarget.nameWithID}";
                    owner.logComponent.PrintLogErrorIfActive(error);
                }
                string response =
                    $"Witness action reaction of {owner.name} to {node.action.goapName} of {node.actor.name} with target {node.poiTarget.name}: {emotionsOfTarget}";
                owner.logComponent.PrintLogIfActive(response);
            }
        }

        //CRIME_TYPE crimeType = CrimeManager.Instance.GetCrimeTypeConsideringAction(node);
        //if (crimeType != CRIME_TYPE.NONE) {
        //    CrimeManager.Instance.ReactToCrime(owner, node, node.associatedJobType, crimeType);
        //}
    }
    private string ReactToInformedAction(ActualGoapNode node) {
        if (node.descriptionLog == null) {
            throw new Exception(
                $"{GameManager.Instance.TodayLogString()}{owner.name} informed event {node.action.goapName} by {node.actor.name} with state {node.currentStateName} but it does not have a description log!");
        }
        Log informedLog = new Log(GameManager.Instance.Today(), "Character", "Generic", "informed_event", node);
        informedLog.AddToFillers(node.descriptionLog.fillers);
        informedLog.AddToFillers(owner, owner.name, LOG_IDENTIFIER.OTHER);
        informedLog.AddToFillers(null, UtilityScripts.Utilities.LogDontReplace(node.descriptionLog), LOG_IDENTIFIER.APPEND);
        owner.logComponent.AddHistory(informedLog);

        string response = string.Empty;
        if (node.actor != owner && node.poiTarget != owner) {
            string emotionsToActor = node.action.ReactionToActor(owner, node, REACTION_STATUS.INFORMED);
            if (emotionsToActor != string.Empty && !CharacterManager.Instance.EmotionsChecker(emotionsToActor)) {
                string error = "Action Error in Witness Reaction To Actor (Duplicate/Incompatible Emotions Triggered)";
                error += $"\n-Witness: {owner}";
                error += $"\n-Action: {node.action.goapName}";
                error += $"\n-Actor: {node.actor.name}";
                error += $"\n-Target: {node.poiTarget.nameWithID}";
                owner.logComponent.PrintLogErrorIfActive(error);
            }
            string emotionsToTarget = node.action.ReactionToTarget(owner, node, REACTION_STATUS.INFORMED);
            if (emotionsToTarget != string.Empty && !CharacterManager.Instance.EmotionsChecker(emotionsToTarget)) {
                string error = "Action Error in Witness Reaction To Target (Duplicate/Incompatible Emotions Triggered)";
                error += $"\n-Witness: {owner}";
                error += $"\n-Action: {node.action.goapName}";
                error += $"\n-Actor: {node.actor.name}";
                error += $"\n-Target: {node.poiTarget.nameWithID}";
                owner.logComponent.PrintLogErrorIfActive(error);
            }
            response += $"{emotionsToActor}/{emotionsToTarget}";
        } else if(node.poiTarget == owner && node.poiTarget is Character) {
            string emotionsOfTarget = node.action.ReactionOfTarget(node, REACTION_STATUS.INFORMED);
            if (emotionsOfTarget != string.Empty && !CharacterManager.Instance.EmotionsChecker(emotionsOfTarget)) {
                string error = "Action Error in Witness Reaction Of Target (Duplicate/Incompatible Emotions Triggered)";
                error += $"\n-Witness: {owner}";
                error += $"\n-Action: {node.action.goapName}";
                error += $"\n-Actor: {node.actor.name}";
                error += $"\n-Target: {node.poiTarget.nameWithID}";
                owner.logComponent.PrintLogErrorIfActive(error);
            }
            response = emotionsOfTarget;
        }
        // else if (node.actor == owner) {
        //     response = "I know what I did.";
        // }
        return response;
        //CRIME_TYPE crimeType = CrimeManager.Instance.GetCrimeTypeConsideringAction(node);
        //if (crimeType != CRIME_TYPE.NONE) {
        //    CrimeManager.Instance.ReactToCrime(owner, node, node.associatedJobType, crimeType);
        //}
    }
    private void ReactTo(Character targetCharacter, ref string debugLog) {
        debugLog += $"{owner.name} is reacting to {targetCharacter.name}";
        if(owner.faction.IsHostileWith(targetCharacter.faction)) {
            debugLog += "\n-Target is hostile";
            if (!targetCharacter.isDead) {
                debugLog += "\n-Target is not dead";
                debugLog += "\n-Fight or Flight response";
                //Fight or Flight
                if (owner.combatComponent.combatMode == COMBAT_MODE.Aggressive) {
                    bool isLethal = !owner.behaviourComponent.isHarassing && !owner.behaviourComponent.isRaiding;
                    bool isTopPrioJobLethal = owner.jobQueue.jobsInQueue.Count <= 0 || owner.jobQueue.jobsInQueue[0].jobType.IsJobLethal();
                    if (owner.jobQueue.jobsInQueue.Count > 0) {
                        debugLog += $"\n-{owner.jobQueue.jobsInQueue[0].jobType}";
                    }
                    if (!targetCharacter.traitContainer.HasTrait("Unconscious") || (isLethal && isTopPrioJobLethal)) {
                        owner.combatComponent.FightOrFlight(targetCharacter, isLethal);
                    }
                }
            } else {
                debugLog += "\n-Target is dead";
                debugLog += "\n-Do nothing";
            }
        } else if (!owner.isInCombat) {
            debugLog += "\n-Target is not hostile and Character is not in combat";
            if (owner.minion == null && !(owner is Summon) && owner.faction != FactionManager.Instance.zombieFaction && !IsPOICurrentlyTargetedByAPerformingAction(targetCharacter)) {
                debugLog += "\n-Character is not minion and not summon and Target is not being targeted by an action, continue reaction";
                if (!targetCharacter.isDead) {
                    debugLog += "\n-Target is not dead";
                    if (!owner.isConversing && !targetCharacter.isConversing && owner.nonActionEventsComponent.CanInteract(targetCharacter)) {
                        debugLog += "\n-Character and Target are not Chatting or Flirting and Character can interact with Target, has 3% chance to Chat";
                        int chance = UnityEngine.Random.Range(0, 100);
                        debugLog += $"\n-Roll: {chance}";
                        if (chance < 3) {
                            debugLog += "\n-Chat triggered";
                            owner.interruptComponent.TriggerInterrupt(INTERRUPT.Chat, targetCharacter);
                        } else {
                            debugLog += "\n-Chat did not trigger, will now trigger Flirt if Character is Sexually Compatible with Target and Character is Unfaithful, or Target is Lover or Affair, or Character has no Lover";
                            if (RelationshipManager.IsSexuallyCompatibleOneSided(owner.sexuality, targetCharacter.sexuality, owner.gender, targetCharacter.gender)
                                && owner.relationshipContainer.IsFamilyMember(targetCharacter) == false) {
                                if (owner.relationshipContainer.HasRelationshipWith(targetCharacter, RELATIONSHIP_TYPE.LOVER, RELATIONSHIP_TYPE.AFFAIR)
                                    || owner.relationshipContainer.GetFirstRelatableIDWithRelationship(RELATIONSHIP_TYPE.LOVER) == -1
                                    || owner.traitContainer.HasTrait("Unfaithful")) {
                                    debugLog += "\n-Flirt has 1% (multiplied by Compatibility value) chance to trigger";
                                    int compatibility = RelationshipManager.Instance.GetCompatibilityBetween(owner, targetCharacter);
                                    int value = 2;
                                    if (compatibility != -1) {
                                        value = 1 * compatibility;
                                        debugLog += $"\n-Chance: {value}";
                                    } else {
                                        debugLog += $"\n-Chance: {value} (No Compatibility)";
                                    }
                                    int flirtChance = UnityEngine.Random.Range(0, 100);
                                    debugLog += $"\n-Roll: {flirtChance}";
                                    if (flirtChance < value) {
                                        owner.interruptComponent.TriggerInterrupt(INTERRUPT.Flirt, targetCharacter);
                                    } else {
                                        debugLog += "\n-Flirt did not trigger";
                                    }
                                } else {
                                    debugLog += "\n-Flirt did not trigger";
                                }
                            }
                        }
                    }

                    if (owner.faction == targetCharacter.faction || owner.homeSettlement == targetCharacter.homeSettlement) {
                        debugLog += "\n-Character and Target are with the same faction or settlement";
                        if (owner.relationshipContainer.IsEnemiesWith(targetCharacter)) {
                            debugLog += "\n-Character considers Target as Enemy or Rival";
                            if (!targetCharacter.canMove || !targetCharacter.canPerform) {
                                debugLog += "\n-Target can neither move or perform, will trigger Mock or Laugh At interrupt";
                                if (UnityEngine.Random.Range(0, 2) == 0) {
                                    debugLog += "\n-Character triggered Mock interrupt";
                                    owner.interruptComponent.TriggerInterrupt(INTERRUPT.Mock, targetCharacter);
                                } else {
                                    debugLog += "\n-Character triggered Laugh At interrupt";
                                    owner.interruptComponent.TriggerInterrupt(INTERRUPT.Laugh_At, targetCharacter);
                                }
                            }
                        } else if (!owner.traitContainer.HasTrait("Psychopath")) {
                            debugLog += "\n-Character is not Psychopath and does not consider Target as Enemy or Rival";
                            if (!targetCharacter.canMove/* || !targetCharacter.canWitness*/) {
                                debugLog += "\n-Target cannot move"; // or cannot witness
                                if (targetCharacter.needsComponent.isHungry || targetCharacter.needsComponent.isStarving) {
                                    debugLog += "\n-Target is hungry or starving, will create feed job";
                                    owner.jobComponent.TryTriggerFeed(targetCharacter);
                                } else if (targetCharacter.needsComponent.isTired || targetCharacter.needsComponent.isExhausted) {
                                    debugLog += "\n-Target is tired or exhausted, will create Move Character job to bed if Target has a home and an available bed";
                                    if (targetCharacter.homeStructure != null) {
                                        Bed bed = targetCharacter.homeStructure.GetUnoccupiedTileObject(TILE_OBJECT_TYPE.BED) as Bed;
                                        if (bed != null && bed.gridTileLocation != targetCharacter.gridTileLocation) {
                                            debugLog += "\n-Target has a home and an available bed, will trigger Move Character job to bed";
                                            owner.jobComponent.TryTriggerMoveCharacter(targetCharacter, targetCharacter.homeStructure.GetLocationStructure(), bed.gridTileLocation);
                                        } else {
                                            debugLog += "\n-Target has a home but does not have an available bed or already in bed, will not trigger Move Character job";
                                        }
                                    } else {
                                        debugLog += "\n-Target does not have a home, will not trigger Move Character job";
                                    }
                                } else if (targetCharacter.needsComponent.isBored || targetCharacter.needsComponent.isSulking) {
                                    debugLog += "\n-Target is bored or sulking, will trigger Move Character job if character is not in the right place to do Daydream or Pray";
                                    if (UnityEngine.Random.Range(0, 2) == 0 && targetCharacter.homeStructure != null) {
                                        //Pray
                                        if (targetCharacter.currentStructure != targetCharacter.homeStructure) {
                                            debugLog += "\n-Target chose Pray and is not inside his/her house, will trigger Move Character job";
                                            owner.jobComponent.TryTriggerMoveCharacter(targetCharacter, targetCharacter.homeStructure.GetLocationStructure());
                                        } else {
                                            debugLog += "\n-Target chose Pray but is already inside his/her house, will not trigger Move Character job";
                                        }
                                    } else {
                                        //Daydream
                                        if (!targetCharacter.currentStructure.structureType.IsOpenSpace()) {
                                            debugLog += "\n-Target chose Daydream and is not in an open space structure, will trigger Move Character job";
                                            owner.jobComponent.TryTriggerMoveCharacter(targetCharacter, targetCharacter.currentRegion.GetRandomStructureOfType(STRUCTURE_TYPE.WILDERNESS));
                                        } else {
                                            debugLog += "\n-Target chose Daydream but is already in an open space structure, will not trigger Move Character job";
                                        }
                                    }
                                }
                            }
                        }
                    } else {
                        debugLog += "\n-Character and Target are not with the same faction and settlement";
                        if (owner.relationshipContainer.IsEnemiesWith(targetCharacter)) {
                            debugLog += "\n-Character considers Target as Enemy or Rival, will trigger Fight or Flight response";
                            //Fight or Flight
                            if (owner.combatComponent.combatMode == COMBAT_MODE.Aggressive) {
                                bool isTopPrioJobLethal = owner.jobQueue.jobsInQueue.Count <= 0 || owner.jobQueue.jobsInQueue[0].jobType.IsJobLethal();
                                if (!targetCharacter.traitContainer.HasTrait("Unconscious") || isTopPrioJobLethal) {
                                    owner.combatComponent.FightOrFlight(targetCharacter);
                                }
                            }
                        }
                    }
                } else {
                    debugLog += "\n-Target is dead";
                    debugLog += "\n-Do nothing";
                }
            } else {
                debugLog += "\n-Character is minion or summon or Target is currently being targeted by an action, not going to react";
            }
        }
    }
    private void ReactTo(TileObject targetTileObject, ref string debugLog) {
        if (owner.minion != null || owner is Summon || owner.faction == FactionManager.Instance.zombieFaction) {
            //Minions or Summons cannot react to objects
            return;
        }
        debugLog += $"{owner.name} is reacting to {targetTileObject.nameWithID}";
        if (!owner.isInCombat && !owner.hasSeenFire) {
            if (targetTileObject.traitContainer.HasTrait("Burning")
                && targetTileObject.gridTileLocation != null
                && targetTileObject.gridTileLocation.IsPartOfSettlement(owner.homeSettlement)
                && !owner.traitContainer.HasTrait("Pyrophobic")
                && !owner.jobQueue.HasJob(JOB_TYPE.DOUSE_FIRE)) {
                debugLog += "\n-Target is Burning and Character is not Pyrophobic";
                owner.SetHasSeenFire(true);
                owner.homeSettlement.settlementJobTriggerComponent.TriggerDouseFire();
                for (int i = 0; i < owner.homeSettlement.availableJobs.Count; i++) {
                    JobQueueItem job = owner.homeSettlement.availableJobs[i];
                    if (job.jobType == JOB_TYPE.DOUSE_FIRE) {
                        if (job.assignedCharacter == null && owner.jobQueue.CanJobBeAddedToQueue(job)) {
                            owner.jobQueue.AddJobInQueue(job);
                        } else {
                            if (owner.combatComponent.combatMode == COMBAT_MODE.Aggressive) {
                                owner.combatComponent.Flight(targetTileObject, "saw fire");
                            }
                        }
                        return;
                    }
                }
            }
        }

        if (targetTileObject is TornadoTileObject) {
            if (!owner.traitContainer.HasTrait("Elemental Master")) {
                if (owner.combatComponent.combatMode == COMBAT_MODE.Aggressive) {
                    if (owner.traitContainer.HasTrait("Berserked")) {
                        owner.combatComponent.FightOrFlight(targetTileObject);
                    } else {
                        owner.combatComponent.Flight(targetTileObject, "saw a tornado");
                    }
                }
            }
        } else {
            if (targetTileObject.tileObjectType.IsTileObjectAnItem()) {
                if (targetTileObject.gridTileLocation != null && targetTileObject.gridTileLocation.structure != owner.homeSettlement.mainStorage && targetTileObject.gridTileLocation.structure is Dwelling) {
                    owner.jobComponent.CreateTakeItemJob(targetTileObject);
                }
            }
        }
    }
    // private void ReactTo(SpecialToken targetItem, ref string debugLog) {
    //     if (owner.minion != null || owner is Summon) {
    //         //Minions or Summons cannot react to items
    //         return;
    //     }
    //     debugLog += owner.name + " is reacting to " + targetItem.nameWithID;
    //     if (!owner.hasSeenFire) {
    //         if (targetItem.traitContainer.HasTrait("Burning")
    //             && targetItem.gridTileLocation != null
    //             && targetItem.gridTileLocation.IsPartOfSettlement(owner.homeSettlement)
    //             && !owner.traitContainer.HasTrait("Pyrophobic")) {
    //             debugLog += "\n-Target is Burning and Character is not Pyrophobic";
    //             owner.SetHasSeenFire(true);
    //             owner.homeSettlement.settlementJobTriggerComponent.TriggerDouseFire();
    //             for (int i = 0; i < owner.homeSettlement.availableJobs.Count; i++) {
    //                 JobQueueItem job = owner.homeSettlement.availableJobs[i];
    //                 if (job.jobType == JOB_TYPE.DOUSE_FIRE) {
    //                     if (job.assignedCharacter == null && owner.jobQueue.CanJobBeAddedToQueue(job)) {
    //                         owner.jobQueue.AddJobInQueue(job);
    //                     } else {
    //                         owner.combatComponent.Flight(targetItem);
    //                     }
    //                     return;
    //                 }
    //             }
    //         }
    //     }
    // }
    #endregion

    #region General
    private bool IsPOICurrentlyTargetedByAPerformingAction(IPointOfInterest poi) {
        for (int i = 0; i < poi.allJobsTargetingThis.Count; i++) {
            if(poi.allJobsTargetingThis[i] is GoapPlanJob) {
                GoapPlanJob planJob = poi.allJobsTargetingThis[i] as GoapPlanJob;
                if(planJob.assignedPlan != null && planJob.assignedPlan.currentActualNode.actionStatus == ACTION_STATUS.PERFORMING) {
                    return true;
                }
            }
        }
        return false;
    }
    #endregion
}
