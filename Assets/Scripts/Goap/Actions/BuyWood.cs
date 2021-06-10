﻿using System;
using Inner_Maps;
using Inner_Maps.Location_Structures;
using UnityEngine;
using UnityEngine.Assertions;

public class BuyWood : GoapAction {
    
    public BuyWood() : base(INTERACTION_TYPE.BUY_WOOD) {
        actionIconString = GoapActionStateDB.Steal_Icon;
        actionLocationType = ACTION_LOCATION_TYPE.NEAR_TARGET;
        racesThatCanDoAction = new RACE[] { RACE.HUMANS, RACE.ELVES, RACE.GOBLIN, RACE.FAERY, RACE.RATMAN };
        logTags = new[] {LOG_TAG.Needs};
    }

    #region Overrides
    protected override void ConstructBasePreconditionsAndEffects() {
        AddExpectedEffect(new GoapEffect(GOAP_EFFECT_CONDITION.HAS_POI, "Wood Pile", false, GOAP_EFFECT_TARGET.ACTOR));
        AddExpectedEffect(new GoapEffect(GOAP_EFFECT_CONDITION.BUY_OBJECT, "Wood Pile", false, GOAP_EFFECT_TARGET.ACTOR));
    }
    public override void Perform(ActualGoapNode goapNode) {
        base.Perform(goapNode);
        if (goapNode.poiTarget is WoodPile && goapNode.poiTarget.gridTileLocation != null && goapNode.poiTarget.gridTileLocation.structure is ManMadeStructure manMadeStructure) {
            if (manMadeStructure.CanPurchaseFromHereBasedOnAssignedWorker(goapNode.actor, out bool needsToPay)) {
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
            if (node.poiTarget is WoodPile woodPile && node.poiTarget.gridTileLocation != null && node.poiTarget.gridTileLocation.structure is ManMadeStructure manMadeStructure) {
                if (manMadeStructure.CanPurchaseFromHereBasedOnAssignedWorker(node.actor, out bool needsToPay)) {
                    var canAfford = !needsToPay || node.actor.moneyComponent.CanAfford(GetBuyCost(node));
                    if (!canAfford) {
                        invalidity.isInvalid = true;
                        invalidity.reason = "not_enough_money";    
                    } else {
                        if (woodPile.resourceInPile < GetResourceAmount(node)) {
                            invalidity.isInvalid = true;
                            invalidity.reason = "not_enough_resources";
                        }
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
        log.AddToFillers(null, GetBuyCost(node).ToString(), LOG_IDENTIFIER.STRING_1);
    }
    #endregion
    
    #region Requirements
    protected override bool AreRequirementsSatisfied(Character actor, IPointOfInterest poiTarget, OtherData[] otherData, JobQueueItem job) {
        bool satisfied = base.AreRequirementsSatisfied(actor, poiTarget, otherData, job);
        if (satisfied) {
            if (poiTarget is WoodPile woodPile && poiTarget.gridTileLocation != null && poiTarget.gridTileLocation.structure is ManMadeStructure manMadeStructure) {
                if (manMadeStructure.CanPurchaseFromHereBasedOnAssignedWorker(actor, out bool needsToPay)) {
                    if (woodPile.resourceInPile < GetResourceAmount(otherData)) {
                        return false;
                    }
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
        TakeWood(goapNode);
        goapNode.actor.moneyComponent.AdjustCoins(-GetBuyCost(goapNode));
    }
    public void AfterTakeSuccess(ActualGoapNode goapNode) {
        TakeWood(goapNode);
    }
    private void TakeWood(ActualGoapNode goapNode) {
        int amount = GetResourceAmount(goapNode);
        WoodPile targetPile = goapNode.target as WoodPile;
        Assert.IsNotNull(targetPile);
        if (targetPile.resourceInPile <= amount) {
            //take the whole pile.
            goapNode.actor.PickUpItem(targetPile);
        } else {
            //create new pile and take needed amount
            WoodPile newPile = InnerMapManager.Instance.CreateNewTileObject<WoodPile>(targetPile.tileObjectType);
            newPile.SetResourceInPile(amount);
            goapNode.actor.PickUpItem(newPile);
            targetPile.AdjustResourceInPile(-amount);
        }
    }
    #endregion

    #region Utilities
    private int GetBuyCost(ActualGoapNode goapNode) {
        if (goapNode.otherData.Length >= 1) {
            return ((IntOtherData) goapNode.otherData[0]).integer;
        }
        return 10;
    }
    private int GetResourceAmount(ActualGoapNode goapNode) {
        return GetResourceAmount(goapNode.otherData);
    }
    private int GetResourceAmount(OtherData[] otherData) {
        if (otherData != null && otherData.Length >= 2) {
            return ((IntOtherData) otherData[1]).integer;
        }
        return 10;
    }
    #endregion
}
