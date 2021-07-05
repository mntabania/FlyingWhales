using System;
using System.Collections;
using System.Collections.Generic;
using Logs;
using UnityEngine;  
using Traits;
using UtilityScripts;

public class CraftFurnitureWood : GoapAction {

    public CraftFurnitureWood() : base(INTERACTION_TYPE.CRAFT_FURNITURE_WOOD) {
        actionIconString = GoapActionStateDB.Build_Icon;
        racesThatCanDoAction = new RACE[] { RACE.HUMANS, RACE.ELVES, RACE.GOBLIN, RACE.FAERY, RACE.RATMAN };
        canBeAdvertisedEvenIfTargetIsUnavailable = true;
        logTags = new[] {LOG_TAG.Work};
    }

    #region Overrides
    protected override void ConstructBasePreconditionsAndEffects() {
        SetPrecondition(new GoapEffect(GOAP_EFFECT_CONDITION.BUY_OBJECT, "Wood Pile", false, GOAP_EFFECT_TARGET.ACTOR), HasWood);
    }
    public override void Perform(ActualGoapNode goapNode) {
        base.Perform(goapNode);
        SetState("Craft Success", goapNode);
    }
    public override GoapActionInvalidity IsInvalid(ActualGoapNode node) {
        string stateName = "Target Missing";
        GoapActionInvalidity goapActionInvalidity = new GoapActionInvalidity(false, stateName);
        //craft cannot be invalid because all cases are handled by the requirements of the action
        return goapActionInvalidity;
    }
    public override void AddFillersToLog(Log log, ActualGoapNode node) {
        base.AddFillersToLog(log, node);
        TileObject obj = node.poiTarget as TileObject;
        log.AddToFillers(null, UtilityScripts.Utilities.GetArticleForWord(obj.name), LOG_IDENTIFIER.STRING_1);
        log.AddToFillers(null, obj.name, LOG_IDENTIFIER.ITEM_1);
    }
    protected override int GetBaseCost(Character actor, IPointOfInterest target, JobQueueItem job, OtherData[] otherData) {
        int cost = 10;
#if DEBUG_LOG
        string costLog = $"\n{name} {target.nameWithID}:";
        costLog += $" +{cost}(Initial)";
        actor.logComponent.AppendCostLog(costLog);
#endif
        return cost;
    }
    public override void OnStopWhilePerforming(ActualGoapNode node) {
        base.OnStopWhilePerforming(node);
        Character actor = node.actor;
        (node.poiTarget as TileObject).SetMapObjectState(MAP_OBJECT_STATE.UNBUILT);    
    }
    public override void OnActionStarted(ActualGoapNode node) {
        TileObject stonePile = node.actor.GetItem(TILE_OBJECT_TYPE.WOOD_PILE);
        if (stonePile != null) {
            node.actor.ShowItemVisualCarryingPOI(stonePile);    
        }
    }
    #endregion

    #region State Effects
    public void PreCraftSuccess(ActualGoapNode goapNode) {
        Character actor = goapNode.actor;
        TileObject obj = goapNode.poiTarget as TileObject;
        
        List<WoodPile> allPiles = RuinarchListPool<WoodPile>.Claim();
        actor.PopulateItemsOfType(allPiles);
        int remainingAmount = TileObjectDB.GetTileObjectData(obj.tileObjectType).craftResourceCost;
        for (int i = 0; i < allPiles.Count; i++) {
            if (remainingAmount <= 0) { break; }
            WoodPile woodPile = allPiles[i];
            int amountToReduce = remainingAmount;
            if (amountToReduce > woodPile.resourceInPile) {
                amountToReduce = woodPile.resourceInPile;
            }
            woodPile.AdjustResourceInPile(-amountToReduce);
            remainingAmount -= amountToReduce;
        }
       
        obj.SetMapObjectState(MAP_OBJECT_STATE.BUILDING);
        goapNode.descriptionLog.AddToFillers(null, UtilityScripts.Utilities.GetArticleForWord(obj.name), LOG_IDENTIFIER.STRING_1);
        goapNode.descriptionLog.AddToFillers(null, obj.name, LOG_IDENTIFIER.ITEM_1);
        if (goapNode.thoughtBubbleLog != null) {
            goapNode.thoughtBubbleLog.AddToFillers(null, UtilityScripts.Utilities.GetArticleForWord(obj.name), LOG_IDENTIFIER.STRING_1);
            goapNode.thoughtBubbleLog.AddToFillers(null, obj.name, LOG_IDENTIFIER.ITEM_1);
        }
    }
    public void AfterCraftSuccess(ActualGoapNode goapNode) {
        TileObject tileObj = goapNode.poiTarget as TileObject;
        tileObj.SetMapObjectState(MAP_OBJECT_STATE.BUILT);
        if (goapNode.associatedJobType == JOB_TYPE.CRAFT_MISSING_FURNITURE) {
            //after character finishes building the target furniture, set it as owned by him/her
            tileObj.SetCharacterOwner(goapNode.actor);
        }
    }
    #endregion

    #region Preconditions
    private bool HasWood(Character actor, IPointOfInterest poiTarget, OtherData[] otherData, JOB_TYPE jobType) {
        return poiTarget is TileObject tileObject && actor.GetItem(TILE_OBJECT_TYPE.WOOD_PILE) is ResourcePile pile && 
               pile.resourceInPile >= TileObjectDB.GetTileObjectData(tileObject.tileObjectType).craftResourceCost; 
    }
    #endregion

    #region Requirement
    protected override bool AreRequirementsSatisfied(Character actor, IPointOfInterest poiTarget, OtherData[] otherData, JobQueueItem job) {
        bool satisfied = base.AreRequirementsSatisfied(actor, poiTarget, otherData, job);
        if (satisfied) {
            if (poiTarget is TileObject) {
                TileObject target = poiTarget as TileObject;
                return target.mapObjectState == MAP_OBJECT_STATE.UNBUILT;    
            }
        }
        return false;
    }
    #endregion

}