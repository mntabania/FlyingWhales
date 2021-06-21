using System.Collections.Generic;
using Inner_Maps;
using Inner_Maps.Location_Structures;
using UnityEngine;
using UnityEngine.Assertions;

public class BuyItem : GoapAction {
    
    public BuyItem() : base(INTERACTION_TYPE.BUY_ITEM) {
        actionIconString = GoapActionStateDB.Steal_Icon;
        actionLocationType = ACTION_LOCATION_TYPE.NEAR_TARGET;
        racesThatCanDoAction = new RACE[] { RACE.HUMANS, RACE.ELVES, RACE.GOBLIN, RACE.FAERY, RACE.RATMAN };
        logTags = new[] {LOG_TAG.Needs};
    }

    #region Overrides
    protected override void ConstructBasePreconditionsAndEffects() {
        AddPossibleExpectedEffectForTypeAndTargetMatching(new GoapEffectConditionTypeAndTargetType(GOAP_EFFECT_CONDITION.HAS_POI, GOAP_EFFECT_TARGET.TARGET));
        AddPossibleExpectedEffectForTypeAndTargetMatching(new GoapEffectConditionTypeAndTargetType(GOAP_EFFECT_CONDITION.HAS_POI, GOAP_EFFECT_TARGET.ACTOR));
    }
    protected override List<GoapEffect> GetExpectedEffects(Character actor, IPointOfInterest target, OtherData[] otherData, out bool isOverridden) {
        List<GoapEffect> ee = ObjectPoolManager.Instance.CreateNewExpectedEffectsList();
        List<GoapEffect> baseEE = base.GetExpectedEffects(actor, target, otherData, out isOverridden);
        if (baseEE != null && baseEE.Count > 0) {
            ee.AddRange(baseEE);
        }
        TileObject item = target as TileObject;
        ee.Add(new GoapEffect(GOAP_EFFECT_CONDITION.HAS_POI, item.name, false, GOAP_EFFECT_TARGET.TARGET));
        ee.Add(new GoapEffect(GOAP_EFFECT_CONDITION.HAS_POI, item.name, false, GOAP_EFFECT_TARGET.ACTOR));
        isOverridden = true;
        return ee;
    }
    public override void Perform(ActualGoapNode goapNode) {
        base.Perform(goapNode);
        if (goapNode.poiTarget.gridTileLocation != null && goapNode.poiTarget.gridTileLocation.structure is ManMadeStructure manMadeStructure) {
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
            if (node.poiTarget is TileObject tileObject && node.poiTarget.gridTileLocation != null && node.poiTarget.gridTileLocation.structure is ManMadeStructure manMadeStructure) {
                if (manMadeStructure.CanPurchaseFromHere(node.actor, out bool needsToPay, out _)) {
                    var canAfford = !needsToPay || node.actor.moneyComponent.CanAfford(GetPurchaseCost(tileObject));
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
    public override void AddFillersToLog(Log log, ActualGoapNode node) {
        base.AddFillersToLog(log, node);
        TileObject tileObject = node.poiTarget as TileObject;
        Assert.IsNotNull(tileObject);
        log.AddToFillers(null, GetPurchaseCost(tileObject).ToString(), LOG_IDENTIFIER.STRING_1);
    }
    #endregion
    
    #region Requirements
    protected override bool AreRequirementsSatisfied(Character actor, IPointOfInterest poiTarget, OtherData[] otherData, JobQueueItem job) {
        bool satisfied = base.AreRequirementsSatisfied(actor, poiTarget, otherData, job);
        if (satisfied) {
            if (poiTarget is TileObject && poiTarget.gridTileLocation != null && poiTarget.gridTileLocation.structure is ManMadeStructure manMadeStructure) {
                if (manMadeStructure.CanPurchaseFromHere(actor, out bool needsToPay, out _)) {
                    // if (needsToPay) {
                    //     return actor.moneyComponent.CanAfford(FoodCost);
                    // } else {
                    //     //actor doesn't need to pay.
                    //     return true;
                    // }
                    return true;
                }
            }
            return false;
        }
        return false;
    }
    #endregion
    
    #region State Effects
    public void AfterBuySuccess(ActualGoapNode goapNode) {
        TakeItem(goapNode);
        TileObject tileObject = goapNode.poiTarget as TileObject;
        Assert.IsNotNull(tileObject);
        goapNode.actor.moneyComponent.AdjustCoins(-GetPurchaseCost(tileObject));
    }
    public void AfterTakeSuccess(ActualGoapNode goapNode) {
        TakeItem(goapNode);
    }
    private void TakeItem(ActualGoapNode goapNode) {
        TileObject tileObject = goapNode.poiTarget as TileObject;
        Assert.IsNotNull(tileObject);
        goapNode.actor.PickUpItem(tileObject, setOwnership: true);
    }
    #endregion

    #region Utilities
    private int GetPurchaseCost(TileObject p_tileObject) {
        if (p_tileObject is EquipmentItem equipmentItem) {
            EquipmentData data = equipmentItem.equipmentData;
            if (data != null) {
                return data.purchaseCost;
            }
        }
        TileObjectData tileObjectData = TileObjectDB.GetTileObjectData(p_tileObject.tileObjectType);
        return tileObjectData.purchaseCost;
    }
    #endregion
}
