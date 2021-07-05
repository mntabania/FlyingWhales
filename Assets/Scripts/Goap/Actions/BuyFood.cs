using Inner_Maps;
using Inner_Maps.Location_Structures;
using UnityEngine;
using UnityEngine.Assertions;

public class BuyFood : GoapAction {

    public const int FoodCost = 10;
    
    public BuyFood() : base(INTERACTION_TYPE.BUY_FOOD) {
        actionIconString = GoapActionStateDB.Steal_Icon;
        actionLocationType = ACTION_LOCATION_TYPE.NEAR_TARGET;
        racesThatCanDoAction = new RACE[] { RACE.HUMANS, RACE.ELVES, RACE.GOBLIN, RACE.FAERY, RACE.RATMAN };
        logTags = new[] {LOG_TAG.Needs};
    }

    #region Overrides
    protected override void ConstructBasePreconditionsAndEffects() {
        AddExpectedEffect(new GoapEffect(GOAP_EFFECT_CONDITION.HAS_POI, "Food Pile", false, GOAP_EFFECT_TARGET.ACTOR));
        AddExpectedEffect(new GoapEffect(GOAP_EFFECT_CONDITION.BUY_OBJECT, "Food Pile", false, GOAP_EFFECT_TARGET.ACTOR));
        AddExpectedEffect(new GoapEffect(GOAP_EFFECT_CONDITION.FEED, "Food Pile", false, GOAP_EFFECT_TARGET.ACTOR));
    }
    public override void Perform(ActualGoapNode goapNode) {
        base.Perform(goapNode);
        if (goapNode.poiTarget is FoodPile && goapNode.poiTarget.gridTileLocation != null && goapNode.poiTarget.gridTileLocation.structure is ManMadeStructure manMadeStructure) {
            if (manMadeStructure.CanPurchaseFromHere(goapNode.actor, out bool needsToPay, out _)) {
                SetState(needsToPay ? "Buy Success" : "Take Success", goapNode);
            } else {
#if DEBUG_LOG
                Debug.LogError($"{goapNode.actor.name} could not purchase from {manMadeStructure.name} but invalidity checking was passed!");
#endif
            }
        } else {
#if DEBUG_LOG
            Debug.LogError($"{goapNode.actor.name} had problems  with buying food but all requirements passed!");
#endif            
        }
        
    }
    public override GoapActionInvalidity IsInvalid(ActualGoapNode node) {
        GoapActionInvalidity invalidity = base.IsInvalid(node);
        if (!invalidity.isInvalid) {
            if (node.poiTarget is FoodPile foodPile && node.poiTarget.gridTileLocation != null && node.poiTarget.gridTileLocation.structure is ManMadeStructure manMadeStructure) {
                if (manMadeStructure.CanPurchaseFromHere(node.actor, out bool needsToPay, out _)) {
                    var canAfford = !needsToPay || node.actor.moneyComponent.CanAfford(FoodCost);
                    if (!canAfford) {
                        invalidity.isInvalid = true;
                        invalidity.reason = "not_enough_money";    
                    }
                } else {
                    invalidity.isInvalid = true;
                    invalidity.reason = "cannot_buy";
                }
            }
        }
        return invalidity;
    }
    protected override int GetBaseCost(Character actor, IPointOfInterest target, JobQueueItem job, OtherData[] otherData) {
#if DEBUG_LOG
        string costLog = string.Empty;
#endif
        if (target.gridTileLocation != null && actor.movementComponent.structuresToAvoid.Contains(target.gridTileLocation.structure)) {
            //target is at structure that character is avoiding
#if DEBUG_LOG
            costLog += $" +2000(Location of target is in avoid structure)";
            actor.logComponent.AppendCostLog(costLog);
#endif
            return 2000;
        }
#if DEBUG_LOG
        costLog = $"\n{name} {target.nameWithID}: +10(Constant)";
        actor.logComponent.AppendCostLog(costLog);
#endif
        return 10;
    }
    #endregion
    
    #region Requirements
    protected override bool AreRequirementsSatisfied(Character actor, IPointOfInterest poiTarget, OtherData[] otherData, JobQueueItem job) {
        bool satisfied = base.AreRequirementsSatisfied(actor, poiTarget, otherData, job);
        if (satisfied) {
            if (poiTarget is FoodPile foodPile && poiTarget.gridTileLocation != null && poiTarget.gridTileLocation.structure is ManMadeStructure manMadeStructure) {
                if (!actor.traitContainer.HasTrait("Cannibal") && (foodPile.tileObjectType == TILE_OBJECT_TYPE.HUMAN_MEAT || foodPile.tileObjectType == TILE_OBJECT_TYPE.ELF_MEAT)) {
                    //non cannibals should only buy non human and elf meat
                    return false;
                }
                if (manMadeStructure.CanPurchaseFromHere(actor, out bool needsToPay, out _)) {
                    // if (needsToPay) {
                    //     return actor.moneyComponent.CanAfford(FoodCost);
                    // } else {
                    //     //actor doesn't need to pay.
                    //     return true;
                    // }
                    //make sure that character doesn't have that type of food yet.
                    if (actor.homeStructure == null) {
                        return true;
                    } else {
                        return !actor.homeStructure.HasBuiltTileObjectOfType(foodPile.tileObjectType);
                    }
                    //return actor.homeStructure != null && !actor.homeStructure.HasBuiltTileObjectOfType(foodPile.tileObjectType);
                }
            }
            return false;
        }
        return false;
    }
    #endregion
    
    #region State Effects
    public void AfterBuySuccess(ActualGoapNode goapNode) {
        TakeFood(goapNode);
        goapNode.actor.moneyComponent.AdjustCoins(-FoodCost);
    }
    public void AfterTakeSuccess(ActualGoapNode goapNode) {
        TakeFood(goapNode);
    }
    private void TakeFood(ActualGoapNode goapNode) {
        int amount = 60;
        if (goapNode.otherData != null && goapNode.otherData.Length > 0 && goapNode.otherData[0] is IntOtherData intData) {
            amount = intData.integer;
        }
        FoodPile targetFoodPile = goapNode.target as FoodPile;
        Assert.IsNotNull(targetFoodPile);
        if (targetFoodPile.resourceInPile <= amount) {
            //take the whole food pile.
            goapNode.actor.PickUpItem(targetFoodPile);
        } else {
            //create new food pile and take needed amount
            FoodPile newPile = InnerMapManager.Instance.CreateNewTileObject<FoodPile>(targetFoodPile.tileObjectType);
            newPile.SetResourceInPile(amount);
            goapNode.actor.PickUpItem(newPile);
            targetFoodPile.AdjustResourceInPile(-amount);
        }
    }
    #endregion
}
