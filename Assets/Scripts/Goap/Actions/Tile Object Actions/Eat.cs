using System.Collections;
using System.Collections.Generic;
using UnityEngine;  
using Traits;

public class Eat : GoapAction {

    public override ACTION_CATEGORY actionCategory { get { return ACTION_CATEGORY.CONSUME; } }

    public Eat() : base(INTERACTION_TYPE.EAT) {
        //actionLocationType = ACTION_LOCATION_TYPE.ON_TARGET;
        actionIconString = GoapActionStateDB.Eat_Icon;
        showNotification = false;
        advertisedBy = new POINT_OF_INTEREST_TYPE[] { POINT_OF_INTEREST_TYPE.TILE_OBJECT };
        racesThatCanDoAction = new RACE[] { RACE.HUMANS, RACE.ELVES, RACE.GOBLIN, RACE.FAERY, RACE.SKELETON, RACE.WOLF, RACE.SPIDER, RACE.DRAGON };
    }

    #region Overrides
    protected override void ConstructBasePreconditionsAndEffects() {
        AddExpectedEffect(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.FULLNESS_RECOVERY, conditionKey = string.Empty, target = GOAP_EFFECT_TARGET.ACTOR });
        AddExpectedEffect(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.COMFORT_RECOVERY, conditionKey = string.Empty, target = GOAP_EFFECT_TARGET.ACTOR });
    }
    public override List<Precondition> GetPreconditions(Character actor, IPointOfInterest target, object[] otherData) {
        if (target is Table) { // || target is FoodPile
            List<Precondition> p = new List<Precondition>(base.GetPreconditions(actor, target, otherData));
            p.Add(new Precondition(new GoapEffect(GOAP_EFFECT_CONDITION.HAS_POI, "Food Pile" /*+ (int)otherData[0]*/, false, GOAP_EFFECT_TARGET.TARGET), HasFood));
            return p;
        }
        return base.GetPreconditions(actor, target, otherData);
    }
    public override void Perform(ActualGoapNode goapNode) {
        base.Perform(goapNode);
        SetState("Eat Success", goapNode);
    }
    protected override int GetBaseCost(Character actor, IPointOfInterest target, JobQueueItem job, object[] otherData) {
        string costLog = $"\n{name} {target.nameWithID}:";
        int cost = 0;
        if (target is Table) {
            Table table = target as Table;
            if (table.IsOwnedBy(actor)) {
                cost = UtilityScripts.Utilities.Rng.Next(20, 36);
                costLog += $" +{cost}(Owned)";
            } else {
                List<Character> tableOwners = table.GetOwners();
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
                    cost = UtilityScripts.Utilities.Rng.Next(45, 66);
                    costLog += $" +{cost}(Owned by Friend)";
                } else if (isTargetObjectOwnedByEnemy) {
                    cost = 2000;
                    costLog += " +2000(Owned by Enemy)";
                } else {
                    cost = UtilityScripts.Utilities.Rng.Next(60, 81);
                    costLog += $" +{cost}(Otherwise)";
                }
            }
        } else if (target is FoodPile) {
            cost = UtilityScripts.Utilities.Rng.Next(400, 451);
            costLog += $" +{cost}(Food Pile)";
        }
        actor.logComponent.AppendCostLog(costLog);
        return cost;
    }
    public override void OnStopWhilePerforming(ActualGoapNode node) {
        base.OnStopWhilePerforming(node);
        Character actor = node.actor;
        actor.needsComponent.AdjustDoNotGetHungry(-1);
    }
    public override GoapActionInvalidity IsInvalid(ActualGoapNode node) {
        GoapActionInvalidity goapActionInvalidity = base.IsInvalid(node);
        IPointOfInterest poiTarget = node.poiTarget;
        if (goapActionInvalidity.isInvalid == false) {
            if (poiTarget.IsAvailable() == false) {
                goapActionInvalidity.isInvalid = true;
                goapActionInvalidity.stateName = "Eat Fail";
            }
        }
        return goapActionInvalidity;
    }
    #endregion

    #region Effects
    public void PreEatSuccess(ActualGoapNode goapNode) {
        //goapNode.descriptionLog.AddToFillers(goapNode.targetStructure.location, goapNode.targetStructure.GetNameRelativeTo(goapNode.actor), LOG_IDENTIFIER.LANDMARK_1);
        //goapNode.poiTarget.SetPOIState(POI_STATE.INACTIVE);
        goapNode.actor.needsComponent.AdjustDoNotGetHungry(1);
        //actor.traitContainer.AddTrait(actor,"Eating");
    }
    //public void PerTickEatSuccess(ActualGoapNode goapNode) {
    //    //goapNode.actor.AdjustFullness(520);
    //}
    public void AfterEatSuccess(ActualGoapNode goapNode) {
        goapNode.actor.needsComponent.AdjustDoNotGetHungry(-1);
        //goapNode.poiTarget.SetPOIState(POI_STATE.ACTIVE);
    }
    //public void PreEatFail(ActualGoapNode goapNode) {
    //    GoapActionState currentState = goapNode.action.states[goapNode.currentStateName];
    //    goapNode.descriptionLog.AddToFillers(goapNode.targetStructure.location, goapNode.targetStructure.GetNameRelativeTo(goapNode.actor), LOG_IDENTIFIER.LANDMARK_1);
    //}
    //public void PreTargetMissing(ActualGoapNode goapNode) {
    //    goapNode.descriptionLog.AddToFillers(goapNode.actor.currentStructure.location, goapNode.actor.currentStructure.GetNameRelativeTo(goapNode.actor), LOG_IDENTIFIER.LANDMARK_1);
    //}
    #endregion

    #region Requirements
    protected override bool AreRequirementsSatisfied(Character actor, IPointOfInterest poiTarget, object[] otherData) { 
        bool satisfied = base.AreRequirementsSatisfied(actor, poiTarget, otherData);
        if (satisfied) {
            if (!poiTarget.IsAvailable()) {
                return false;
            }
            if (poiTarget.gridTileLocation != null && actor.trapStructure.structure != null && actor.trapStructure.structure != poiTarget.gridTileLocation.structure) {
                return false;
            }
            if (actor.traitContainer.HasTrait("Vampiric")) {
                return false;
            }
            if(poiTarget is SmallAnimal || poiTarget is EdiblePlant) {
                //If plant or animal, only eat if the actor is homeless
                if(actor.homeStructure != null) {
                    return false;
                }
            } 
            // else {
            //     if(poiTarget.storedResources[RESOURCE.FOOD] < 12) {
            //         return false;
            //     }
            // }
            if (poiTarget.gridTileLocation != null) {
                return true;
            }
        }
        return false;
    }
    #endregion
    
    #region Preconditions
    private bool HasFood(Character actor, IPointOfInterest poiTarget, object[] otherData) {
        return poiTarget.HasResourceAmount(RESOURCE.FOOD, 12);
    }
    #endregion
}

public class EatData : GoapActionData {
    public EatData() : base(INTERACTION_TYPE.EAT) {
        racesThatCanDoAction = new RACE[] { RACE.HUMANS, RACE.ELVES, RACE.GOBLIN, RACE.FAERY, RACE.SKELETON, RACE.WOLF, RACE.SPIDER, RACE.DRAGON };
        requirementAction = Requirement;
    }

    private bool Requirement(Character actor, IPointOfInterest poiTarget, object[] otherData) {
        if (!poiTarget.IsAvailable()) {
            return false;
        }
        if (poiTarget.gridTileLocation != null && actor.trapStructure.structure != null && actor.trapStructure.structure != poiTarget.gridTileLocation.structure) {
            return false;
        }
        if (poiTarget.gridTileLocation != null) {
            return true;
        }
        return false;
    }
}