using System.Collections;
using System.Collections.Generic;
using Inner_Maps.Location_Structures;
using UnityEngine;  
using Traits;

public class Nap : GoapAction {

    public Nap() : base(INTERACTION_TYPE.NAP) {
        actionIconString = GoapActionStateDB.Sleep_Icon;
        showNotification = false;
        
        advertisedBy = new POINT_OF_INTEREST_TYPE[] { POINT_OF_INTEREST_TYPE.TILE_OBJECT };
        racesThatCanDoAction = new RACE[] { RACE.HUMANS, RACE.ELVES, RACE.GOBLIN, RACE.FAERY, RACE.ELEMENTAL, RACE.KOBOLD };
        validTimeOfDays = new TIME_IN_WORDS[] { TIME_IN_WORDS.AFTERNOON, TIME_IN_WORDS.LUNCH_TIME };
    }

    #region Overrides
    protected override void ConstructBasePreconditionsAndEffects() {
        AddExpectedEffect(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.TIREDNESS_RECOVERY, conditionKey = string.Empty, target = GOAP_EFFECT_TARGET.ACTOR });
        AddExpectedEffect(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.COMFORT_RECOVERY, conditionKey = string.Empty, target = GOAP_EFFECT_TARGET.ACTOR });
    }
    public override void Perform(ActualGoapNode goapNode) {
        base.Perform(goapNode);
        SetState("Nap Success", goapNode);
    }
    public override GoapActionInvalidity IsInvalid(ActualGoapNode node) {
        GoapActionInvalidity goapActionInvalidity = base.IsInvalid(node);
        Character actor = node.actor;
        IPointOfInterest poiTarget = node.poiTarget;
        if (goapActionInvalidity.isInvalid == false) {
            if (CanSleepInBed(actor, poiTarget as TileObject) == false) {
                goapActionInvalidity.isInvalid = true;
                goapActionInvalidity.stateName = "Nap Fail";
            } else if (poiTarget.IsAvailable() == false) {
                goapActionInvalidity.isInvalid = true;
            }
        }
        return goapActionInvalidity;
    }
    protected override int GetBaseCost(Character actor, IPointOfInterest target, JobQueueItem job, object[] otherData) {
        //LocationStructure targetStructure = target.gridTileLocation.structure;
        //if(targetStructure.structureType == STRUCTURE_TYPE.DWELLING) {
        //    Dwelling dwelling = targetStructure as Dwelling;
        //    if (dwelling.IsResident(actor)) {
        //        return 8;
        //    } else {
        //        for (int i = 0; i < dwelling.residents.Count; i++) {
        //            Character resident = dwelling.residents[i];
        //            if (resident != actor) {
        //                if (actor.opinionComponent.HasOpinion(resident) && actor.opinionComponent.GetTotalOpinion(resident) > 0) {
        //                    return 25;
        //                }
        //            }
        //        }
        //        return 45;
        //    }
        //} else if(targetStructure.structureType == STRUCTURE_TYPE.INN) {
        //    return 45;
        //}
        //return 100;
        string costLog = $"\n{name} {target.nameWithID}:";
        int cost = 0;
        if (target is Bed) {
            Bed targetBed = target as Bed;
            if (!targetBed.IsSlotAvailable()) {
                cost += 2000;
                costLog += " +2000(Fully Occupied)";
            } else {
                if (targetBed.IsOwnedBy(actor)) {
                    cost += UtilityScripts.Utilities.rng.Next(30, 36);
                    costLog += $" +{cost}(Owned)";
                } else {
                    List<Character> tableOwners = targetBed.GetOwners();
                    bool isTargetObjectOwnedByFriend = false;
                    bool isTargetObjectOwnedByEnemy = false;
                    if (tableOwners != null) {
                        for (int i = 0; i < tableOwners.Count; i++) {
                            Character objectOwner = tableOwners[i];
                            if (actor.relationshipContainer.IsFriendsWith(objectOwner)) {
                                isTargetObjectOwnedByFriend = true;
                                break;
                            } else if (actor.relationshipContainer.IsEnemiesWith(objectOwner)) {
                                isTargetObjectOwnedByEnemy = true;
                            }
                        }
                    }
                    if (isTargetObjectOwnedByFriend) {
                        cost = UtilityScripts.Utilities.rng.Next(55, 66);
                        costLog += $" +{cost}(Owned by Friend)";
                    } else if (isTargetObjectOwnedByEnemy) {
                        cost += 2000;
                        costLog += " +2000(Owned by Enemy)";
                    } else {
                        cost = UtilityScripts.Utilities.rng.Next(50, 71);
                        costLog += $" +{cost}(Else)";
                    }
                }

                Character alreadySleepingCharacter = null;
                for (int i = 0; i < targetBed.users.Length; i++) {
                    if(targetBed.users[i] != null) {
                        alreadySleepingCharacter = targetBed.users[i];
                        break;
                    }
                }

                if(alreadySleepingCharacter != null) {
                    string opinionLabel = actor.relationshipContainer.GetOpinionLabel(alreadySleepingCharacter);
                    if(opinionLabel == OpinionComponent.Friend) {
                        cost += 20;
                        costLog += " +20(Friend Occupies)";
                    } else if (opinionLabel == OpinionComponent.Acquaintance) {
                        cost += 25;
                        costLog += " +25(Acquaintance Occupies)";
                    } else if (opinionLabel == OpinionComponent.Enemy || opinionLabel == OpinionComponent.Rival || opinionLabel == string.Empty) {
                        cost += 100;
                        costLog += " +100(Enemy/Rival/None Occupies)";
                    }
                }
            }
        }
        actor.logComponent.AppendCostLog(costLog);
        return cost;
    }
    public override void OnStopWhilePerforming(ActualGoapNode node) {
        base.OnStopWhilePerforming(node);
        Character actor = node.actor;
        actor.traitContainer.RemoveTrait(actor, "Resting");
    }
    #endregion

    #region Requirements
    protected override bool AreRequirementsSatisfied(Character actor, IPointOfInterest poiTarget, object[] otherData) { 
        bool satisfied = base.AreRequirementsSatisfied(actor, poiTarget, otherData);
        if (satisfied) {
            if (poiTarget.gridTileLocation != null && actor.trapStructure.structure != null && actor.trapStructure.structure != poiTarget.gridTileLocation.structure) {
                return false;
            }

            return poiTarget.IsAvailable() && CanSleepInBed(actor, poiTarget as TileObject) && poiTarget.gridTileLocation != null;
        }
        return false;
    }
    #endregion

    #region State Effects
    public void PreNapSuccess(ActualGoapNode goapNode) {
        goapNode.actor.traitContainer.AddTrait(goapNode.actor, "Resting");
    }
    public void PerTickNapSuccess(ActualGoapNode goapNode) {
        Character actor = goapNode.actor;
        CharacterNeedsComponent needsComponent = actor.needsComponent;

        needsComponent.AdjustTiredness(1.1f);

        float comfortAdjustment = 0f;
        if (actor.currentStructure == actor.homeStructure) {
            comfortAdjustment = 1f;
        } else if (actor.currentStructure is Dwelling && actor.currentStructure != actor.homeStructure) {
            comfortAdjustment = 0.5f;
        } else if (actor.currentStructure.structureType == STRUCTURE_TYPE.INN) {
            comfortAdjustment = 0.8f;
        } else if (actor.currentStructure.structureType == STRUCTURE_TYPE.PRISON) {
            comfortAdjustment = 0.4f;
        } else if (actor.currentStructure.structureType.IsOpenSpace()) {
            comfortAdjustment = 0.3f;
        }
        needsComponent.AdjustComfort(comfortAdjustment);
    }
    public void AfterNapSuccess(ActualGoapNode goapNode) {
        goapNode.actor.traitContainer.RemoveTrait(goapNode.actor, "Resting");
    }
    //public void PreNapFail() {
    //    goapNode.descriptionLog.AddToFillers(targetStructure.location, targetStructure.GetNameRelativeTo(actor), LOG_IDENTIFIER.LANDMARK_1);
    //}
    //public void PreNapMissing() {
    //    goapNode.descriptionLog.AddToFillers(actor.currentStructure.location, actor.currentStructure.GetNameRelativeTo(actor), LOG_IDENTIFIER.LANDMARK_1);
    //}
    #endregion

    private bool CanSleepInBed(Character character, TileObject tileObject) {
        return (tileObject as Bed).CanSleepInBed(character);
    }
}

public class NapData : GoapActionData {
    public NapData() : base(INTERACTION_TYPE.NAP) {
        racesThatCanDoAction = new RACE[] { RACE.HUMANS, RACE.ELVES, RACE.GOBLIN, RACE.FAERY, RACE.ELEMENTAL, RACE.KOBOLD };
        requirementAction = Requirement;
    }

    private bool Requirement(Character actor, IPointOfInterest poiTarget, object[] otherData) {
        if (poiTarget.gridTileLocation != null && actor.trapStructure.structure != null && actor.trapStructure.structure != poiTarget.gridTileLocation.structure) {
            return false;
        }
        return poiTarget.IsAvailable() && poiTarget.gridTileLocation != null;
    }
}