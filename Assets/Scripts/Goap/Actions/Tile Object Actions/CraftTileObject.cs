using System.Collections;
using System.Collections.Generic;
using Logs;
using UnityEngine;  
using Traits;

public class CraftTileObject : GoapAction {

    public CraftTileObject() : base(INTERACTION_TYPE.CRAFT_TILE_OBJECT) {
        actionIconString = GoapActionStateDB.Build_Icon;
        advertisedBy = new POINT_OF_INTEREST_TYPE[] { POINT_OF_INTEREST_TYPE.TILE_OBJECT };
        racesThatCanDoAction = new RACE[] { RACE.HUMANS, RACE.ELVES, RACE.GOBLIN, RACE.FAERY };
        validTimeOfDays = new TIME_IN_WORDS[] { TIME_IN_WORDS.MORNING, TIME_IN_WORDS.LUNCH_TIME, TIME_IN_WORDS.AFTERNOON, TIME_IN_WORDS.EARLY_NIGHT };
        canBeAdvertisedEvenIfTargetIsUnavailable = true;
        logTags = new[] {LOG_TAG.Work};
    }

    #region Overrides
    //protected override void ConstructBasePreconditionsAndEffects() {
    //    AddPrecondition(new GoapEffect(GOAP_EFFECT_CONDITION.TAKE_POI, "Wood Pile", false, GOAP_EFFECT_TARGET.ACTOR), HasSupply);
    //}
    public override List<Precondition> GetPreconditions(Character actor, IPointOfInterest target, OtherData[] otherData) {
        if(target is TileObject tileObject) {
            TileObjectData data = TileObjectDB.GetTileObjectData(tileObject.tileObjectType);
            if (data != null && data.itemRequirementsForCreation != null) {
                List<Precondition> p = new List<Precondition>();
                for (int i = 0; i < data.itemRequirementsForCreation.Length; i++) {
                    string req = data.itemRequirementsForCreation[i];
                    if (req == "Wood Pile") {
                        p.Add(new Precondition(new GoapEffect(GOAP_EFFECT_CONDITION.TAKE_POI, req, false, GOAP_EFFECT_TARGET.ACTOR), HasWood));
                    } else if (req == "Stone Pile") {
                        p.Add(new Precondition(new GoapEffect(GOAP_EFFECT_CONDITION.TAKE_POI, req, false, GOAP_EFFECT_TARGET.ACTOR), HasStone));
                    } else if (req == "Metal Pile") {
                        p.Add(new Precondition(new GoapEffect(GOAP_EFFECT_CONDITION.TAKE_POI, req, false, GOAP_EFFECT_TARGET.ACTOR), HasStone));
                    } else {
                        p.Add(new Precondition(new GoapEffect(GOAP_EFFECT_CONDITION.HAS_POI, req, false, GOAP_EFFECT_TARGET.ACTOR), (thisActor, thisTarget, thisOtherData, jobType) => IsCarriedOrInInventory(thisActor, thisTarget, thisOtherData, req)));    
                    }
                }
                return p;
            }
        }
        return base.GetPreconditions(actor, target, otherData);
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
    public override void AddFillersToLog(ref Log log, ActualGoapNode node) {
        base.AddFillersToLog(ref log, node);
        TileObject obj = node.poiTarget as TileObject;
        log.AddToFillers(null, UtilityScripts.Utilities.GetArticleForWord(obj.tileObjectType.ToString()), LOG_IDENTIFIER.STRING_1);
        log.AddToFillers(null, UtilityScripts.Utilities.NormalizeStringUpperCaseFirstLetters(obj.tileObjectType.ToString()), LOG_IDENTIFIER.ITEM_1);
    }
    protected override int GetBaseCost(Character actor, IPointOfInterest target, JobQueueItem job, OtherData[] otherData) {
        string costLog = $"\n{name} {target.nameWithID}:";
        int cost = UtilityScripts.Utilities.Rng.Next(150, 201);
        costLog += $" +{cost}(Initial)";
        actor.logComponent.AppendCostLog(costLog);
        return cost;
    }
    public override void OnStopWhileStarted(ActualGoapNode node) {
        base.OnStopWhileStarted(node);
        Character actor = node.actor;
        actor.UncarryPOI();
    }
    public override void OnStopWhilePerforming(ActualGoapNode node) {
        base.OnStopWhilePerforming(node);
        Character actor = node.actor;
        IPointOfInterest poiTarget = node.poiTarget;
        actor.UncarryPOI();
        (node.poiTarget as TileObject).SetMapObjectState(MAP_OBJECT_STATE.UNBUILT);    
    }
    #endregion

    #region State Effects
    public void PreCraftSuccess(ActualGoapNode goapNode) {
        Character actor = goapNode.actor;
        TileObject obj = goapNode.poiTarget as TileObject;
        TileObjectData data = TileObjectDB.GetTileObjectData(obj.tileObjectType);
        if (data != null && data.itemRequirementsForCreation != null) {
            for (int i = 0; i < data.itemRequirementsForCreation.Length; i++) {
                string neededItem = data.itemRequirementsForCreation[i];
                if (neededItem == "Wood Pile") {
                    ResourcePile resourcePile = actor.GetItem(TILE_OBJECT_TYPE.WOOD_PILE) as ResourcePile;
                    resourcePile?.AdjustResourceInPile(-data.constructionCost);
                } else if (neededItem == "Stone Pile") {
                    ResourcePile resourcePile = actor.GetItem(TILE_OBJECT_TYPE.STONE_PILE) as ResourcePile;
                    resourcePile?.AdjustResourceInPile(-data.constructionCost);
                } else if (neededItem == "Metal Pile") {
                    ResourcePile resourcePile = actor.GetItem(TILE_OBJECT_TYPE.METAL_PILE) as ResourcePile;
                    resourcePile?.AdjustResourceInPile(-data.constructionCost);
                } else {
                    if (!actor.UnobtainItem(neededItem)) {
                        actor.logComponent.PrintLogErrorIfActive(
                            "Trying to craft " + obj.name + " but " + actor + " does not have " + neededItem);
                    }
                }
            }
        }
        obj.SetMapObjectState(MAP_OBJECT_STATE.BUILDING);
        goapNode.descriptionLog.AddToFillers(null, UtilityScripts.Utilities.GetArticleForWord(obj.tileObjectType.ToString()), LOG_IDENTIFIER.STRING_1);
        goapNode.descriptionLog.AddToFillers(null, UtilityScripts.Utilities.NormalizeStringUpperCaseFirstLetters(obj.tileObjectType.ToString()), LOG_IDENTIFIER.ITEM_1);
        if (goapNode.thoughtBubbleLog.hasValue) {
            goapNode.thoughtBubbleLog.AddToFillers(null, UtilityScripts.Utilities.GetArticleForWord(obj.tileObjectType.ToString()), LOG_IDENTIFIER.STRING_1);
            goapNode.thoughtBubbleLog.AddToFillers(null, UtilityScripts.Utilities.NormalizeStringUpperCaseFirstLetters(obj.tileObjectType.ToString()), LOG_IDENTIFIER.ITEM_1);
        }
    }
    public void AfterCraftSuccess(ActualGoapNode goapNode) {
        TileObject tileObj = goapNode.poiTarget as TileObject;
        tileObj.SetMapObjectState(MAP_OBJECT_STATE.BUILT);
        if (goapNode.associatedJobType == JOB_TYPE.CRAFT_MISSING_FURNITURE) {
            //after character finishes building the target furniture, set it as owned by him/her
            tileObj.SetCharacterOwner(goapNode.actor);
        }
        //goapNode.actor.AdjustResource(RESOURCE.WOOD, -TileObjectDB.GetTileObjectData((goapNode.poiTarget as TileObject).tileObjectType).constructionCost);
        //int cost = TileObjectDB.GetTileObjectData((goapNode.poiTarget as TileObject).tileObjectType).constructionCost;
        //goapNode.poiTarget.AdjustResource(RESOURCE.WOOD, -cost);
        //ResourcePile carriedPile = goapNode.actor.ownParty.carriedPOI as ResourcePile;
        //carriedPile.AdjustResourceInPile(-TileObjectDB.GetTileObjectData((goapNode.poiTarget as TileObject).tileObjectType).constructionCost);
    }
    #endregion

    #region Preconditions
    //private bool HasSupply(Character actor, IPointOfInterest poiTarget, object[] otherData) {
    //    int cost = TileObjectDB.GetTileObjectData((poiTarget as TileObject).tileObjectType).constructionCost;
    //    if (poiTarget.HasResourceAmount(RESOURCE.WOOD, cost)) {
    //        return true;
    //    }
    //    if (actor.ownParty.isCarryingAnyPOI && actor.ownParty.carriedPOI is WoodPile) {
    //        //ResourcePile carriedPile = actor.ownParty.carriedPOI as ResourcePile;
    //        //return carriedPile.resourceInPile >= cost;
    //        return true;
    //    }
    //    return false;
    //    //return actor.supply >= TileObjectDB.GetTileObjectData((poiTarget as TileObject).tileObjectType).constructionCost;
    //}
    private bool IsCarriedOrInInventory(Character actor, IPointOfInterest poiTarget, OtherData[] otherData, string itemName) {
        return actor.IsPOICarriedOrInInventory(itemName);
    }
    private bool HasWood(Character actor, IPointOfInterest poiTarget, OtherData[] otherData, JOB_TYPE jobType) {
        return poiTarget is TileObject tileObject && actor.GetItem(TILE_OBJECT_TYPE.WOOD_PILE) is ResourcePile pile && 
               pile.resourceInPile >= TileObjectDB.GetTileObjectData(tileObject.tileObjectType).constructionCost; 
    }
    private bool HasStone(Character actor, IPointOfInterest poiTarget, OtherData[] otherData, JOB_TYPE jobType) {
        return poiTarget is TileObject tileObject && actor.GetItem(TILE_OBJECT_TYPE.STONE_PILE) is ResourcePile pile && 
               pile.resourceInPile >= TileObjectDB.GetTileObjectData(tileObject.tileObjectType).constructionCost; 
    }
    private bool HasMetal(Character actor, IPointOfInterest poiTarget, OtherData[] otherData, JOB_TYPE jobType) {
        return poiTarget is TileObject tileObject && actor.GetItem(TILE_OBJECT_TYPE.METAL_PILE) is ResourcePile pile && 
               pile.resourceInPile >= TileObjectDB.GetTileObjectData(tileObject.tileObjectType).constructionCost; 
    }
    #endregion

    #region Requirement
    protected override bool AreRequirementsSatisfied(Character actor, IPointOfInterest poiTarget, OtherData[] otherData) {
        bool satisfied = base.AreRequirementsSatisfied(actor, poiTarget, otherData);
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