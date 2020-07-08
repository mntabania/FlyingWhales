using System.Collections;
using System.Collections.Generic;
using Inner_Maps;
using UnityEngine;  
using Traits;

public class CraftFurniture : GoapAction {

    public CraftFurniture() : base(INTERACTION_TYPE.CRAFT_FURNITURE) {
        actionLocationType = ACTION_LOCATION_TYPE.OVERRIDE;
        actionIconString = GoapActionStateDB.Build_Icon;
        showNotification = false;
        advertisedBy = new POINT_OF_INTEREST_TYPE[] { POINT_OF_INTEREST_TYPE.CHARACTER, POINT_OF_INTEREST_TYPE.TILE_OBJECT };
        racesThatCanDoAction = new RACE[] { RACE.HUMANS, RACE.ELVES, RACE.GOBLIN, RACE.FAERY };
    }

    #region Overrides
    protected override void ConstructBasePreconditionsAndEffects() {
        AddPrecondition(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.TAKE_POI, conditionKey = "Wood Pile", isKeyANumber = false, target = GOAP_EFFECT_TARGET.ACTOR }, HasSupply);
    }
    public override void Perform(ActualGoapNode goapNode) {
        base.Perform(goapNode);
        SetState("Craft Success", goapNode);
    }
    public override LocationGridTile GetOverrideTargetTile(ActualGoapNode goapNode) {
        return goapNode.otherData[0] as LocationGridTile;
    }
    protected override int GetBaseCost(Character actor, IPointOfInterest target, JobQueueItem job, object[] otherData) {
        return 2;
    }
    public override void AddFillersToLog(Log log, ActualGoapNode node) {
        base.AddFillersToLog(log, node);
        TILE_OBJECT_TYPE furnitureToCreate = (TILE_OBJECT_TYPE)node.otherData[1];
        log.AddToFillers(null, UtilityScripts.Utilities.GetArticleForWord(furnitureToCreate.ToString()), LOG_IDENTIFIER.STRING_1);
        log.AddToFillers(null, UtilityScripts.Utilities.NormalizeStringUpperCaseFirstLetters(furnitureToCreate.ToString()), LOG_IDENTIFIER.ITEM_1);
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

    #region Requirements
    protected override bool AreRequirementsSatisfied(Character actor, IPointOfInterest poiTarget, object[] otherData) {
        bool satisfied = base.AreRequirementsSatisfied(actor, poiTarget, otherData);
        if (satisfied) {
            if (actor != poiTarget) {
                return false;
            }
            LocationGridTile targetSpot = otherData[0] as LocationGridTile;
            TILE_OBJECT_TYPE furnitureToCreate = (TILE_OBJECT_TYPE)otherData[1];
            if (targetSpot.objHere != null) {
                return false; //cannot create furniture here because there is already something occupying it.
            }
            //if the crafted item enum has been set, check if the actor has the needed trait to craft it
            return furnitureToCreate.CanBeCraftedBy(actor);
        }
        return false;
    }
    #endregion

    #region Preconditions
    private bool HasSupply(Character actor, IPointOfInterest poiTarget, object[] otherData, JOB_TYPE jobType) {
        TILE_OBJECT_TYPE furnitureToCreate = (TILE_OBJECT_TYPE)otherData[1];
        int cost = TileObjectDB.GetTileObjectData(furnitureToCreate).constructionCost;
        if (poiTarget.HasResourceAmount(RESOURCE.WOOD, cost)) {
            return true;
        }
        if (actor.carryComponent.isCarryingAnyPOI && actor.carryComponent.carriedPOI is WoodPile) {
            //ResourcePile carriedPile = actor.ownParty.carriedPOI as ResourcePile;
            //return carriedPile.resourceInPile >= cost;
            return true;
        }
        return false;
        //return actor.supply >= TileObjectDB.GetTileObjectData(furnitureToCreate).constructionCost;
    }
    #endregion

    #region State Effects
    public void PreCraftSuccess(ActualGoapNode goapNode) {
        TILE_OBJECT_TYPE furnitureToCreate = (TILE_OBJECT_TYPE)goapNode.otherData[1];
        if(goapNode.actor.carryComponent.carriedPOI != null) {
            ResourcePile carriedPile = goapNode.actor.carryComponent.carriedPOI as ResourcePile;
            int cost = TileObjectDB.GetTileObjectData(furnitureToCreate).constructionCost;
            carriedPile.AdjustResourceInPile(-cost);
            goapNode.poiTarget.AdjustResource(RESOURCE.WOOD, cost);
        }

        goapNode.descriptionLog.AddToFillers(null, UtilityScripts.Utilities.GetArticleForWord(furnitureToCreate.ToString()), LOG_IDENTIFIER.STRING_1);
        goapNode.descriptionLog.AddToFillers(null, UtilityScripts.Utilities.NormalizeStringUpperCaseFirstLetters(furnitureToCreate.ToString()), LOG_IDENTIFIER.ITEM_1);
    }
    public void AfterCraftSuccess(ActualGoapNode goapNode) {
        LocationGridTile targetSpot = goapNode.otherData[0] as LocationGridTile;
        TILE_OBJECT_TYPE furnitureToCreate = (TILE_OBJECT_TYPE)goapNode.otherData[1];
        //goapNode.actor.AdjustSupply(-TileObjectDB.GetTileObjectData(furnitureToCreate).constructionCost);
        int cost = TileObjectDB.GetTileObjectData(furnitureToCreate).constructionCost;
        goapNode.poiTarget.AdjustResource(RESOURCE.WOOD, -cost);
        if (targetSpot.objHere == null) {
            targetSpot.structure.AddPOI(InnerMapManager.Instance.CreateNewTileObject<TileObject>(furnitureToCreate), targetSpot);
        }
    }
    #endregion
}

public class CraftFurnitureData : GoapActionData {
    public CraftFurnitureData() : base(INTERACTION_TYPE.CRAFT_FURNITURE) {
        racesThatCanDoAction = new RACE[] { RACE.HUMANS, RACE.ELVES, RACE.GOBLIN, RACE.FAERY };
        requirementAction = Requirement;
    }

    private bool Requirement(Character actor, IPointOfInterest poiTarget, object[] otherData) {
        if (actor != poiTarget) {
            return false;
        }
        if(otherData == null) {
            return true;
        }
        if (otherData.Length == 2 && otherData[1] is FURNITURE_TYPE && otherData[0] is LocationGridTile) {
            if ((otherData[0] as LocationGridTile).objHere != null) {
                return false; //cannot create furniture here because there is already something occupying it.
            }
            FURNITURE_TYPE furnitureToCreate = (FURNITURE_TYPE) otherData[1];
            //if the creafted enum has NOT been set, always allow, since we know that the character has the ability to craft furniture 
            //because craft furniture action is only added to characters with traits that allow crafting
            return furnitureToCreate.ConvertFurnitureToTileObject().CanBeCraftedBy(actor);
        }
        return false;
    }
}
