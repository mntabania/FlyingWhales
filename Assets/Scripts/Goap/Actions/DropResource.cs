using System.Collections;
using System.Collections.Generic;
using Logs;
using UnityEngine;  
using Traits;
using Inner_Maps.Location_Structures;

public class DropResource : GoapAction {

    private Precondition _foodPrecondition;
    private Precondition _buyFoodPrecondition;

    public DropResource() : base(INTERACTION_TYPE.DROP_RESOURCE) {
        actionIconString = GoapActionStateDB.Haul_Icon;
        //advertisedBy = new POINT_OF_INTEREST_TYPE[] { POINT_OF_INTEREST_TYPE.TILE_OBJECT };
        racesThatCanDoAction = new RACE[] { RACE.HUMANS, RACE.ELVES, RACE.GOBLIN, RACE.FAERY, RACE.RATMAN };
        logTags = new[] {LOG_TAG.Work};

        _foodPrecondition = new Precondition(new GoapEffect(GOAP_EFFECT_CONDITION.TAKE_POI, "Food Pile" /*+ (int)otherData[0]*/, false, GOAP_EFFECT_TARGET.ACTOR), HasTakenEnoughAmount);
        _buyFoodPrecondition = new Precondition(new GoapEffect(GOAP_EFFECT_CONDITION.BUY_OBJECT, "Food Pile", false, GOAP_EFFECT_TARGET.ACTOR), HasBoughtFood);
    }

    #region Overrides
    //protected override void ConstructBasePreconditionsAndEffects() {
    //    //AddPrecondition(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.HAS_FOOD, conditionKey = "0", isKeyANumber = true, target = GOAP_EFFECT_TARGET.ACTOR }, HasHauledEnoughAmount);
    //    //AddExpectedEffect(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.HAS_FOOD, conditionKey = "0", isKeyANumber = true, target = GOAP_EFFECT_TARGET.TARGET });
    //    AddPossibleExpectedEffectForTypeAndTargetMatching(new GoapEffectConditionTypeAndTargetType(GOAP_EFFECT_CONDITION.HAS_FOOD, GOAP_EFFECT_TARGET.TARGET));
    //    AddPossibleExpectedEffectForTypeAndTargetMatching(new GoapEffectConditionTypeAndTargetType(GOAP_EFFECT_CONDITION.HAS_WOOD, GOAP_EFFECT_TARGET.TARGET));
    //    AddPossibleExpectedEffectForTypeAndTargetMatching(new GoapEffectConditionTypeAndTargetType(GOAP_EFFECT_CONDITION.HAS_STONE, GOAP_EFFECT_TARGET.TARGET));
    //    AddPossibleExpectedEffectForTypeAndTargetMatching(new GoapEffectConditionTypeAndTargetType(GOAP_EFFECT_CONDITION.HAS_METAL, GOAP_EFFECT_TARGET.TARGET));
    //}
    protected override List<GoapEffect> GetExpectedEffects(Character actor, IPointOfInterest target, OtherData[] otherData, out bool isOverridden) {
        List<GoapEffect> ee = ObjectPoolManager.Instance.CreateNewExpectedEffectsList();
        List<GoapEffect> baseEE = base.GetExpectedEffects(actor, target, otherData, out isOverridden);
        if (baseEE != null && baseEE.Count > 0) {
            ee.AddRange(baseEE);
        }
        if (target is Table) {
            ee.Add(new GoapEffect(GOAP_EFFECT_CONDITION.HAS_POI, "Food Pile", false, GOAP_EFFECT_TARGET.TARGET));
        } else {
            ee.Add(new GoapEffect(GOAP_EFFECT_CONDITION.HAS_POI, target.name, false, GOAP_EFFECT_TARGET.TARGET));
        }
        isOverridden = true;
        return ee;
    }
    public override Precondition GetPrecondition(Character actor, IPointOfInterest target, OtherData[] otherData, JOB_TYPE jobType, out bool isOverridden) {
        //List<Precondition> baseP = base.GetPrecondition(actor, target, otherData, out isOverridden);
        //List<Precondition> p = ObjectPoolManager.Instance.CreateNewPreconditionsList();
        //p.AddRange(baseP);
        Precondition p = null;
        if (target is Table) {
            if (jobType == JOB_TYPE.BUY_FOOD_FOR_TAVERN) {
                p = _buyFoodPrecondition;
            } else {
                p = _foodPrecondition;
            }
        } else {
            p = new Precondition(new GoapEffect(GOAP_EFFECT_CONDITION.TAKE_POI, target.name /*+ (int) otherData[0]*/, false, GOAP_EFFECT_TARGET.ACTOR), HasTakenEnoughAmount);
        }
        isOverridden = true;
        return p;
    }
    public override void Perform(ActualGoapNode goapNode) {
        base.Perform(goapNode);
        SetState("Drop Success", goapNode);
    }
    protected override int GetBaseCost(Character actor, IPointOfInterest target, JobQueueItem job, OtherData[] otherData) {
#if DEBUG_LOG
        string costLog = $"\n{name} {target.nameWithID}: +10(Constant)";
        actor.logComponent.AppendCostLog(costLog);
#endif
        return 10;
    }
    public override void AddFillersToLog(Log log, ActualGoapNode node) {
        base.AddFillersToLog(log, node);
        ResourcePile pile = node.actor.carryComponent.carriedPOI as ResourcePile;
        log.AddToFillers(null, UtilityScripts.Utilities.NormalizeStringUpperCaseFirstLetterOnly(pile.providedResource.ToString()), LOG_IDENTIFIER.STRING_2);
    }
    public override void OnActionStarted(ActualGoapNode node) {
        base.OnActionStarted(node);
        if (node.associatedJobType == JOB_TYPE.BUY_FOOD_FOR_TAVERN) {
            FoodPile item = node.actor.GetItem<FoodPile>();
            node.actor.ShowItemVisualCarryingPOI(item);
        }
    }
    public override void OnStopWhileStarted(ActualGoapNode node) {
        base.OnStopWhileStarted(node);
        Character actor = node.actor;
        actor.UncarryPOI();
    }
    public override void OnStopWhilePerforming(ActualGoapNode node) {
        base.OnStopWhilePerforming(node);
        Character actor = node.actor;
        actor.UncarryPOI();
    }
#endregion

    #region Preconditions
    private bool HasTakenEnoughAmount(Character actor, IPointOfInterest poiTarget, OtherData[] otherData, JOB_TYPE jobType) {
        if (actor.carryComponent.isCarryingAnyPOI && actor.carryComponent.carriedPOI is ResourcePile) {
            return true;
        }
        return false;
    }
    private bool HasBoughtFood(Character actor, IPointOfInterest poiTarget, OtherData[] otherData, JOB_TYPE jobType) {
        if (actor.HasItem<FoodPile>()) {
            return true;
        }
        return false;
    }
    #endregion

    #region Requirements
    protected override bool AreRequirementsSatisfied(Character actor, IPointOfInterest poiTarget, OtherData[] otherData, JobQueueItem job) { 
        bool satisfied = base.AreRequirementsSatisfied(actor, poiTarget, otherData, job);
        if (satisfied) {
            if (job.jobType.IsFullnessRecoveryTypeJob()) {
                LocationStructure structure = poiTarget.gridTileLocation?.structure;
                if (structure != null) {
                    if (structure is Dwelling) {
                        if (!structure.IsResident(actor)) {
                            return false;
                        }
                    } else if (structure.structureType.IsFoodProducingStructure()) {
                        if (structure is ManMadeStructure manMadeStructure && !manMadeStructure.DoesCharacterWorkHere(actor)) {
                            return false;
                        }
                    }
                }
            }
            if (poiTarget.gridTileLocation == null) {
                return false;
            }
            //return actor.homeRegion != poiTarget.gridTileLocation.structure.location;
            return true;
        }
        return false;
    }
#endregion

#region State Effects
    public void PreDropSuccess(ActualGoapNode goapNode) {
        //int givenFood = goapNode.actor.food;
        //GoapActionState currentState = goapNode.action.states[goapNode.currentStateName];
        ResourcePile pile = goapNode.actor.carryComponent.carriedPOI as ResourcePile;
        goapNode.descriptionLog.AddToFillers(null, UtilityScripts.Utilities.NormalizeStringUpperCaseFirstLetters(pile.resourceInPile.ToString()), LOG_IDENTIFIER.STRING_1);
        //goapNode.descriptionLog.AddToFillers(null, pile.providedResource.ToString(), LOG_IDENTIFIER.STRING_2);
        //goapNode.descriptionLog.AddToFillers(goapNode.targetStructure, goapNode.targetStructure.GetNameRelativeTo(goapNode.actor), LOG_IDENTIFIER.LANDMARK_1);
    }
    public void AfterDropSuccess(ActualGoapNode goapNode) {
        if (goapNode.actor.carryComponent.carriedPOI is ResourcePile carriedPile) {
            if (goapNode.poiTarget is Table table) {
                table.AdjustFood(carriedPile.specificProvidedResource, carriedPile.resourceInPile);
            } else if (goapNode.poiTarget is ResourcePile resourcePile) {
                resourcePile.AdjustResourceInPile(carriedPile.resourceInPile);
            }
            TraitManager.Instance.CopyStatuses(carriedPile, goapNode.poiTarget);
            carriedPile.AdjustResourceInPile(-carriedPile.resourceInPile);    
        }
        //goapNode.actor.ownParty.RemoveCarriedPOI(false);
        //else if (poiTarget is FoodPile) {
        //    FoodPile foodPile = poiTarget as FoodPile;
        //    actor.AdjustFood(-givenFood);
        //    foodPile.AdjustFoodInPile(givenFood);
        //}
    }
#endregion
}