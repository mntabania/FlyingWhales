using System.Collections;
using System.Collections.Generic;
using Inner_Maps.Location_Structures;
using UnityEngine;  
using Traits;

public class Sleep : GoapAction {

    public override ACTION_CATEGORY actionCategory { get { return ACTION_CATEGORY.DIRECT; } }

    public Sleep() : base(INTERACTION_TYPE.SLEEP) {
        actionIconString = GoapActionStateDB.Sleep_Icon;
        showNotification = false;
        
        advertisedBy = new POINT_OF_INTEREST_TYPE[] { POINT_OF_INTEREST_TYPE.TILE_OBJECT };
        racesThatCanDoAction = new RACE[] { RACE.HUMANS, RACE.ELVES, RACE.GOBLIN, RACE.FAERY };
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
    protected override int GetBaseCost(Character actor, IPointOfInterest target, JobQueueItem job, object[] otherData) {
        string costLog = $"\n{name} {target.nameWithID}:";
        int cost = 0;
        if (target is Bed) {
            Bed bed = target as Bed;
            if (!bed.IsSlotAvailable()) {
                cost += 2000;
                costLog += " +2000(Fully Occupied)";
            } else {
                if (bed.IsOwnedBy(actor)) {
                    cost = UtilityScripts.Utilities.Rng.Next(10, 16);
                    costLog += $" +{cost}(Owned)";
                } else {
                    //List<Character> tableOwners = bed.GetOwners();
                    //bool isTargetObjectOwnedByFriend = false;
                    //bool isTargetObjectOwnedByEnemy = false;
                    //if (tableOwners != null) {
                    //    for (int i = 0; i < tableOwners.Count; i++) {
                    //        Character objectOwner = tableOwners[i];
                    //        if (actor.relationshipContainer.IsFriendsWith(objectOwner)) {
                    //            isTargetObjectOwnedByFriend = true;
                    //            break;
                    //        } else if (actor.relationshipContainer.IsEnemiesWith(objectOwner)) {
                    //            isTargetObjectOwnedByEnemy = true;
                    //        }
                    //    }
                    //}
                    //if (isTargetObjectOwnedByFriend) {
                    //    cost = UtilityScripts.Utilities.Rng.Next(25, 46);
                    //    costLog += $" +{cost}(Owned by Friend)";
                    //} else if (isTargetObjectOwnedByEnemy) {
                    //    cost += 2000;
                    //    costLog += " +2000(Owned by Enemy)";
                    //} else {
                    //    cost += UtilityScripts.Utilities.Rng.Next(40, 51);
                    //    costLog += $" +{cost}(Else)";
                    //}

                    if (bed.characterOwner != null) {
                        if (actor.relationshipContainer.IsFriendsWith(bed.characterOwner)) {
                            cost += UtilityScripts.Utilities.Rng.Next(25, 46);
                            costLog += $" +{cost}(Owned by Friend)";
                        } else if (actor.relationshipContainer.IsEnemiesWith(bed.characterOwner)) {
                            cost += 2000;
                            costLog += " +2000(Owned by Enemy)";
                        } else {
                            cost = UtilityScripts.Utilities.Rng.Next(40, 51);
                            costLog += $" +{cost}(Else)";
                        }
                    } else {
                        cost = UtilityScripts.Utilities.Rng.Next(40, 51);
                        costLog += $" +{cost}(Else)";
                    }


                    Character alreadySleepingCharacter = null;
                    for (int i = 0; i < bed.users.Length; i++) {
                        if (bed.users[i] != null) {
                            alreadySleepingCharacter = bed.users[i];
                            break;
                        }
                    }

                    if (alreadySleepingCharacter != null) {
                        string opinionLabel = actor.relationshipContainer.GetOpinionLabel(alreadySleepingCharacter);
                        if (opinionLabel == RelationshipManager.Friend) {
                            cost += 20;
                            costLog += " +20(Friend Occupies)";
                        } else if (opinionLabel == RelationshipManager.Acquaintance) {
                            cost += 25;
                            costLog += " +25(Acquaintance Occupies)";
                        } else if (opinionLabel == RelationshipManager.Enemy || opinionLabel == RelationshipManager.Rival || opinionLabel == string.Empty) {
                            cost += 100;
                            costLog += " +100(Enemy/Rival/None Occupies)";
                        }
                    }
                }
            } 
        }
        actor.logComponent.AppendCostLog(costLog);
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
            }
        }
        return goapActionInvalidity;
    }
    #endregion

    #region Requirements
    protected override bool AreRequirementsSatisfied(Character actor, IPointOfInterest poiTarget, object[] otherData) {
        bool satisfied = base.AreRequirementsSatisfied(actor, poiTarget, otherData);
        if (satisfied) {
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
        goapNode.actor.CancelAllJobsExceptForCurrent(false);
        //goapNode.action.states[goapNode.currentStateName].OverrideDuration(goapNode.actor.currentSleepTicks);
    }
    public void PerTickRestSuccess(ActualGoapNode goapNode) {
        Character actor = goapNode.actor;
        CharacterNeedsComponent needsComponent = actor.needsComponent;
        if (needsComponent.currentSleepTicks == 1) { //If sleep ticks is down to 1 tick left, set current duration to end duration so that the action will end now, we need this because the character must only sleep the remaining hours of his sleep if ever that character is interrupted while sleeping
            goapNode.OverrideCurrentStateDuration(goapNode.currentState.duration);
        }
        needsComponent.AdjustTiredness(1.1f);
        needsComponent.AdjustSleepTicks(-1);

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

public class SleepData : GoapActionData {
    public SleepData() : base(INTERACTION_TYPE.SLEEP) {
        racesThatCanDoAction = new RACE[] { RACE.HUMANS, RACE.ELVES, RACE.GOBLIN, RACE.FAERY };
        requirementAction = Requirement;
    }

    private bool Requirement(Character actor, IPointOfInterest poiTarget, object[] otherData) {
        if (poiTarget.gridTileLocation != null && actor.trapStructure.structure != null && actor.trapStructure.structure != poiTarget.gridTileLocation.structure) {
            return false;
        }
        return poiTarget.IsAvailable() && poiTarget.gridTileLocation != null;
    }
}