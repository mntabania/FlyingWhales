using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Inner_Maps.Location_Structures;
using UnityEngine;  
using Traits;
using Inner_Maps;
using Locations.Settlements;

public class Sleep : GoapAction {

    public override ACTION_CATEGORY actionCategory { get { return ACTION_CATEGORY.DIRECT; } }

    private const int SleepAtTavernCost = 33;
    
    public Sleep() : base(INTERACTION_TYPE.SLEEP) {
        actionIconString = GoapActionStateDB.Sleep_Icon;
        //advertisedBy = new POINT_OF_INTEREST_TYPE[] { POINT_OF_INTEREST_TYPE.TILE_OBJECT };
        racesThatCanDoAction = new RACE[] { RACE.HUMANS, RACE.ELVES, RACE.GOBLIN, RACE.FAERY, RACE.RATMAN };
        logTags = new[] {LOG_TAG.Needs};

    }

    #region Overrides
    protected override void ConstructBasePreconditionsAndEffects() {
        AddExpectedEffect(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.TIREDNESS_RECOVERY, conditionKey = string.Empty, target = GOAP_EFFECT_TARGET.ACTOR });
        AddExpectedEffect(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.STAMINA_RECOVERY, conditionKey = string.Empty, target = GOAP_EFFECT_TARGET.ACTOR });
    }
    public override void Perform(ActualGoapNode goapNode) {
        base.Perform(goapNode);
        SetState("Rest Success", goapNode); 
    }
    protected override int GetBaseCost(Character actor, IPointOfInterest target, JobQueueItem job, OtherData[] otherData) {
#if DEBUG_LOG
        string costLog = $"\n{name} {target.nameWithID}:";
#endif
        if (actor.traitContainer.HasTrait("Enslaved")) {
            if (target.gridTileLocation == null || !target.gridTileLocation.IsInHomeOf(actor)) {
#if DEBUG_LOG
                costLog += $" +2000(Slave, target is not in actor's home)";
                actor.logComponent.AppendCostLog(costLog);
#endif
                return 2000;
            }
        }
        if (actor.partyComponent.hasParty && actor.partyComponent.currentParty.isActive) {
            if (actor.partyComponent.isActiveMember) {
                if (target.gridTileLocation != null && actor.gridTileLocation != null) {
                    LocationGridTile centerGridTileOfTarget = target.gridTileLocation.area.gridTileComponent.centerGridTile;
                    LocationGridTile centerGridTileOfActor = actor.gridTileLocation.area.gridTileComponent.centerGridTile;
                    float distance = centerGridTileOfActor.GetDistanceTo(centerGridTileOfTarget);
                    int distanceToCheck = InnerMapManager.AreaLocationGridTileSize.x * 3;

                    if (distance > distanceToCheck) {
                        //target is at structure that character is avoiding
#if DEBUG_LOG
                        costLog += $" +2000(Active Party, Location of target too far from actor)";
                        actor.logComponent.AppendCostLog(costLog);
#endif
                        return 2000;
                    }
                }
            }
        }
        int cost = 0;
        if (target.gridTileLocation != null && actor.movementComponent.structuresToAvoid.Contains(target.gridTileLocation.structure)) {
            if (!actor.partyComponent.hasParty) {
                //target is at structure that character is avoiding
                cost = 2000;
#if DEBUG_LOG
                costLog += $" +{cost}(Location of target is in avoid structure)";
                actor.logComponent.AppendCostLog(costLog);
#endif
                return cost;
            }
        }
#if DEBUG_LOG
        costLog = $"\n{name} {target.nameWithID}:";
#endif
        cost = 0;
        if (target is BaseBed) {
            BaseBed targetBed = target as BaseBed;
            if (!targetBed.IsSlotAvailable()) {
                if (actor != null && targetBed.users.Contains(actor)) {
                    cost = 10;
#if DEBUG_LOG
                    costLog += " 10(Already in bed)"; //Mainly used for quarantine
#endif
                } else {
                    cost += 2000;
#if DEBUG_LOG
                    costLog += " +2000(Fully Occupied)";
#endif
                }
            } else if (actor.traitContainer.HasTrait("Travelling")) {
                cost += 100;
#if DEBUG_LOG
                costLog += " +100(Travelling)";
#endif
            } else {
                if (targetBed.IsOwnedBy(actor) || targetBed.structureLocation == actor.homeStructure) {
                    if(actor.needsComponent.isExhausted || actor.traitContainer.HasTrait("Drunk")) {
                        cost += UtilityScripts.Utilities.Rng.Next(30, 51);
#if DEBUG_LOG
                        costLog += $" +{cost}(Owned/Location is in home structure, Exhausted/Drunk)";
#endif
                    } else {
                        cost += UtilityScripts.Utilities.Rng.Next(5, 16);
#if DEBUG_LOG
                        costLog += $" +{cost}(Owned/Location is in home structure)";
#endif
                    }
                } else if (targetBed.structureLocation != null && targetBed.structureLocation.structureType == STRUCTURE_TYPE.TAVERN) {
                    if (actor.homeStructure != null && actor.currentSettlement != null && actor.currentSettlement == actor.homeSettlement) {
                        cost += 2000;
#if DEBUG_LOG
                        costLog += $" +2000(Bed is in Tavern and Actor has a home and is currently at his/her home settlement)";
#endif
                    } else {
                        if (actor.moneyComponent.CanAfford(SleepAtTavernCost)) {
                            cost += UtilityScripts.Utilities.Rng.Next(20, 26);;
#if DEBUG_LOG
                            costLog += $" +{cost}(Bed is in Tavern and Actor doesn't have a home or is currently not at his/her home settlement and actor can afford to pay Tavern)";
#endif  
                        } else {
                            cost += 2000;
#if DEBUG_LOG
                            costLog += $" +2000(Bed is in Tavern and Actor doesn't have a home or is currently not at his/her home settlement. But actor cannot afford to pay Tavern)";
#endif  
                        }
                    }
                } else if (actor.needsComponent.isExhausted) {
                    BaseSettlement settlement = null;
                    if (targetBed.IsInHomeStructureOfCharacterWithOpinion(actor, RelationshipManager.Close_Friend, RelationshipManager.Friend)) {
                        cost += UtilityScripts.Utilities.Rng.Next(130, 151);
#if DEBUG_LOG
                        costLog += $" +{cost}(Exhausted, Is in Friend home structure)";
#endif
                    } else if (targetBed.IsInHomeStructureOfCharacterWithOpinion(actor, RelationshipManager.Rival, RelationshipManager.Enemy)) {
                        cost += 2000;
#if DEBUG_LOG
                        costLog += " +2000(Exhausted, Is in Enemy home structure)";
#endif
                    } else if (targetBed.gridTileLocation != null && targetBed.gridTileLocation.IsPartOfSettlement(out settlement) && settlement.owner != null && settlement.owner != actor.faction) {
                        cost += 200;
#if DEBUG_LOG
                        costLog += " +200(Exhausted, Inside settlement of different faction)";
#endif
                    } else {
                        cost = UtilityScripts.Utilities.Rng.Next(80, 101);
#if DEBUG_LOG
                        costLog += $" +{cost}(Else)";
#endif
                    }
                } else {
                    cost += 2000;
#if DEBUG_LOG
                    costLog += $" +{cost}(Not Exhausted)";
#endif
                }

                Character alreadySleepingCharacter = null;
                for (int i = 0; i < targetBed.users.Length; i++) {
                    if (targetBed.users[i] != null) {
                        alreadySleepingCharacter = targetBed.users[i];
                        break;
                    }
                }

                if (alreadySleepingCharacter != null) {
                    string opinionLabel = actor.relationshipContainer.GetOpinionLabel(alreadySleepingCharacter);
                    if (opinionLabel == RelationshipManager.Friend) {
                        cost += 20;
#if DEBUG_LOG
                        costLog += " +20(Friend Occupies)";
#endif
                    } else if (opinionLabel == RelationshipManager.Acquaintance) {
                        cost += 25;
#if DEBUG_LOG
                        costLog += " +25(Acquaintance Occupies)";
#endif
                    } else if (opinionLabel == RelationshipManager.Enemy || opinionLabel == RelationshipManager.Rival || opinionLabel == string.Empty) {
                        cost += 100;
#if DEBUG_LOG
                        costLog += " +100(Enemy/Rival/None Occupies)";
#endif
                    }
                }
            }
        }
#if DEBUG_LOG
        actor.logComponent.AppendCostLog(costLog);
#endif
        return cost;
        //LocationStructure targetStructure = target.gridTileLocation.structure;
        //if (targetStructure.structureType == STRUCTURE_TYPE.DWELLING) {
        //    Dwelling dwelling = targetStructure as Dwelling;
        //    if (dwelling.IsResident(actor)) {
        //        return 1;
        //    } else {
        //        for (int i = 0; i < dwelling.residents.Count; i++) {
        //            Character resident = dwelling.residents[i];
        //            if (resident != actor) {
        //                if (actor.RelationshipManager.HasOpinion(resident) && actor.RelationshipManager.GetTotalOpinion(resident) > 0) {
        //                    return 30;
        //                }
        //            }
        //        }
        //        return 60;
        //    }
        //} else if (targetStructure.structureType == STRUCTURE_TYPE.INN) {
        //    return 60;
        //}
        //return 50;
    }
    public override void OnStopWhilePerforming(ActualGoapNode node) {
        base.OnStopWhilePerforming(node);
        Character actor = node.actor;
        actor.traitContainer.RemoveTrait(actor, "Resting");
    }
    public override GoapActionInvalidity IsInvalid(ActualGoapNode node) {
        GoapActionInvalidity goapActionInvalidity = base.IsInvalid(node);
        Character actor = node.actor;
        IPointOfInterest poiTarget = node.poiTarget;
        if (goapActionInvalidity.isInvalid == false) {
            // if (CanSleepInBed(actor, poiTarget as TileObject) == false) {
            //     goapActionInvalidity.isInvalid = true;
            //     goapActionInvalidity.stateName = "Rest Fail";
            // } else 
            if (poiTarget.IsAvailable() == false) {
                goapActionInvalidity.isInvalid = true;
                goapActionInvalidity.reason = "no_space_bed";
            }
        }
        return goapActionInvalidity;
    }
#endregion

#region Requirements
    protected override bool AreRequirementsSatisfied(Character actor, IPointOfInterest poiTarget, OtherData[] otherData, JobQueueItem job) {
        bool satisfied = base.AreRequirementsSatisfied(actor, poiTarget, otherData, job);
        if (satisfied) {
            if (poiTarget.gridTileLocation != null && actor.trapStructure.IsTrappedAndTrapStructureIsNot(poiTarget.gridTileLocation.structure)) {
                return false;
            }
            if (poiTarget.gridTileLocation != null && actor.trapStructure.IsTrappedAndTrapAreaIsNot(poiTarget.gridTileLocation.area)) {
                return false;
            }
            //if (poiTarget.gridTileLocation != null && actor.trapStructure.structure != null && actor.trapStructure.structure != poiTarget.gridTileLocation.structure) {
            //    return false;
            //}
            // if (CanSleepInBed(actor, poiTarget as TileObject) == false) {
            //     return false;
            // }
            return poiTarget.IsAvailable() && poiTarget.gridTileLocation != null;
        }
        return false;
    }
#endregion

#region State Effects
    public void PreRestSuccess(ActualGoapNode goapNode) {
        //goapNode.descriptionLog.AddToFillers(goapNode.targetStructure.location, goapNode.targetStructure.GetNameRelativeTo(goapNode.actor), LOG_IDENTIFIER.LANDMARK_1);
        goapNode.actor.traitContainer.AddTrait(goapNode.actor, "Resting");
        goapNode.actor.CancelAllJobsExceptForCurrent();

        LocationStructure targetStructure = goapNode.poiTarget.gridTileLocation?.structure;
        if (targetStructure != null && targetStructure.structureType == STRUCTURE_TYPE.TAVERN) {
            if (targetStructure is ManMadeStructure mmStructure) {
                goapNode.actor.moneyComponent.AdjustCoins(-SleepAtTavernCost);
                if (mmStructure.HasAssignedWorker()) {
                    //only added coins to first worker since we expect that the tavern only has 1 worker.
                    //if that changes, this needs to be changed as well.
                    string assignedWorkerID = mmStructure.assignedWorkerIDs[0];
                    Character assignedWorker = DatabaseManager.Instance.characterDatabase.GetCharacterByPersistentID(assignedWorkerID);
                    assignedWorker.moneyComponent.AdjustCoins(SleepAtTavernCost);
                }
                // Character assignedWorker = mmStructure.assignedWorker;
                // if (assignedWorker != null) {
                //     assignedWorker.moneyComponent.AdjustCoins(10);
                // }
            }
        }
        //goapNode.action.states[goapNode.currentStateName].OverrideDuration(goapNode.actor.currentSleepTicks);
    }
    public void PerTickRestSuccess(ActualGoapNode goapNode) {
        Character actor = goapNode.actor;
        CharacterNeedsComponent needsComponent = actor.needsComponent;
        // if (needsComponent.currentSleepTicks == 1) { //If sleep ticks is down to 1 tick left, set current duration to end duration so that the action will end now, we need this because the character must only sleep the remaining hours of his sleep if ever that character is interrupted while sleeping
        //     goapNode.OverrideCurrentStateDuration(goapNode.currentState.duration);
        // }
        needsComponent.AdjustTiredness(0.417f);
        needsComponent.AdjustHappiness(0.083f);
        // needsComponent.AdjustSleepTicks(-1);

        //float staminaAdjustment = 0f;
        //if(actor.currentStructure == actor.homeStructure) {
        //    staminaAdjustment = 1f;
        //} else if (actor.currentStructure is Dwelling && actor.currentStructure != actor.homeStructure) {
        //    staminaAdjustment = 0.5f;
        //} else if (actor.currentStructure.structureType == STRUCTURE_TYPE.INN) {
        //    staminaAdjustment = 0.8f;
        //} else if (actor.currentStructure.structureType == STRUCTURE_TYPE.PRISON) {
        //    staminaAdjustment = 0.4f;
        //} else if (actor.currentStructure.structureType.IsOpenSpace()) {
        //    staminaAdjustment = 0.3f;
        //}
        //needsComponent.AdjustStamina(staminaAdjustment);
    }
    public void AfterRestSuccess(ActualGoapNode goapNode) {
        goapNode.actor.traitContainer.RemoveTrait(goapNode.actor, "Resting");
    }
    //public void PreRestFail(ActualGoapNode goapNode) {
    //    if (parentPlan != null && parentPlan.job != null && parentPlan.job.id == actor.sleepScheduleJobID) {
    //        actor.SetHasCancelledSleepSchedule(true);
    //    }
    //    goapNode.descriptionLog.AddToFillers(targetStructure.location, targetStructure.GetNameRelativeTo(actor), LOG_IDENTIFIER.LANDMARK_1);
    //}
    //public void PreTargetMissing() {
    //    goapNode.descriptionLog.AddToFillers(actor.currentStructure.location, actor.currentStructure.GetNameRelativeTo(actor), LOG_IDENTIFIER.LANDMARK_1);
    //}
#endregion

    // private bool CanSleepInBed(Character character, TileObject tileObject) {
    //     return (tileObject as Bed).CanSleepInBed(character);
    // }
}